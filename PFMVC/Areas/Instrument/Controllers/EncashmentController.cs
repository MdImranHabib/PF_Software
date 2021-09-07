using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using DLL;
using DLL.Repository;
using DLL.ViewModel;
using PFMVC.common;
using System.Data.Entity.Validation;
using System.Web.Routing;

namespace PFMVC.Areas.Instrument.Controllers
{
    public class EncashmentController : Controller
    {
        UnitOfWork unitOfWork = new UnitOfWork();

        [Authorize]
        public ActionResult EncashmentEntry()
        {
            //Added By Avishek Date:Jan-19-2016
            int OCode = ((int?)Session["OCode"]) ?? 0;
            if (OCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End
            return View();
        }


        public JsonResult AutocompleteSuggestionsForInstrument(string term)
        {
            try
            {
                int OCode = ((int?)Session["OCode"]) ?? 0;
                var suggestions = unitOfWork.InstrumentRepository.Get(x => x.InstrumentNumber.ToLower().Trim().Contains(term.ToUpper().Trim()) && (x.OCode == OCode) && x.Closed == false).Select(s => new
                {
                    label = s.InstrumentType + "-" + s.InstrumentNumber,
                    value = s.InstrumentID
                }).ToList();
                return Json(suggestions, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        //public void InvestmentOptions(string s = "")
        //{
        //    //17 for Investment
        //    int OCode = ((int?)Session["OCode"]) ?? 0;
        //    var v = unitOfWork.ACC_LedgerRepository.Get().Where(f => f.GroupID == 17 && f.OCode == OCode).ToList();
        //    ViewData["InvestmentOptions"] = new SelectList(v, "LedgerID", "LedgerName", s);
        //}

        public void LedgerOptions(string s = "")
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            ViewData["LedgerOptions"] = new SelectList(unitOfWork.ACC_LedgerRepository.Get().Where(f => string.IsNullOrEmpty(f.PFMemberID) && f.OCode == oCode), "LedgerID", "LedgerName", s);
        }


        public ActionResult InstrumentIndex(int id =0)
        {
            //int PageID = 1;
            ////Added By Avishek Date:Jan-19-2016
            //int oCode = ((int?)Session["OCode"]) ?? 0;
            //if (oCode == 0)
            //{
            //    return RedirectToAction("Login", "Account", new { area = "" });
            //}
            //bool b = PagePermission.HasPermission(User.Identity.Name, PageID, 0);
            //if (!b)
            //{
            //    ViewBag.PageName = "Instrument";
            //    return View("Unauthorized");
            //}
            ////End
            //return RedirectToAction("Index", "Instrument", new { area = "Instrument", instrumentID = instrumentID });

            return RedirectToAction("Index", "Instrument", new { area = "Instrument", @instrumentID = id });

            //return PartialView("InstrumentForm", instrument);
        }
        [Authorize]
        public ActionResult EncashmentForm(int instrumentID = 0)
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            tbl_Instrument instrument = new tbl_Instrument();
            instrument = unitOfWork.InstrumentRepository.Get(x => x.InstrumentID == instrumentID).FirstOrDefault();
            string instrumentLedger = instrument.InstrumentType + "-" + instrument.InstrumentNumber;
            var ledger = unitOfWork.ACC_LedgerRepository.Get(x => x.LedgerName == instrumentLedger).FirstOrDefault();
            Guid ledgerId = ledger.LedgerID;
            string ledgerName = ledger.LedgerName;

            VM_Instrument vmInstrument = new VM_Instrument();
            vmInstrument.InstrumentType = instrument.InstrumentType;
            vmInstrument.InstrumentNumber = instrument.InstrumentNumber;
            vmInstrument.InstrumentID = instrument.InstrumentID;
            vmInstrument.Institution = instrument.Institution;
            vmInstrument.InterestRate = instrument.InterestRate;
            vmInstrument.LedgerID = ledgerId;
            vmInstrument.LedgerName = ledgerName;
            vmInstrument.Amount = instrument.Amount;
            vmInstrument.Branch = instrument.Branch;
            vmInstrument.MaturityPeriod = instrument.MaturityPeriod;
            vmInstrument.MaturityDate = instrument.MaturityDate;
            vmInstrument.DepositDate = instrument.DepositDate;
            vmInstrument.EncashmentDate = DateTime.Now;
            DateTime dateLastProcess = Convert.ToDateTime(instrument.LastProcessDate ?? DateTime.Now);
            vmInstrument.LastProcessShow = dateLastProcess.ToShortDateString();
            ViewBag.InstrumentID = instrument.InstrumentID;
            LedgerOptions(ledgerId + "");
            return PartialView("_EncashmentFormWithLedger", vmInstrument);
        }

        [HttpPost]
        public ActionResult EncashmentForm(VM_Instrument instrument)
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            tbl_Instrument tblInstrument = new tbl_Instrument();
            tblInstrument = unitOfWork.InstrumentRepository.Get(x => x.InstrumentID == instrument.InstrumentID && oCode == x.OCode).FirstOrDefault();
            if (tblInstrument == null)
            {
                return Json(new { Success = false, ErrorMessage = "No data found to save!" }, JsonRequestBehavior.DenyGet);
            }
            if (tblInstrument.Closed == true)
            {
                return Json(new { Success = false, ErrorMessage = "Instrument already Closed!" }, JsonRequestBehavior.DenyGet);
            }

            //This amount must be treated. If this amount is negative that means the input amount is greater than the tblInstrument amount.
            //if it is positive then the amount is less than tblInstrument amount. 17/Apr/2016
            decimal dustAmount = tblInstrument.Amount - instrument.Amount;

            string instrumentLedger = tblInstrument.InstrumentType + "-" + tblInstrument.InstrumentNumber;
            var ledger = unitOfWork.ACC_LedgerRepository.Get(x => x.LedgerName == instrumentLedger).FirstOrDefault();
            Guid ledgerId = ledger.LedgerID;

            //Added by Asif 14 Feb 2017

            List<acc_Chart_of_Account_Maping> _acc_Chart_of_Account_Maping = unitOfWork.ChartofAccountMapingRepository.Get().ToList();
            //Asif End
            //Added by Izab Ahmed 20 July 2020
            decimal incomeTaxExpence = 0;

            var LedgerID = _acc_Chart_of_Account_Maping.Where(x => x.MIS_Id == 25 && x.OCode == oCode).Select(x => x.Ledger_Id).FirstOrDefault();
           
            
            string instrumentName = tblInstrument.InstrumentType + "-" + tblInstrument.InstrumentNumber;

            var entryVoucherList = unitOfWork.ACC_VoucherEntryRepository.Get().Where(x => x.TransactionDate >= tblInstrument.DepositDate && x.TransactionDate <= instrument.EncashmentDate && x.Narration.Contains(instrumentName)).ToList();

            foreach (var item in entryVoucherList)
            {
                var voucherDetail = unitOfWork.ACC_VoucherDetailRepository.Get().Where(x => x.VoucherID == item.VoucherID && x.LedgerID == LedgerID).FirstOrDefault();
                if (voucherDetail != null)
                {
                   incomeTaxExpence += voucherDetail.Debit ?? 0;
                }
            }
            
            //End by  Izab Ahmed
           

            tblInstrument.Closed = true;
            tblInstrument.TDSonFDR = instrument.TDSonFDR;
            //tblInstrument.FDRTax = instrument.FDRTax;
            tblInstrument.BankCharge = instrument.BankCharge;
            unitOfWork.InstrumentRepository.Update(tblInstrument);


            var editUser = unitOfWork.CustomRepository.GetUserID(User.Identity.Name);
            var editDate = DateTime.Now;
            DateTime transactionDate = instrument.EncashmentDate;

            acc_VoucherEntry vEntry = new acc_VoucherEntry();
            vEntry.TransactionDate = instrument.EncashmentDate;
            vEntry.VNumber = "EV-" //Commented by Shohid 24_09_2016 because DB field size nchar 11 //+ transactionDate.ToString("yy") + "-" + transactionDate.ToString("MM") + "-" 
                + unitOfWork.AccountingRepository.GetMaxVoucherTypeID(6, oCode).ToString().PadLeft(6, '0'); //let 6 for encashment voucher
            vEntry.VoucherID = unitOfWork.AccountingRepository.GetMaxVoucherID(oCode);
            vEntry.VoucherName = "";
            vEntry.VTypeID = 5; // encashment entry type voucher
            vEntry.EditDate = editDate;
            vEntry.EditUser = editUser;
            //vEntry.UsedProject = "PF";
            vEntry.OCode = oCode;
            vEntry.RestrictDelete = true;
            //vEntry.InstrumentID = tblInstrument.InstrumentID;
            vEntry.Narration = tblInstrument.InstrumentType + " Encashment ";
            unitOfWork.ACC_VoucherEntryRepository.Insert(vEntry);



            acc_VoucherDetail vDetail = new acc_VoucherDetail();
            vDetail.Credit = instrument.Amount;
            vDetail.Debit = 0;
            vDetail.VoucherDetailID = Guid.NewGuid();
            vDetail.LedgerID = ledger.LedgerID; //This instrument's Ledger ID
            vDetail.VoucherID = vEntry.VoucherID;
            vDetail.EditDate = editDate;
            vDetail.EditUser = editUser;
            vDetail.TransactionDate = instrument.EncashmentDate;
            vDetail.VTypeID = 5;  // 6 for encashment voucher
            //vDetail.UsedProject = "PF";
            vDetail.OCode = oCode;
            //vDetail.InstrumentID = tblInstrument.InstrumentID;
            unitOfWork.ACC_VoucherDetailRepository.Insert(vDetail);

            //Added by Asif 14 Feb 2017
            //for Accured Interest on investment
            vDetail = new acc_VoucherDetail();
            vDetail.Credit = instrument.EncashmentAmount;
            vDetail.Debit = 0;
            vDetail.VoucherDetailID = Guid.NewGuid();
            vDetail.LedgerID = _acc_Chart_of_Account_Maping.Where(x => x.MIS_Id == 16 && x.OCode == oCode).Select(x => x.Ledger_Id).FirstOrDefault();
            vDetail.VoucherID = vEntry.VoucherID;
            vDetail.EditDate = editDate;
            vDetail.EditUser = editUser;
            vDetail.TransactionDate = instrument.EncashmentDate;
            vDetail.VTypeID = 5;  // 6 for encashment voucher
            //vDetail.UsedProject = "PF";
            vDetail.OCode = oCode;
            //vDetail.InstrumentID = tblInstrument.InstrumentID;
            unitOfWork.ACC_VoucherDetailRepository.Insert(vDetail);


            //for Interest Income on investment
            vDetail = new acc_VoucherDetail();
            vDetail.Credit = instrument.InterestIncome;
            vDetail.Debit = 0;
            vDetail.VoucherDetailID = Guid.NewGuid();
            vDetail.LedgerID = _acc_Chart_of_Account_Maping.Where(x => x.MIS_Id == 22 && x.OCode == oCode).Select(x => x.Ledger_Id).FirstOrDefault();
            vDetail.VoucherID = vEntry.VoucherID;
            vDetail.EditDate = editDate;
            vDetail.EditUser = editUser;
            vDetail.TransactionDate = instrument.EncashmentDate;
            vDetail.VTypeID = 5;  // 6 for encashment voucher
            //vDetail.UsedProject = "PF";
            vDetail.OCode = oCode;
            //vDetail.InstrumentID = tblInstrument.InstrumentID;
            unitOfWork.ACC_VoucherDetailRepository.Insert(vDetail);



            //for Bank Charges
            vDetail = new acc_VoucherDetail();
            vDetail.Credit = 0;
            vDetail.Debit = instrument.BankCharge;
            vDetail.VoucherDetailID = Guid.NewGuid();
            vDetail.LedgerID = _acc_Chart_of_Account_Maping.Where(x => x.MIS_Id == 21 && x.OCode == oCode).Select(x => x.Ledger_Id).FirstOrDefault();
            vDetail.VoucherID = vEntry.VoucherID;
            vDetail.EditDate = editDate;
              vDetail.EditUser = editUser;
            vDetail.TransactionDate = instrument.EncashmentDate;
            vDetail.VTypeID = 5;  // 6 for encashment voucher
            //vDetail.UsedProject = "PF";
            vDetail.OCode = oCode;
            //vDetail.InstrumentID = tblInstrument.InstrumentID;
            unitOfWork.ACC_VoucherDetailRepository.Insert(vDetail);

            //for Tax
            vDetail = new acc_VoucherDetail();
            vDetail.Credit = 0;
            vDetail.Debit = instrument.FDRTax;
            vDetail.VoucherDetailID = Guid.NewGuid();
            vDetail.LedgerID = _acc_Chart_of_Account_Maping.Where(x => x.MIS_Id == 19 && x.OCode == oCode).Select(x => x.Ledger_Id).FirstOrDefault();
            vDetail.VoucherID = vEntry.VoucherID;
            vDetail.EditDate = editDate;
            vDetail.EditUser = editUser;
            vDetail.TransactionDate = instrument.EncashmentDate;
            vDetail.VTypeID = 5;  // 6 for encashment voucher
            //vDetail.UsedProject = "PF";
            vDetail.OCode = oCode;
            //vDetail.InstrumentID = tblInstrument.InstrumentID;
            unitOfWork.ACC_VoucherDetailRepository.Insert(vDetail);

            //for Other Charges
            vDetail = new acc_VoucherDetail();
            vDetail.Credit = 0;
            vDetail.Debit = instrument.TDSonFDR;
            vDetail.VoucherDetailID = Guid.NewGuid();
            vDetail.LedgerID = _acc_Chart_of_Account_Maping.Where(x => x.MIS_Id == 20 && x.OCode == oCode).Select(x => x.Ledger_Id).FirstOrDefault();
            vDetail.VoucherID = vEntry.VoucherID;
            vDetail.EditDate = editDate;
            vDetail.EditUser = editUser;
            vDetail.TransactionDate = instrument.EncashmentDate;
            vDetail.VTypeID = 5;  // 6 for encashment voucher
           // vDetail.UsedProject = "PF";
            vDetail.OCode = oCode;
            //vDetail.InstrumentID = tblInstrument.InstrumentID;
            unitOfWork.ACC_VoucherDetailRepository.Insert(vDetail);

            //Asif End

            //Added by Izab Ahmed 20/07/2020
            //for Income Tax Expence
            vDetail = new acc_VoucherDetail();
            vDetail.Credit = 0;
            vDetail.Debit = incomeTaxExpence;
            vDetail.VoucherDetailID = Guid.NewGuid();
            vDetail.LedgerID = _acc_Chart_of_Account_Maping.Where(x => x.MIS_Id == 25 && x.OCode == oCode).Select(x => x.Ledger_Id).FirstOrDefault();
            vDetail.VoucherID = vEntry.VoucherID;
            vDetail.EditDate = editDate;
            vDetail.EditUser = editUser;
            vDetail.TransactionDate = instrument.EncashmentDate;
            vDetail.VTypeID = 5;  // 6 for encashment voucher
            // vDetail.UsedProject = "PF";
            vDetail.OCode = oCode;
            //vDetail.InstrumentID = tblInstrument.InstrumentID;
            unitOfWork.ACC_VoucherDetailRepository.Insert(vDetail);


            //for Income Tax Provision
            vDetail = new acc_VoucherDetail();
            vDetail.Credit = incomeTaxExpence;   // income tax expence and income tax provision are equal/same amount.
            vDetail.Debit = 0;
            vDetail.VoucherDetailID = Guid.NewGuid();
            vDetail.LedgerID = _acc_Chart_of_Account_Maping.Where(x => x.MIS_Id == 17 && x.OCode == oCode).Select(x => x.Ledger_Id).FirstOrDefault();
            vDetail.VoucherID = vEntry.VoucherID;
            vDetail.EditDate = editDate;
            vDetail.EditUser = editUser;
            vDetail.TransactionDate = instrument.EncashmentDate;
            vDetail.VTypeID = 5;  // 6 for encashment voucher
            //vDetail.UsedProject = "PF";
            vDetail.OCode = oCode;
            //vDetail.InstrumentID = tblInstrument.InstrumentID;
            unitOfWork.ACC_VoucherDetailRepository.Insert(vDetail);

            //End by Izab Ahmed


            vDetail = new acc_VoucherDetail();
            vDetail.Credit = 0;
            //edited by Asif 14 Feb 2017

            //vDetail.Debit = instrument.Amount;
            vDetail.Debit = instrument.Amount + instrument.EncashmentAmount + instrument.InterestIncome - instrument.BankCharge - instrument.FDRTax - instrument.TDSonFDR;

            //End Asif
            vDetail.VoucherDetailID = Guid.NewGuid();
            vDetail.LedgerID = instrument.LedgerID ?? Guid.Empty; //Selected Ledger ID
            vDetail.VoucherID = vEntry.VoucherID;
            vDetail.EditDate = editDate;
            vDetail.EditUser = editUser;
            vDetail.TransactionDate = instrument.EncashmentDate;
            vDetail.VTypeID = 5;  // 6 for encashment voucher
            //vDetail.UsedProject = "PF";
            vDetail.OCode = oCode;
            unitOfWork.ACC_VoucherDetailRepository.Insert(vDetail);

            try
            {
                unitOfWork.Save();
                return Json(new { Success = true, Message = "Encashment Voucher Passed! Voucher Number: " + vEntry.VNumber }, JsonRequestBehavior.DenyGet);
            }
            catch (DbEntityValidationException ex)
            {
                string st = "";
                foreach (var error in ex.EntityValidationErrors)
                {
                    foreach (var ve in error.ValidationErrors)
                    {
                        //Console.WriteLine("\tProperty: {0}, Error: {1}",
                        //    ve.PropertyName, ve.ErrorMessage);
                        st += ve.ErrorMessage;
                    }
                }
                return Json(new { Success = false, ErrorMessage = "Problem Occure while saving the data." + st }, JsonRequestBehavior.DenyGet);
            }
            return Json(new { Success = false, ErrorMessage = "No data found to save!" }, JsonRequestBehavior.DenyGet);
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
                vEntry.VNumber = "EV-" + TransactionDate.ToString("yy") + "-" + TransactionDate.ToString("MM") + "-" + unitOfWork.AccountingRepository.GetMaxVoucherTypeID(6, OCode).ToString().PadLeft(6, '0'); //let 6 for encashment voucher                
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

    }
}
