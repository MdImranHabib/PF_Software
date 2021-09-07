using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using DLL;
using DLL.Repository;
using DLL.ViewModel;
using PFMVC.common;

namespace PFMVC.Areas.Instrument.Controllers
{
    public class EncashmentController : Controller
    {
        UnitOfWork unitOfWork = new UnitOfWork();

        int PageID = 19;

        [Authorize]
        public ActionResult EncashmentEntry()
        {
            //Added By Avishek Date:Jan-19-2016
            int OCode = ((int?)Session["OCode"]) ?? 0;
            if (OCode == 0)
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

        public void InvestmentOptions(string s = "")
        {
            //17 for Investment
            int OCode = ((int?)Session["OCode"]) ?? 0;
            var v = unitOfWork.ACC_LedgerRepository.Get().Where(f => f.GroupID == 17 && f.OCode == OCode).ToList();
            ViewData["InvestmentOptions"] = new SelectList(v, "LedgerID", "LedgerName", s);
        }

        [Authorize]
        public ActionResult EncashmentForm(int VoucherID = 0)
        {
            //Added By Avishek Date:Jan-19-2016
            int OCode = ((int?)Session["OCode"]) ?? 0;
            if (OCode == 0)
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
                return PartialView("_EncashmentForm", v);
            }
            else
            {
                acc_VoucherEntry vEntry = unitOfWork.ACC_VoucherEntryRepository.GetByID(VoucherID);
                ViewBag.VoucherName = ""; // not required
                ViewBag.VoucherNo = vEntry.VNumber;
                ViewBag.VoucherID = vEntry.VoucherID;
                ViewBag.TransactionDate = vEntry.TransactionDate;
                ViewBag.Narration = vEntry.Narration;

                var vDetail = unitOfWork.AccountingRepository.GetVoucherDetail(VoucherID,OCode).ToList();
                ViewBag.User = "This record was modified by " + unitOfWork.UserProfileRepository.Get().Where(w => w.UserID == vEntry.EditUser).SingleOrDefault().UserFullName + " at " + vEntry.EditDate;
                return PartialView("_EncashmentForm", vDetail);
            }
        }

        public ActionResult GetJQFile()
        {
            return PartialView("_EncashmentJQ");
        }


        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult EncashmentVoucher(DateTime TransactionDate, int VoucherID, string Narration, IList<Guid> LedgerID, IList<decimal> Debit, IList<decimal> Credit, IList<string> ChequeNumber)
        {
            //Added By Avishek Date:Jan-19-2016
            int OCode = ((int?)Session["OCode"]) ?? 0;
            if (OCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End
            //count record number
            if (!(LedgerID.Count == Credit.Count && Credit.Count == Debit.Count && Debit.Count == ChequeNumber.Count))
            {
                return Json(new { Success = false, ErrorMessage = "Input problem... count mismatch" }, JsonRequestBehavior.DenyGet);
            }
            //now check if debit and credit id equal for contra and journal voucher
            decimal total_debit = 0;
            decimal total_credit = 0;
            for (int i = 0; i < Debit.Count; i++)
            {
                if (LedgerID[i] != Guid.Empty) // it's important
                {
                    total_debit += Debit[i];
                    total_credit += Credit[i];
                }
            }
            if (total_debit != total_credit)
            {
                return Json(new { Success = false, ErrorMessage = "DEBIT and CREDIT should be equal." }, JsonRequestBehavior.DenyGet);
            }
            //
            bool atLeastOneEntryFound = false;
            string VoucherNumber = "";
            Guid EditUser = unitOfWork.CustomRepository.GetUserID(User.Identity.Name);
            DateTime EditDate = DateTime.Now;

            if (VoucherID == 0)
            {
                #region New Voucher Entry
                acc_VoucherEntry vEntry = new acc_VoucherEntry();
                vEntry.TransactionDate = TransactionDate;
                string initialNo = "CV-" + TransactionDate.ToString("yy") + "-" + TransactionDate.ToString("MM") + "-";
                vEntry.VNumber = initialNo + GetMaxVoucherTypeID(6, OCode, initialNo).ToString().PadLeft(4, '0'); //let 6 for encashment voucher
                VoucherNumber = vEntry.VNumber;
                vEntry.VoucherID = unitOfWork.AccountingRepository.GetMaxVoucherID(OCode);
                VoucherID = vEntry.VoucherID;
                vEntry.VoucherName = "";
                vEntry.VTypeID = 6; // encashment entry type voucher
                vEntry.EditDate = EditDate;
                vEntry.EditUser = EditUser;
                vEntry.Narration = Narration;
                vEntry.OCode = OCode; 
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
                        vDetail.VTypeID = 6;  // 6 for encashment voucher
                        vDetail.OCode = OCode;
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
                        vDetail.VTypeID = 6;  // 6 for encashment voucher
                        vDetail.OCode = OCode;
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
                    return Json(new { Success = true, Message = "Payment voucher Saved!", VoucherNumber = VoucherNumber, VoucherID = VoucherID }, JsonRequestBehavior.DenyGet);
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


        public int GetMaxVoucherTypeID(int i, int oCode, string vNuminitial)
        {
            int count = unitOfWork.ACC_VoucherEntryRepository.Get(f => f.OCode == null || f.OCode == oCode).Count(s => s.VTypeID == i && s.VNumber.Contains(vNuminitial));
            return count + 1;
        }

    }
}
