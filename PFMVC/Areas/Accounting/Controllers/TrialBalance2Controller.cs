using DLL.Repository;
using DLL.ViewModel;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace PFMVC.Areas.Accounting.Controllers
{
    public class TrialBalance2Controller : Controller
    {

        int PageID = 20;
        UnitOfWork unitOfWork = new UnitOfWork();

        public ActionResult Index()
        {
            //Added By Avishek Date:Jan-19-2016
            int OCode = ((int?)Session["OCode"]) ?? 0;
            if (OCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End
            return View("TrialBalance2Index");
        }

        public ActionResult GetAsset(DateTime fromDate, DateTime toDate)
        {
            //Added By Avishek Date:Jan-19-2016
            int OCode = ((int?)Session["OCode"]) ?? 0;
            if (OCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End
            var v = unitOfWork.AccountingRepository.sp_GetGroupSummaryByNatureID(fromDate, toDate, 1, OCode); // here 1 for Asset
            return PartialView("_AssetSummary", v);
        }

        public decimal GetRetainEarningOpening(DateTime fromDateAsToDate)
        {
            int OCode = ((int?)Session["OCode"]) ?? 0;
            var retainEarningOpening = unitOfWork.AccountingRepository.GetRetainEarningOpening(fromDateAsToDate, OCode);
            return retainEarningOpening;
        }

        public decimal GetRetainEarningPeriod(DateTime fromDate, DateTime toDate)
        {
            int OCode = ((int?)Session["OCode"]) ?? 0;
            var retainEarningPeriod = unitOfWork.AccountingRepository.GetRetainEarningPeriod(fromDate, toDate, OCode);
            return retainEarningPeriod;
        }

        public ActionResult GetGroupDetail(int groupID, DateTime fromDate, DateTime toDate)
        {
            var v = unitOfWork.AccountingRepository.sp_GetGroupDetail(fromDate, toDate, groupID);
            return PartialView("_GroupDetail", v);
        }

        public ActionResult GetTrialBalance(DateTime fromDate, DateTime toDate)
        {
            //Added By Avishek Date:Jan-19-2016
            int OCode = ((int?)Session["OCode"]) ?? 0;
            if (OCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End
            decimal sumDebit = 0;
            decimal sumCredit = 0;

            #region let's check RetainOpening and Retain Period
            decimal RetainOpening = GetRetainEarningOpening(fromDate);
            decimal RetainPeriod = GetRetainEarningPeriod(fromDate, toDate);
            //Retain earning credit type nature
            if (RetainOpening < 0)
            {
                ViewBag.RetainEarningOpening = RetainOpening * (-1) + " Dr.";
            }
            else if (RetainOpening > 0)
            {
                ViewBag.RetainEarningOpening = RetainOpening + " Cr.";
            }
            if (RetainPeriod < 0)
            {
                ViewBag.RetainEarningPeriod = RetainPeriod * (-1) + " Dr.";
            }
            else if (RetainPeriod > 0)
            {
                ViewBag.RetainEarningPeriod = RetainPeriod + " Cr.";
            }
            #endregion


            //let's prepare group summary 
            var all_ledger_list = unitOfWork.AccountingRepository.sp_GetLedgerSummaryAll(fromDate, toDate, OCode);
            
            //get credit type. 2 for Liability and 4 for Income
            var credit_ledger_list = all_ledger_list.Where(f => f.NatureID == 2 || f.NatureID == 4).ToList();
            
            foreach (var item in credit_ledger_list)
            {
                if (item.BalanceType == 1) // 1 for credit
                {
                    item.Balance = ((item.InitialBalance ?? 0) + item.Credit) - item.Debit;
                }
                else if (item.BalanceType == 2) // 2 for debit
                {
                    item.Balance = ((-item.InitialBalance ?? 0) + item.Credit) - item.Debit;
                }
                else
                {
                    item.Balance = item.Credit - item.Debit;
                }
                if (item.Balance < 0)
                {
                    item.strDebit = item.Balance*(-1)+"";//Dr.
                   
                    item.strCredit = "";
                    //i am doing this for easy calculatin group balance
                    item.Debit = item.Balance*(-1);
                    item.Credit = 0;
                    //sum result
                    sumDebit += item.Debit;
                }
                else
                {
                    item.strCredit = item.Balance+"";// Cr.
                    item.strDebit = "";
                    item.Credit = item.Balance;
                    item.Debit = 0;
                    sumCredit += item.Credit;
                }
            }
            var credit_group_list = from a in credit_ledger_list
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
                                        //strBalance = grp.Sum(s => s.Balance) < 0 ? grp.Sum(s => s.Balance) * (-1) + " Dr." : grp.Sum(s => s.Balance) + " Cr."
                                        Credit = grp.Sum(s => s.Credit),
                                        strCredit = grp.Sum(s => s.Credit) -grp.Sum(s =>s.Debit) > 0 ? grp.Sum(s => s.Credit) -grp.Sum(s =>s.Debit) +"":"",
                                        strDebit = grp.Sum(s => s.Credit) - grp.Sum(s => s.Debit) < 0 ? (grp.Sum(s => s.Credit) - grp.Sum(s => s.Debit))*(-1) + "" : ""
                                    };

            //get debit type. 1 for libilities and 3 for expense
            var debit_ledger_list = all_ledger_list.Where(f => f.NatureID == 1 || f.NatureID == 3).ToList();

            foreach (var item in debit_ledger_list)
            {
                if (item.BalanceType == 1) // 1 for credit but process is for debit type
                {
                    item.Balance = ((- item.InitialBalance ?? 0) - item.Credit) + item.Debit;
                }
                else if (item.BalanceType == 2) // 2 for debit and process is for debit type
                {
                    item.Balance = ((item.InitialBalance ?? 0) - item.Credit) + item.Debit;
                }
                else
                {
                    item.Balance = item.Debit - item.Credit;
                }
                if (item.Balance < 0)
                {
                    //item.strCredit = item.Balance * (-1) + "";// Cr.
                    //item.strDebit = "";
                    ////i am doing this for easy calculatin group balance
                    //item.Credit = item.Balance*(-1);
                    //item.Debit = 0;
                    //sumCredit += item.Credit;

                    item.strDebit = item.Balance * (-1) + "";//Dr.
                    item.strDebit = "(" + item.strDebit + ")";
                    item.strCredit = "";
                    item.Debit = item.Balance;
                    item.Credit = 0;
                    sumDebit += item.Debit;
                }
                else
                {
                    item.strDebit = item.Balance + "";//Dr.
                    item.strCredit = "";
                    item.Debit = item.Balance;
                    item.Credit = 0;
                    sumDebit += item.Debit;
                }
            }
            var debit_group_list = from a in debit_ledger_list
                                    group a by new
                                    {
                                        a.GroupID,
                                        a.GroupName
                                    } into grp
                                    select new VM_acc_ledger
                                    {
                                        GroupID = grp.Key.GroupID,
                                        GroupName = grp.Key.GroupName,
                                        strDebit = grp.Sum(s => s.Debit) - grp.Sum(s => s.Credit) > 0 ? grp.Sum(s => s.Debit) - grp.Sum(s => s.Credit) + "" : "",
                                        strCredit = grp.Sum(s => s.Debit) - grp.Sum(s => s.Credit) < 0 ? (grp.Sum(s => s.Debit) - grp.Sum(s => s.Credit))*(-1) + "" : "",
                                    };
            var merge_group_list = credit_group_list.Union(debit_group_list).OrderBy(o => o.GroupName);
            
            ViewBag.sumCredit = sumCredit;
            ViewBag.sumDebit = sumDebit;
            return PartialView("_TrialBalance", new Tuple<List<VM_acc_ledger>, List<VM_acc_ledger>>(merge_group_list.ToList(), all_ledger_list.ToList() ));
        }

        public ActionResult GetFilterBox()
        {
            return PartialView("_FilterBox");
        }

        public ActionResult TrialBalanceReport(DateTime fromDate, DateTime toDate)
        {

            //Added By Avishek Date:Jan-19-2016
            int OCode = ((int?)Session["OCode"]) ?? 0;
            if (OCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End
            LocalReport lr = new LocalReport();

            string path = Path.Combine(Server.MapPath("~/Areas/Accounting/AccountingReport"), "TrialBalance.rdlc");
            if (System.IO.File.Exists(path))
            {
                lr.ReportPath = path;
            }
            else
            {
                return View("Report/ReportPF/Index");
            }


            #region 
            decimal sumDebit = 0;
            decimal sumCredit = 0;
            decimal RetainOpening = GetRetainEarningOpening(fromDate);
            decimal RetainPeriod = GetRetainEarningPeriod(fromDate, toDate);
            string strRetainOpening = "";
            string strRetainPeriod = "";
            //Retain earning credit type nature
            if (RetainOpening < 0)
            {
                strRetainOpening = RetainOpening * (-1) + " Dr.";
            }
            else if (RetainOpening > 0)
            {
                strRetainOpening = RetainOpening + " Cr.";
            }
            if (RetainPeriod < 0)
            {
                strRetainPeriod = RetainPeriod * (-1) + " Dr.";
            }
            else if (RetainPeriod > 0)
            {
                strRetainPeriod = RetainPeriod + " Cr.";
            }
            #endregion

            ////start processing
            ////let's prepare group summary 
           
            var all_ledger_list = unitOfWork.AccountingRepository.sp_GetLedgerSummaryAll(fromDate, toDate, OCode);

            //get credit type. 2 for Liability and 4 for Income
            var credit_ledger_list = all_ledger_list.Where(f => f.NatureID == 2 || f.NatureID == 4).ToList();

            foreach (var item in credit_ledger_list)
            {
                if (item.BalanceType == 1) // 1 for credit
                {
                    item.Balance = ((item.InitialBalance ?? 0) + item.Credit) - item.Debit;
                }
                else if (item.BalanceType == 2) // 2 for debit
                {
                    item.Balance = ((-item.InitialBalance ?? 0) + item.Credit) - item.Debit;
                }
                else
                {
                    item.Balance = item.Credit - item.Debit;
                }
                if (item.Balance < 0)
                {
                    item.strDebit = item.Balance * (-1) + "";
                    item.decDebit = Convert.ToDecimal(item.strDebit);
                    item.strCredit = "";
                    //item.decCredit = 0;
                    //i am doing this for easy calculatin group balance
                    item.Debit = item.Balance * (-1);
                    item.Credit = 0;
                    //sum result
                    sumDebit += item.Debit;
                }
                else
                {
                    item.strCredit = item.Balance + "";// Cr.
                    item.decCredit = Convert.ToDecimal(item.strCredit);
                    item.strDebit = "";
                    //item.decCredit = 0;
                    item.Credit = item.Balance;
                    item.Debit = 0;
                    sumCredit += item.Credit;
                }
            }
           

            //get debit type. 1 for libilities and 3 for expense
            var debit_ledger_list = all_ledger_list.Where(f => f.NatureID == 1 || f.NatureID == 3).ToList();

            foreach (var item in debit_ledger_list)
            {
                if (item.BalanceType == 1) // 1 for credit but process is for debit type
                {
                    item.Balance = ((-item.InitialBalance ?? 0) - item.Credit) + item.Debit;
                }
                else if (item.BalanceType == 2) // 2 for debit and process is for debit type
                {
                    item.Balance = ((item.InitialBalance ?? 0) - item.Credit) + item.Debit;
                }
                else
                {
                    item.Balance = item.Debit - item.Credit;
                }
                if (item.Balance < 0)
                {
                    //item.strCredit = item.Balance * (-1) + "";// Cr.
                    //item.strDebit = "";
                    ////i am doing this for easy calculatin group balance
                    //item.Credit = item.Balance*(-1);
                    //item.Debit = 0;
                    //sumCredit += item.Credit;

                    item.strDebit = item.Balance + "";//Dr.
                    item.decDebit = Convert.ToDecimal(item.strDebit);
                    item.strCredit = "";
                    //item.decCredit = 0;
                    item.Debit = item.Balance;
                    item.Credit = 0;
                    sumDebit += item.Debit;
                }
                else
                {
                    item.strDebit = item.Balance + "";//Dr.
                    item.decDebit = Convert.ToDecimal(item.strDebit);
                    item.strDebit = "";
                    //item.decDebit = 0;
                    item.Debit = item.Balance;
                    item.Credit = 0;
                    sumDebit += item.Debit;
                }
            }
            
            //var merge_group_list = credit_group_list.Union(debit_group_list).OrderBy(o => o.GroupName);
            var merge_ledger_list = credit_ledger_list.Union(debit_ledger_list).OrderBy(o => o.GroupName);

            //let's prepare group summary 
            


            //end processing

            var companyInformation = unitOfWork.CompanyInformationRepository.Get().Where(o => o.CompanyID == OCode).Single();
            if (companyInformation != null)
            {
                ReportParameterCollection reportParameters = new ReportParameterCollection();
                reportParameters.Add(new ReportParameter("rpCompanyName", companyInformation.CompanyName));
                reportParameters.Add(new ReportParameter("rpCompanyAddress", companyInformation.CompanyAddress));
                reportParameters.Add(new ReportParameter("rpFromDate", fromDate.ToString("dd/MMM/yyyy")));
                reportParameters.Add(new ReportParameter("rpToDate", toDate.ToString("dd/MMM/yyyy")));
                reportParameters.Add(new ReportParameter("rpRetainOpening", strRetainOpening));
                reportParameters.Add(new ReportParameter("rpRetainPeriod", strRetainPeriod));
                lr.SetParameters(reportParameters);
            }

            lr.DataSources.Clear();
            ReportDataSource rd1 = new ReportDataSource("DataSet1", merge_ledger_list);
            lr.DataSources.Add(rd1);
            

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
    }
}
