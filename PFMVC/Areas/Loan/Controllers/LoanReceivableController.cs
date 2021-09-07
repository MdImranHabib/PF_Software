using DLL;
using DLL.PayRollAccess.Repository;
using DLL.PayRollAccess.ViewModel;
using DLL.Repository;
using DLL.ViewModel;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PFMVC.Areas.Loan.Controllers
{
    public class LoanReceivableController : Controller

    {
        private PRUnitOfWork rpOfWork;
        private UnitOfWork unitOfWork = new UnitOfWork();
        private MvcApplication _MvcApplication;

        //
        // GET: /Loan/LoanReceivable/

        public ActionResult Index()
        {
            return View();
        }

        //[HttpPost]


        public ActionResult LoanReceivableProcessingView(string conMonth, string conYear)

        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }



            var v = unitOfWork.CustomRepository.GetLoanReceivableList(conMonth.PadLeft(2, '0'), conYear);

            return PartialView("LoanReceivaleProcessingRecord", v);
        }

        
        //==========================================================//
        //==============loan-processing save========================//
       
            
        
        //[HttpPost]
        //public ActionResult LoanReceivableprocessingSave(string conMonth, string conYear)
        //{
        //    int oCode = ((int?)Session["OCode"]) ?? 0;
        //    if (oCode == 0)
        //    {
        //        return RedirectToAction("Login", "Account", new { area = "" });
        //    }


        //    var v = unitOfWork.CustomRepository.GetLoanReceivableList(conMonth.PadLeft(2, '0'), conYear);


        //    List<VM_tbl_Loan_Receivable> GetLoanReceivableList = unitOfWork.CustomRepository.GetLoanReceivableList(conMonth.PadLeft(2, '0'), conYear).ToList();
        //    if (GetLoanReceivableList.Count == 0)
        //    {
        //        return Json(new { Success = false, ErrorMessage = "NO Loan Receivable Processing Found!" }, JsonRequestBehavior.DenyGet);
        //    }



        //    using (var context = new PFTMEntities() )
        //    {
        //        context.Database.ExecuteSqlCommand("truncate table tbl_Loan_Receivable");
        //    }
            
        //    foreach (var item in v)
        //    {
        //        tbl_Loan_Receivable loanReceivable = new tbl_Loan_Receivable();
        //        //new entry               
        //        loanReceivable.IdentificationNumber = item.IdentificationNumber;
        //        loanReceivable.Loan_id = item.Loan_id;
        //        loanReceivable.PF_Month = item.PF_Month.PadLeft(2, '0');
        //        loanReceivable.PF_Year = item.PF_Year;
        //        loanReceivable.Principal = item.Principal;
        //        loanReceivable.interest = item.interest;
        //        loanReceivable.OCode = oCode;
        //        loanReceivable.Edit_user = unitOfWork.CustomRepository.GetUserID(User.Identity.Name);
        //        loanReceivable.Edit_date = DateTime.Now;
        //        loanReceivable.Is_HR_Receive = false;
        //        unitOfWork.LoanReceivableprocessingRepository.Insert(loanReceivable);
                

        //    }
        //    unitOfWork.Save();

        //    return Json(new { Success = true, Message = "Contribution process successfull for the Month: " + conMonth + " Year: " + conYear }, JsonRequestBehavior.DenyGet);
        //}
            
             
        

        




        
 
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
        






        public ActionResult LoanReceivaleProcessingRecord()
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;

            new CultureInfo("en-IN");
            //var v = unitOfWork.LoanReceivableprocessingRepository.Get(o => o.OCode == oCode && o.IdentificationNumber==o.IdentificationNumber && o.Loan_id ==o.Loan_id && o.OCode==o.OCode).SingleOrDefault();
            var v = unitOfWork.CustomRepository.GetLoanReceivableList("PF_Month", "PF_Year");
           
            return PartialView("LoanReceivaleProcessingRecord", v);
        }

    }
}
