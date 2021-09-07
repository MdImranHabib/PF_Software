using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using DLL;
using DLL.ViewModel;
using PFMVC.common;

namespace PFMVC.Areas.Accounting.Controllers
{
    public class SystemVoucherController : BaseController
    {
        int PageID = 17;
        public ActionResult SystemVoucherIndex()
        {
            //Added By Avishek Date:Jan-19-2016
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            bool b = PagePermission.HasPermission(User.Identity.Name, PageID, 0);
            if (!b)
            {
                ViewBag.PageName = "Employee Setup";
                return View("Unauthorized");
            }
            //End
                return View();
        }


        public ActionResult SystemVoucher(int VoucherID = 0)
        {
            //Added By Avishek Date:Jan-19-2016
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End

            //Eddited by Izab Ahmed on 05-12-2018
            //DateTime lastDate = unitOfWork.AuditLogRepository.Get().Select(x => x.LastAuditDate.Value).Max();

            //if (TransactionDate <= lastDate)
            //{
            //    return Json(new { Success = false, ErrorMessage = "You cann't save this voucher because " + TransactionDate.ToString("dd-MMM-yyyy") + " Audit is complete." }, JsonRequestBehavior.DenyGet);
            //}
            //End

            if (VoucherID == 0)
            {
                return Content("<fieldset><legend>System Voucher</legend>Select voucher from list to get detail...</fieldset>");
            }
            acc_VoucherEntry vEntry = unitOfWork.ACC_VoucherEntryRepository.GetByID(VoucherID);
            ViewBag.VoucherName = ""; // not required
            ViewBag.VoucherNo = vEntry.VNumber;
            ViewBag.VoucherID = vEntry.VoucherID;
            ViewBag.TransactionDate = vEntry.TransactionDate; //This line will be deleted after Back Dated Data entry
            ViewBag.Narration = vEntry.Narration;
            var vDetail = unitOfWork.AccountingRepository.GetVoucherDetail(VoucherID).ToList();
            ViewBag.User = "This record was modified by " + unitOfWork.UserProfileRepository.Get(w => w.UserID == vEntry.EditUser).SingleOrDefault().UserFullName + " at " + vEntry.EditDate;
            return PartialView("_SystemVoucher", vDetail);
        }

        [HttpPost]
        public ActionResult JournalVoucher(DateTime TransactionDate, int VoucherID, string Narration, IList<Guid> LedgerID, IList<decimal> Debit, IList<decimal> Credit, IList<string> ChequeNumber)
        {
            return Json(new { Success = false, ErrorMessage = "System voucher cannot be modified currently!!! If you want to modify this voucher please contact sys admin!" }, JsonRequestBehavior.DenyGet);
        }

        public ActionResult GetJQFile()
        {
            return PartialView("_SystemVoucherJQ");
        }

        /// <summary>
        /// Earlies the profit distribution view.
        /// </summary>
        /// <returns>view</returns>
        /// <CreatedBy>Avishek<CreatedBy>
        /// <Date>Mar-04-2015</Date>
        public ActionResult EarlyProfitDistributionView()
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


        /// <summary>
        /// Gets the pv file.
        /// </summary>
        /// <returns>view</returns>
        /// <CreatedBy>Avishek<CreatedBy>
        /// <Date>Mar-04-2015</Date>
        public ActionResult GetPVFile()
        {
            return PartialView("_EarlyProfitDistributionPartialView");
        }

        /// <summary>
        /// Systems the voucher.
        /// </summary>
        /// <param name="VoucherID">The voucher identifier.</param>
        /// <returns>view</returns>
        /// <CreatedBy>Avishek<CreatedBy>
        /// <Date>Mar-04-2015</Date>
        public ActionResult GetEmployee(int id = 0)
        {

            tbl_Employees employee = unitOfWork.EmployeesRepository.GetByID(id);
            List<tbl_Contribution> tblContribution = unitOfWork.CustomRepository.ValidContributionDetail(employee.EmpID);
            VM_Employee vmEmployee = new VM_Employee();
            //Session["EmpIdforProfitVoucher"] = employee.EmpID;
            vmEmployee.IdentificationNumber = employee.IdentificationNumber;
            vmEmployee.EmpName = employee.EmpName;
            vmEmployee.OwnCont = (decimal)employee.opOwnContribution + tblContribution.Where(x => x.EmpID == employee.EmpID).Sum(x => x.SelfContribution);
            vmEmployee.EmpCont = (decimal)employee.opEmpContribution + tblContribution.Where(x => x.EmpID == employee.EmpID).Sum(x => x.EmpContribution);
            return Json(vmEmployee);
        }

        /// <summary>
        /// Earlies the sattlement voucher.
        /// </summary>
        /// <param name="TransactionDate">The transaction date.</param>
        /// <param name="VoucherID">The voucher identifier.</param>
        /// <param name="Narration">The narration.</param>
        /// <param name="LedgerID">The ledger identifier.</param>
        /// <param name="Debit">The debit.</param>
        /// <param name="Credit">The credit.</param>
        /// <param name="ChequeNumber">The cheque number.</param>
        /// <returns>message</returns>
        /// <CreatedBy>Avishek<CreatedBy>
        /// <Date>Mar-04-2015</Date>
        [HttpPost]
        public ActionResult EarlySattlementVoucher(DateTime TransactionDate, int VoucherID, string Narration, IList<Guid> LedgerID, IList<decimal> Debit, IList<decimal> Credit, IList<string> ChequeNumber)
        {
            return Json(new { Success = false, ErrorMessage = "System voucher cannot be modified currently!!! If you want to modify this voucher please contact sys admin!" }, JsonRequestBehavior.DenyGet);
        }

        /// <summary>
        /// Saves the specified debit.
        /// </summary>
        /// <param name="debit">The debit.</param>
        /// <param name="searchTerm">The search term.</param>
        /// <param name="chequeNumber">The cheque number.</param>
        /// <returns>bool</returns>
        /// <CreatedBy>Avishek</CreatedBy>
        /// <CreatedDate>7-Mar-2015</CreatedDate>
        public ActionResult Save(decimal debit, int empId, string chequeNumber = "")
        {
            //Added By Avishek Date:Jan-19-2016
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End

            int voucherId = 0;

            List<string> ledgerNameList = new List<string>();
            List<decimal> credit = new List<decimal>();
            List<decimal> debits = new List<decimal>();
            List<string> chqNumber = new List<string>();
            List<string> pfLoanId = new List<string>();
            List<string> pfMemberId = new List<string>();
            string refMessage = "";

            ledgerNameList.Add("Early Profit Distribution");
            debits.Add(debit);
            credit.Add(0);
            chqNumber.Add("");
            pfMemberId.Add(empId + "");
            pfLoanId.Add("");

            ledgerNameList.Add("Members Fund");
            credit.Add(debit);
            debits.Add(0);
            chqNumber.Add("");
            pfMemberId.Add(empId + "");
            pfLoanId.Add("");
            pfMemberId.Add(empId.ToString());

            bool isOperationSuccess = unitOfWork.AccountingRepository.DualEntryVoucher(empId, 5, DateTime.Now, ref voucherId, "Early Profit Distribution", ledgerNameList, debits, credit, chqNumber, ref refMessage, User.Identity.Name, unitOfWork.CustomRepository.GetUserID(User.Identity.Name), pfMemberId, "Early Profit Distribution", "", "", null, pfLoanId, oCode, "Early Profit Distribution");

            return Json(isOperationSuccess ? "Success" : "Transaction Failded with error");
        }



    }
}
