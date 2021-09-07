using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using DLL;
using DLL.Repository;
using DLL.ViewModel;
using PFMVC.common;
using Telerik.Web.Mvc;

namespace PFMVC.Areas.Loan.Controllers
{
    public class ApprovalController : Controller
    {
        MvcApplication _mvcApplication;

        int PageID = 8;
        private UnitOfWork unitOfWork = new UnitOfWork();

        /// <summary>
        /// Indexes this instance.
        /// </summary>
        /// <returns>View</returns>
        /// <ModifiedBy>Avishek</ModifiedBy>
        /// <mODIFICATIONDate>24-Feb-2016</mODIFICATIONDate>
        [Authorize]
        public ActionResult Index()
        {
            //Added By Avishek Date:Jan-19-2016
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End
            ViewBag.PageName = "Approval Information";

            bool b = PagePermission.HasPermission(User.Identity.Name, PageID, 0);
            if (b)
            {
                return View();
            }
            ViewBag.PageName = "Loan Approval";
            return View("Unauthorized");
        }

        [GridAction]
        public ActionResult _SelectUnapprovedLoan()
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                RedirectToAction("Login", "Account");
            }
            return View(new GridModel(GetEmployeesLoanHistory(oCode)));
        }

        private IEnumerable<VM_PFLoan> GetEmployeesLoanHistory(int oCode)
        {
            var result = unitOfWork.CustomRepository.GetPFLoanHistoryByEmpID(oCode).Where(w => w.IsApproved == 0).ToList();
            return result;
        }

        /// <summary>
        /// Approves the loan.
        /// </summary>
        /// <param name="empID">The emp identifier.</param>
        /// <param name="loanID">The loan identifier.</param>
        /// <returns></returns>
        /// <ReViewBy>Avishek</ReViewBy>
        /// <ReviewDate>24-feb-2016</ReviewDate>
        public ActionResult ApproveLoan(int empID, string loanID)
        {
            //Added By Avishek Date:Jan-19-2016
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End
            var v = unitOfWork.PFLoanRepository.Get(f => f.EmpID == empID && f.PFLoanID == loanID && f.OCode == oCode).Single();
            CashEquivalentLedgerOptions();
            return PartialView("_ApproveLoan", v);
        }
        //public ActionResult ApproveRequestLoan(int empID)
        //{
        //    int OCode = ((int?)Session["OCode"]) ?? 0;
        //    if (OCode == 0)
        //    {
        //        return RedirectToAction("Login", "Account", new { area = "" });
        //    }
        //    var s = unitOfWork.PFLoanRequestRepository.Get().Where(f => f.EmpID == empID && f.OCode == OCode).Single();
        //    CashEquivalentLedgerOptions();
        //    return PartialView("_ApproveRequestLoan", s);
        //}
        /// <summary>
        /// Cashes the equivalent ledger options.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <ReViewBy>Avishek</ReViewBy>
        /// <ReviewDate>24-feb-2016</ReviewDate>
        public void CashEquivalentLedgerOptions(int id = 0)
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                RedirectToAction("Login", "Account");
            }
            List<int> groupList = unitOfWork.AccountingRepository.CashAccountGroupList(oCode);
            var v = unitOfWork.ACC_LedgerRepository.Get().Where(w => groupList.Contains(w.GroupID)).ToList();
            ViewData["CashEquivalentLedgerOptions"] = new SelectList(v, "LedgerID", "LedgerName", id);
        }

        /// <summary>
        /// Approves the loan.
        /// </summary>
        /// <param name="v">The v.</param>
        /// <param name="CashLedgerName">Name of the cash ledger.</param>
        /// <returns>Bool</returns>
        /// <ModifyBy>Avishek</ModifyBy>
        /// <ModifyDate>24-feb-2016</ModifyDate>
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult ApproveLoan(VM_PFLoan v, string CashLedgerName)
        {
            int voucherId = 0;
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            bool isAllowedToEdit = PagePermission.HasPermission(User.Identity.Name, PageID, 1);
            if (!isAllowedToEdit) return Json(new { Success = false, ErrorMessage = "You are not allowed to edit information! contact system admin!" }, JsonRequestBehavior.AllowGet);

            if (string.IsNullOrEmpty(CashLedgerName))
            {
                return Json(new { Success = false, ErrorMessage = "You must select a cash ledger." }, JsonRequestBehavior.DenyGet);
            }

            var model = unitOfWork.PFLoanRepository.Get(w => w.EmpID == v.EmpID && w.PFLoanID == v.PFLoanID).Single();
            if (model != null)
            {
                if (v.LoanAmount > v.PayableAmount)
                {
                    return Json(new { Success = false, ErrorMessage = "Loan amount must not be more than payable amount." }, JsonRequestBehavior.DenyGet);
                }
                model.IsApproved = 1;
                try
                {

                    model.EditUserName = User.Identity.Name;
                    model.CashLedgerID = v.CashLedgerID;
                    model.ApprovedBy = unitOfWork.CustomRepository.GetUserID(User.Identity.Name);

                    #region now make an accounting entry
                    //List<string> LedgerNameList = new List<string>();
                    List<Guid> ledgerIdList = new List<Guid>();
                    List<decimal> credit = new List<decimal>();
                    List<decimal> debit = new List<decimal>();
                    List<string> chqNumber = new List<string>();
                    List<string> pfLoanId = new List<string>();
                    List<string> pfMemberId = new List<string>();
                    string refMessage = "";
                    List<acc_Chart_of_Account_Maping> accChartOfAccountMaping = unitOfWork.ChartofAccountMapingRepository.Get().ToList();
                    //LedgerNameList.Add("Members Fund"); //This ledger should be created previously
                    //LedgerNameList.Add("Member Loan");
                    //profit distribution ledger should be debited
                    ledgerIdList.Add(accChartOfAccountMaping.Where(x => x.MIS_Id == 5 && x.OCode == oCode).Select(x => x.Ledger_Id).FirstOrDefault());
                    debit.Add(v.LoanAmount);
                    //Credit.Add(v.LoanAmount);
                    //On the other hand debit should be zero.
                    credit.Add(0);
                    //Debit.Add(0);
                    chqNumber.Add("");
                    pfMemberId.Add(v.EmpID + "");
                    pfLoanId.Add(v.PFLoanID);
                    //}

                    //LedgerNameList.Add(CashLedgerName);
                    Guid ledgerId = unitOfWork.ACC_LedgerRepository.Get(x => x.LedgerName.Trim().ToLower() == CashLedgerName.Trim().ToLower()).Select(x => x.LedgerID).FirstOrDefault();
                    ledgerIdList.Add(ledgerId);
                    credit.Add(v.LoanAmount);
                    debit.Add(0);
                    //Debit.Add(v.LoanAmount);
                    //Credit.Add(0);
                    chqNumber.Add("");
                    pfMemberId.Add("");
                    pfLoanId.Add("");
                    //Edited By Suman
                    //bool isOperationSuccess = unitOfWork.AccountingRepository.DualEntryVoucher(v.EmpID, 5, DateTime.Now, ref voucherID, "Loan approve ", LedgerNameList, Debit, Credit, ChqNumber, ref refMessage, User.Identity.Name, unitOfWork.CustomRepository.GetUserID(User.Identity.Name), PFMemberID, "Loan approved", "", "", null, PFLoanID, OCode, "Approve Loan");
                    bool isOperationSuccess = unitOfWork.AccountingRepository.DualEntryVoucherById(v.EmpID, 5, model.StartDate, ref voucherId, "Loan approve ", ledgerIdList, debit, credit, chqNumber, ref refMessage, User.Identity.Name, unitOfWork.CustomRepository.GetUserID(User.Identity.Name), pfMemberId, "Loan approved", "", "", null, pfLoanId, oCode, "Approve Loan");

                    if (DLL.Utility.ApplicationSetting.GenerateAmortization == true)
                    {
                        bool isSaved = GenerateAmortization(model.EmpID, model.PFLoanID, model.StartDate);

                        if (!isSaved)
                        {
                            return Json(new { Success = false, ErrorMessage = "Problem while saving the Amortization. " + refMessage }, JsonRequestBehavior.DenyGet);
                        }
                    }
                    
                    // model.StartDate will be replaced by DateTime.Now after Back Dated entry
                    if (isOperationSuccess)
                    {
                        try
                        {
                            unitOfWork.PFLoanRepository.Update(model);
                            unitOfWork.Save();
                            return Json(new { Success = true, Message = "Transction Sucessfull and status updated!" }, JsonRequestBehavior.DenyGet);
                        }
                        catch (Exception x)
                        {
                            return Json(new { Success = false, ErrorMessage = "Transction Sucessfull BUT STATUS UPDATE FAILED WITH FOLLOWING ERROR: " + x.Message + " PLEASE CONTACT SYS ADMIN!" }, JsonRequestBehavior.DenyGet);
                        }
                    }
                    return Json(new { Success = false, ErrorMessage = "Transaction Failded with following error : " + refMessage }, JsonRequestBehavior.DenyGet);

                    #endregion
                }
                catch (Exception x)
                {
                    return Json(new { Success = false, ErrorMessage = "Check error : " + x.Message }, JsonRequestBehavior.DenyGet);
                }
            }
            return Json(new { Success = false, ErrorMessage = "Object not found!" }, JsonRequestBehavior.DenyGet);
        }

        public bool LoanApprovePossible(int empID, decimal pfLoanAmount)
        {
            // this method needs to be changed! loan rules should be read...
            return true;
        }

        /// <summary>
        /// Generates the amortization.
        /// </summary>
        /// <param name="empID">The emp identifier.</param>
        /// <param name="pfLoanID">The pf loan identifier.</param>
        /// <param name="startDate">The start date.</param>
        /// <returns></returns>
        /// <ModifyBy>Avishek</ModifyBy>
        /// <ModifyDate>24-feb-2016</ModifyDate>
        public bool GenerateAmortization(int empID, string pfLoanID, DateTime startDate)
        {
            _mvcApplication = new MvcApplication();
            int i = 1;
            List<tbl_PFL_Amortization> lstAmortization = new List<tbl_PFL_Amortization>();
            int oCode = ((int?)Session["OCode"]) ?? 0;
            var v = unitOfWork.PFLoanRepository.Get().Where(w => w.EmpID == empID && w.PFLoanID == pfLoanID && w.OCode == oCode).Select(s => new { _LoanAmount = s.LoanAmount, _Tenor = s.TermMonth, _Interest = s.Interest }).SingleOrDefault();

            if (v == null)
            {
                return false;
            }

            double intRate = Convert.ToDouble(v._Interest) / 100;
            double loanTenor = Convert.ToDouble(v._Tenor);
            double loanAmount = Convert.ToDouble(v._LoanAmount);

            //decimal totalInterestPaid = 0;//Commentount By Avishek Date: Sep-20-2015 reason that it isn't in used
            var tmpk = 1 / (1 + intRate * 1 / 12);
            var tmpgp = (Math.Pow(tmpk, loanTenor) - 1) / (tmpk - 1) * tmpk;

            var pHlemi = loanAmount / tmpgp / 1;

            double pHlemiRound = Math.Round(pHlemi, 4);

            //lblMonthlyPayment.Text = p_hlemi_round + " TK.";

            if (loanAmount > 0 && loanTenor > 0)
            {

                decimal balance = Convert.ToDecimal(loanAmount);
                //startDate = startDate.AddMonths(-1);
                //startDate = startDate.AddMonths(-1); // first less add month because it will be increase in the loop.
                
                while (balance > 0)
                {

                    tbl_PFL_Amortization tblPflAmortization = new tbl_PFL_Amortization();
                    tblPflAmortization.InstallmentNumber = i;
                    //Edited By Avishek Date: Sep-20-2015
                    //Start
                    //tbl_pfl_amortization.Amount = Math.Round(Balance, 4);
                    //tbl_pfl_amortization.Interest = Math.Round(tbl_pfl_amortization.Amount * Convert.ToDecimal(_intRate) / 12, 4);
                    ////totalInterestPaid += tbl_pfl_amortization.Interest;
                    ////tbl_pfl_amortization.MonthlyPaid = p_hlemi_round;
                    //tbl_pfl_amortization.Principal = Math.Round(Convert.ToDecimal(p_hlemi) - tbl_pfl_amortization.Interest, 4);
                    //tbl_pfl_amortization.Balance = Math.Round(tbl_pfl_amortization.Amount - tbl_pfl_amortization.Principal, 4);
                    tblPflAmortization.Amount = _mvcApplication.GetNumber(balance);
                    tblPflAmortization.Interest = _mvcApplication.GetNumber(tblPflAmortization.Amount * Convert.ToDecimal(intRate) / 12);
                    //tblPflAmortization.Principal = _mvcApplication.GetNumber(Math.Round(Convert.ToDecimal(pHlemi) - tblPflAmortization.Interest));
                    //For Showing Actual Principal according to Installment Amount. Added by Kamrul
                    //tblPflAmortization.Principal = _mvcApplication.GetNumber(Convert.ToDecimal(pHlemi) - tblPflAmortization.Interest);
                    tblPflAmortization.Principal = _mvcApplication.GetNumber(Math.Round(Convert.ToDecimal(pHlemi) - tblPflAmortization.Interest));

                    tblPflAmortization.Balance = _mvcApplication.GetNumber(tblPflAmortization.Amount - tblPflAmortization.Principal);
                    //End
                    tblPflAmortization.Processed = 0;

                    //new added for tracking conyear and month
                    startDate = startDate.AddMonths(1); // for first month should be add 0 but installment number 1
                    string year = startDate.Year + "";
                    string month = startDate.Month.ToString().PadLeft(2, '0');
                    tblPflAmortization.ConMonth = month;
                    tblPflAmortization.ConYear = year;
                    //

                    if (tblPflAmortization.Amount < Convert.ToDecimal(pHlemiRound))
                    {
                        balance = 0;
                        tblPflAmortization.Balance = 0;
                    }
                    else
                    {
                        balance = _mvcApplication.GetNumber(tblPflAmortization.Balance);
                    }

                    lstAmortization.Add(tblPflAmortization);
                    i++;
                }
                Guid userId = unitOfWork.CustomRepository.GetUserID(User.Identity.Name);
                foreach (tbl_PFL_Amortization item in lstAmortization)
                {
                    item.EditUser = userId;
                    item.EditDate = DateTime.Now;
                    item.ReScheduleID = 1;
                    item.EmpID = empID;
                    item.PFLoanID = pfLoanID;
                    item.OCode = oCode;
                    unitOfWork.AmortizationRepository.Insert(item);
                }
                try
                {
                    unitOfWork.Save();
                    return true;
                }
                catch (Exception x)
                {
                    return false;
                }
            }
            return false;
        }
    }
}
