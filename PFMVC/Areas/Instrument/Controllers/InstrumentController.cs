using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using CustomJsonResponse;
using DLL;
using DLL.Repository;
using DLL.ViewModel;
using Microsoft.Reporting.WebForms;
using PFMVC.common;

namespace PFMVC.Areas.Instrument.Controllers
{
    public class InstrumentController : Controller
    {
        UnitOfWork unitOfWork = new UnitOfWork();
        int PageID = 3;
        ReportDataSource rd;

        /// <summary>
        /// Indexes this instance.
        /// </summary>
        /// <returns></returns>
        /// <ReviewBY>Avishek</ReviewBY>
        /// <ReviewDate>Feb-24-2016</ReviewDate>
        [Authorize]
        public ActionResult Index(int instrumentID = 0)
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
                ViewBag.PageName = "Instrument";
                return View("Unauthorized");
            }
            //Create(instrumentID);
            
            ViewBag.instrumentId = instrumentID;
           
            //End
            return View();
        }

        public void RenewInstrument(int instrumentID = 0)
        {
            Create(instrumentID);
        }

        public void LedgerOptions(string s = "")
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            ViewData["LedgerOptions"] = new SelectList(unitOfWork.ACC_LedgerRepository.Get().Where(f => string.IsNullOrEmpty(f.PFMemberID) && f.OCode == oCode), "LedgerID", "LedgerName", s);
        }

        //Added by Shohid 24 Aug 2016
        /// <summary>
        /// AutocompleteSuggestions for Renew Instrument in create instrument page
        /// </summary>
        /// <param name="term"></param>
        /// <ReviewBY>Shohid</ReviewBY>
        /// <ReviewDate>24 Aug 2016</ReviewDate>
        /// <returns></returns>
        
        public JsonResult AutocompleteSuggestions(string term)
        {
            try
            {
                int oCode = ((int?)Session["OCode"]) ?? 0;
                var suggestions = unitOfWork.InstrumentRepository.Get(w => w.InstrumentType.ToLower().Trim().Contains(term.ToLower().Trim()) && w.OCode == oCode && w.InstrumentNumber != null).Select(s => new { value = s.InstrumentID, label = s.InstrumentType+"-"+s.InstrumentNumber}).ToList();
                return Json(suggestions, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, ErrorMessage = "Some problem in autocompelet"+ ex }, JsonRequestBehavior.DenyGet);
            }
        }

        /// <summary>
        /// Creates the specified instrument identifier.
        /// </summary>
        /// <param name="instrumentID">The instrument identifier.</param>
        /// <returns></returns>
        /// <ReviewBY>Avishek</ReviewBY>
        /// <ReviewDate>Feb-24-2016</ReviewDate>
        [Authorize]
        public ActionResult Create(int instrumentID = 0)
        {
            //Added By Avishek Date:Jan-19-2016
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End
            if (instrumentID == 0)
            {
                LedgerOptions();
                return PartialView("InstrumentForm", new VM_Instrument());
            }
            var v = unitOfWork.InstrumentRepository.GetByID(instrumentID);
            VM_Instrument instrument = new VM_Instrument();
            instrument.InstrumentID = v.InstrumentID;
            instrument.Amount = v.Amount;
            instrument.Branch = v.Branch;
            instrument.Comment = v.Comment;
            instrument.DepositDate = v.DepositDate;
            instrument.EditDate = DateTime.Now;
            instrument.EditUserName = v.EditUserName;
            instrument.Institution = v.Institution;
            instrument.InstrumentNumber = v.InstrumentNumber;
            instrument.InstrumentType = v.InstrumentType;
            instrument.InterestRate = v.InterestRate;
            instrument.MaturityDate = v.MaturityDate;
            instrument.MaturityPeriod = v.MaturityPeriod;
            instrument.LedgerID = v.AccLedgerID;  
            instrument.BankCharge = v.BankCharge ?? 0;
            instrument.TDSonFDR = v.TDSonFDR ?? 0;
            instrument.FDRTax = v.FDRTax ?? 0;
            instrument.EntryForPF = v.EntryForPF;
            instrument.Closed = false;
            LedgerOptions(instrument.LedgerID + "");

            // Added by Shohid 24 Aug 2016
            instrument.Renew = v.PreviousInstrumentId != null ? true : false;
            instrument.RenewInstrumentID = v.PreviousInstrumentId;
            if (v.PreviousInstrumentId != null)
            {
                var s = unitOfWork.InstrumentRepository.GetByID(v.PreviousInstrumentId);
                ViewBag.instrumentName = s.InstrumentType + "-" + s.InstrumentNumber;
            }
          

            return PartialView("InstrumentForm", instrument);
        }

        /// <summary>
        /// Creates the specified v.
        /// </summary>
        /// <param name="v">The v.</param>
        /// <returns></returns>
        /// <ModifyBy>Avishek</ModifyBy>
        /// <ModificationDate>Feb-24-2016</ModificationDate>
       
        [Authorize]
        [HttpPost]
        public ActionResult Create(VM_Instrument v)
        {
            bool b = PagePermission.HasPermission(User.Identity.Name, PageID, 1);
            if (!b) return Json(new { Success = false, ErrorMessage = "You are not allowed to Edit/Create information! contact system admin!" }, JsonRequestBehavior.AllowGet);

            //Added By Avishek Date:Jan-19-2016
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End
            string message = "";
            if (ModelState.IsValid)
            {
                var isExist = unitOfWork.InstrumentRepository.Get(w => w.InstrumentType == v.InstrumentType && w.InstrumentNumber == v.InstrumentNumber && w.InstrumentID != v.InstrumentID && w.OCode == oCode).SingleOrDefault();
                if (isExist == null)
                {
                    tbl_Instrument instrument;
                    //create
                    if (v.InstrumentID == 0)
                    {
                        instrument = new tbl_Instrument();
                        var maxInstrumentId = unitOfWork.InstrumentRepository.Get(m => m.OCode == oCode).Max(m => (int?)m.InstrumentID) ?? 0;
                        instrument.InstrumentID = maxInstrumentId + 1;
                    }
                    //edit
                    else if (v.InstrumentID > 0)
                    {
                        instrument = unitOfWork.InstrumentRepository.GetByID(v.InstrumentID);
                        if (instrument.PassVoucher == true)
                        {
                            instrument.PassVoucher = false;
                            //if (DeleteInstrumentFromAccounting(instrument.InstrumentID, ref message))
                            //{
                            //    instrument.PassVoucher = false;
                              message += "This instrument previously passed but you have to pass it again if you edit this data!";
                            //}
                            //else
                            //{
                            //    return Json(new { Success = false, ErrorMessage = "VOUCHER PASSED AND ERROR FOUND WHILE DELETING PREVIOUS DATA: " + message }, JsonRequestBehavior.DenyGet);
                            //}
                        }
                    }
                    else
                    {
                        return Json(new { Success = false, ErrorMessage = "Instrument id " + v.InstrumentID + " is not accepted!" }, JsonRequestBehavior.DenyGet);
                    }

                    //Start of Accrude Interest Calculation //By Fahim //Date 06/06/2016 

                    if (v.DepositDate == null)
                    {
                        return Json(new { Success = false, ErrorMessage = "Please Select Deposite Date!" }, JsonRequestBehavior.DenyGet);
                    }
                    DateTime _depositeDate = v.DepositDate ?? DateTime.MinValue;
                    
                    v.MaturityDate = _depositeDate.AddMonths(v.MaturityPeriod);
                    DateTime _maturityDate = v.MaturityDate ?? DateTime.MinValue;
                    int _totalDays = (int)(_maturityDate - _depositeDate).TotalDays;
                    decimal accrudeInterest = Math.Round(((_totalDays * v.InterestRate * v.Amount) / (36500)), 6);
                    if (instrument.MaturityPeriod > 71)
                    {
                        accrudeInterest = instrument.Amount;
                    }

                    //End of Accrude Interest Calculation

                    try
                    {
                        instrument.InstrumentType = v.InstrumentType.ToUpper();
                        instrument.InstrumentNumber = v.InstrumentNumber;
                        instrument.InterestRate = v.InterestRate;
                        instrument.Amount = v.Amount;
                        instrument.Branch = v.Branch;
                        instrument.Comment = v.Comment;
                        instrument.DepositDate = v.DepositDate ?? DateTime.Now; //It's will be only DateTime after Back Dated data entry
                        instrument.EditDate = DateTime.Now;
                        instrument.EditUser = unitOfWork.CustomRepository.GetUserID(User.Identity.Name);
                        instrument.EditUserName = User.Identity.Name;
                        instrument.Institution = v.Institution;
                        instrument.MaturityDate = v.MaturityDate;
                        instrument.MaturityPeriod = v.MaturityPeriod;
                        instrument.AcruadIntarestAmount = accrudeInterest;
                        instrument.OCode = oCode;
                        instrument.Closed = false;
                        instrument.EntryForPF = "Y";
                        instrument.BankCharge = v.BankCharge;
                        instrument.TDSonFDR = v.TDSonFDR;
                        instrument.FDRTax = v.FDRTax;
                        
                        instrument.PassVoucherMessage = "M";
                        decimal perdayAccruedIntarest = (((instrument.InterestRate * instrument.Amount) / 100) / 365);
                        instrument.AccLedgerID = v.LedgerID ?? null;
                        instrument.PreviousInstrumentId = v.RenewInstrumentID;
                        if (v.InstrumentID == 0)
                        {
                            unitOfWork.InstrumentRepository.Insert(instrument);
                        }
                        else
                        {
                            unitOfWork.InstrumentRepository.Update(instrument);
                        }

                        //added by izab 09 April 2020

                        //if (v.CheckBoxValue && v.NoofPeriod > 0)
                        //{
                        //DateTime previousCouponDae = DateTime.Now;
                        var _year = v.MaturityPeriod / 12;
                        string interestRate = Session["InterestRate"].ToString();
                        string[] rateString = new string[100];
                        rateString = interestRate.Split(',');

                        List<tbl_Instrument_Interest_Rate> intlist = unitOfWork.InstrumentInterestRateRepository.Get().Where(id => id.InstrumentID == instrument.InstrumentID).ToList();
                        foreach (tbl_Instrument_Interest_Rate item in intlist)
                        {
                            unitOfWork.InstrumentInterestRateRepository.Delete(item);
                        }

                        if (rateString.Length - 1 == _year - 1)
                        {
                            DateTime StartDate = v.DepositDate ?? DateTime.Now;
                            for (int i = 0; i <= (Decimal)_year - 1; i++)
                            {
                                tbl_Instrument_Interest_Rate instrumentRate = new tbl_Instrument_Interest_Rate();
                                instrumentRate.InstrumentID = instrument.InstrumentID;

                                if (i > 0)
                                {
                                    var _interestRate = Convert.ToDecimal(rateString[i - 1]);
                                    instrumentRate.InterestRate = _interestRate;
                                    instrumentRate.InterestYear = i + 1;
                                }
                                else
                                {
                                    instrumentRate.InterestRate = v.InterestRate;
                                    instrumentRate.InterestYear = 1;
                                }
                                instrumentRate.EditUser = unitOfWork.CustomRepository.GetUserID(User.Identity.Name);
                                instrumentRate.EditDate = DateTime.Now;
                                instrumentRate.StartDate = StartDate;
                                instrumentRate.EndDate = instrumentRate.StartDate.AddDays(-1).AddYears(1);
                                unitOfWork.InstrumentInterestRateRepository.Insert(instrumentRate);
                                StartDate = instrumentRate.EndDate.AddDays(1);
                            }

                        }
                        else
                        {
                            return Json(new { Success = false, Message = "Don't save No of Year and no of Rate are not same" + message, instrument.InstrumentID }, JsonRequestBehavior.DenyGet);
                        }
                        //}

                        //End by izab

                        unitOfWork.Save();

                        if (v.Renew == false)
                        {
                            var ledger = unitOfWork.ACC_LedgerRepository.GetByID(instrument.AccLedgerID);

                            var isLedgerExist = unitOfWork.ACC_LedgerRepository.Get(f => f.LedgerName == instrument.InstrumentType + "-" + instrument.InstrumentNumber && f.OCode == oCode).FirstOrDefault();

                            if (isLedgerExist != null)
                            {

                                string narration = instrument.InstrumentType + "-" + instrument.InstrumentNumber;

                                unitOfWork.CustomRepository.DeleteVDetailsVEntryLedgInterest(narration, instrument.InstrumentID);
                                unitOfWork.Save();
                            }
                        }
                        

                        return Json(new { Success = true, Message = "Instrument information updated! " + message, InstrumentID = instrument.InstrumentID }, JsonRequestBehavior.DenyGet);
                    }
                    catch (Exception x)
                    {
                        return Json(new { Success = false, ErrorMessage = "Error : " + x.Message }, JsonRequestBehavior.DenyGet);
                    }
                }
                return Json(new { Success = false, ErrorMessage = "Same instrument type and number previously exist!" }, JsonRequestBehavior.DenyGet);
            }
            var errorList = (from item in ModelState.Values
                             from error in item.Errors
                             select error.ErrorMessage).ToList();

            return Json(JsonResponseFactory.ErrorResponse(errorList), JsonRequestBehavior.DenyGet);
        }

        /// <summary>
        /// Instruments the list.
        /// </summary>
        /// <returns></returns>
        /// <ReviewBY>Avishek</ReviewBY>
        /// <ReviewDate>Feb-24-2016</ReviewDate>
        public ActionResult InstrumentList()
        {
            //Added By Avishek Date:Jan-19-2016
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End
            var v = unitOfWork.InstrumentRepository.Get().Where(x => x.OCode == oCode).ToList();
            return PartialView("InstrumentList", v);
        }

        //this method will hit accounts
        /// <summary>
        /// Passes the voucher.
        /// </summary>
        /// <param name="instrumentID">The instrument identifier.</param>
        /// <returns></returns>
        /// <ModifyBy>Avishek</ModifyBy>
        /// <ModificationDate>Feb-24-2016</ModificationDate>
        /// 
       
        public ActionResult PassVoucher(int instrumentID)
        {
            bool b = PagePermission.HasPermission(User.Identity.Name, PageID, 1);
            if (!b) return Json(new { Success = false, ErrorMessage = "You are not allowed to Pass information! contact system admin!" }, JsonRequestBehavior.AllowGet);
            int voucherId = 0;
            //Added By Avishek Date:Jan-19-2016
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End

            var instrument = unitOfWork.InstrumentRepository.GetByID(instrumentID);
            if (instrument == null)
            {
                return Json(new { Success = false, ErrorMessage = "This instrument object not found!!!" }, JsonRequestBehavior.DenyGet);
            }
            if (instrument.PassVoucher)
            {
                return Json(new { Success = false, ErrorMessage = "Voucher previously passed by " + instrument.EditUserName + " at " + instrument.EditDate }, JsonRequestBehavior.DenyGet);
            }

            string rMessage = "";
            string curUserName = User.Identity.Name;
            Guid curUserId = unitOfWork.CustomRepository.GetUserID(User.Identity.Name);
            if (instrument.AccLedgerID == null)
            {
                //No ledger id means it will create with initial balance as ledger.
                instrument.PassVoucher = unitOfWork.AccountingRepository.CreateAccountingLedgerEntry("", instrument.InstrumentType + "-" + instrument.InstrumentNumber, "Investment", "Debit", instrument.Amount, "This is Instrument openting balance. Instrument Type:" + instrument.InstrumentType + " & Instrument Number:" + instrument.InstrumentNumber + ".", ref rMessage, curUserName, curUserId, "", "", true, oCode);
            }
            else
            {
                var isLedgerExist = unitOfWork.ACC_LedgerRepository.Get(f => f.LedgerName == instrument.InstrumentType + "-" + instrument.InstrumentNumber && f.OCode == oCode).FirstOrDefault();
                if (isLedgerExist == null)
                {
                    bool isCreated = unitOfWork.AccountingRepository.CreateAccountingLedgerEntry("", instrument.InstrumentType + "-" + instrument.InstrumentNumber, "Investment", "", 0, "New Ledger Name", ref rMessage, curUserName, curUserId, "", "", true, oCode);
                    if (!isCreated)
                    {
                        return Json(new { Success = false, ErrorMessage = rMessage }, JsonRequestBehavior.DenyGet);
                    }
                    try
                    {
                        //if no error found proceed to save 
                        unitOfWork.Save();
                    }
                    catch (Exception x)
                    {
                        return Json(new { Success = false, ErrorMessage = "Ledger: " + instrument.InstrumentType + "-" + instrument.InstrumentNumber + " creation failed! " + x.Message }, JsonRequestBehavior.DenyGet);
                    }
                }
                else
                {
                    unitOfWork.Save();
                }
                //Voucher will be created
                List<Guid> ledgerIdList = new List<Guid>();
                List<string> ledgerNameList = new List<string>();
                List<decimal> credit = new List<decimal>();
                List<decimal> debit = new List<decimal>();
                List<string> chqNumber = new List<string>();
                List<string> pfLoanId = new List<string>();
                List<string> pfMemberId = new List<string>();

                //remember diff btwn empID and identification number
                //Account should be exist with Each Identification Number 

                //ledgerNameList.Add(instrument.InstrumentType + "-" + instrument.InstrumentNumber); //this is convention
                string ledgerName = instrument.InstrumentType + "-" + instrument.InstrumentNumber; //this is convention
                Guid ledgerId = unitOfWork.ACC_LedgerRepository.Get(x => x.LedgerName.Trim().Contains(ledgerName.Trim())).Select(x => x.LedgerID).FirstOrDefault();
                ledgerIdList.Add(ledgerId);
                ledgerNameList.Add(ledgerName);

                //members fund should be credited!
                credit.Add(0);
                //On the other hand debit should be zero.
                debit.Add(instrument.Amount);
                chqNumber.Add("");
                pfLoanId.Add("");
                pfMemberId.Add("");


                var ledger = unitOfWork.ACC_LedgerRepository.GetByID(instrument.AccLedgerID);
                ledgerNameList.Add(ledger.LedgerName);
                ledgerIdList.Add(ledger.LedgerID);

                credit.Add(instrument.Amount);
                debit.Add(0);
                chqNumber.Add("");
                pfLoanId.Add("");
                pfMemberId.Add("");
                //Edited by me 
                instrument.PassVoucher = unitOfWork.AccountingRepository.DualEntryVoucher(0, 5, instrument.DepositDate, ref voucherId, "FDR name :" + instrument.InstrumentType + "-" + instrument.InstrumentNumber + " under: " + ledger.LedgerName, ledgerNameList, debit, credit, chqNumber, ref rMessage, User.Identity.Name, unitOfWork.CustomRepository.GetUserID(User.Identity.Name), pfMemberId, "", "", "", null, pfLoanId, oCode, "Instrument");
                // instrument.DepositDate replaced by DateTime.Now after back datted entry
                //Accured VoucherPass for edit
                AccruedVoucherPass(instrument, oCode);
            
            }
            instrument.PassVoucherMessage = rMessage;
            if (instrument.PassVoucher)
            {
                //unitOfWork.InstrumentRepository.Update(instrument);
                try
                {
                    instrument.AccVoucherEntryID = voucherId;
                    unitOfWork.Save();
                    ViewBag.Message = "See the transaction log... If any error found try again!";
                    return Json(new { Success = true, Message = "Voucher successfully passed!" }, JsonRequestBehavior.DenyGet);
                }
                catch (Exception x)
                {
                    return Json(new { Success = false, ErrorMessage = "Error: " + x.Message }, JsonRequestBehavior.DenyGet);
                }
            }
            return Json(new { Success = false, ErrorMessage = "Error: " + instrument.PassVoucherMessage }, JsonRequestBehavior.DenyGet);
        }

        /// <summary>
        /// Accrueds the voucher pass.
        /// </summary>
        /// <param name="instrument">The instrument.</param>
        /// <param name="oCode">The o code.</param>
        /// <CreatedBy>Avishek</CreatedBy>
        /// <CreatedDate>23-Feb-2016</CreatedDate>

        private void AccruedVoucherPass(tbl_Instrument instrument, int oCode)
        {
            int voucherId = 0;
            string rMessage = "";
            string curUserName = User.Identity.Name;
            Guid curUserId = unitOfWork.CustomRepository.GetUserID(User.Identity.Name);

            List<Guid> ledgerIdList = new List<Guid>();
            List<decimal> credit = new List<decimal>();
            List<decimal> debit = new List<decimal>();
            List<string> chqNumber = new List<string>();
            List<string> pfLoanId = new List<string>();
            List<string> pfMemberId = new List<string>();

            //No ledger id means it will create with initial balance as ledger.
            string groupName = "Investment";

            instrument.PassVoucher = unitOfWork.AccountingRepository.CreateAccountingLedgerEntry("",
                instrument.InstrumentType + "-" + instrument.InstrumentNumber, groupName, "Debit", 0,
                "This is Instrument openting balance. Instrument Type:" + instrument.InstrumentType + " & Instrument Number:" +
                instrument.InstrumentNumber + ".", ref rMessage, curUserName, curUserId, "", "", true, oCode);
            unitOfWork.Save();

            string ledgerName = instrument.InstrumentType + "-" + instrument.InstrumentNumber; //this is convention

            int totalDay = Convert.ToInt32(((DateTime)instrument.MaturityDate - instrument.DepositDate).TotalDays);
            int totalYear = (instrument.MaturityDate.Value.Year - instrument.DepositDate.Year);
            //var acruadIntarestAmount1 = instrument.AcruadIntarestAmount;
            decimal amount = 0;
            decimal PreviousYearlyAccuredInterest = 0;
            DateTime previousEndDate = DateTime.Now;
            if (instrument.PassVoucherMessage == "M")
            {
                var interestRateList = unitOfWork.InstrumentInterestRateRepository.Get().Where(x => x.InstrumentID == instrument.InstrumentID).ToList();
                int count = 1;
                foreach (var item in interestRateList)
                {
                    DateTime endDate = new DateTime(item.StartDate.Year, item.StartDate.Month, 1).AddMonths(1).AddDays(-1);
                    DateTime startDate = item.StartDate;
                    decimal _interest = 0;
                    int j = 0;


                    decimal YearlyIntarestAmount = Math.Round(((instrument.Amount * item.InterestRate) / 100), 4);


                    DateTime endYearDate = item.EndDate;


                    double year = ((previousEndDate - instrument.DepositDate).Days +1) / 365;

                    int YearlytotalDay = (int)Math.Truncate(year) * 365;

                    //AccuredInterestMIS(instrument, oCode, endDate, totalDayofMonth);
                    if (amount != 0)
                    {
                        decimal accruedInterest = (((YearlyIntarestAmount / 365) * YearlytotalDay) - (amount + PreviousYearlyAccuredInterest));
                        //AccuredInterestMIS(instrument, oCode, endYearDate, totalYearlyDay, YearlytotalDay, amount, instrument.AcruadIntarestAmount);
                        AccuredInterestMIS(instrument, oCode, endYearDate, count, accruedInterest);
                        count++;
                        decimal PreviousYearlyAccuredInterest1 = PreviousYearlyAccuredInterest;
                        PreviousYearlyAccuredInterest = accruedInterest + PreviousYearlyAccuredInterest1;

                    }
                    var tax = Math.Round(((YearlyIntarestAmount * 15) / 100), 4);

                    var TotalAmount = Math.Round((instrument.Amount + YearlyIntarestAmount - tax), 4);

                    int totalDayofMonth = Convert.ToInt32((endDate - startDate).TotalDays + 1);
                    
                    int totalDayOfPerYearly = Convert.ToInt32((item.EndDate - item.StartDate).TotalDays + 1);
                    

                    int dayOfYear = new DateTime(startDate.Year, 12, 31).DayOfYear;

                    decimal interestCreditAmount;
                    decimal taxCreditAmount;
                    while (j < totalDayOfPerYearly)
                    {
                        Guid ledgerId = unitOfWork.ChartofAccountMapingRepository1.Get(x => x.MIS_Id == 16).Select(x => x.Ledger_Id).FirstOrDefault();
                        Guid _ledgerId = unitOfWork.ChartofAccountMapingRepository1.Get(x => x.MIS_Id == 23).Select(x => x.Ledger_Id).FirstOrDefault();
                        Guid interestonLedgerId = unitOfWork.ChartofAccountMapingRepository1.Get(x => x.MIS_Id == 14).Select(x => x.Ledger_Id).FirstOrDefault();
                        Guid _interestonLedgerId = unitOfWork.ChartofAccountMapingRepository1.Get(x => x.MIS_Id == 24).Select(x => x.Ledger_Id).FirstOrDefault();
                        Guid incomeTaxProvisionLedgerId = unitOfWork.ChartofAccountMapingRepository1.Get(x => x.MIS_Id == 17).Select(x => x.Ledger_Id).FirstOrDefault();
                        Guid incomeTaxExpenceLedgerId = unitOfWork.ChartofAccountMapingRepository1.Get(x => x.MIS_Id == 25).Select(x => x.Ledger_Id).FirstOrDefault();




                        int totalDayofMonth1 = Convert.ToInt32((endDate - startDate).TotalDays + 1);

                        if (totalDayofMonth1 == 29)
                        {
                            int temp = totalDayofMonth1;
                            totalDayofMonth1 = 28;
                            interestCreditAmount = Math.Round((((decimal)YearlyIntarestAmount * totalDayofMonth1) / 365));
                            taxCreditAmount = Math.Round((((decimal)tax * totalDayofMonth1) / 365));
                            totalDayofMonth1 = temp;
                        }
                        else
                        {
                            interestCreditAmount = Math.Round((((decimal)YearlyIntarestAmount * totalDayofMonth1) / 365));
                            taxCreditAmount = Math.Round((((decimal)tax * totalDayofMonth1) / 365));
                        }

                        //decimal interestCreditAmount = Math.Round((((decimal)instrument.AcruadIntarestAmount * totalDayofMonth1) / 365));

                        //decimal taxCreditAmount = Math.Round((((decimal)tax * totalDayofMonth1) / 365));

                        //decimal interestDebitAmount = Math.Round((interestCreditAmount - taxCreditAmount), 4);


                        decimal interestDebitAmount = Math.Round(interestCreditAmount);
                        decimal incomeTaxExpence = Math.Round((interestCreditAmount* instrument.FDRTax??0)/100);
                        decimal incomeTaxProvision = Math.Round((interestCreditAmount * instrument.FDRTax ?? 0) / 100);


                        if (instrument.InstrumentType=="BSP")
                        {
                            ledgerIdList.Add(_ledgerId);
                            debit.Add(interestDebitAmount);
                            credit.Add(0);
                            chqNumber.Add("");
                            pfLoanId.Add("");
                            pfMemberId.Add("");
                            voucherId = 0;


                            ledgerIdList.Add(incomeTaxExpenceLedgerId);
                            debit.Add(incomeTaxExpence);
                            credit.Add(0);
                            chqNumber.Add("");
                            pfLoanId.Add("");
                            pfMemberId.Add("");
                            


                            ledgerIdList.Add(_interestonLedgerId);
                            credit.Add(interestCreditAmount);
                            debit.Add(0);
                            chqNumber.Add("");
                            pfLoanId.Add("");
                            pfMemberId.Add("");


                            ledgerIdList.Add(incomeTaxProvisionLedgerId);
                            credit.Add(incomeTaxProvision);
                            debit.Add(0);
                            chqNumber.Add("");
                            pfLoanId.Add("");
                            pfMemberId.Add("");

                        }
                        else
                        {
                            ledgerIdList.Add(ledgerId);
                            credit.Add(0);
                            debit.Add(interestDebitAmount);
                            chqNumber.Add("");
                            pfLoanId.Add("");
                            pfMemberId.Add("");
                            voucherId = 0;

                            ledgerIdList.Add(incomeTaxExpenceLedgerId);
                            debit.Add(incomeTaxExpence);
                            credit.Add(0);
                            chqNumber.Add("");
                            pfLoanId.Add("");
                            pfMemberId.Add("");


                            ledgerIdList.Add(interestonLedgerId);
                            credit.Add(interestCreditAmount);
                            debit.Add(0);
                            chqNumber.Add("");
                            pfLoanId.Add("");
                            pfMemberId.Add("");

                            ledgerIdList.Add(incomeTaxProvisionLedgerId);
                            credit.Add(incomeTaxProvision);
                            debit.Add(0);
                            chqNumber.Add("");
                            pfLoanId.Add("");
                            pfMemberId.Add("");
                        }
                        

                        //ledgerIdList.Add(taxLedgerId);
                        //debit.Add(taxCreditAmount);

                        //credit.Add(0);
                        //chqNumber.Add("");
                        //pfLoanId.Add("");
                        //pfMemberId.Add("");

                        instrument.PassVoucher = unitOfWork.AccountingRepository.DualEntryVoucherByIdForInvestment(0, 5, endDate,
                            ref voucherId, "Accured Interest on " + instrument.InstrumentType + "-" + instrument.InstrumentID + " under: " +
                            ledgerName, ledgerIdList, debit, credit, chqNumber, ref rMessage, User.Identity.Name,
                            unitOfWork.CustomRepository.GetUserID(User.Identity.Name), pfMemberId, "Accured Interest", "", "", null, pfLoanId,
                            instrument.InstrumentID, oCode, "Accrued interest Month wise");
                        j += totalDayofMonth1;
                        startDate = endDate.AddDays(1);
                        endDate = new DateTime(startDate.AddDays(1).Year, startDate.AddDays(1).Month, 1).AddMonths(1).AddDays(-1);
                        if (endDate.Month == item.EndDate.Month && endDate.Year == item.EndDate.Year)
                        {
                            endDate = item.EndDate;
                            previousEndDate = item.EndDate;
                        }

                        var _totalInterestAmount = amount + interestCreditAmount;
                        amount = _totalInterestAmount;

                        ledgerIdList.Clear();
                        credit.Clear();
                        debit.Clear();
                        chqNumber.Clear();
                        pfLoanId.Clear();
                        pfMemberId.Clear();
                    }

                    decimal totalInterest = _interest + YearlyIntarestAmount;
                    _interest = totalInterest;
                    startDate = new DateTime(endDate.Year, endDate.Month, 1);

                    var lastYearTotalMonth = (((instrument.MaturityDate.Value.Year - startDate.Year) * 12) + endYearDate.Month - startDate.Month) + 1;

                    //if (lastYearTotalMonth == 12)
                    //{
                    //    instrument.AcruadIntarestAmount = acruadIntarestAmount1 - totalInterest;
                    //}
                    //else
                    //{
                    //    instrument.AcruadIntarestAmount = Math.Round(((instrument.Amount * item.InterestRate) / 100), 4);
                    //}

                    if (endDate >= instrument.MaturityDate)
                    {
                        endDate = (DateTime)instrument.MaturityDate;
                    }

                }

                
            }
        }

        //private void AccuredInterestMIS(tbl_Instrument instrument, int oCode, DateTime endDate, int totalDayofMonth)
        //{
        //    tbl_Instrument_Accured_Interest accuredInterest = new tbl_Instrument_Accured_Interest();
        //    accuredInterest.InstrumentID = instrument.InstrumentID;
        //    accuredInterest.AccuredInterestDate = endDate;
        //    accuredInterest.AccuredInterestAmount = (decimal)instrument.AcruadIntarestAmount * totalDayofMonth;
        //    accuredInterest.EditDate = endDate;
        //    accuredInterest.EditUser = unitOfWork.CustomRepository.GetUserID(User.Identity.Name);
        //    accuredInterest.OCode = oCode;
        //    accuredInterest.UsedProject = "PF";
        //    unitOfWork.InstrumentAccuredInterestRepository.Insert(accuredInterest);
        //}

        // added by Izab 15 April 2020
        
        private void AccuredInterestMIS(tbl_Instrument instrument, int oCode, DateTime endDate, int count, decimal accruedInterest)
        {
            if (count == 1)
            {
                instrument.AcruadIntarestAmount = Math.Round(accruedInterest);
            }
            else
            {
                tbl_Instrument_Accured_Interest accuredInterest = new tbl_Instrument_Accured_Interest();
                accuredInterest.InstrumentID = instrument.InstrumentID;
                accuredInterest.AccuredInterestDate = endDate;
                // accuredInterest.AccuredInterestAmount = ((((decimal)instrument.AcruadIntarestAmount / totalYearlyDay) * YearlytotalDay) - amount);
                accuredInterest.AccuredInterestAmount = accruedInterest;
                accuredInterest.EditDate = endDate;
                accuredInterest.EditUser = unitOfWork.CustomRepository.GetUserID(User.Identity.Name);
                accuredInterest.OCode = oCode;
                accuredInterest.UsedProject = "PF";
                unitOfWork.InstrumentAccuredInterestRepository.Insert(accuredInterest);
            }
        }
        //End by Izab

        /// <summary>
        /// Deletes the instrument confirm.
        /// </summary>
        /// <param name="instrumentID">The instrument identifier.</param>
        /// <returns></returns>
        /// <ModifyBy>Avishek</ModifyBy>
        /// <ModificationDate>Feb-24-2016</ModificationDate>
        public ActionResult DeleteInstrumentConfirm(int instrumentID)
        {
            bool b = PagePermission.HasPermission(User.Identity.Name, PageID, 2);
            if (!b) return Json(new { Success = false, ErrorMessage = "You are not allowed to Delete information! contact system admin!" }, JsonRequestBehavior.AllowGet);

            //Added By Avishek Date:Jan-19-2016
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End

            string message = "";

            if (DeleteInstrumentFromAccounting(instrumentID, ref message))
            {
                unitOfWork.InstrumentRepository.Delete(instrumentID);
                try
                {
                    unitOfWork.Save();
                    return Json(new { Success = true, Message = "Instrument successfully deleted!" }, JsonRequestBehavior.DenyGet);
                }
                catch (Exception x)
                {
                    return Json(new { Success = false, ErrorMessage = "DATA DELETED FROM ACCOUNTING AND " + x.Message }, JsonRequestBehavior.DenyGet);
                }
            }
            return Json(new { Success = false, ErrorMessage = "ERROR WHILE DELETING INSTRUMENT FROM ACCOUNTING : " + message }, JsonRequestBehavior.DenyGet);
        }

        /// <summary>
        /// Deletes the instrument from accounting.
        /// </summary>
        /// <param name="instrumentID">The instrument identifier.</param>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        /// <ModifyBy>Avishek</ModifyBy>
        /// <ModificationDate>Feb-24-2016</ModificationDate>
        public bool DeleteInstrumentFromAccounting(int instrumentID, ref string message)
        {
            var v = unitOfWork.InstrumentRepository.GetByID(instrumentID);
            if (v != null)
            {
                if (v.PassVoucher)
                {
                    var instrumentLedger = unitOfWork.ACC_LedgerRepository.Get(f => f.LedgerName == v.InstrumentType + "-" + v.InstrumentNumber).FirstOrDefault();
                    if (instrumentLedger != null)
                    {
                        var relatedVoucherIdList = unitOfWork.ACC_VoucherDetailRepository.Get(f => f.LedgerID == instrumentLedger.LedgerID).Select(s => s.VoucherID).ToList();
                        var relatedVoucherEntryList = unitOfWork.ACC_VoucherEntryRepository.Get(f => relatedVoucherIdList.Contains(f.VoucherID)).ToList();
                        var relatedVoucherDetailList = unitOfWork.ACC_VoucherDetailRepository.Get(f => relatedVoucherIdList.Contains(f.VoucherID)).ToList();
                        relatedVoucherDetailList.ForEach(f => unitOfWork.ACC_VoucherDetailRepository.Delete(f));
                        relatedVoucherEntryList.ForEach(f => unitOfWork.ACC_VoucherEntryRepository.Delete(f));
                        unitOfWork.ACC_LedgerRepository.Delete(instrumentLedger);
                    }
                }
                message = "Voucher not yet passed!!!";
                try
                {
                    unitOfWork.Save();
                    return true;
                }
                catch (Exception x)
                {
                    message = "Error Occured - " + x.Message;
                    return false;
                }
            }
            message = "object not found!";
            return false;
        }

        /// <summary>
        /// Accrudes the interest.
        /// </summary>
        /// <returns>view</returns>
        /// <CreatedBy>Fahim</CreatedBy>
        /// <DateofCreation>Jun-5-2016</DateofCreation>
        public ActionResult AccrudeInterest()
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            bool b = PagePermission.HasPermission(User.Identity.Name, PageID, 0);
            if (!b)
            {
                ViewBag.PageName = "Instrument";
                return View("Unauthorized");
            }
            return View();
        }

        // added by Izab 07 April 2020
        public JsonResult GetInterestRate(string interestRate)
        {
            Session["InterestRate"] = interestRate;
            return Json("OK");
        }
        //End by Izab
        public ActionResult GetInterestList(int instrumentID)
        {
            try
            {
                List<VM_InterestRateViewModel> aList = new List<VM_InterestRateViewModel>();
                VM_InterestRateViewModel obj;
                List<tbl_Instrument_Interest_Rate> interestRate = new List<tbl_Instrument_Interest_Rate>();

                List<tbl_Instrument_Interest_Rate> result = unitOfWork.InstrumentInterestRateRepository.Get().Where(id => id.InstrumentID == instrumentID).ToList();
                int count = result.Count();
                List<decimal> a2List = new List<decimal>();
                for (int i = 1; i < result.Count; i++)        // no need 1st data so i=1
                {
                    obj = new VM_InterestRateViewModel();
                    obj.InterestRateCount = count - 1;
                    obj.InterestRate = result[i].InterestRate;
                    aList.Add(obj);
                }
                return Json(new { Success = true, data = aList }, JsonRequestBehavior.AllowGet);
            }

            catch (Exception e)
            {

            }

            return null;
        }

        public ActionResult AccrudeInterestProcess(DateTime date)
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            bool b = PagePermission.HasPermission(User.Identity.Name, PageID, 0);
            if (b)
            {
                //DateTime date = Convert.ToDateTime(dates);
                LocalReport lr = new LocalReport();

                #region PFMemberContribution
                string path = Path.Combine(Server.MapPath("~/Areas/Instrument/Report/"), "AccuredInterest.rdlc");
                if (System.IO.File.Exists(path))
                {
                    lr.ReportPath = path;
                }
                else
                {                     
                    return RedirectToAction("Index");
                }

                var temp = unitOfWork.InstrumentRepository.Get(x => x.Closed != true && x.OCode == oCode).ToList();

                DateTime d2 = date.AddDays(-1);

                foreach (var item in temp)
                {
                    item.InstrumentNumber = item.InstrumentType + "-" + item.InstrumentNumber;
                    TimeSpan t = item.DepositDate - d2;
                    double noOfDays = t.TotalDays;
                    item.AcruadIntarestAmount = Math.Abs(noOfDays * Convert.ToDouble(item.AcruadIntarestAmount)) < 0 ? 0 : item.AcruadIntarestAmount;
                }

                var getCompany = unitOfWork.CompanyInformationRepository.GetByID(oCode);
                ReportParameterCollection reportParameters = new ReportParameterCollection();
                reportParameters.Add(new ReportParameter("rpCompanyName", getCompany.CompanyName + ""));
                reportParameters.Add(new ReportParameter("rpCompanyAddress", getCompany.CompanyAddress + ""));
                reportParameters.Add(new ReportParameter("rpuserName", User.Identity.Name + ""));
                lr.SetParameters(reportParameters);

                rd = new ReportDataSource("DataSet1", temp);



                #endregion



                lr.DataSources.Add(rd);

                string reportType = "PDF";
                string mimeType;
                string encoding;
                string fileNameExtension;
                string deviceInfo =

                "<DeviceInfo>" +
                "  <OutputFormat>" + "PDF" + "</OutputFormat>" +
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
            return View();
        }

    }
}

