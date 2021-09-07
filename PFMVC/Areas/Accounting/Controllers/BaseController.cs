using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using DLL;
using DLL.Repository;
using Microsoft.Reporting.WebForms;
using PFMVC.common;
using System.Globalization;

namespace PFMVC.Areas.Accounting.Controllers
{
    public class BaseController : Controller
    {

        int PageID = 23;
        ReportDataSource rd;
        public UnitOfWork unitOfWork = new UnitOfWork();
        MvcApplication _mvcApplication; 
        
        [Authorize]
        public ActionResult Index()
        {
            //Added By Avishek Date:Jan-19-2016
            int oCode = ((int?)Session["oCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End 
            return View();
        }

        public ActionResult Redirect(string u, Guid g)
        {
            using (PFTMEntities context = new PFTMEntities())
            {
                var userInfo = context.tbl_UserRedirected.Where(w => w.UserID == u).SingleOrDefault();
                if (userInfo != null)
                {
                    if (userInfo.SessionID == g)
                    {
                        if (User.Identity.Name == u)
                        {
                            return RedirectToAction("Index");
                        }
                        //checking if user exist in this.database
                        var isUserRegistered = context.tbl_User.Where(w => w.LoginName == u).SingleOrDefault();
                        if (isUserRegistered != null)
                        {
                            FormsAuthentication.SignOut();
                            FormsAuthentication.SetAuthCookie(u, false);
                            return RedirectToAction("Index");
                        }
                        return Json(new { Success = false, ErrorMessage = "You're redirected but not registered in this application database! Please register and login again!" }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            TempData["Message"] = "You are redirected but you session information is missing. Please login again...";
            return RedirectToAction("Login", "Account", new { Area = ""}); // Later it will redirect to Main application user interface.
        }

        public ActionResult AccountHeadList()
        {
            //var v = unitOfWork.ACC_LedgerRepository.Get().Where(w => w.LedgerName.Contains(term)).Select(s => new { value = s.LedgerID, label = s.LedgerName }).ToList();
            int oCode = ((int?)Session["oCode"]) ?? 0;
            ViewBag.LegendName = "Head of Account";
            var v = unitOfWork.ACC_LedgerRepository.Get().Where(f => f.OCode == null || f.OCode == oCode).ToList();
            return PartialView("_LedgerListTable", v);
        }

        public ActionResult CashAccountHeadList()
        {
            //var v = unitOfWork.ACC_LedgerRepository.Get().Where(w => w.LedgerName.Contains(term)).Select(s => new { value = s.LedgerID, label = s.LedgerName }).ToList();
            int oCode = ((int?)Session["oCode"]) ?? 0;
            ViewBag.LegendName = "Cash Account";
            List<int> groupList = unitOfWork.AccountingRepository.CashAccountGroupList();
            var v = unitOfWork.ACC_LedgerRepository.Get().Where(w => groupList.Contains(w.GroupID)).Where(f => f.OCode == null || f.OCode == oCode).ToList();
            return PartialView("_CashLedgerListTable", v);
        }

        //Max primary key Note: if you chage this method then you have to do the same in AccountigAPI
        public int GetMaxVoucherID()
        {
            int vId = 0;
            try
            {
                vId = unitOfWork.ACC_VoucherEntryRepository.Get().Select(s => s.VoucherID).Max();
            }
            catch
            {
                vId = 0;
            }
            return vId + 1;
        }

        //Max voucher count for specific voucher ++ in addition this must associate with oCode
        //Edited by Avishek Date: Apr-12-2016
        public int GetMaxVoucherTypeID(int i, int oCode, string vNuminitial)
        {
            int count = unitOfWork.ACC_VoucherEntryRepository.Get(f => f.OCode == null || f.OCode == oCode).Count(s => s.VTypeID == i && s.VNumber.Contains(vNuminitial));
            return count + 1;
        }
        //End

        public ActionResult GetVoucherNameList(int voucherType = 0)
        {
            //Added By Avishek Date:Jan-19-2016
            int oCode = ((int?)Session["oCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End

            if (oCode == 0)
            {
                return Json(new { Success = false, ErrorMessage = "To create a group you must be under a compnay!" }, JsonRequestBehavior.AllowGet);
            }

            ViewBag.LegendName = "Voucher List";
            if (voucherType > 0)
            {
                var v = unitOfWork.ACC_VoucherEntryRepository.Get().Where(w => w.VTypeID == voucherType && (w.OCode == null || w.OCode == oCode)).ToList();
                return PartialView("_VoucherEntryList", v);
            }
            return PartialView("_VoucherEntryList", null);
        }

        public ActionResult DeleteVoucherPossible(int voucherID = 0)
        {
            int oCode = ((int?)Session["oCode"]) ?? 0;
            if (oCode == 0)
            {
                RedirectToAction("Login", "Account");
            }
            if (voucherID > 0)
            {
                acc_VoucherEntry vEntry = unitOfWork.ACC_VoucherEntryRepository.GetByID(voucherID);
                //added later. system voucher deleting disabled
                if (vEntry.VTypeID == 5)
                {
                    return Json(new { Success = false, ErrorMessage = "System voucher deleting restricted currently..." }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { Success = true, Message = "Sure deleting this voucher?" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { Success = false, ErrorMessage = "No data found to save!" }, JsonRequestBehavior.AllowGet);
        }

        #region Delete Voucher Confirm
        public ActionResult DeleteVoucherConfirm(int voucherID = 0)
        {
            //Added By Avishek Date:Jan-19-2016
            int oCode = ((int?)Session["oCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End
            acc_VoucherEntry vEntry = unitOfWork.ACC_VoucherEntryRepository.GetByID(voucherID);

            //added later. system voucher deleting disabled
            if (vEntry.VTypeID == 5)
            {
                return Json(new { Success = false, ErrorMessage = "System voucher deleting restricted currently..." }, JsonRequestBehavior.AllowGet);
            }
            unitOfWork.ACC_VoucherEntryRepository.Delete(vEntry);

            var delvDetail = unitOfWork.ACC_VoucherDetailRepository.Get().Where(w => w.VoucherID == voucherID).ToList();
            delvDetail.ForEach(f => unitOfWork.ACC_VoucherDetailRepository.Delete(f));
            try
            {
                unitOfWork.Save();
                return Json(new { Success = true, Message = "Voucher information deleted successfully..." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception x)
            {
                LogApplicationError.LogApplicationError1(x.Message, x.InnerException + "", User.Identity.Name);
                return Json(new { Success = false, ErrorMessage = "Error: " + x.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Voucher Detail Mini
        public ActionResult VoucherDetailMini(int voucherID)
        {
            //Edited By Avishek Date Sep-22-2015
            //Start
            //Added By Avishek Date:Jan-19-2016
            int oCode = ((int?)Session["oCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End
            CultureInfo cInfo = new CultureInfo("en-IN");
            _mvcApplication = new MvcApplication(); 
            acc_VoucherEntry vEntry = unitOfWork.ACC_VoucherEntryRepository.GetByID(voucherID);
            ViewBag.VoucherName = ""; // not required
            ViewBag.VoucherNo = vEntry.VNumber;
            ViewBag.VoucherID = vEntry.VoucherID;
            ViewBag.TransactionDate = vEntry.TransactionDate;
            ViewBag.Narration = vEntry.Narration;
            if (vEntry.VTypeID == 1)
            {
                ViewBag.VoucherType = "Payment Voucher";
            }
            if (vEntry.VTypeID == 2)
            {
                ViewBag.VoucherType = "Receipt Voucher";
            }
            if (vEntry.VTypeID == 3)
            {
                ViewBag.VoucherType = "Contra Voucher";
            }
            if (vEntry.VTypeID == 4)
            {
                ViewBag.VoucherType = "Journal Voucher";
            }


            var vDetail = unitOfWork.AccountingRepository.GetVoucherDetail(voucherID).ToList();
            decimal credit = 0;
            decimal debinte = 0;
            foreach(var item in vDetail){
                item.aCredit = _mvcApplication.GetNumber(item.Credit).ToString("N", cInfo);
                item.aDebit = _mvcApplication.GetNumber(item.Debit).ToString("N", cInfo);
            }
            //End
            if (vDetail.Count > 0)
            {
                ViewBag.User = "This record was modified by " + unitOfWork.UserProfileRepository.Get(w => w.UserID == vEntry.EditUser).SingleOrDefault().UserFullName + " at " + vEntry.EditDate;
                return PartialView("_VoucherDetailMini", vDetail);
            }
            return Json(new { Success = false, ErrorMessage = "Serious problem! voucher detail not found! Contact ADMIN." }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Voucher Report
        public ActionResult VoucherReport(int voucherID, string type)
        {
            LocalReport lr = new LocalReport();
            //Added By Avishek Date:Jan-19-2016
            int oCode = ((int?)Session["oCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End

            string path = Path.Combine(Server.MapPath("~/Areas/Accounting/AccountingReport"), "Voucher.rdlc");
            if (System.IO.File.Exists(path))
            {
                lr.ReportPath = path;
            }
            else
            {
                return View("Report/ReportPF/Index");
            }

            var vEntry = unitOfWork.ACC_VoucherEntryRepository.GetByID(voucherID);
            var v = unitOfWork.AccountingRepository.GetVoucherDetail(voucherID).ToList();
            var getCompany = unitOfWork.CompanyInformationRepository.GetByID(oCode);
            string userInfo = "This record was modified by " + unitOfWork.UserProfileRepository.Get(w => w.UserID == vEntry.EditUser).SingleOrDefault().UserFullName + " at " + vEntry.EditDate;

            string vType = unitOfWork.ACC_VoucherTypeRepository.Get().Where(x=>x.VTypeID == vEntry.VTypeID).Select(x=>x.VTypeName).FirstOrDefault();
            ReportParameterCollection reportParameters = new ReportParameterCollection();
            reportParameters.Add(new ReportParameter("rpVoucherNumber", vEntry.VNumber + ""));
            reportParameters.Add(new ReportParameter("rpTransactionDate", vEntry.TransactionDate + ""));
            reportParameters.Add(new ReportParameter("rpCreatedBy", userInfo ));
            reportParameters.Add(new ReportParameter("rpNarration", vEntry.Narration+""));
            reportParameters.Add(new ReportParameter("rpCompanyName", getCompany.CompanyName + ""));
            reportParameters.Add(new ReportParameter("rpCompanyAddress", getCompany.CompanyAddress + ""));
            reportParameters.Add(new ReportParameter("rpVoucherType", vType));
            reportParameters.Add(new ReportParameter("rpuserName", User.Identity.Name));
            lr.SetParameters(reportParameters);
            rd = new ReportDataSource("DataSet1", v);
            lr.DataSources.Add(rd);

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
        #endregion

        #region Voucher Report (PF Specific)
        public ActionResult PFVoucherReport(string month, string year)
        {
            LocalReport lr = new LocalReport();
            //Added By Avishek Date:Jan-19-2016
            int oCode = ((int?)Session["oCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End

            string path = Path.Combine(Server.MapPath("~/Areas/Accounting/AccountingReport"), "Voucher.rdlc");
            if (System.IO.File.Exists(path))
            {
                lr.ReportPath = path;
            }
            else
            {
                return View("Report/ReportPF/Index");
            }

            //var vEntry = unitOfWork.ACC_VoucherEntryRepository.Get(w => w.PFMonth == month.PadLeft(2, '0') && w.PFYear == year).SingleOrDefault();
            var vEntry = unitOfWork.ACC_VoucherEntryRepository.Get().Where(w => w.PFMonth == month.PadLeft(2, '0') && w.PFYear == year).FirstOrDefault();
            if (vEntry == null)
            {
                return Content("Voucher not found!");
            }
            var v = unitOfWork.AccountingRepository.GetVoucherDetail(vEntry.VoucherID, oCode).ToList();

            string userInfo = "This record was modified by " + unitOfWork.UserProfileRepository.Get(w => w.UserID == vEntry.EditUser).SingleOrDefault().UserFullName + " at " + vEntry.EditDate;
            var getCompany = unitOfWork.CompanyInformationRepository.GetByID(oCode);
            ReportParameterCollection reportParameters = new ReportParameterCollection();
            reportParameters.Add(new ReportParameter("rpVoucherNumber", vEntry.VNumber + ""));
            reportParameters.Add(new ReportParameter("rpTransactionDate", vEntry.TransactionDate + ""));
            reportParameters.Add(new ReportParameter("rpCreatedBy", userInfo));
            reportParameters.Add(new ReportParameter("rpNarration", vEntry.Narration + ""));
            //reportParameters.Add(new ReportParameter("rpCreatedBy", userInfo));
            //reportParameters.Add(new ReportParameter("rpNarration", vEntry.Narration + ""));
            reportParameters.Add(new ReportParameter("rpVoucherType", "System Generated Voucher"));
            reportParameters.Add(new ReportParameter("rpCompanyName", getCompany.CompanyName + ""));
            reportParameters.Add(new ReportParameter("rpCompanyAddress", getCompany.CompanyAddress + ""));
            lr.SetParameters(reportParameters);
            rd = new ReportDataSource("DataSet1", v);
            lr.DataSources.Add(rd);

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
        #endregion

        public ActionResult FilterBox()
        {
            return PartialView("_FilterBox");
        }

        public DateTime GetSystemImplementationDate(ref string message)
        {   
            var v = unitOfWork.CompanyInformationRepository.Get().FirstOrDefault();
            if (v != null)
            {
                if (v.SystemImplementationDate != null)
                {
                    return v.SystemImplementationDate ?? DateTime.MaxValue;
                }
            }
            message = "Implentation Date Not Found! You must give system implementation date to continue...";
            return DateTime.MaxValue;
        }
    }
}
