using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DLL;
using DLL.ViewModel;
using PFMVC.common;

namespace PFMVC.Areas.Accounting.Controllers
{
    public class JournalVoucherController : BaseController
    {
        //
        // GET: /Accounting/JournalVoucher/

        public ActionResult JournalIndex()
        {
            return View();
        }


        public ActionResult JournalVoucher(int VoucherID = 0)
        {
            if (VoucherID == 0)
            {
                ViewBag.VoucherName = "";
                ViewBag.VoucherNo = "New";
                ViewBag.VoucherID = 0;
                ViewBag.TransactionDate = DateTime.Now;
                List<VM_acc_VoucherDetail> v = new List<VM_acc_VoucherDetail>();
                return PartialView("_JournalVoucher", v);
            }
            else
            {
                acc_VoucherEntry vEntry = unitOfWork.ACC_VoucherEntryRepository.GetByID(VoucherID);
                ViewBag.VoucherName = ""; // not required
                ViewBag.VoucherNo = vEntry.VNumber;
                ViewBag.VoucherID = vEntry.VoucherID;
                ViewBag.TransactionDate = vEntry.TransactionDate;
                ViewBag.Narration = vEntry.Narration;

                var vDetail = unitOfWork.AccountingRepository.GetVoucherDetail(VoucherID).ToList();
                ViewBag.User = "This record was modified by " + unitOfWork.UserProfileRepository.Get().Where(w => w.UserID == vEntry.EditUser).SingleOrDefault().UserFullName + " at " + vEntry.EditDate;
                return PartialView("_JournalVoucher", vDetail);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult JournalVoucher(DateTime TransactionDate, int VoucherID, string Narration, IList<Guid> LedgerID, IList<decimal> Debit, IList<decimal> Credit, IList<string> ChequeNumber)
        {
            int OCode = ((int?)Session["OCode"]) ?? 0;

            if (OCode == 0)
            {
                return Json(new { Success = false, ErrorMessage = "To create a group you must be under a compnay!" }, JsonRequestBehavior.DenyGet);
            }


            //checking if row count is as expected... should be equal
            if (!(LedgerID.Count == Credit.Count && Credit.Count == Debit.Count && Debit.Count == ChequeNumber.Count))
            {
                return Json(new { Success = false, ErrorMessage = "Input problem... count mismatch" }, JsonRequestBehavior.DenyGet);
            }
            //now check if debit and credit id equal for contra and journal voucher
            decimal total_debit = 0;
            decimal total_credit = 0;
            for (int i = 0; i < Debit.Count; i++)
            {
                total_debit += Debit[i];
                total_credit += Credit[i];
            }
            if (total_debit != total_credit)
            {
                return Json(new { Success = false, ErrorMessage = "For journal voucher DEBIT and CREDIT should be equal." }, JsonRequestBehavior.DenyGet);
            }
            bool atLeastOneEntryFound = false;
            string VoucherNumber = "";
            Guid EditUser = unitOfWork.CustomRepository.GetUserID(User.Identity.Name);
            DateTime EditDate = DateTime.Now;

            if (VoucherID == 0)
            {
                #region New Voucher Entry
                acc_VoucherEntry vEntry = new acc_VoucherEntry();
                vEntry.TransactionDate = TransactionDate;
                vEntry.VNumber = "JV-" + GetMaxVoucherTypeID(4, OCode).ToString().PadLeft(8, '0');
                VoucherNumber = vEntry.VNumber;
                vEntry.VoucherID = GetMaxVoucherID();
                VoucherID = vEntry.VoucherID;
                vEntry.VoucherName = "";
                vEntry.VTypeID = 4;  // 4 for journal voucher
                vEntry.OCode = OCode;
                vEntry.EditDate = EditDate;
                vEntry.EditUser = EditUser;
                vEntry.Narration = Narration;
                unitOfWork.ACC_VoucherEntryRepository.Insert(vEntry);

                acc_VoucherDetail vDetail;
                for (int i = 0; i < LedgerID.Count; i++)
                {
                    if (LedgerID[i] != Guid.Empty)
                    {
                        vDetail = new acc_VoucherDetail();
                        vDetail.Credit = Credit[i];
                        vDetail.Debit = Debit[i];
                        vDetail.VoucherDetailID = Guid.NewGuid();
                        vDetail.LedgerID = LedgerID[i];
                        vDetail.VoucherID = vEntry.VoucherID;
                        vDetail.EditDate = EditDate;
                        vDetail.EditUser = EditUser;
                        vDetail.ChequeNumber = ChequeNumber[i] + "";
                        vDetail.TransactionDate = TransactionDate;
                        vDetail.OCode = OCode;
                        vDetail.VTypeID = 4;  // 4 for Journal voucher
                        unitOfWork.ACC_VoucherDetailRepository.Insert(vDetail);
                        atLeastOneEntryFound = true;
                    }
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
                vEntry.TransactionDate = TransactionDate;
                vEntry.EditDate = EditDate;
                vEntry.EditUser = EditUser;
                vEntry.Narration = Narration;
                VoucherNumber = vEntry.VNumber;


                var delvDetail = unitOfWork.ACC_VoucherDetailRepository.Get().Where(w => w.VoucherID == VoucherID).ToList();
                delvDetail.ForEach(f => unitOfWork.ACC_VoucherDetailRepository.Delete(f));

                acc_VoucherDetail vDetail;
                for (int i = 0; i < LedgerID.Count; i++)
                {
                    if (LedgerID[i] != Guid.Empty)
                    {
                        vDetail = new acc_VoucherDetail();

                        vDetail.Credit = Credit[i];
                        vDetail.Debit = Debit[i];
                        vDetail.VoucherDetailID = Guid.NewGuid();
                        vDetail.LedgerID = LedgerID[i];
                        vDetail.ChequeNumber = ChequeNumber[i] + "";
                        vDetail.VoucherID = vEntry.VoucherID;
                        vDetail.EditDate = EditDate;
                        vDetail.EditUser = EditUser;
                        vDetail.TransactionDate = TransactionDate;
                        vDetail.VTypeID = 4;  // 4 for Journal voucher
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
                    return Json(new { Success = true, Message = "Journal voucher Saved!", VoucherNumber = VoucherNumber, VoucherID = VoucherID }, JsonRequestBehavior.DenyGet);
                }
                else
                {
                    return Json(new { Success = false, ErrorMessage = "No data found to save!" }, JsonRequestBehavior.DenyGet);
                }
            }
            catch (Exception x)
            {
                LogApplicationError.LogApplicationError1(x.Message, x.InnerException + "", User.Identity.Name);
                return Json(new { Success = false, ErrorMessage = "Error Occured: " + x.Message }, JsonRequestBehavior.DenyGet);
            }
        }

        public ActionResult GetJQFile()
        {
            return PartialView("_JournalVoucherJQ");
        }

    }
}
