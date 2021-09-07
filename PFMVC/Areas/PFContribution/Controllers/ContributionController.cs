using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using DLL;
using DLL.PayRollAccess.ViewModel;
using DLL.PayRollAccess.Repository;
using DLL.Repository;
using System.Globalization;

namespace PFMVC.Areas.PFContribution.Controllers
{
    public class ContributionController : Controller
    {
        private PRUnitOfWork rpOfWork;
        private UnitOfWork unitOfWork = new UnitOfWork();
        private MvcApplication _MvcApplication;


        /// <summary>
        /// Indexes this instance.
        /// </summary>
        /// <returns></returns>
        /// <CreatedBy>Avishek</CreatedBy>
        /// <DateOfCreation>1-Jun-2016</DateOfCreation>
        public ActionResult Index()
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            return View();
        }


        /// <summary>
        /// Monthlies the contribution save.
        /// </summary>
        /// <param name="conMonth">The con month.</param>
        /// <param name="conYear">The con year.</param>
        /// <param name="date">The date.</param>
        /// <CreatedBy>Avishek</CreatedBy>
        /// <DateOfCreation>4-Jun-2016</DateOfCreation>
        /// <returns>bool</returns>
        [HttpPost]
        public ActionResult MonthlyContributionSave(string conMonth, string conYear, DateTime date)
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            rpOfWork = new PRUnitOfWork();
            try
            {
                var contributionVoucherMonthRecorded = unitOfWork.ACC_VoucherEntryRepository.Get(w => w.PFMonth == conMonth && w.PFYear == conYear && w.OCode == oCode).SingleOrDefault();
                if (contributionVoucherMonthRecorded != null)
                {
                   return Json(new { Success = false, ErrorMessage = "Contribution for Month: "+conMonth +" Year: "+ conYear +" and voucher already exist" }, JsonRequestBehavior.DenyGet);
                }

                DateTime fromDate = Convert.ToDateTime("01-" + conMonth +"-"+ conYear);
                int month = Convert.ToInt32(conMonth) == 12 ? 1 : Convert.ToInt32(conMonth)+1;
                DateTime toDate = Convert.ToDateTime("01-" + month + "-" + conYear);

                List<VM_Salary> monthlyContributionSalaryList = rpOfWork.CustomeRepository.MonthlyContribution(conMonth, conYear).ToList();
                if (monthlyContributionSalaryList.Count == 0)
                {
                    return Json(new { Success = false, ErrorMessage = "No contribution Found!" }, JsonRequestBehavior.DenyGet);                
                }
                List<tbl_Employees> tblEmployees = unitOfWork.EmployeesRepository.Get(x => x.PFStatus == 1).ToList();
                monthlyContributionSalaryList = (from con in monthlyContributionSalaryList
                                                 join sal in tblEmployees
                                                     on con.IdentificationNo.Trim() equals sal.IdentificationNumber.Trim()
                                                 select new VM_Salary
                                                 {
                                                     EmpID = sal.EmpID,
                                                     IdentificationNo = con.IdentificationNo,
                                                     OwnContribution = con.OwnContribution,
                                                     EmpContribution = con.EmpContribution,
                                                     LoanPrincipal = con.LoanPrincipal,
                                                     LoanInterest = con.LoanInterest,
                                                     ProcessDate = con.ProcessDate
                                                 }
                    ).ToList();

                if (monthlyContributionSalaryList.Count == 0)
                {
                    return Json(new { Success = false, ErrorMessage = "No matched employee to add contribution" }, JsonRequestBehavior.DenyGet);
                }

                var temp = unitOfWork.CustomRepository.LoanPaymentFromSalary(conYear,conMonth ,oCode);   // added by shohid 21 Aug 2016 for loan payment during contribution

                foreach (var item in monthlyContributionSalaryList)
                {

                    //------------------------ Added by Shohid 21 Aug 2016 -------------------------//
                    //-------------- Loan Payment if Loan Exist for Contribution month -------------//

                    var v = temp.Where(t => t.EmployeeID == item.EmpID).FirstOrDefault();
                    if(v != null)
                    {
                        _MvcApplication = new MvcApplication();
                        tbl_PFL_Amortization _emp_loan_payment = new tbl_PFL_Amortization();
                        _emp_loan_payment = unitOfWork.CustomRepository.GetPaymentofEmployee(item.EmpID, v.LoanID).Where(x => x.Processed == 0 && x.InstallmentNumber == Convert.ToInt32(v.InstallmentNumber) && x.Principal == item.LoanPrincipal && x.Interest == item.LoanInterest).OrderBy(x => x.InstallmentNumber).FirstOrDefault();

                        if (_emp_loan_payment != null)
                        {
                            if (_emp_loan_payment.Balance >= 0)
                            {
                            
                                _emp_loan_payment.Processed = 1;
                                _emp_loan_payment.PFLoanID = v.LoanID;
                                _emp_loan_payment.InstallmentNumber = v.InstallmentNumber;
                                _emp_loan_payment.PaymentDate = new DateTime(Int32.Parse(conYear),Int32.Parse(conMonth),1);
                                _emp_loan_payment.ProcessNumber = GetCurrentAmortizationProcessNumber();
                                _emp_loan_payment.EditUser = unitOfWork.CustomRepository.GetUserID(User.Identity.Name);
                                _emp_loan_payment.EditDate = DateTime.Now;
                                _emp_loan_payment.PaymentAmount = _emp_loan_payment.Interest + _emp_loan_payment.Principal;
                                _emp_loan_payment.PaidAmount = _emp_loan_payment.Interest + _emp_loan_payment.Principal;
                                _emp_loan_payment.Amount = _MvcApplication.GetNumber(_emp_loan_payment.Amount);
                                _emp_loan_payment.Balance = _MvcApplication.GetNumber(_emp_loan_payment.Balance);
                                _emp_loan_payment.Interest = _MvcApplication.GetNumber(_emp_loan_payment.Interest);
                                _emp_loan_payment.Principal = _MvcApplication.GetNumber(_emp_loan_payment.Principal);
                                _emp_loan_payment.OCode = oCode;
                                unitOfWork.AmortizationRepository.Update(_emp_loan_payment);
                               // unitOfWork.Save();
                                tbl_PFLoan tblPfLoan = unitOfWork.CustomRepository.EmpLoanInfo(_emp_loan_payment.EmpID, _emp_loan_payment.PFLoanID).FirstOrDefault();
                                LoanVoucherPass(_emp_loan_payment, tblPfLoan.CashLedgerID, "MonthlyInstallment");
                            }
                        }                      

                    }
                    
                    // ------------------- End of Loan Payment ---------------//

                    //else
                    //{
	            tbl_Contribution tbl_Contribution = new tbl_Contribution();
                    tbl_Contribution.ConMonth = conMonth.PadLeft(2, '0');
                    tbl_Contribution.ConYear = conYear;
                    tbl_Contribution.EmpID = item.EmpID;
                    tbl_Contribution.EmpContribution = item.EmpContribution;//Both contribution are same
                    tbl_Contribution.SelfContribution = item.OwnContribution;
                    tbl_Contribution.ProcessDate = item.ProcessDate == null ? date :Convert.ToDateTime(item.ProcessDate);
                    tbl_Contribution.EditDate = DateTime.Now;
                    tbl_Contribution.EditUser = unitOfWork.CustomRepository.GetUserID(User.Identity.Name);
                    tbl_Contribution.PFRulesID = -1; //Rule ID is -1 because no formula is applying here...
                    tbl_Contribution.ProcessID = 1; // exporting from excel file
                    tbl_Contribution.OCode = oCode;
                    tbl_Contribution.ContributionDate = toDate.AddDays(-1);
                    //_tbl_ContributionList.Add(tbl_Contribution);
                    unitOfWork.ContributionRepository.Insert(tbl_Contribution);
                    //}
                }
                unitOfWork.Save();
                var isContributionMonthRecorded = unitOfWork.ContributionMonthRecordRepository.Get(w => w.ConMonth == conMonth && w.ConYear == conYear && w.OCode == oCode).SingleOrDefault();
                if (isContributionMonthRecorded == null)
                {
                    //new entry
                    tbl_ContributionMonthRecord model = new tbl_ContributionMonthRecord();
                    model.ConYear = conYear;
                    model.ConMonth = conMonth.PadLeft(2, '0');
                    model.PassVoucher = false;
                    model.PassVoucherMessage = "";
                    model.EditDate = DateTime.Now;
                    model.EditUserName = User.Identity.Name; ;
                    model.EditUser = unitOfWork.CustomRepository.GetUserID(User.Identity.Name);
                    model.ID = (unitOfWork.ContributionMonthRecordRepository.Get().Max(m => (int?)m.ID) ?? 0) + 1;
                    model.OCode = oCode;
                    model.ContributionDate = monthlyContributionSalaryList.Select(x => x.ProcessDate).FirstOrDefault();
                    model.TotalSelfCont = unitOfWork.ContributionRepository.Get(x => x.ConMonth == conMonth && x.ConYear == conYear).ToList().Sum(x=>x.SelfContribution);
                    model.TotalEmpCont = unitOfWork.ContributionRepository.Get(x => x.ConMonth == conMonth && x.ConYear == conYear).ToList().Sum(x => x.EmpContribution);

                    unitOfWork.ContributionMonthRecordRepository.Insert(model);
                    unitOfWork.Save(); // added by shohid 22 Aug  2016
                }
                else
                {
                    return Json(new { Success = false, ErrorMessage = "Already Contributed for the Month: " + conMonth + " Year: " + conYear }, JsonRequestBehavior.DenyGet);
                    //update existing]
                    //isContributionMonthRecorded.EditDate = DateTime.Now; ;
                    //isContributionMonthRecorded.EditUserName = User.Identity.Name; ; ;
                    //isContributionMonthRecorded.EditUser = unitOfWork.CustomRepository.GetUserID(User.Identity.Name);
                    //unitOfWork.ContributionMonthRecordRepository.Update(isContributionMonthRecorded);
                }

                //unitOfWork.Save();
                return Json(new { Success = true, Message = "Contribution process successfull for the Month: "+conMonth +" Year: "+ conYear }, JsonRequestBehavior.DenyGet);
            }
            catch (Exception)
            {
                return Json(new { Success = false, ErrorMessage = "Contribution process cannot be done! Error ocured" }, JsonRequestBehavior.DenyGet);
            }
        }


        public int GetCurrentAmortizationProcessNumber()
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            int n = unitOfWork.AmortizationRepository.Get().Where(p => p.ProcessNumber > 0 && p.OCode == oCode).Max(m => m.ProcessNumber) ?? 0;
            return n + 1;
        }


        //--------- added by shohid 21 Aug 2016 for Loan Payment ------//
        private void LoanVoucherPass(tbl_PFL_Amortization emp_loan_payment, Guid? cashLedgerID, string Status)
        {
            _MvcApplication = new MvcApplication();
            int oCode = ((int?)Session["OCode"]) ?? 0;
            int voucherId = 0;
            //List<string> LedgerNameList = new List<string>();
            List<Guid> ledgerIdList = new List<Guid>();
            List<decimal> credit = new List<decimal>();
            List<decimal> debit = new List<decimal>();
            List<string> chqNumber = new List<string>();
            List<string> pfLoanId = new List<string>();
            List<string> pfMemberId = new List<string>();
            string refMessage = "";
            //string ledgerName = unitOfWork.ACC_LedgerRepository.GetByID(cashLedgerID).LedgerName;

            List<acc_Chart_of_Account_Maping> accChartOfAccountMaping = unitOfWork.ChartofAccountMapingRepository.Get().ToList();
            ledgerIdList.Add(accChartOfAccountMaping.Where(x => x.MIS_Id == 10 && x.OCode == oCode).Select(x => x.Ledger_Id).FirstOrDefault());
            debit.Add(_MvcApplication.GetNumber(emp_loan_payment.Interest));
            credit.Add(0);
            chqNumber.Add("");
            pfMemberId.Add(emp_loan_payment.EmpID + "");
            pfLoanId.Add(emp_loan_payment.PFLoanID);

            //LedgerNameList.Add("Member Loan Interest");
            ledgerIdList.Add(accChartOfAccountMaping.Where(x => x.MIS_Id == 8 && x.OCode == oCode).Select(x => x.Ledger_Id).FirstOrDefault());
            credit.Add(_MvcApplication.GetNumber(emp_loan_payment.Interest));
            debit.Add(0);
            chqNumber.Add("");
            pfMemberId.Add(emp_loan_payment.EmpID + "");
            pfLoanId.Add(emp_loan_payment.PFLoanID);

            //bool isOperationSuccess = unitOfWork.AccountingRepository.DualEntryVoucher(emp_loan_payment.EmpID, 7, emp_loan_payment.EditDate, ref voucherID, "Loan installment ", LedgerNameList, Debit, Credit, ChqNumber, ref refMessage, User.Identity.Name, unitOfWork.CustomRepository.GetUserID(User.Identity.Name), PFMemberID, "Loan installment", "", "", null, PFLoanID, OCode, "Loan installment");
            bool isOperationSuccess = unitOfWork.AccountingRepository.DualEntryVoucherById(emp_loan_payment.EmpID, 7, Convert.ToDateTime(emp_loan_payment.PaymentDate), ref voucherId, "Loan installment ", ledgerIdList, debit, credit, chqNumber, ref refMessage, User.Identity.Name, unitOfWork.CustomRepository.GetUserID(User.Identity.Name), pfMemberId, "Loan installment", "", "", null, pfLoanId, oCode, "Loan installment");
            // Convert.ToDateTime(emp_loan_payment.PaymentDate)  will be replaced by DateTime.Now after Back Dated entry Date:Feb-24-2016

            if (isOperationSuccess)
            {
                ledgerIdList = new List<Guid>();
                credit = new List<decimal>();
                debit = new List<decimal>();
                chqNumber = new List<string>();
                pfLoanId = new List<string>();
                pfMemberId = new List<string>();
                voucherId = 0;

                //LedgerIdList.Add("Company Current Account");
                if (cashLedgerID != null)
                {
                    ledgerIdList.Add((Guid)cashLedgerID);
                }
                else
                {
                    ledgerIdList.Add(
                        accChartOfAccountMaping.Where(x => x.MIS_Id == 11 && x.OCode == oCode)
                            .Select(x => x.Ledger_Id)
                            .FirstOrDefault());
                }
                debit.Add(Status == "LoanSettlement"
                    ? _MvcApplication.GetNumber(Convert.ToDecimal(emp_loan_payment.Amount + emp_loan_payment.Interest))
                    : _MvcApplication.GetNumber(Convert.ToDecimal(emp_loan_payment.PaymentAmount)));
                credit.Add(0);
                chqNumber.Add("");
                pfMemberId.Add(emp_loan_payment.EmpID + "");
                pfLoanId.Add(emp_loan_payment.PFLoanID);

                //LedgerNameList.Add("Member Loan");
                ledgerIdList.Add(accChartOfAccountMaping.Where(x => x.MIS_Id == 11 && x.OCode == oCode).Select(x => x.Ledger_Id).FirstOrDefault());
                credit.Add(Status == "LoanSettlement"
                    ? _MvcApplication.GetNumber(Convert.ToDecimal(emp_loan_payment.Amount + emp_loan_payment.Interest))
                    : _MvcApplication.GetNumber(Convert.ToDecimal(emp_loan_payment.PaymentAmount)));
                debit.Add(0);
                chqNumber.Add("");
                pfMemberId.Add(emp_loan_payment.EmpID + "");
                pfLoanId.Add(emp_loan_payment.PFLoanID);
                //bool isSuccess = unitOfWork.AccountingRepository.DualEntryVoucher(emp_loan_payment.EmpID, 7, DateTime.Now, ref voucherID, "Loan adjustment ", LedgerNameList, Debit, Credit, ChqNumber, ref refMessage, User.Identity.Name, unitOfWork.CustomRepository.GetUserID(User.Identity.Name), PFMemberID, "Loan adjustment", "", "", null, PFLoanID, OCode, "Loan adjustment");
                unitOfWork.AccountingRepository.DualEntryVoucherById(emp_loan_payment.EmpID, 7, Convert.ToDateTime(emp_loan_payment.PaymentDate), ref voucherId, "Loan adjustment ", ledgerIdList, debit, credit, chqNumber, ref refMessage, User.Identity.Name, unitOfWork.CustomRepository.GetUserID(User.Identity.Name), pfMemberId, "Loan adjustment", "", "", null, pfLoanId, oCode, "Loan adjustment");
                // Convert.ToDateTime(emp_loan_payment.PaymentDate)  will be replaced by DateTime.Now after Back Dated entry Date:Feb-24-2016
            }
        }
        ///// <summary>
        ///// Imports this instance.
        ///// </summary>
        ///// <returns>view</returns>
        ///// <ReviewBy>Avishek</ReviewBy>
        ///// <ReviewDate>24-Feb-2016</ReviewDate>
        //[Authorize]
        //public ActionResult Import()
        //{
        //    //Added By Avishek Date:Dec-20-2015
        //    int oCode = ((int?)Session["OCode"]) ?? 0;
        //    if (oCode == 0)
        //    {
        //        return RedirectToAction("Login", "Account", new { area = "" });
        //    }
        //    //End
        //    ViewBag.Message = TempData["Message"] as string;
        //    ViewBag.ErrorMessage = TempData["ErrorMessage"] as string;
        //    ViewBag.Month = TempData["Month"] as string;
        //    return View();
        //}

        public ActionResult SalaryMonthRecord()
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            new CultureInfo("en-IN");
            var v = unitOfWork.ContributionMonthRecordRepository.Get(o => o.OCode == oCode).OrderByDescending(o => o.ConYear).ThenByDescending(o => o.ConMonth).ToList();
            return PartialView("SalaryMonthRecord", v);
        }

        #region AccountTransaction Contribution

        /// <summary>
        /// Passes the voucher confirm.
        /// </summary>
        /// <param name="month">The month.</param>
        /// <param name="year">The year.</param>
        /// <returns>bool</returns>
        /// <ModifiedBy>Avishek</ModifiedBy>
        /// <ModificationDate>Dec-20-2015</ModificationDate>
        [HttpPost]
        public ActionResult PassVoucherConfirm(string month, string year)
        {
            int voucherId = 0;
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });//Added By Avishek Date:Dec-20-2015
                //return Json(new { Success = false, ErrorMessage = "You must be under a compnay!" }, JsonRequestBehavior.DenyGet);
            }

            string curUserName = User.Identity.Name;
            Guid curUserId = unitOfWork.CustomRepository.GetUserID(User.Identity.Name);
            string refMessage = "";
            month = month.PadLeft(2, '0');

            var isAlreadyTransacted = unitOfWork.ContributionMonthRecordRepository.Get(w => w.ConMonth == month && w.ConYear == year && w.OCode == oCode).SingleOrDefault();
            if (isAlreadyTransacted != null)
            {
                if (isAlreadyTransacted.PassVoucher == true)
                {
                    return Json(new { Success = false, ErrorMessage = "Transaction completed previously at " + isAlreadyTransacted.EditDate.ToString("dd MMM, yyyy") + " by " + isAlreadyTransacted.EditUserName }, JsonRequestBehavior.DenyGet);
                }
            }

            var v = unitOfWork.CustomRepository.GetContributionDetail().Where(w => w.ConYear == year && w.ConMonth == month && w.OCode == oCode).ToList();
            if (v.Count == 0)
            {
                return Json(new { Success = false, ErrorMessage = "No record found for account transaction..." }, JsonRequestBehavior.DenyGet);
            }

            //List<string> LedgerNameList = new List<string>();
            List<Guid> LedgerIdList = new List<Guid>();
            List<decimal> Credit = new List<decimal>();
            List<decimal> Debit = new List<decimal>();
            List<string> ChqNumber = new List<string>();
            List<string> PFLoanID = new List<string>();
            List<string> PFMemberID = new List<string>();

            //decimal result = 0;// result sum of own and emp cont
            decimal totalSelfContribution = 0;
            decimal totalEmpContribution = 0;
            foreach (var item in v)
            {
                //result += item.SelfContribution + item.EmpContribution;
                totalSelfContribution += item.SelfContribution;
                totalEmpContribution += item.EmpContribution;
            }

            //remember diff btwn empID and identification number
            //Account should be exist with Each Identification Number 
            //LedgerNameList.Add("Members Fund"); //this is convention
            //members fund should be credited!
            List<acc_Chart_of_Account_Maping> accChartOfAccountMaping = unitOfWork.ChartofAccountMapingRepository.Get().ToList();
            Guid ledgerIdforOwnConLiability = accChartOfAccountMaping.Where(x => x.MIS_Id == 3).Select(x => x.Ledger_Id).FirstOrDefault();
            LedgerIdList.Add(ledgerIdforOwnConLiability);
            Credit.Add(totalSelfContribution);
            Debit.Add(0);
            ChqNumber.Add("");
            PFMemberID.Add("");
            PFLoanID.Add("");

            //LedgerNameList.Add("Company Current Account");
            Guid ledgerIdforOwnConAsset = accChartOfAccountMaping.Where(x => x.MIS_Id == 1).Select(x => x.Ledger_Id).FirstOrDefault();
            LedgerIdList.Add(ledgerIdforOwnConAsset);
            Credit.Add(0);
            Debit.Add(totalSelfContribution);
            ChqNumber.Add("");
            PFMemberID.Add("");
            PFLoanID.Add("");
            //Edited by Suman

            Guid ledgerIdforEmpConLiability = accChartOfAccountMaping.Where(x => x.MIS_Id == 4).Select(x => x.Ledger_Id).FirstOrDefault();
            LedgerIdList.Add(ledgerIdforEmpConLiability);
            Credit.Add(totalEmpContribution);
            Debit.Add(0);
            ChqNumber.Add("");
            PFMemberID.Add("");
            PFLoanID.Add("");

            Guid ledgerIdforEmpConAsset = accChartOfAccountMaping.Where(x => x.MIS_Id == 2).Select(x => x.Ledger_Id).FirstOrDefault();
            LedgerIdList.Add(ledgerIdforEmpConAsset);
            Credit.Add(0);
            Debit.Add(totalEmpContribution);
            ChqNumber.Add("");
            PFMemberID.Add("");
            PFLoanID.Add("");

            bool isOperationSuccess = unitOfWork.AccountingRepository.DualEntryVoucherById(0, 5, isAlreadyTransacted.ContributionDate ?? DateTime.Now, ref voucherId, " Contribution for month " + month + "/" + year, LedgerIdList, Debit, Credit, ChqNumber, ref refMessage, curUserName, curUserId, PFMemberID, "", month, year, null, PFLoanID, oCode, "Contribution");

            if (isOperationSuccess)
            {
                isAlreadyTransacted.PassVoucher = true;
                isAlreadyTransacted.PassVoucherMessage = "Tansaction completed successfull at " + DateTime.Now;
                unitOfWork.ContributionMonthRecordRepository.Update(isAlreadyTransacted);
                try
                {
                    unitOfWork.Save();
                    return Json(new { Success = true, Message = "Transction Sucessfull and status updated!" }, JsonRequestBehavior.DenyGet);
                }
                catch (Exception x)
                {
                    return Json(new { Success = false, ErrorMessage = "Transction Sucessfull BUT STATUS UPDATE FAILED WITH FOLLOWING ERROR: " + x.Message + " PLEASE CONTACT SYS ADMIN!" }, JsonRequestBehavior.DenyGet);
                }
            }
            return Json(new { Success = false, ErrorMessage = "Transaction Failded with following error : " + refMessage }, JsonRequestBehavior.DenyGet);
        }

        #endregion

        //private string PassVoucher(string conMonth, string conYear)
        //{
        //    int voucherId = 0;
        //    int oCode = ((int?)Session["OCode"]) ?? 0;
        //    if (oCode == 0)
        //    {
        //        return "Sessionout";
        //    }

        //    string curUserName = User.Identity.Name;
        //    Guid curUserId = unitOfWork.CustomRepository.GetUserID(User.Identity.Name);
        //    string refMessage = "";
        //    conMonth = conMonth.PadLeft(2, '0');

        //    var isAlreadyTransacted = unitOfWork.ContributionMonthRecordRepository.Get(w => w.ConMonth == conMonth && w.ConYear == conYear && w.OCode == oCode).SingleOrDefault();
        //    if (isAlreadyTransacted != null)
        //    {
        //        if (isAlreadyTransacted.PassVoucher == true)
        //        {
        //            return "Transaction completed previously at " +
        //                   isAlreadyTransacted.EditDate.ToString("dd MMM, yyyy") + " by " +
        //                   isAlreadyTransacted.EditUserName;
        //        }
        //    }

        //    var v = unitOfWork.CustomRepository.GetContributionDetail().Where(w => w.ConYear == conMonth && w.ConMonth == conMonth && w.OCode == oCode).ToList();
        //    if (v.Count == 0)
        //    {
        //        return "No record found for account transaction...";
        //    }

        //    //List<string> LedgerNameList = new List<string>();
        //    List<Guid> ledgerIdList = new List<Guid>();
        //    List<decimal> credit = new List<decimal>();
        //    List<decimal> debit = new List<decimal>();
        //    List<string> chqNumber = new List<string>();
        //    List<string> pfLoanId = new List<string>();
        //    List<string> pfMemberId = new List<string>();

        //    decimal totalSelfContribution = 0;
        //    decimal totalEmpContribution = 0;
        //    foreach (var item in v)
        //    {
        //        totalSelfContribution += item.SelfContribution;
        //        totalEmpContribution += item.EmpContribution;
        //    }

        //    //remember diff btwn empID and identification number
        //    //Account should be exist with Each Identification Number 
        //    //LedgerNameList.Add("Members Fund"); //this is convention
        //    //members fund should be credited!
        //    List<acc_Chart_of_Account_Maping> accChartOfAccountMaping = unitOfWork.ChartofAccountMapingRepository.Get().ToList();
        //    Guid ledgerIdforOwnConLiability = accChartOfAccountMaping.Where(x => x.MIS_Id == 3).Select(x => x.Ledger_Id).FirstOrDefault();
        //    ledgerIdList.Add(ledgerIdforOwnConLiability);
        //    credit.Add(totalSelfContribution);
        //    debit.Add(0);
        //    chqNumber.Add("");
        //    pfMemberId.Add("");
        //    pfLoanId.Add("");

        //    //LedgerNameList.Add("Company Current Account");
        //    Guid ledgerIdforOwnConAsset = accChartOfAccountMaping.Where(x => x.MIS_Id == 1).Select(x => x.Ledger_Id).FirstOrDefault();
        //    ledgerIdList.Add(ledgerIdforOwnConAsset);
        //    credit.Add(0);
        //    debit.Add(totalSelfContribution);
        //    chqNumber.Add("");
        //    pfMemberId.Add("");
        //    pfLoanId.Add("");
        //    //Edited by Suman

        //    Guid ledgerIdforEmpConLiability = accChartOfAccountMaping.Where(x => x.MIS_Id == 4).Select(x => x.Ledger_Id).FirstOrDefault();
        //    ledgerIdList.Add(ledgerIdforEmpConLiability);
        //    credit.Add(totalEmpContribution);
        //    debit.Add(0);
        //    chqNumber.Add("");
        //    pfMemberId.Add("");
        //    pfLoanId.Add("");

        //    Guid ledgerIdforEmpConAsset = accChartOfAccountMaping.Where(x => x.MIS_Id == 2).Select(x => x.Ledger_Id).FirstOrDefault();
        //    ledgerIdList.Add(ledgerIdforEmpConAsset);
        //    credit.Add(0);
        //    debit.Add(totalEmpContribution);
        //    chqNumber.Add("");
        //    pfMemberId.Add("");
        //    pfLoanId.Add("");

        //    bool isOperationSuccess = unitOfWork.AccountingRepository.DualEntryVoucherById(0, 5, isAlreadyTransacted.ContributionDate ?? DateTime.Now, ref voucherId, " Contribution for month " + conMonth + "/" + conYear, ledgerIdList, debit, credit, chqNumber, ref refMessage, curUserName, curUserId, pfMemberId, "", conMonth, conYear, null, pfLoanId, oCode, "Contribution collect from payroll");

        //    if (isOperationSuccess)
        //    {
        //        isAlreadyTransacted.PassVoucher = true;
        //        isAlreadyTransacted.PassVoucherMessage = "Tansaction completed successfull at " + DateTime.Now;
        //        unitOfWork.ContributionMonthRecordRepository.Update(isAlreadyTransacted);
        //        try
        //        {
        //            unitOfWork.Save();
        //            return "Transction Sucessfull and status updated!";
        //        }
        //        catch (Exception x)
        //        {
        //            return "Transction Sucessfull BUT STATUS UPDATE FAILED WITH FOLLOWING ERROR: " + x.Message + " PLEASE CONTACT SYS ADMIN!";
        //        }
        //    }
        //    return "Transaction Failded with following error : " + refMessage;

        //}

    }
}
