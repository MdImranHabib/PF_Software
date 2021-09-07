using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Microsoft.Reporting.WebForms;
using DLL.ViewModel;
using System.Globalization;
using DLL.Repository;
using DLL.Utility;

namespace PFMVC.Areas.Accounting.Controllers
{
    public class BalanceSheetController : BaseController
    {

        int _pageId = 6;
        MvcApplication _mvcApplication;
        UnitOfWork  unitOfWork = new UnitOfWork();

        [Authorize]
        public ActionResult BalanceSheetIndex()
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
            int oCode = ((int?)Session["OCode"]) ?? 0;
            //sp_GetGroupSummary
            var v = unitOfWork.AccountingRepository.sp_GetGroupSummaryByNatureID(fromDate, toDate.AddDays(1).AddSeconds(-1), 1, oCode); // here 1 for Asset
            return PartialView("_AssetSummary", v);
        }

        public ActionResult GetLiabilities(DateTime fromDate, DateTime toDate)
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            var v = unitOfWork.AccountingRepository.sp_GetGroupSummaryByNatureID(fromDate, toDate.AddDays(1).AddSeconds(-1), 2, oCode); // here 2 for Liabilities
            return PartialView("_LiabilitiesSummary", v);
        }

        public decimal GetRetainEarningOpening(DateTime fromDateAsToDate)
        {
            //Edited by Avishek Date: Oct-4-2015
            //Start
            int oCode = ((int?)Session["OCode"]) ?? 0;
            _mvcApplication = new MvcApplication();
            var retainEarningOpening = _mvcApplication.GetNumber(unitOfWork.AccountingRepository.GetRetainEarningOpening(fromDateAsToDate, oCode));
            // Modified By Kamrul 2019-02-27 For showing Retained Earning from Ledger Initial Balance
            var retainEarningOpeningLedger = _mvcApplication.GetNumber(unitOfWork.AccountingRepository.GetRetainEarningOpeningLedger( oCode));

            
            if (retainEarningOpening != 0)
            {
                return retainEarningOpening;
            }
            else {
                return retainEarningOpening;
            }
            //End Kamrul
        }

        public decimal GetRetainEarningPeriod(DateTime fromDate, DateTime toDate)
        {
            //Edited by Avishek Date: Oct-4-2015
            //Start
            int oCode = ((int?)Session["OCode"]) ?? 0;
            _mvcApplication = new MvcApplication();
            var retainEarningPeriod = _mvcApplication.GetNumber(unitOfWork.AccountingRepository.GetRetainEarningPeriod(fromDate, toDate, oCode));
            //End
            return retainEarningPeriod;
        }

        public ActionResult GetGroupDetail(int groupId, DateTime fromDate, DateTime toDate)
        {
            CultureInfo cInfo = new CultureInfo("en-IN");
            var v = unitOfWork.AccountingRepository.sp_GetGroupDetail(fromDate, toDate.AddDays(1).AddSeconds(-1), groupId);
            return PartialView("_GroupDetail", v);
        }

        public ActionResult GetBalanceSheet(DateTime fromDate, DateTime toDate)
        {
            //Added by Avishek Date Sep-28-2015
            CultureInfo cInfo = new CultureInfo("en-IN");
            //Added By Avishek Date:Jan-19-2016
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End
            _mvcApplication = new MvcApplication();
            //End
            if (oCode < 1)
            {
                return Json(new { Success = false, ErrorMessage = "User must associate with a company! Current user doestn't associate with any company!" }, JsonRequestBehavior.AllowGet);
            }
            decimal assSumDebit = 0;
            decimal assSumCredit = 0;
            decimal liabSumDebit = 0;
            decimal liabSumCredit = 0;

            #region let's check RetainOpening and Retain Period
            decimal retainOpening = GetRetainEarningOpening(fromDate);
            decimal retainPeriod = GetRetainEarningPeriod(fromDate, toDate);
             
            
            //Retain earning credit type nature
            if (retainOpening < 0)
            {
                ViewBag.RetainEarningOpening = (retainOpening * (-1)).ToString("N",cInfo) + " Dr.";

            }
            else if (retainOpening > 0)
            {
                ViewBag.RetainEarningOpening = retainOpening.ToString("N", cInfo) + " Cr.";
            }
            if (retainPeriod < 0)
            {
                ViewBag.RetainEarningPeriod = (retainPeriod * (-1)).ToString("N",cInfo) + " Dr.";
            }
            else if (retainPeriod > 0)
            {
                ViewBag.RetainEarningPeriod = retainPeriod.ToString("N",cInfo) + " Cr.";
            }

            #endregion

            //let's prepare group summary 

            //get debit type. 1 for asset and 3 for expense
            //logic change again: fromDate eliminating! 
            
           var debitLedgerList = unitOfWork.AccountingRepository.sp_GetLedgerSummary(Convert.ToDateTime("01/01/1800"), toDate.AddDays(1).AddSeconds(-1), 1, oCode);

             foreach (var item in debitLedgerList)
             {
                 if (item.BalanceType == 1) // 1 for credit but process is for debit type
                 {
                     item.Balance = -(_mvcApplication.GetNumber(item.InitialBalance ?? 0)) - (_mvcApplication.GetNumber(item.Credit) - _mvcApplication.GetNumber(item.Debit));
                 }
                 else if (item.BalanceType == 2) // 2 for debit and process is for debit type
                 {
                     item.Balance = _mvcApplication.GetNumber(item.InitialBalance ?? 0) - (_mvcApplication.GetNumber(item.Credit) - _mvcApplication.GetNumber(item.Debit));
                 }
                 else
                 {
                     item.Balance = _mvcApplication.GetNumber(item.Debit) - _mvcApplication.GetNumber(item.Credit);
                 }
                 if (item.Balance < 0)
                 {
                     item.strCredit = _mvcApplication.GetNumber(item.Balance * (-1)) + "";// Cr.
                     item.strDebit = "";
                     //i am doing this for easy calculatin group balance
                     item.Credit = _mvcApplication.GetNumber(item.Balance * (-1));
                     item.Debit = 0;
                     assSumCredit += _mvcApplication.GetNumber(item.Credit);
                     item.strBalance = _mvcApplication.GetNumber(item.Balance * (-1)).ToString("N", cInfo) + " Cr.";
                 }
                 else if (item.Balance > 0)
                 {
                     item.strDebit = _mvcApplication.GetNumber(item.Balance) + "";//Dr.
                     item.strCredit = "";
                     item.Debit = _mvcApplication.GetNumber(item.Balance);
                     item.Credit = 0;
                     assSumDebit += _mvcApplication.GetNumber(item.Debit);
                     item.strBalance = _mvcApplication.GetNumber(item.Balance).ToString("N", cInfo) + " Dr.";
                 }
             }
                                

            var debitGroupList = (from a in debitLedgerList
                                    group a by new
                                    {
                                        a.GroupID,
                                        a.GroupName,
                                        a.EditDate
                                    } into grp
                                    select new VM_acc_ledger
                                    {
                                        GroupID = grp.Key.GroupID,
                                        GroupName = grp.Key.GroupName,
                                        strBalance = _mvcApplication.GetNumber(grp.Sum(s => s.Balance)) < 0 ? _mvcApplication.GetNumber(grp.Sum(s => s.Balance) * (-1)).ToString("N", cInfo) + " Cr." : _mvcApplication.GetNumber(grp.Sum(s => s.Balance)).ToString("N", cInfo) + " Dr.",
                                        EditDate = grp.Key.EditDate
                                    }).OrderBy(x=>x.EditDate).ToList();

            decimal assetBalnace = assSumDebit - assSumCredit;
            ViewBag.AssetBalance = _mvcApplication.GetNumber(assetBalnace) < 0 ? _mvcApplication.GetNumber(assetBalnace * (-1)).ToString("N", cInfo) + " Cr." : _mvcApplication.GetNumber(assetBalnace).ToString("N", cInfo) + " Dr.";

            //get credit type. 2 for liab and 4 for income
            //fromDate eliminating.

             var creditLedgerList = unitOfWork.AccountingRepository.sp_GetLedgerSummary(Convert.ToDateTime("01/01/1800"), toDate.AddDays(1).AddSeconds(-1), 2, oCode);

             foreach (var item in creditLedgerList)
             {
                 if (item.BalanceType == 1) // 1 for credit
                 {
                     item.Balance = _mvcApplication.GetNumber(item.InitialBalance ?? 0) + _mvcApplication.GetNumber(item.Credit) - _mvcApplication.GetNumber(item.Debit);
                 }
                 else if (item.BalanceType == 2) // 2 for debit
                 {
                     item.Balance = -_mvcApplication.GetNumber(item.InitialBalance ?? 0) + _mvcApplication.GetNumber(item.Credit) - _mvcApplication.GetNumber(item.Debit);
                 }
                 else
                 {
                     item.Balance = _mvcApplication.GetNumber(item.Credit) - _mvcApplication.GetNumber(item.Debit);
                 }
                 
                 if (item.Balance < 0)
                 {
                     item.strDebit = _mvcApplication.GetNumber(item.Balance * (-1)) + "";//Dr.
                     item.strCredit = "";
                     //i am doing this for easy calculatin group balance
                     item.Debit = _mvcApplication.GetNumber(item.Balance * (-1));
                     item.Credit = 0;
                     //sum result
                     liabSumDebit += _mvcApplication.GetNumber(item.Debit);
                     item.strBalance = _mvcApplication.GetNumber(item.Balance * (-1)).ToString("N", cInfo) + " Dr.";
                 }
                 else
                 {
                     item.strCredit = _mvcApplication.GetNumber(item.Balance) + "";// Cr.
                     item.strDebit = "";
                     item.Credit = _mvcApplication.GetNumber(item.Balance);
                     item.Debit = 0;
                     liabSumCredit += _mvcApplication.GetNumber(item.Credit);
                     item.strBalance = _mvcApplication.GetNumber(item.Balance).ToString("N", cInfo) + " Cr.";
                 }
             }
             
            //new : have to change those ledger which has term retain: need to analyse more later
            var retainLedgerList = creditLedgerList.Where(f => f.LedgerName.ToLower().Contains("retain")).ToList();
            creditLedgerList.RemoveAll(f => f.LedgerName.ToLower().Contains("retain"));

            var creditGroupList = (from a in creditLedgerList
                                     group a by new
                                     {
                                         a.GroupID,
                                         a.GroupName,
                                         a.EditDate
                                     } into grp
                                     select new VM_acc_ledger
                                     {
                                         GroupID = grp.Key.GroupID,
                                         GroupName = grp.Key.GroupName,
                                         Debit = _mvcApplication.GetNumber(grp.Sum(s => s.Debit)),
                                         EditDate = grp.Key.EditDate,
                                         strBalance = grp.Sum(s => s.Balance) < 0 ? (grp.Sum(s => s.Balance) * (-1)).ToString("N",cInfo) + " Dr." : (grp.Sum(s => s.Balance)).ToString("N",cInfo) + " Cr."
                                     }).OrderBy(x=>x.EditDate).ToList();

            decimal liabBalance = _mvcApplication.GetNumber(liabSumCredit) - _mvcApplication.GetNumber(liabSumDebit) + _mvcApplication.GetNumber(retainOpening + retainPeriod);
            ViewBag.LiabBalance = _mvcApplication.GetNumber(liabBalance) < 0 ? _mvcApplication.GetNumber(liabBalance * (-1)).ToString("N", cInfo) + " Dr." : _mvcApplication.GetNumber(liabBalance).ToString("N", cInfo) + " Cr.";

            if (creditGroupList.Count > 0)
            {
                TempData["CreditGroupList"] = creditGroupList.OrderBy(x=>x.EditDate).ToList();
                TempData["CreditLedgerList"] = creditLedgerList.OrderBy(x=>x.EditDate).ToList();
            }
            if (debitGroupList.Count > 0)
            {
                TempData["DebitGroupList"] = debitGroupList.OrderBy(x => x.EditDate).ToList();
                TempData["DebitLedgerList"] = debitLedgerList.OrderBy(x=>x.EditDate).ToList();
                TempData["RetainLedgerList"] = retainLedgerList.OrderBy(x=>x.EditDate).ToList();
            }

            return PartialView("_BalanceSheet");
        }

        public ActionResult BalanceSheetBriefReport(DateTime fromDate, DateTime toDate)
        {
            //Added by Avishek
            //Added By Avishek Date:Jan-19-2016
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End
            CultureInfo cInfo = new CultureInfo("en-IN");
            _mvcApplication = new MvcApplication();
            //toDate = toDate.AddHours(23).AddMinutes(59).AddSeconds(59);
            //End
            LocalReport lr = new LocalReport();
           
            string path = Path.Combine(Server.MapPath("~/Areas/Accounting/AccountingReport"), "BriefBalanceSheet.rdlc");

            
            if (System.IO.File.Exists(path))
            {
                lr.ReportPath = path;
            }
            else
            {
                return View("Report/ReportPF/Index");
            }
            //process
            decimal assSumDebit = 0;
            decimal assSumCredit = 0;
            decimal liabSumDebit = 0;
            decimal liabSumCredit = 0;

            //Prepare retain ledger
            VM_acc_ledger vmAccLedgerRetainOpening = new VM_acc_ledger();
            VM_acc_ledger vmAccLedgerRetainPeriod = new VM_acc_ledger();
            vmAccLedgerRetainOpening.LedgerName = "Retain earning opening";
            vmAccLedgerRetainPeriod.LedgerName = "During period";
            vmAccLedgerRetainOpening.GroupName = "Retain Earnings";
            vmAccLedgerRetainPeriod.GroupName = "Retain Earnings";

            #region let's check RetainOpening and Retain Period
            decimal retainOpening = GetRetainEarningOpening(fromDate);

            decimal retainPeriod = GetRetainEarningPeriod(fromDate, toDate);

            //Retain earning credit type nature
            if (retainOpening < 0)
            {
                ViewBag.RetainEarningOpening = _mvcApplication.GetNumber(retainOpening * (-1)).ToString("N", cInfo) + " Dr.";
                vmAccLedgerRetainOpening.Balance = _mvcApplication.GetNumber(retainOpening * (-1));
                vmAccLedgerRetainOpening.strDebit = _mvcApplication.GetNumber(retainOpening * (-1)).ToString("N", cInfo) + " Dr.";
            }
            else if (retainOpening > 0)
            {
                ViewBag.RetainEarningOpening = _mvcApplication.GetNumber(retainOpening).ToString("N", cInfo) + " Cr.";
                vmAccLedgerRetainOpening.Balance = _mvcApplication.GetNumber(retainOpening);
                vmAccLedgerRetainOpening.strCredit = _mvcApplication.GetNumber(retainOpening).ToString("N", cInfo) + " Cr.";
            }
            if (retainPeriod < 0)
            {
                ViewBag.RetainEarningPeriod = _mvcApplication.GetNumber(retainPeriod * (-1)).ToString("N", cInfo) + " Dr.";
                vmAccLedgerRetainPeriod.Balance = _mvcApplication.GetNumber(retainPeriod * (-1));
                vmAccLedgerRetainPeriod.strDebit = _mvcApplication.GetNumber(retainPeriod * (-1)).ToString("N", cInfo) + " Dr.";
            }
            else if (retainPeriod > 0)
            {
                ViewBag.RetainEarningPeriod = _mvcApplication.GetNumber(retainPeriod).ToString("N", cInfo) + " Cr.";
                vmAccLedgerRetainPeriod.Balance = _mvcApplication.GetNumber(retainPeriod);
                vmAccLedgerRetainPeriod.strCredit = _mvcApplication.GetNumber(retainPeriod).ToString("N", cInfo) + " Cr.";
            }
            #endregion

            var debitLedgerListReport = unitOfWork.AccountingRepository.sp_GetLedgerSummary(Convert.ToDateTime("01/01/1800"), toDate.AddDays(1).AddSeconds(-1), 1, oCode);


            foreach (var item in debitLedgerListReport)
            {
                if (item.BalanceType == 1) // 1 for credit but process is for debit type
                {
                    item.Balance = _mvcApplication.GetNumber(-(item.InitialBalance ?? 0)) - (_mvcApplication.GetNumber(item.Credit) - _mvcApplication.GetNumber(item.Debit));
                }
                else if (item.BalanceType == 2) // 2 for debit and process is for debit type
                {
                    item.Balance = _mvcApplication.GetNumber(item.InitialBalance ?? 0) - (_mvcApplication.GetNumber(item.Credit) - _mvcApplication.GetNumber(item.Debit));
                }
                else
                {
                    item.Balance = _mvcApplication.GetNumber(item.Debit) - _mvcApplication.GetNumber(item.Credit);
                }
                if (item.Balance < 0)
                {
                    item.strCredit = _mvcApplication.GetNumber(item.Balance * (-1)).ToString("N", cInfo) + "";// Cr.
                    item.strDebit = "";
                    //i am doing this for easy calculatin group balance
                    item.Credit = _mvcApplication.GetNumber(item.Balance * (-1));
                    item.Debit = 0;
                    assSumCredit += _mvcApplication.GetNumber(item.Credit);
                    item.strBalance = _mvcApplication.GetNumber(item.Balance * (-1)).ToString("N", cInfo) + " Cr.";
                }
                else if (item.Balance > 0)
                {
                    item.strDebit = _mvcApplication.GetNumber(item.Balance).ToString("N", cInfo) + "";//Dr.
                    item.strCredit = "";
                    item.Debit = _mvcApplication.GetNumber(item.Balance);
                    item.Credit = 0;
                    assSumDebit += _mvcApplication.GetNumber(item.Debit);
                    item.strBalance = _mvcApplication.GetNumber(item.Balance).ToString("N", cInfo) + " Dr.";
                }
            }

            decimal assetBalnace = assSumDebit - assSumCredit;
            ViewBag.AssetBalance = _mvcApplication.GetNumber(assetBalnace) < 0 ? _mvcApplication.GetNumber(assetBalnace * (-1)).ToString("N", cInfo) + " Cr." : _mvcApplication.GetNumber(assetBalnace).ToString("N", cInfo) + " Dr.";

            var creditLedgerListReport = unitOfWork.AccountingRepository.sp_GetLedgerSummary(Convert.ToDateTime("01/01/1800"), toDate.AddDays(1).AddSeconds(-1), 2, oCode);

            foreach (var item in creditLedgerListReport)
            {
                if (item.BalanceType == 1) // 1 for credit
                {
                    item.Balance = _mvcApplication.GetNumber(item.InitialBalance ?? 0) + _mvcApplication.GetNumber(item.Credit) - _mvcApplication.GetNumber(item.Debit);
                }
                else if (item.BalanceType == 2) // 2 for debit
                {
                    item.Balance = _mvcApplication.GetNumber(-(item.InitialBalance ?? 0)) + _mvcApplication.GetNumber(item.Credit) - _mvcApplication.GetNumber(item.Debit);
                }
                else
                {
                    item.Balance = _mvcApplication.GetNumber(item.Credit) - _mvcApplication.GetNumber(item.Debit);
                }

                if (item.Balance < 0)
                {
                    item.strDebit = _mvcApplication.GetNumber(item.Balance * (-1)).ToString("N", cInfo) + "";//Dr.
                    item.strCredit = "";
                    item.Debit = _mvcApplication.GetNumber(item.Balance * (-1));
                    item.Credit = 0;
                    liabSumDebit += _mvcApplication.GetNumber(item.Debit);
                    item.strBalance = _mvcApplication.GetNumber(item.Balance * (-1)).ToString("N", cInfo) + " Dr.";
                }
                else
                {
                    item.strCredit = _mvcApplication.GetNumber(item.Balance).ToString("N", cInfo) + "";// Cr.
                    item.strDebit = "";
                    item.Credit = _mvcApplication.GetNumber(item.Balance);
                    item.Debit = 0;
                    liabSumCredit += _mvcApplication.GetNumber(item.Credit);
                    item.strBalance = _mvcApplication.GetNumber(item.Balance).ToString("N", cInfo) + " Cr.";
                }
            }


            creditLedgerListReport.Add(vmAccLedgerRetainOpening);
            creditLedgerListReport.Add(vmAccLedgerRetainPeriod);

            decimal liabBalance = liabSumCredit - liabSumDebit + (_mvcApplication.GetNumber(retainOpening) + _mvcApplication.GetNumber(retainPeriod));
            ViewBag.LiabBalance = liabBalance < 0 ? _mvcApplication.GetNumber(liabBalance * (-1)).ToString("N", cInfo) + " Dr." : _mvcApplication.GetNumber(liabBalance).ToString("N", cInfo) + " Cr.";
            //end process
            CultureInfo ci = new CultureInfo("en-US");
            var companyInformation = unitOfWork.CompanyInformationRepository.Get().Single(o => o.CompanyID == oCode);
            if (companyInformation != null)
            {
                ReportParameterCollection reportParameters = new ReportParameterCollection();
                reportParameters.Add(new ReportParameter("rpCompanyName", companyInformation.CompanyName));
                reportParameters.Add(new ReportParameter("rpCompanyAddress", companyInformation.CompanyAddress));
                reportParameters.Add(new ReportParameter("rpFromDate", fromDate.ToString("dd/MMM/yyyy", ci)));
                reportParameters.Add(new ReportParameter("rpToDate", toDate.ToString("dd/MMM/yyyy", ci)));

                lr.SetParameters(reportParameters);
            }

            lr.DataSources.Clear();
            ReportDataSource rd1 = new ReportDataSource("DS_Asset", debitLedgerListReport);
            ReportDataSource rd2 = new ReportDataSource("DS_Liabilities", creditLedgerListReport);
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

        public ActionResult BalanceSheetReport(DateTime fromDate, DateTime toDate)
        {
            //Added by Avishek
            //Added By Avishek Date:Jan-19-2016
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End
            CultureInfo cInfo = new CultureInfo("en-IN");
            _mvcApplication = new MvcApplication();
            //toDate = toDate.AddHours(23).AddMinutes(59).AddSeconds(59);
            //End
            LocalReport lr = new LocalReport();

            string path = Path.Combine(Server.MapPath("~/Areas/Accounting/AccountingReport"), "BalanceSheet.rdlc");
            if (System.IO.File.Exists(path))
            {
                lr.ReportPath = path;
            }
            else
            {
                return View("Report/ReportPF/Index");
            }
            //process
            decimal assSumDebit = 0;
            decimal assSumCredit = 0;
            decimal liabSumDebit = 0;
            decimal liabSumCredit = 0;

            //Prepare retain ledger
            VM_acc_ledger vmAccLedgerRetainOpening = new VM_acc_ledger();
            VM_acc_ledger vmAccLedgerRetainPeriod = new VM_acc_ledger();
            vmAccLedgerRetainOpening.LedgerName = "Retain earning opening";
            vmAccLedgerRetainPeriod.LedgerName = "During period";
            vmAccLedgerRetainOpening.GroupName = "Retain Earnings";
            vmAccLedgerRetainPeriod.GroupName = "Retain Earnings";

            #region let's check RetainOpening and Retain Period
            decimal retainOpening = GetRetainEarningOpening(fromDate);
            decimal retainPeriod = GetRetainEarningPeriod(fromDate, toDate);

            //Retain earning credit type nature
            if (retainOpening < 0)
            {
                ViewBag.RetainEarningOpening = _mvcApplication.GetNumber(retainOpening * (-1)).ToString("N", cInfo) + " Dr.";
                vmAccLedgerRetainOpening.Balance = _mvcApplication.GetNumber(retainOpening * (-1));
                vmAccLedgerRetainOpening.strDebit = _mvcApplication.GetNumber(retainOpening * (-1)).ToString("N", cInfo) + " Dr.";
            }
            else if (retainOpening > 0)
            {
                ViewBag.RetainEarningOpening = _mvcApplication.GetNumber(retainOpening).ToString("N", cInfo) + " Cr.";
                vmAccLedgerRetainOpening.Balance = _mvcApplication.GetNumber(retainOpening);
                vmAccLedgerRetainOpening.strCredit = _mvcApplication.GetNumber(retainOpening).ToString("N", cInfo) + " Cr.";
            }
            if (retainPeriod < 0)
            {
                ViewBag.RetainEarningPeriod = _mvcApplication.GetNumber(retainPeriod * (-1)).ToString("N", cInfo) + " Dr.";
                vmAccLedgerRetainPeriod.Balance = _mvcApplication.GetNumber(retainPeriod * (-1));
                vmAccLedgerRetainPeriod.strDebit = _mvcApplication.GetNumber(retainPeriod * (-1)).ToString("N", cInfo) + " Dr.";
            }
            else if (retainPeriod > 0)
            {
                ViewBag.RetainEarningPeriod = _mvcApplication.GetNumber(retainPeriod).ToString("N", cInfo) + " Cr.";
                vmAccLedgerRetainPeriod.Balance = _mvcApplication.GetNumber(retainPeriod);
                vmAccLedgerRetainPeriod.strCredit = _mvcApplication.GetNumber(retainPeriod).ToString("N", cInfo) + " Cr.";
            }
            #endregion

            var debitLedgerListReport = unitOfWork.AccountingRepository.sp_GetLedgerSummary(Convert.ToDateTime("01/01/1800"), toDate.AddDays(1).AddSeconds(-1), 1, oCode);


            foreach (var item in debitLedgerListReport)
            {
                if (item.BalanceType == 1) // 1 for credit but process is for debit type
                {
                    item.Balance = _mvcApplication.GetNumber(-(item.InitialBalance ?? 0)) - (_mvcApplication.GetNumber(item.Credit) - _mvcApplication.GetNumber(item.Debit));
                }
                else if (item.BalanceType == 2) // 2 for debit and process is for debit type
                {
                    item.Balance = _mvcApplication.GetNumber(item.InitialBalance ?? 0) - (_mvcApplication.GetNumber(item.Credit) - _mvcApplication.GetNumber(item.Debit));
                }
                else
                {
                    item.Balance = _mvcApplication.GetNumber(item.Debit) - _mvcApplication.GetNumber(item.Credit);
                }
                if (item.Balance < 0)
                {
                    item.strCredit = _mvcApplication.GetNumber(item.Balance * (-1)).ToString("N", cInfo) + "";// Cr.
                    item.strDebit = "";
                    //i am doing this for easy calculatin group balance
                    item.Credit = _mvcApplication.GetNumber(item.Balance * (-1));
                    item.Debit = 0;
                    assSumCredit += _mvcApplication.GetNumber(item.Credit);
                    item.strBalance = _mvcApplication.GetNumber(item.Balance * (-1)).ToString("N", cInfo) + " Cr.";
                }
                else if (item.Balance > 0)
                {
                    item.strDebit = _mvcApplication.GetNumber(item.Balance).ToString("N", cInfo) + "";//Dr.
                    item.strCredit = "";
                    item.Debit = _mvcApplication.GetNumber(item.Balance);
                    item.Credit = 0;
                    assSumDebit += _mvcApplication.GetNumber(item.Debit);
                    item.strBalance = _mvcApplication.GetNumber(item.Balance).ToString("N", cInfo) + " Dr.";
                }
            }

            decimal assetBalnace = assSumDebit - assSumCredit;
            ViewBag.AssetBalance = _mvcApplication.GetNumber(assetBalnace) < 0 ? _mvcApplication.GetNumber(assetBalnace * (-1)).ToString("N", cInfo) + " Cr." :_mvcApplication.GetNumber(assetBalnace).ToString("N", cInfo) + " Dr.";
            
            var creditLedgerListReport = unitOfWork.AccountingRepository.sp_GetLedgerSummary(Convert.ToDateTime("01/01/1800"), toDate.AddDays(1).AddSeconds(-1), 2, oCode);

            foreach (var item in creditLedgerListReport)
            {
                if (item.BalanceType == 1) // 1 for credit
                {
                    item.Balance = _mvcApplication.GetNumber(item.InitialBalance ?? 0) + _mvcApplication.GetNumber(item.Credit) - _mvcApplication.GetNumber(item.Debit);
                }
                else if (item.BalanceType == 2) // 2 for debit
                {
                    item.Balance = _mvcApplication.GetNumber(-(item.InitialBalance ?? 0)) + _mvcApplication.GetNumber(item.Credit) - _mvcApplication.GetNumber(item.Debit);
                }
                else
                {
                    item.Balance = _mvcApplication.GetNumber(item.Credit) - _mvcApplication.GetNumber(item.Debit);
                }

                if (item.Balance < 0)
                {
                    item.strDebit = _mvcApplication.GetNumber(item.Balance * (-1)).ToString("N", cInfo) + "";//Dr.
                    item.strCredit = "";
                    item.Debit = _mvcApplication.GetNumber(item.Balance * (-1));
                    item.Credit = 0;
                    liabSumDebit += _mvcApplication.GetNumber(item.Debit);
                    item.strBalance = _mvcApplication.GetNumber(item.Balance * (-1)).ToString("N", cInfo) + " Dr.";
                }
                else
                {
                    item.strCredit =_mvcApplication.GetNumber( item.Balance).ToString("N",cInfo) + "";// Cr.
                    item.strDebit = "";
                    item.Credit = _mvcApplication.GetNumber(item.Balance);
                    item.Debit = 0;
                    liabSumCredit += _mvcApplication.GetNumber(item.Credit);
                    item.strBalance = _mvcApplication.GetNumber(item.Balance).ToString("N", cInfo) + " Cr.";
                }
            }
             

            creditLedgerListReport.Add(vmAccLedgerRetainOpening);
            creditLedgerListReport.Add(vmAccLedgerRetainPeriod);

            decimal liabBalance = liabSumCredit - liabSumDebit + (_mvcApplication.GetNumber(retainOpening) + _mvcApplication.GetNumber(retainPeriod));
            ViewBag.LiabBalance = liabBalance < 0 ? _mvcApplication.GetNumber(liabBalance * (-1)).ToString("N", cInfo) + " Dr." : _mvcApplication.GetNumber(liabBalance).ToString("N", cInfo) + " Cr.";
            //end process
            CultureInfo ci = new CultureInfo("en-US");
            var companyInformation = unitOfWork.CompanyInformationRepository.Get().Single(o => o.CompanyID == oCode);
            if (companyInformation != null)
            {
                ReportParameterCollection reportParameters = new ReportParameterCollection();
                reportParameters.Add(new ReportParameter("rpCompanyName", companyInformation.CompanyName));
                reportParameters.Add(new ReportParameter("rpCompanyAddress", companyInformation.CompanyAddress));
                reportParameters.Add(new ReportParameter("rpFromDate", fromDate.ToString("dd/MMM/yyyy", ci)));
                reportParameters.Add(new ReportParameter("rpToDate", toDate.ToString("dd/MMM/yyyy", ci)));

                lr.SetParameters(reportParameters);
            }

            lr.DataSources.Clear();
            ReportDataSource rd1 = new ReportDataSource("DS_Asset", debitLedgerListReport);
            ReportDataSource rd2 = new ReportDataSource("DS_Liabilities", creditLedgerListReport);
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

    }
}
