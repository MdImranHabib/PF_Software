using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Microsoft.Reporting.WebForms;
using System.IO;
using DLL.ViewModel;
using System.Globalization;

namespace PFMVC.Areas.Accounting.Controllers
{
    public class LedgerBookController : BaseController
    {
        MvcApplication _mvcApplication;
        int _pageId = 6;
        ReportDataSource _rd;
        List<VM_acc_VoucherDetail> _listVmAccVoucherDetail = new List<VM_acc_VoucherDetail>();

        [Authorize]
        public ActionResult LedgerBookIndex()
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

        public ActionResult LedgerBookMain()
        {
            return PartialView("_LedgerBookMain");
        }

        public ActionResult LedgerListFilterTable()
        {   
            ViewBag.LegendName = "Head of Account";
            var v = unitOfWork.ACC_LedgerRepository.Get().ToList();
            return PartialView("_LedgerListFilterTable", v);
        }

        public bool PrepareLedgerBook(Guid ledgerId, DateTime? fromDate, DateTime? toDate, string ledgerName, ref string rMessage, ref string InitialBalance,  ref string BalanceBeforeDate )
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            CultureInfo cInfo = new CultureInfo("en-IN");
            if (ledgerId == Guid.Empty)
            {
                rMessage = "Please select all data correctly...";
                return false;
            }
            if (fromDate == null)
            {
                fromDate = GetSystemImplementationDate(ref rMessage);
                if (!string.IsNullOrEmpty(rMessage))
                {
                    return false;
                }
            }
            var companyInformation = unitOfWork.CompanyInformationRepository.GetByID(oCode);
            var ledgerNatureType = unitOfWork.AccountingRepository.GetLedgerNatureType(ledgerId);

            decimal initialBalance;
            int initialBalanceType;
            decimal creditBalanceBeforeDate;
            decimal debitBalanceBeforeDate;
            string groupName;
            decimal balanceBeforeDate = 0;
            //calculate cumulative balance - get ledger nature type
            decimal temp;


            DateTime f = fromDate ?? DateTime.MinValue;
            DateTime t = toDate ?? DateTime.Now;

            //list_vm_acc_voucherDetail = unitOfWork.AccountingRepository.sp_GetLedgerTransactionDetail(LedgerID, f, t.AddHours(23).AddMinutes(59).AddSeconds(59), OCode).ToList();
            _listVmAccVoucherDetail = unitOfWork.AccountingRepository.sp_GetLedgerTransactionDetail(ledgerId, f, t.AddDays(1).AddSeconds(-1), oCode).OrderBy(x=>x.TransactionDate).ToList();
           // list_vm_acc_voucherDetail = list_vm_acc_voucherDetail.OrderBy(o => o.TransactionDate).ToList();
            //foreach (var item in voucherDetailListforOpeningBalance)
            //{
            //    InitialBalance = InitialBalance - item.Credit + item.Debit; //debit type ledger asset
            //    if (temp >= 0)
            //    {
            //        item.strCBalance = temp + " Dr.";
            //    }
            //    else
            //    {
            //        item.strCBalance = temp * (-1) + " Cr.";
            //    }
            //}
            ViewBag.LedgerName = ledgerName;
            try
            {
                unitOfWork.AccountingRepository.sp_GetTransactionBalanceBeforeDate(ledgerId, fromDate ?? DateTime.MinValue, out initialBalance, out initialBalanceType, out creditBalanceBeforeDate, out debitBalanceBeforeDate, out groupName, oCode);
                if (initialBalanceType == 1)
                {
                    InitialBalance = _mvcApplication.GetNumber(initialBalance).ToString("N", cInfo) + " Cr.";
                }
                else
                {
                    InitialBalance = _mvcApplication.GetNumber(initialBalance).ToString("N", cInfo) + " Dr.";
                }

                #region Liabilities Ledger ( credit type)
                if (ledgerNatureType == "Liabilities")
                {
                    //Opening balance will be count
                    //credit type ledger
                    if (initialBalanceType == 1)
                    {
                        balanceBeforeDate = _mvcApplication.GetNumber(initialBalance) + _mvcApplication.GetNumber(creditBalanceBeforeDate) - _mvcApplication.GetNumber(debitBalanceBeforeDate);
                    }
                    else if (initialBalanceType == 2)
                    {
                        balanceBeforeDate = -(_mvcApplication.GetNumber(initialBalance)) + _mvcApplication.GetNumber(creditBalanceBeforeDate) - _mvcApplication.GetNumber(debitBalanceBeforeDate);
                    }
                    if (balanceBeforeDate >= 0)
                    {
                        BalanceBeforeDate = _mvcApplication.GetNumber(balanceBeforeDate).ToString("N",cInfo) + " Cr.";
                    }
                    else
                    {
                        BalanceBeforeDate = _mvcApplication.GetNumber(balanceBeforeDate * (-1)).ToString("N",cInfo) + " Dr.";
                    }

                    //Process transaction
                    //balance before date will be calculated for  balanse sheet ledger
                    temp = _mvcApplication.GetNumber(balanceBeforeDate);
                    foreach (var item in _listVmAccVoucherDetail)
                    {
                        temp = _mvcApplication.GetNumber(temp) + _mvcApplication.GetNumber(item.Credit) - _mvcApplication.GetNumber(item.Debit);
                        if (temp >= 0)
                        {
                            item.strCBalance = _mvcApplication.GetNumber(temp).ToString("N",cInfo) + " Cr.";
                        }
                        else
                        {
                            item.strCBalance = _mvcApplication.GetNumber((temp * (-1))).ToString("N",cInfo) + " Dr.";
                        }
                    }
                }
                #endregion
                #region Asset Ledger (debit type)
                else if (ledgerNatureType == "Asset")
                {
                    //Opening balance will be count
                    //debit type ledger
                    if (initialBalanceType == 1) //credit but ledger for debit type
                    {
                        balanceBeforeDate = -(_mvcApplication.GetNumber(initialBalance)) - _mvcApplication.GetNumber(creditBalanceBeforeDate) + _mvcApplication.GetNumber(debitBalanceBeforeDate);
                    }
                    else if (initialBalanceType == 2)
                    {
                        balanceBeforeDate = _mvcApplication.GetNumber(initialBalance) - _mvcApplication.GetNumber(creditBalanceBeforeDate) + _mvcApplication.GetNumber(debitBalanceBeforeDate);
                    }
                    if (balanceBeforeDate >= 0)
                    {
                        BalanceBeforeDate = _mvcApplication.GetNumber(balanceBeforeDate).ToString("N", cInfo) + " Dr.";
                    }
                    else
                    {
                        BalanceBeforeDate = _mvcApplication.GetNumber(balanceBeforeDate * (-1)).ToString("N", cInfo) + " Cr.";
                    }
                    //Process transaction
                    //balance before date will be calculated for  balanse sheet ledger
                    temp = _mvcApplication.GetNumber(balanceBeforeDate);
                    foreach (var item in _listVmAccVoucherDetail)
                    {
                        temp = _mvcApplication.GetNumber(temp) - _mvcApplication.GetNumber(item.Credit) + _mvcApplication.GetNumber(item.Debit); //debit type ledger asset
                        if (temp >= 0)
                        {
                            item.strCBalance = _mvcApplication.GetNumber(temp).ToString("N",cInfo) + " Dr.";
                        }
                        else
                        {
                            item.strCBalance = _mvcApplication.GetNumber(temp * (-1)).ToString("N",cInfo) + " Cr.";
                        }
                    }
                }
                #endregion

                //#region Expense Ledger (debit type)
                //else if (ledgerNatureType == "Expenses")
                //{
                //    //Process transaction
                //    //balance before date will not be calculated for income statement ledger
                //    //temp = 0;
                //    //but if fromdate includes company opening date then initial balance will be calculated
                //    //if (companyInformation.SystemImplementationDate >= fromDate)
                //    //{
                //    //    if (initialBalanceType == 1) //credit but ledger for debit type
                //    //    {
                //    //        temp = -(_mvcApplication.GetNumber(initialBalance));
                //    //    }
                //    //    else if (initialBalanceType == 2) //debit and ledger for debit type
                //    //    {
                //    //        temp = _mvcApplication.GetNumber(initialBalance);
                //    //    }
                //    //}

                //    //if (initialBalanceType == 1) //credit but ledger for debit type
                //    //{
                //    //    balanceBeforeDate = -(_mvcApplication.GetNumber(initialBalance)) - _mvcApplication.GetNumber(creditBalanceBeforeDate) + _mvcApplication.GetNumber(debitBalanceBeforeDate);
                //    //}
                //    //else if (initialBalanceType == 2)
                //    //{
                //    //    balanceBeforeDate = _mvcApplication.GetNumber(initialBalance) - _mvcApplication.GetNumber(creditBalanceBeforeDate) + _mvcApplication.GetNumber(debitBalanceBeforeDate);
                //    //}
                //    //if (balanceBeforeDate >= 0)
                //    //{
                //    //    BalanceBeforeDate = _mvcApplication.GetNumber(balanceBeforeDate).ToString("N", cInfo) + " Dr.";
                //    //}
                //    //else
                //    //{
                //    //    BalanceBeforeDate = _mvcApplication.GetNumber(balanceBeforeDate * (-1)).ToString("N", cInfo) + " Cr.";
                //    //}
                //    //Process transaction
                //    //balance before date will be calculated for  balanse sheet ledger
                //    //temp = _mvcApplication.GetNumber(balanceBeforeDate);

                //    //Process transaction
                //    //balance before date will be calculated for  balanse sheet ledger
                //    temp = _mvcApplication.GetNumber(balanceBeforeDate);
                //    foreach (var item in _listVmAccVoucherDetail)
                //    {
                //        temp = _mvcApplication.GetNumber(temp) - _mvcApplication.GetNumber(item.Credit) + _mvcApplication.GetNumber(item.Debit);
                //        if (temp >= 0)
                //        {
                //            item.strCBalance = _mvcApplication.GetNumber(temp).ToString("N",cInfo) + " Dr.";
                //        }
                //        else
                //        {
                //            item.strCBalance = _mvcApplication.GetNumber(temp * (-1)).ToString("N",cInfo) + " Cr.";
                //        }
                //    }
                //}
                //#endregion
                //#region Income Ledger ( credit type)
                //if (ledgerNatureType == "Income")
                //{
                //    //Process transaction
                //    //balance before date will be calculated for  balanse sheet ledger
                //    //temp = 0;
                //    //but if fromdate includes company opening date then initial balance will be calculated
                //    //if (initialBalanceType == 1)
                //    //{
                //    //    balanceBeforeDate = _mvcApplication.GetNumber(initialBalance) + _mvcApplication.GetNumber(creditBalanceBeforeDate) - _mvcApplication.GetNumber(debitBalanceBeforeDate);
                //    //}
                //    //else if (initialBalanceType == 2)
                //    //{
                //    //    balanceBeforeDate = -(_mvcApplication.GetNumber(initialBalance)) + _mvcApplication.GetNumber(creditBalanceBeforeDate) - _mvcApplication.GetNumber(debitBalanceBeforeDate);
                //    //}
                //    //if (balanceBeforeDate >= 0)
                //    //{
                //    //    BalanceBeforeDate = _mvcApplication.GetNumber(balanceBeforeDate).ToString("N", cInfo) + " Cr.";
                //    //}
                //    //else
                //    //{
                //    //    BalanceBeforeDate = _mvcApplication.GetNumber(balanceBeforeDate * (-1)).ToString("N", cInfo) + " Dr.";
                //    //}

                //    //Process transaction
                //    //balance before date will be calculated for  balanse sheet ledger
                //    temp = _mvcApplication.GetNumber(balanceBeforeDate);

                //    foreach (var item in _listVmAccVoucherDetail)
                //    {
                //        temp = _mvcApplication.GetNumber(temp) + _mvcApplication.GetNumber(item.Credit) - _mvcApplication.GetNumber(item.Debit); // credit type
                //        if (temp >= 0)
                //        {
                //            item.strCBalance = _mvcApplication.GetNumber(temp).ToString("N",cInfo) + " Cr.";
                //        }
                //        else
                //        {
                //            item.strCBalance = _mvcApplication.GetNumber(temp * (-1)).ToString("N",cInfo) + " Dr.";
                //        }
                //    }
                //}
                //#endregion

                #region Expense Ledger (debit type)
                else if (ledgerNatureType == "Expenses")
                {
                    //Process transaction
                    //balance before date will not be calculated for income statement ledger
                    temp = 0;
                    //but if fromdate includes company opening date then initial balance will be calculated
                    if (companyInformation.SystemImplementationDate >= fromDate)
                    {
                        if (initialBalanceType == 3) //credit but ledger for debit type
                        {
                            balanceBeforeDate = -(_mvcApplication.GetNumber(initialBalance)) + _mvcApplication.GetNumber(creditBalanceBeforeDate) - _mvcApplication.GetNumber(debitBalanceBeforeDate);
                            //temp = -(_mvcApplication.GetNumber(initialBalance));
                        }
                        else if (initialBalanceType == 4) //debit and ledger for debit type
                        {
                            balanceBeforeDate = _mvcApplication.GetNumber(initialBalance) + _mvcApplication.GetNumber(creditBalanceBeforeDate) - _mvcApplication.GetNumber(debitBalanceBeforeDate);
                            //temp = _mvcApplication.GetNumber(initialBalance);
                        }
                        if (balanceBeforeDate >= 0)
                        {
                            ViewBag.BalanceBeforeDate = _mvcApplication.GetNumber(balanceBeforeDate).ToString("N", cInfo) + " Dr.";
                            BalanceBeforeDate = _mvcApplication.GetNumber(balanceBeforeDate).ToString("N", cInfo) + " Dr.";
                        }
                        else
                        {
                            ViewBag.BalanceBeforeDate = _mvcApplication.GetNumber(balanceBeforeDate * (-1)).ToString("N", cInfo) + " Cr.";
                            BalanceBeforeDate = _mvcApplication.GetNumber(balanceBeforeDate * (-1)).ToString("N", cInfo) + " Cr.";
                        }
                    }
                    foreach (var item in _listVmAccVoucherDetail)
                    {
                        temp = _mvcApplication.GetNumber(temp) - _mvcApplication.GetNumber(item.Credit) + _mvcApplication.GetNumber(item.Debit);
                        if (temp >= 0)
                        {
                            item.strCBalance = _mvcApplication.GetNumber(temp).ToString("N", cInfo) + " Dr.";
                        }
                        else
                        {
                            item.strCBalance = _mvcApplication.GetNumber(temp * (-1)).ToString("N", cInfo) + " Cr.";
                        }
                    }
                }
                #endregion
                #region Income Ledger ( credit type)
                if (ledgerNatureType == "Income")
                {
                    //Process transaction
                    //balance before date will be calculated for  balanse sheet ledger
                    temp = 0;
                    //but if fromdate includes company opening date then initial balance will be calculated
                    if (companyInformation.SystemImplementationDate >= fromDate)
                    {
                        if (initialBalanceType == 3) //credit and ledger for credit type
                        {
                            balanceBeforeDate = _mvcApplication.GetNumber(initialBalance) - _mvcApplication.GetNumber(creditBalanceBeforeDate) + _mvcApplication.GetNumber(debitBalanceBeforeDate);
                            //temp = _mvcApplication.GetNumber(initialBalance);
                        }
                        else if (initialBalanceType == 4) //debit but ledger for credit type
                        {
                            balanceBeforeDate = -(_mvcApplication.GetNumber(initialBalance)) - _mvcApplication.GetNumber(creditBalanceBeforeDate) + _mvcApplication.GetNumber(debitBalanceBeforeDate);
                            //temp = _mvcApplication.GetNumber(initialBalance);
                        }
                        if (balanceBeforeDate >= 0)
                        {
                            ViewBag.BalanceBeforeDate = _mvcApplication.GetNumber(balanceBeforeDate).ToString("N", cInfo) + " Dr.";
                            BalanceBeforeDate = _mvcApplication.GetNumber(balanceBeforeDate).ToString("N", cInfo) + " Dr.";
                        }
                        else
                        {
                            ViewBag.BalanceBeforeDate = _mvcApplication.GetNumber(balanceBeforeDate * (-1)).ToString("N", cInfo) + " Cr.";
                            BalanceBeforeDate = _mvcApplication.GetNumber(balanceBeforeDate * (-1)).ToString("N", cInfo) + " Cr.";
                        }
                    }
                    foreach (var item in _listVmAccVoucherDetail)
                    {
                        temp = _mvcApplication.GetNumber(temp) + _mvcApplication.GetNumber(item.Credit) - _mvcApplication.GetNumber(item.Debit); // credit type
                        if (temp >= 0)
                        {
                            item.strCBalance = _mvcApplication.GetNumber(temp).ToString("N", cInfo) + " Cr.";
                        }
                        else
                        {
                            item.strCBalance = _mvcApplication.GetNumber(temp * (-1)).ToString("N", cInfo) + " Dr.";
                        }
                    }
                }
                #endregion
                ViewBag.GroupName = groupName + "";
            }
            catch (Exception x)
            {
                rMessage = x.Message;
                return false;
            }
            return true;
        }

        [HttpPost]
        public ActionResult GetData(Guid ledgerId, DateTime? fromDate, DateTime? toDate, string ledgerName)
        {
            //Added By Avishek Date:Jan-19-2016
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End
            _mvcApplication = new MvcApplication();
            string rMessage = "";
            string rInitialBalance = "";
            string rBalanceBeforeDate = "";
            bool b = PrepareLedgerBook(ledgerId, fromDate, toDate, ledgerName, ref rMessage, ref rInitialBalance, ref rBalanceBeforeDate );
            CultureInfo cInfo = new CultureInfo("en-IN");
            ViewBag.LedgerID = ledgerId;
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate ?? DateTime.Now;
            foreach(var item in _listVmAccVoucherDetail) {
                item.aDebit = _mvcApplication.GetNumber(item.Debit).ToString("N", cInfo);
                item.aCredit = _mvcApplication.GetNumber(item.Credit).ToString("N", cInfo);
            }
            if (b)
            {
                ViewBag.InitialBalance = rInitialBalance;
                ViewBag.BalanceBeforeDate = rBalanceBeforeDate;
                return PartialView("_LedgerBookMain", _listVmAccVoucherDetail);
            }
            return Json(new { Success = false, ErrorMessage = rMessage }, JsonRequestBehavior.DenyGet);
        }

        public ActionResult Report(Guid ledgerId, DateTime? fromDate, DateTime? toDate, string ledgerName)
        {
            _mvcApplication = new MvcApplication();
            //Added By Avishek Date:Jan-19-2016
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End
            LocalReport lr = new LocalReport();
            string path = Path.Combine(Server.MapPath("~/Areas/Accounting/AccountingReport"), "LedgerBook.rdlc");
            if (System.IO.File.Exists(path))
            {
                lr.ReportPath = path;
            }
            else
            {
                return View("Report/ReportPF/Index");
            }
            // process start
            #region Process Start
            string rMessage = "";
            string rInitialBalance = "";
            string rBalanceBeforeDate = "";
            bool b = PrepareLedgerBook(ledgerId, fromDate, toDate, ledgerName, ref rMessage, ref rInitialBalance, ref rBalanceBeforeDate);

            ViewBag.LedgerID = ledgerId;
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate ?? DateTime.Now;

            if (!b)// if false return from here...
            {
                return Json(new { Success = false, ErrorMessage = rMessage }, JsonRequestBehavior.DenyGet);
            }
            #endregion
            // end process


            ReportParameterCollection reportParameters = new ReportParameterCollection();
            reportParameters.Add(new ReportParameter("rpFromDate", fromDate.Value.ToString("dd/MMM/yyyy")));
            reportParameters.Add(new ReportParameter("rpToDate", toDate.Value.ToString("dd/MMM/yyyy")));
            reportParameters.Add(new ReportParameter("rpLedgerName", ledgerName));
            reportParameters.Add(new ReportParameter("rpInitialBalance", rInitialBalance + ""));
            reportParameters.Add(new ReportParameter("rpOpeningBalance", rBalanceBeforeDate + ""));
            //get the company information
            var companyInformation = unitOfWork.CompanyInformationRepository.Get(o => o.CompanyID == oCode).Single();
            if (companyInformation != null)
            {
                reportParameters.Add(new ReportParameter("rpCompanyName", companyInformation.CompanyName));
                reportParameters.Add(new ReportParameter("rpCompanyAddress", companyInformation.CompanyAddress));
            }
            lr.SetParameters(reportParameters);
            _rd = new ReportDataSource("DataSet1", _listVmAccVoucherDetail);
            lr.DataSources.Add(_rd);

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
