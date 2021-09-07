using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using DLL;
using DLL.ViewModel;
using PFMVC.common;

namespace PFMVC.Areas.Accounting.Controllers
{
    public class JournalVoucher2Controller : BaseController
    {
        int PageID = 14;

        /// <summary>
        /// Journals the index2.
        /// </summary>
        /// <returns>view</returns>
        /// <ModifiesBy>Avishek</ModifiesBy>
        /// <ModificationDate>24-Feb-2016</ModificationDate>
        public ActionResult JournalIndex2()
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


        /// <summary>
        /// Journals the voucher.
        /// </summary>
        /// <param name="VoucherID">The voucher identifier.</param>
        /// <returns>PartialView</returns>
        /// <ModifiesBy>Avishek</ModifiesBy>
        /// <ModificationDate>24-Feb-2016</ModificationDate>
        public ActionResult JournalVoucher(int VoucherID = 0)
        {
            //Added By Avishek Date:Jan-19-2016
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End

            if (VoucherID == 0)
            {
                ViewBag.VoucherName = "";
                ViewBag.VoucherNo = "New";
                ViewBag.VoucherID = 0;
                ViewBag.TransactionDate = DateTime.Now;
                List<VM_acc_VoucherDetail> v = new List<VM_acc_VoucherDetail>();
                return PartialView("_JournalVoucher2", v);
            }
            acc_VoucherEntry vEntry = unitOfWork.ACC_VoucherEntryRepository.GetByID(VoucherID);
            ViewBag.VoucherName = ""; // not required
            ViewBag.VoucherNo = vEntry.VNumber;
            ViewBag.VoucherID = vEntry.VoucherID;
            ViewBag.TransactionDate = vEntry.TransactionDate;
            ViewBag.Narration = vEntry.Narration;

            var vDetail = unitOfWork.AccountingRepository.GetVoucherDetail(VoucherID).ToList();
            ViewBag.User = "This record was modified by " + unitOfWork.UserProfileRepository.Get(w => w.UserID == vEntry.EditUser).SingleOrDefault().UserFullName + " at " + vEntry.EditDate;
            return PartialView("_JournalVoucher2", vDetail);
        }

        /// <summary>
        /// Journals the voucher.
        /// </summary>
        /// <param name="TransactionDate">The transaction date.</param>
        /// <param name="VoucherID">The voucher identifier.</param>
        /// <param name="Narration">The narration.</param>
        /// <param name="LedgerID">The ledger identifier.</param>
        /// <param name="Debit">The debit.</param>
        /// <param name="Credit">The credit.</param>
        /// <param name="ChequeNumber">The cheque number.</param>
        /// <returns>Bool</returns>
        /// <ModifiesBy>Avishek</ModifiesBy>
        /// <ModificationDate>24-Feb-2016</ModificationDate>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult JournalVoucher(DateTime TransactionDate, int VoucherID, string Narration, IList<Guid> LedgerID, IList<decimal> Debit, IList<decimal> Credit, IList<string> ChequeNumber)
        {
            //Added By Avishek Date:Jan-19-2016
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            bool b = PagePermission.HasPermission(User.Identity.Name, PageID, 1);
            if (!b)
            {
                return Json(new { Success = false, ErrorMessage = "Your are not authorized in this section" }, JsonRequestBehavior.DenyGet);
            }
            //End

            //Eddited by Izab Ahmed on 05-12-2018
            DateTime lastDate = unitOfWork.AuditLogRepository.Get().Select(x => x.LastAuditDate.Value).Max();

            if (TransactionDate <= lastDate)
            {
                return Json(new { Success = false, ErrorMessage = "You cann't save this voucher because " + TransactionDate.ToString("dd-MMM-yyyy") + " Audit is complete." }, JsonRequestBehavior.DenyGet);
            }
            //End
            if (!(LedgerID.Count == Credit.Count && Credit.Count == Debit.Count && Debit.Count == ChequeNumber.Count))
            {
                return Json(new { Success = false, ErrorMessage = "Input problem... count mismatch" }, JsonRequestBehavior.DenyGet);
            }
            //now check if debit and credit id equal for contra and journal voucher
            decimal totalDebit = 0;
            decimal totalCredit = 0;
            for (int i = 0; i < Debit.Count; i++)
            {
                if (LedgerID[i] == Guid.Empty) continue;
                totalDebit += Debit[i];
                totalCredit += Credit[i];
            }
            if (totalDebit != totalCredit)
            {
                return Json(new { Success = false, ErrorMessage = "For contra voucher DEBIT and CREDIT should be equal." }, JsonRequestBehavior.DenyGet);
            }
            //
            bool atLeastOneEntryFound = false;
            string voucherNumber = "";
            Guid editUser = unitOfWork.CustomRepository.GetUserID(User.Identity.Name);
            DateTime editDate = DateTime.Now;

            if (VoucherID == 0)
            {
                #region New Voucher Entry
                acc_VoucherEntry vEntry = new acc_VoucherEntry();
                string initialNo = "JV-" + TransactionDate.ToString("yy") + "-" + TransactionDate.ToString("MM") + "-";
                vEntry.VNumber = initialNo + GetMaxVoucherTypeID(4, oCode, initialNo).ToString().PadLeft(4, '0');
                voucherNumber = vEntry.VNumber;
                vEntry.VoucherID = GetMaxVoucherID();
                VoucherID = vEntry.VoucherID;
                vEntry.VoucherName = "";
                vEntry.VTypeID = 4;  // 4 for journal voucher
                vEntry.OCode = oCode;
                vEntry.EditDate = editDate;
                vEntry.EditUser = editUser;
                vEntry.Narration = Narration;
                vEntry.TransactionDate = TransactionDate; //This line will be deleted after Back Dated Data entry
                unitOfWork.ACC_VoucherEntryRepository.Insert(vEntry);

                for (int i = 0; i < LedgerID.Count; i++)
                {
                    if (LedgerID[i] == Guid.Empty) continue;
                    acc_VoucherDetail vDetail = new acc_VoucherDetail();
                    vDetail.Credit = Credit[i];
                    vDetail.Debit = Debit[i];
                    vDetail.VoucherDetailID = Guid.NewGuid();
                    vDetail.LedgerID = LedgerID[i];
                    vDetail.VoucherID = vEntry.VoucherID;
                    vDetail.EditDate = editDate;
                    vDetail.EditUser = editUser;
                    vDetail.ChequeNumber = ChequeNumber[i] + "";
                    vDetail.OCode = oCode;
                    vDetail.TransactionDate = TransactionDate; //This line will be deleted after Back Dated Data entry
                    vDetail.VTypeID = 4;  // 4 for journal voucher
                    unitOfWork.ACC_VoucherDetailRepository.Insert(vDetail);
                    atLeastOneEntryFound = true;
                }
                #endregion
            }
            else if (VoucherID > 0)
            {
                #region Update existing voucher
                acc_VoucherEntry vEntry = unitOfWork.ACC_VoucherEntryRepository.GetByID(VoucherID);
                if (vEntry == null)
                {
                    return Json(new { Success = false, ErrorMessage = "The voucher which id is " + VoucherID + " not found!" }, JsonRequestBehavior.DenyGet);
                }
                vEntry.TransactionDate = TransactionDate;//This line will be deleted after Back Dated Data entry
                vEntry.EditDate = editDate;
                vEntry.EditUser = editUser;
                vEntry.Narration = Narration;
                voucherNumber = vEntry.VNumber;
                vEntry.OCode = oCode;
                var delvDetail = unitOfWork.ACC_VoucherDetailRepository.Get().Where(w => w.VoucherID == VoucherID).ToList();
                delvDetail.ForEach(f => unitOfWork.ACC_VoucherDetailRepository.Delete(f));
                for (int i = 0; i < LedgerID.Count; i++)
                {
                    if (LedgerID[i] != Guid.Empty)
                    {
                        acc_VoucherDetail vDetail = new acc_VoucherDetail();
                        vDetail.Credit = Credit[i];
                        vDetail.Debit = Debit[i];
                        vDetail.VoucherDetailID = Guid.NewGuid();
                        vDetail.LedgerID = LedgerID[i];
                        vDetail.ChequeNumber = ChequeNumber[i] + "";
                        vDetail.VoucherID = vEntry.VoucherID;
                        vDetail.EditDate = editDate;
                        vDetail.EditUser = editUser;
                        vDetail.OCode = oCode;
                        vDetail.TransactionDate = TransactionDate;  //This line will be deleted after Back Dated Data entry
                        vDetail.VTypeID = 4; //4 for journal voucher
                        unitOfWork.ACC_VoucherDetailRepository.Insert(vDetail);
                        atLeastOneEntryFound = true;
                    }
                }
                #endregion
            }
            try
            {
                if (atLeastOneEntryFound)
                {
                    unitOfWork.Save();
                    return Json(new { Success = true, Message = "Journal voucher Saved!", VoucherNumber = voucherNumber, VoucherID = VoucherID }, JsonRequestBehavior.DenyGet);
                }
                return Json(new { Success = false, ErrorMessage = "No data found to save!" }, JsonRequestBehavior.DenyGet);
            }
            catch (Exception x)
            {
                LogApplicationError.LogApplicationError1(x.Message, x.InnerException + "", User.Identity.Name);
                return Json(new { Success = false, ErrorMessage = "Error Occured: " + x.Message }, JsonRequestBehavior.DenyGet);
            }
        }



        public ActionResult GetJQFile()
        {
            return PartialView("_JournalVoucherJQ2");
        }
    }
}
