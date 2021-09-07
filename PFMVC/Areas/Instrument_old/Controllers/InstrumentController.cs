using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CustomJsonResponse;
using DLL;
using DLL.Repository;
using DLL.ViewModel;
using PFMVC.common;
using Microsoft.Reporting.WebForms;
using System.IO;
using System.Web;
using DLL.Utility;

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
        public ActionResult Index()
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
            //End
            return View();
        }

        public void LedgerOptions(string s = "")
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            ViewData["LedgerOptions"] = new SelectList(unitOfWork.ACC_LedgerRepository.Get().Where(f => string.IsNullOrEmpty(f.PFMemberID) && f.OCode == oCode), "LedgerID", "LedgerName", s);
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
            LedgerOptions(instrument.LedgerID + "");
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
        public ActionResult Create(VM_Instrument v, HttpPostedFileBase photo)
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
            // For File Upload  Added By Kamrul
            //if (System.IO.File.Exists(Server.MapPath(v.file_full_path)))
            //    return Json(new { success = "false", message = "One file already exists with this file name" }, JsonRequestBehavior.AllowGet);



            //HttpPostedFileBase file = v.file_data;

            //if (file != null && file.ContentLength > 0)
            //{
            //    // extract only the filename
            //    var fileExtension = Path.GetExtension(file.FileName);

            //    if (MediaContentManager.FileType(fileExtension) == "")
            //        return Json(new { success = "false", message = "Only  DOC,JPEG & PDF file support" }, JsonRequestBehavior.AllowGet);

            //    // store the file inside ~/App_Data/uploads folder

            //    string directory = ((Request.ApplicationPath == @"/" ? "" : Request.ApplicationPath) + ApplicationSetting.DbBackUpPath);
            //    file.SaveAs(Server.MapPath(Path.Combine(directory, v.file_name + fileExtension)));

            //    return RedirectToAction("Index", new { directory = ApplicationSetting.DbBackUpPath });
            //}
            //else
            //{
            //    return View(v);
            //}
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
                            if (DeleteInstrumentFromAccounting(instrument.InstrumentID, ref message))
                            {
                                instrument.PassVoucher = false;
                                message += "This instrument previously passed but you have to pass it again if you edit this data!";
                            }
                            else
                            {
                                return Json(new { Success = false, ErrorMessage = "VOUCHER PASSED AND ERROR FOUND WHILE DELETING PREVIOUS DATA: "+message }, JsonRequestBehavior.DenyGet);
                            }
                        }
                    }
                    else
                    {
                        return Json(new { Success = false, ErrorMessage = "Instrument id " + v.InstrumentID + " is not accepted!" }, JsonRequestBehavior.DenyGet);
                    }
                    /// Accrude interest Amount Calculation
                    if (v.DepositDate == null)
                    {
                        return Json(new { Success = false, ErrorMessage = "Please Select Deposite Date!" }, JsonRequestBehavior.DenyGet);
                    }
                    DateTime _depositeDate = v.DepositDate ?? DateTime.MinValue;

                    v.MaturityDate = _depositeDate.AddMonths(v.MaturityPeriod);
                    DateTime _maturityDate = v.MaturityDate ?? DateTime.MinValue;
                    int _totalDays = (int)(_maturityDate - _depositeDate).TotalDays;
                    decimal accrudeInterest = Math.Round(((_totalDays * v.InterestRate * v.Amount) / (36500)), 6);

                    //End of Accrude Interest Calculation
                    ///
                    if (photo != null)
                    {
                        string path = Path.Combine(Server.MapPath("~/Instrument"), Path.GetFileName(photo.FileName));
                        string fileName = Path.GetFileName(photo.FileName);
                        photo.SaveAs(path);
                        v.photo = "~/Instrument/" + fileName;
                        //cLB_MembersInfo.Member_Photo = path;
                        ViewBag.FileStatus = "Instrument Photo uploaded successfully.";
                    }
                    try
                    {
                        instrument.InstrumentType = v.InstrumentType;
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
                        //instrument.InstrumentPhoto = v.photo;

                        instrument.MaturityPeriod = v.MaturityPeriod;
                        instrument.OCode = oCode;
                        instrument.AccLedgerID = v.LedgerID ?? null;
                        instrument.AcruadIntarestAmount = accrudeInterest;
                        //instrument.PreviousInstrumentId = v.RenewInstrumentID;
                        if (v.InstrumentID == 0)
                        {
                            unitOfWork.InstrumentRepository.Insert(instrument);
                        }
                        else
                        {
                            unitOfWork.InstrumentRepository.Update(instrument);
                        }
                        unitOfWork.Save();
                        return Json(new { Success = true, Message = "Instrument information updated! "+message, InstrumentID = instrument.InstrumentID }, JsonRequestBehavior.DenyGet);
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
            var v = unitOfWork.InstrumentRepository.Get().Where(x=>x.OCode == oCode).ToList();
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
                instrument.PassVoucher = unitOfWork.AccountingRepository.CreateAccountingLedgerEntry("", instrument.InstrumentType + "-" + instrument.InstrumentNumber, "Investment", "Debit", instrument.Amount, "This is Instrument openting balance. Instrument Type:" + instrument.InstrumentType + " & Instrument Number:" + instrument.InstrumentNumber + ".", ref rMessage, curUserName,curUserId, "", "", true, oCode);
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
                        return Json(new { Success = false, ErrorMessage = "Ledger: "+instrument.InstrumentType+"-"+instrument.InstrumentNumber+" creation failed! "+x.Message }, JsonRequestBehavior.DenyGet);
                    }
                }
                //Voucher will be created
                List<string> ledgerNameList = new List<string>();
                List<decimal> credit = new List<decimal>();
                List<decimal> debit = new List<decimal>();
                List<string> chqNumber = new List<string>();
                List<string> pfLoanId = new List<string>();
                List<string> pfMemberId = new List<string>();

                //remember diff btwn empID and identification number
                //Account should be exist with Each Identification Number 
                ledgerNameList.Add(instrument.InstrumentType+"-"+instrument.InstrumentNumber); //this is convention
                
                //members fund should be credited!
                credit.Add(0);
                //On the other hand debit should be zero.
                debit.Add(instrument.Amount);
                chqNumber.Add("");
                pfLoanId.Add("");
                pfMemberId.Add("");
                var ledger = unitOfWork.ACC_LedgerRepository.GetByID(instrument.AccLedgerID);
                ledgerNameList.Add(ledger.LedgerName);
                credit.Add(instrument.Amount);
                debit.Add(0);
                chqNumber.Add("");
                pfLoanId.Add("");
                pfMemberId.Add("");
                //Edited by me 
                instrument.PassVoucher = unitOfWork.AccountingRepository.DualEntryVoucher(0, 5, instrument.DepositDate, ref voucherId, "FDR name :" + instrument.InstrumentType + "-" + instrument.InstrumentID + " under: " + ledger.LedgerName, ledgerNameList, debit, credit, chqNumber, ref rMessage, User.Identity.Name, unitOfWork.CustomRepository.GetUserID(User.Identity.Name), pfMemberId, "", "", "", null, pfLoanId, oCode, "Instrument");
                // instrument.DepositDate replaced by DateTime.Now after back datted entry
            }
            instrument.PassVoucherMessage = rMessage;
            if (instrument.PassVoucher)
            {
                unitOfWork.InstrumentRepository.Update(instrument);
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
          
            if(DeleteInstrumentFromAccounting(instrumentID, ref message))
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
            message = "object not found!" ;
            return false;
        }

        /// <summary>
        /// Accrudes the interest.
        /// </summary>
        /// <returns>view</returns>
        /// <CreatedBy>Kamrul</CreatedBy>
        /// <DateofCreation>April-9-2019</DateofCreation>
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
        /// <summary>
        /// Investment Accrud Calculation by Date
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
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

