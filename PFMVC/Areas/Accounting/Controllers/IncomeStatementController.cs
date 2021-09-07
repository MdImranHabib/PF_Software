using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Microsoft.Reporting.WebForms;
using DLL.ViewModel;
using System.Globalization;

namespace PFMVC.Areas.Accounting.Controllers
{
    public class IncomeStatementController : BaseController
    {

        int PageID = 21;
        MvcApplication _MvcApplication;

        public ActionResult IncomeStatementIndex()
        {
            //Added By Avishek Date:Jan-19-2016
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End
            return View();
        }

        public ActionResult GetAsset(DateTime fromDate, DateTime toDate)
        {
            //Added By Avishek Date:Jan-19-2016
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End
            var v = unitOfWork.AccountingRepository.sp_GetGroupSummaryByNatureID(fromDate, toDate, 1, oCode); // here 1 for Asset
            return PartialView("_AssetSummary", v);
        }

        public ActionResult GetLiabilities(DateTime fromDate, DateTime toDate)
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            //var v = unitOfWork.AccountingRepository.GenerateBalanceBook(null, 2, null, Convert.ToDateTime("01/01/3099")); // 2 for liabilities
            var v = unitOfWork.AccountingRepository.sp_GetGroupSummaryByNatureID(fromDate, toDate, 2, oCode); // here 2 for Liabilities
            //var netProfit = unitOfWork.AccountingRepository.GetNetProfit(DateTime.Now);
            //ViewBag.NetProfit = netProfit;
            return PartialView("_LiabilitiesSummary", v);
        }

        public decimal GetRetainEarningOpening(DateTime fromDateAsToDate)
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            var retainEarningOpening = unitOfWork.AccountingRepository.GetRetainEarningOpening(fromDateAsToDate, oCode);
            return retainEarningOpening;
        }

        public decimal GetRetainEarningPeriod(DateTime fromDate, DateTime toDate)
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            var retainEarningPeriod = unitOfWork.AccountingRepository.GetRetainEarningPeriod(fromDate, toDate, oCode);
            return retainEarningPeriod;
        }

        public ActionResult GetGroupDetail(int groupID, DateTime fromDate, DateTime toDate)
        {
            var v = unitOfWork.AccountingRepository.sp_GetGroupDetail(fromDate, toDate, groupID);
            return PartialView("_GroupDetail", v);
        }

        public ActionResult GetIncomeStatement(DateTime fromDate, DateTime toDate)
        {
            CultureInfo cInfo = new CultureInfo("en-IN");
            //Added By Avishek Date:Jan-19-2016
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End
            //added by Avishek Date Sep-22-2015
            //Start
            _MvcApplication = new MvcApplication();
            if (oCode < 1)
            {
                return Json(new { Success = false, ErrorMessage = "User must associate with a company! Current user doestn't associate with any company!" }, JsonRequestBehavior.AllowGet);
            }
            //DateTime sysImplimentationDate = unitOfWork.CompanyInformationRepository.GetByID(oCode).SystemImplementationDate ?? DateTime.MaxValue;   //Edited by Avishek Date: May-18-2016

            decimal expSumDebit = 0;
            decimal expSumCredit = 0;
            decimal incomeSumDebit = 0;
            decimal incomeSumCredit = 0;
            
            toDate = toDate.AddHours(23).AddMinutes(59).AddSeconds(59);


            var debitLedgerList = unitOfWork.AccountingRepository.sp_GetLedgerSummary(fromDate, toDate, 3, oCode);
            foreach (var item in debitLedgerList)
            {
                //if (sysImplimentationDate >= fromDate)    //Edited by Avishek Date: May-18-2016
                //{
                    if (item.BalanceType == 1 || item.BalanceType == 3) // 1 for credit but process is for debit type
                    {
                        item.Balance = -(_MvcApplication.GetNumber(item.InitialBalance ?? 0)) - _MvcApplication.GetNumber(item.Credit) + _MvcApplication.GetNumber(item.Debit);
                    }
                    else if (item.BalanceType == 2 || item.BalanceType == 4) // 2 for debit and process is for debit type
                    {
                        item.Balance = _MvcApplication.GetNumber(item.InitialBalance ?? 0) - _MvcApplication.GetNumber(item.Credit) + _MvcApplication.GetNumber(item.Debit);
                    }
                    else
                    {
                        item.Balance = _MvcApplication.GetNumber(item.Debit) - _MvcApplication.GetNumber(item.Credit);
                    }
                //}
                //else
                 // {
                //    item.Balance = _MvcApplication.GetNumber(item.Debit) - _MvcApplication.GetNumber(item.Credit);
                //}
                //Edited by Avishek Date: May-18-2016
                if (item.Balance < 0)
                {
                    item.strCredit = _MvcApplication.GetNumber(item.Balance * (-1)).ToString("N", cInfo) + "";// Cr.
                    item.strDebit = "";
                    //i am doing this for easy calculatin group balance
                    item.Credit = _MvcApplication.GetNumber(item.Balance) * (-1);
                    item.Debit = 0;
                    expSumCredit += _MvcApplication.GetNumber(item.Credit);
                    item.strBalance = _MvcApplication.GetNumber(item.Balance * (-1)).ToString("N", cInfo) + " Cr.";
                }
                else
                {
                    item.strDebit = _MvcApplication.GetNumber(item.Balance) + "";//Dr.
                    item.strCredit = "";
                    item.Debit = _MvcApplication.GetNumber(item.Balance);
                    item.Credit = 0;
                    expSumDebit += _MvcApplication.GetNumber(item.Debit);
                    item.strBalance = _MvcApplication.GetNumber(item.Balance).ToString("N", cInfo) + " Dr.";
                }
            }
            var debitGroupList = (from a in debitLedgerList
                                   group a by new
                                   {
                                       a.GroupID,
                                       a.GroupName
                                   } into grp
                                   select new VM_acc_ledger
                                   {
                                       GroupID = grp.Key.GroupID,
                                       GroupName = grp.Key.GroupName,
                                       strBalance = grp.Sum(s => s.Balance) < 0 ? (grp.Sum(s => s.Balance) * (-1)).ToString("N",cInfo) + " Cr." : (grp.Sum(s => s.Balance)).ToString("N",cInfo) + " Dr.",
                                       //strDebit = grp.Sum(s => s.Debit) - grp.Sum(s => s.Credit) > 0 ? grp.Sum(s => s.Debit) - grp.Sum(s => s.Credit) + "" : "",
                                       //strCredit = grp.Sum(s => s.Debit) - grp.Sum(s => s.Credit) < 0 ? (grp.Sum(s => s.Debit) - grp.Sum(s => s.Credit)) * (-1) + "" : "",
                                   }).ToList();
            decimal expBalance = _MvcApplication.GetNumber(expSumDebit) - _MvcApplication.GetNumber(expSumCredit);
            ViewBag.ExpBalance = _MvcApplication.GetNumber(expBalance) < 0 ? (_MvcApplication.GetNumber(expBalance) * (-1)).ToString("N",cInfo) + " Cr." : _MvcApplication.GetNumber(expBalance).ToString("N",cInfo)+ " Dr.";

            //get credit type. 2 for liab and 4 for income
            var creditLedgerList = unitOfWork.AccountingRepository.sp_GetLedgerSummary(fromDate, toDate, 4, oCode);

            foreach (var item in creditLedgerList)
            {
                //if (sysImplimentationDate >= fromDate)      //Edited by Avishek Date: May-18-2016
                //{
                    if (item.BalanceType == 1 || item.BalanceType == 3) // 1 for credit
                    {
                        item.Balance = _MvcApplication.GetNumber(item.InitialBalance ?? 0) + _MvcApplication.GetNumber(item.Credit) - _MvcApplication.GetNumber(item.Debit);
                    }
                    else if (item.BalanceType == 2 || item.BalanceType == 4) // 2 for debit
                    {
                        item.Balance = -(_MvcApplication.GetNumber(item.InitialBalance ?? 0)) + _MvcApplication.GetNumber(item.Credit) - _MvcApplication.GetNumber(item.Debit);
                    }
                    else
                    {
                        item.Balance = _MvcApplication.GetNumber(item.Credit) - _MvcApplication.GetNumber(item.Debit);
                    }
                //}
                    //else    //Edited by Avishek Date: May-18-2016
                //{
                //    item.Balance = _MvcApplication.GetNumber(item.Credit) - _MvcApplication.GetNumber(item.Debit);
                //}
                if (item.Balance < 0)
                {
                    item.strDebit = _MvcApplication.GetNumber(item.Balance * (-1)) + "";//Dr.
                    item.strCredit = "";
                    //i am doing this for easy calculatin group balance
                    item.Debit = _MvcApplication.GetNumber(item.Balance * (-1));
                    item.Credit = 0;
                    //sum result
                    incomeSumDebit += _MvcApplication.GetNumber(item.Debit);
                    item.strBalance = _MvcApplication.GetNumber(item.Balance * (-1)).ToString("N", cInfo) + " Dr.";
                }
                else
                {
                    item.strCredit = _MvcApplication.GetNumber(item.Balance) + "";// Cr.
                    item.strDebit = "";
                    item.Credit = _MvcApplication.GetNumber(item.Balance);
                    item.Debit = 0;
                    incomeSumCredit += _MvcApplication.GetNumber(item.Credit);
                    item.strBalance = _MvcApplication.GetNumber(item.Balance).ToString("N", cInfo) + " Cr.";
                }
            }
            //new : have to change those ledger which has term retain: need to analyse more later
            //var retain_ledger_list = credit_ledger_list.Where(f => f.LedgerName.ToLower().Contains("retain")).ToList();
            //credit_ledger_list.RemoveAll(f => f.LedgerName.ToLower().Contains("retain"));
            //
            var creditGroupList = (from a in creditLedgerList
                                    group a by new
                                    {
                                        a.GroupID,
                                        a.GroupName
                                    } into grp
                                    select new VM_acc_ledger
                                    {
                                        GroupID = grp.Key.GroupID,
                                        GroupName = grp.Key.GroupName,
                                        Debit = grp.Sum(s => s.Debit),
                                        strBalance = grp.Sum(s => s.Balance) < 0 ? (grp.Sum(s => s.Balance) * (-1)).ToString("N",cInfo) + " Dr." : (grp.Sum(s => s.Balance)).ToString("N",cInfo) + " Cr."
                                        //Credit = grp.Sum(s => s.Credit),
                                        //strCredit = grp.Sum(s => s.Credit) - grp.Sum(s => s.Debit) > 0 ? grp.Sum(s => s.Credit) - grp.Sum(s => s.Debit) + "" : "",
                                        //strDebit = grp.Sum(s => s.Credit) - grp.Sum(s => s.Debit) < 0 ? (grp.Sum(s => s.Credit) - grp.Sum(s => s.Debit)) * (-1) + "" : ""
                                    }).ToList();

            decimal incomeBalance = _MvcApplication.GetNumber(incomeSumCredit) - _MvcApplication.GetNumber(incomeSumDebit);// +(RetainOpening + RetainPeriod);
            ViewBag.IncomeBalance = _MvcApplication.GetNumber(incomeBalance) < 0 ? _MvcApplication.GetNumber(incomeBalance * (-1)).ToString("N",cInfo) + " Dr." : _MvcApplication.GetNumber(incomeBalance).ToString("N",cInfo) + " Cr.";
            //return PartialView("_BalanceSheet", new Tuple<List<VM_acc_ledger>, List<VM_acc_ledger>, List<VM_acc_ledger>, List<VM_acc_ledger>, List<VM_acc_ledger>>( debit_group_list.ToList(), debit_ledger_list.ToList(), credit_group_list.ToList(), credit_ledger_list.ToList(), retain_ledger_list.ToList()));

            if (creditGroupList.Count > 0)
            {
                TempData["CreditLedgerList"] = creditLedgerList.ToList();
                TempData["CreditGroupList"] = creditGroupList.ToList();
            }
            
            if (debitGroupList.Count > 0)
            {
                TempData["DebitGroupList"] = debitGroupList.ToList();
                TempData["DebitLedgerList"] = debitLedgerList.ToList();
            }

            if ((incomeBalance - expBalance) < 0)
            {
                ViewBag.Loss = _MvcApplication.GetNumber((incomeBalance - expBalance) * (-1)).ToString("N", cInfo) + " Dr.";
            }
            else
            {
                ViewBag.Profit = _MvcApplication.GetNumber(incomeBalance - expBalance).ToString("N", cInfo) + " Cr.";
            }
            //TempData["RetainLedgerList"] = retain_ledger_list.ToList();
            //End
            return PartialView("_IncomeStatement");
        }

        public ActionResult IncomeStatementReport(DateTime fromDate, DateTime toDate)
        {
            //Added By Avishek Date:Jan-19-2016
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End

            //Added By Avishek Date Apr-6-2015
            //Start
            CultureInfo CInfo = new CultureInfo("en-IN");
            _MvcApplication = new MvcApplication();
            if (oCode < 1)
            {
                return Json(new { Success = false, ErrorMessage = "User must associate with a company! Current user doestn't associate with any company!" }, JsonRequestBehavior.AllowGet);
            }
            DateTime sysImplimentationDate = unitOfWork.CompanyInformationRepository.GetByID(oCode).SystemImplementationDate ?? DateTime.MaxValue;

            decimal expSumDebit = 0;
            decimal expSumCredit = 0;
            decimal incomeSumDebit = 0;
            decimal incomeSumCredit = 0;
            string lossOrProfit = "";
            LocalReport lr = new LocalReport();
            string path = Path.Combine(Server.MapPath("~/Areas/Accounting/AccountingReport"), "IncomeStatement.rdlc");
            if (System.IO.File.Exists(path))
            {
                lr.ReportPath = path;
            }
            else
            {
                return View("Report/ReportPF/Index");
            }
            toDate = toDate.AddHours(23).AddMinutes(59).AddSeconds(59);


            var debitLedgerList = unitOfWork.AccountingRepository.sp_GetLedgerSummary(fromDate, toDate, 3, oCode);
            foreach (var item in debitLedgerList)
            {
                //if (sysImplimentationDate >= fromDate)
                //{
                    if (item.BalanceType == 1 || item.BalanceType == 3) // 1 for credit but process is for debit type
                    {
                        item.Balance = -(_MvcApplication.GetNumber(item.InitialBalance ?? 0)) - _MvcApplication.GetNumber(item.Credit) +  _MvcApplication.GetNumber(item.Debit);
                    }
                    else if (item.BalanceType == 2 || item.BalanceType == 4) // 2 for debit and process is for debit type
                    {
                        item.Balance = _MvcApplication.GetNumber(item.InitialBalance ?? 0) - _MvcApplication.GetNumber(item.Credit) + _MvcApplication.GetNumber(item.Debit);
                    }
                    else
                    {
                        item.Balance = _MvcApplication.GetNumber(item.Debit) - (item.Credit == 0 ? 0 : _MvcApplication.GetNumber(item.Credit));
                    }
                    //Edited by Avishek Date: May-18-2016
                if (item.Balance < 0)
                {
                    item.strCredit = _MvcApplication.GetNumber(item.Balance) * (-1) + "";// Cr.
                    item.strDebit = "";
                    //i am doing this for easy calculatin group balance
                    item.Credit = _MvcApplication.GetNumber(item.Balance) * (-1);
                    item.Debit = 0;
                    expSumCredit += _MvcApplication.GetNumber(item.Credit);
                    item.strBalance = _MvcApplication.GetNumber(item.Balance * (-1)).ToString("N", CInfo) + " Cr.";
                }
                else
                {
                    item.strDebit = _MvcApplication.GetNumber(item.Balance) + "";//Dr.
                    item.strCredit = "";
                    item.Debit = _MvcApplication.GetNumber(item.Balance);
                    item.Credit = 0;
                    expSumDebit += _MvcApplication.GetNumber(item.Debit);
                    item.strBalance = _MvcApplication.GetNumber(item.Balance).ToString("N", CInfo) + " Dr.";
                }
            }
            decimal expBalance = _MvcApplication.GetNumber(expSumDebit) - _MvcApplication.GetNumber(expSumCredit);
            ViewBag.ExpBalance = _MvcApplication.GetNumber(expBalance) < 0 ? (_MvcApplication.GetNumber(expBalance * (-1))).ToString("N", CInfo) + " Cr." : _MvcApplication.GetNumber(expBalance).ToString("N", CInfo) + " Dr.";

            //get credit type. 2 for liab and 4 for income
            var creditLedgerList = unitOfWork.AccountingRepository.sp_GetLedgerSummary(fromDate, toDate, 4, oCode);

            foreach (var item in creditLedgerList)
            {
                //if (sysImplimentationDate >= fromDate)
                //{
                    if (item.BalanceType == 1 || item.BalanceType == 3) // 1 for credit
                    {
                        item.Balance = _MvcApplication.GetNumber(item.InitialBalance ?? 0) + _MvcApplication.GetNumber(item.Credit) - _MvcApplication.GetNumber(item.Debit);
                    }
                    else if (item.BalanceType == 2 || item.BalanceType == 4) // 2 for debit
                    {
                        item.Balance = -(_MvcApplication.GetNumber(item.InitialBalance ?? 0)) + _MvcApplication.GetNumber(item.Credit) - _MvcApplication.GetNumber(item.Debit);
                    }
                    else
                    {
                        item.Balance = _MvcApplication.GetNumber(item.Credit) - _MvcApplication.GetNumber(item.Debit);
                    }

                if (item.Balance < 0)
                {
                    item.strDebit = _MvcApplication.GetNumber(item.Balance) * (-1) + "";//Dr.
                    item.strCredit = "";
                    //i am doing this for easy calculatin group balance
                    item.Debit = _MvcApplication.GetNumber(item.Balance) * (-1);
                    item.Credit = 0;
                    //sum result
                    incomeSumDebit += _MvcApplication.GetNumber(item.Debit);
                    item.strBalance = _MvcApplication.GetNumber(item.Balance * (-1)).ToString("N", CInfo) + " Dr.";
                }
                else
                {
                    item.strCredit = _MvcApplication.GetNumber(item.Balance) + "";// Cr.
                    item.strDebit = "";
                    item.Credit = _MvcApplication.GetNumber(item.Balance);
                    item.Debit = 0;
                    incomeSumCredit += _MvcApplication.GetNumber(item.Credit);
                    item.strBalance = _MvcApplication.GetNumber(item.Balance).ToString("N", CInfo) + " Cr.";
                }
            }
           
            decimal incomeBalance = incomeSumCredit - incomeSumDebit;// +(RetainOpening + RetainPeriod);
            ViewBag.IncomeBalance = incomeBalance < 0 ? (incomeBalance * (-1)).ToString("N",CInfo) + " Dr." : incomeBalance.ToString("N",CInfo) + " Cr.";
         

            if ((incomeBalance - expBalance) < 0)
            {
                lossOrProfit = "Net Loss: " + _MvcApplication.GetNumber((incomeBalance - expBalance) * (-1)).ToString("N", CInfo) + " Dr.";
            }
            else
            {
                lossOrProfit = "Net Profit: " + _MvcApplication.GetNumber(incomeBalance - expBalance).ToString("N", CInfo) + " Cr.";
            }

            var companyInformation = unitOfWork.CompanyInformationRepository.Get(o => o.CompanyID == oCode).Single();
            if (companyInformation != null)
            {
                ReportParameterCollection reportParameters = new ReportParameterCollection();
                reportParameters.Add(new ReportParameter("rpCompanyName", companyInformation.CompanyName));
                reportParameters.Add(new ReportParameter("rpCompanyAddress", companyInformation.CompanyAddress));
                reportParameters.Add(new ReportParameter("rpFromDate", fromDate.ToString("dd/MMM/yyyy")));
                reportParameters.Add(new ReportParameter("rpToDate", toDate.ToString("dd/MMM/yyyy")));
                reportParameters.Add(new ReportParameter("rpLossOrProfit", lossOrProfit));
                lr.SetParameters(reportParameters);
            }
            //End
            lr.DataSources.Clear();
            ReportDataSource rd1 = new ReportDataSource("DS_Expense", debitLedgerList);
            ReportDataSource rd2 = new ReportDataSource("DS_Income", creditLedgerList);
            lr.DataSources.Add(rd1);
            lr.DataSources.Add(rd2);

            string reportType = "PDF";
            string mimeType;
            string encoding;
            string fileNameExtension;
            string deviceInfo =

            @"<DeviceInfo>" +
            "  <OutputFormat>PDF</OutputFormat>" +
            "</DeviceInfo>";

            Warning[] warnings;
            string[] streams;
            byte[] renderedBytes;

            renderedBytes = lr.Render(
                reportType,
                deviceInfo,
                out mimeType,
                out encoding,
                out fileNameExtension,
                out streams,
                out warnings);


            return File(renderedBytes, mimeType);
        }

        public ActionResult GetFilterBox()
        {
            return PartialView("_FilterBoxIncomeStatement");
        }

    }
}
