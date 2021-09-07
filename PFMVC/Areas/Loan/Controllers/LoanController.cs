

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CustomJsonResponse;
using DLL;
using DLL.DataPrepare;
using DLL.Repository;
using DLL.ViewModel;
using PFMVC.common;
using Telerik.Web.Mvc;
using DLL.Utility;

namespace PFMVC.Areas.Loan.Controllers
{
    public class LoanController : Controller
    {
        MvcApplication _mvcApplication;
        int PageID = 7;

        UnitOfWork unitOfWork = new UnitOfWork();
        DP_PFLoan _dpPfLoan = new DP_PFLoan();

        /// <summary>
        /// Indexes this instance.
        /// </summary>
        /// <returns>view</returns>
        /// <ModifiedBy>Avishek</ModifiedBy>
        /// <DateofModification>Date:Feb-11-2016</DateofModification>
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
            ViewBag.PageName = "Loan";

            bool b = PagePermission.HasPermission(User.Identity.Name, PageID, 0);
            if (b)
            {
                var result = unitOfWork.EmployeesRepository.Get().Where(x => x.OCode == oCode).Select(s => new { s.EmpID });
                ViewBag.Employee = new SelectList(result, "EmpID", "EmpID");

                return View();
            }
            ViewBag.PageName = "Loan Approval";
             

            return View("Unauthorized");
        }

        /// <summary>
        /// Loans the form.
        /// </summary>
        /// <param name="empID">The emp identifier.</param>
        /// <param name="pfLoanID">The pf loan identifier.</param>
        /// <returns>Partial View</returns>
        /// <ModifiedBy>Avishek</ModifiedBy>
        /// <DateofModification>Date:Feb-11-2016</DateofModification>
        public ActionResult LoanForm(int empID, string pfLoanID)
        {
            VM_PFLoan vmPfLoan;
            //Added By Avishek Date:Jan-19-2016
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End
            if (string.IsNullOrEmpty(pfLoanID))
            {
                vmPfLoan = new VM_PFLoan { EmpID = empID };
                //auto generate PFLoanID
                var v = unitOfWork.PFLoanRepository.Get().Select(s => new { PFLoanID = s.PFLoanID }).OrderBy(o => o.PFLoanID).LastOrDefault();
                if (v != null)
                {
                    int i = Convert.ToInt32(v.PFLoanID);
                    i = i + 1;
                    vmPfLoan.PFLoanID = i.ToString().PadLeft(6, '0');
                }
                else
                {
                    vmPfLoan.PFLoanID = "000001";
                }
            }
            else
            {
                var v = unitOfWork.PFLoanRepository.Get(w => w.EmpID == empID && w.PFLoanID == pfLoanID).SingleOrDefault();
                vmPfLoan = _dpPfLoan.VM_PFLoan(v);
            }
            int ruleId = 0;
            string ruleDetail = "";
            decimal eligibleLoanAmount = PayableLoanAmount(empID, ref ruleId, ref ruleDetail);
            ViewBag.Message = "Payable loan amount " + eligibleLoanAmount; //as string
            ViewBag.PayableAmount = eligibleLoanAmount; // as decimal
            ViewBag.RuleID = ruleId;
            ViewBag.RuleDetail = ruleDetail;
            var loanRule = unitOfWork.LoanRulesRepository.Get(X => X.ROWID == ruleId).FirstOrDefault();
            vmPfLoan.TermMonth = (int)loanRule.InstallmentNoumber;
            vmPfLoan.Interest = (decimal)loanRule.IntarestRate == null ? 0 : (decimal)loanRule.IntarestRate;
            return PartialView("_LoanForm", vmPfLoan);
        }

        /// <summary>
        /// Loans the form.
        /// </summary>
        /// <param name="vm_pfLoan">The VM_PF loan.</param>
        /// <param name="RuleID">The rule identifier.</param>
        /// <param name="PayableAmount">The payable amount.</param>
        /// <returns>View</returns>
        /// <ModifiedBy>Avishek</ModifiedBy>
        /// <DateofModification>Date:Feb-11-2016</DateofModification>
        [HttpPost]
        public ActionResult LoanForm(VM_PFLoan vm_pfLoan, int RuleID, decimal PayableAmount)
        {
            //Added By Avishek Date:Jan-19-2016
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End
            _mvcApplication = new MvcApplication();
            if (RuleID == 0)
            {
                return Json(new { Success = false, ErrorMessage = "Loan must follow a rule!" }, JsonRequestBehavior.DenyGet);
            }
            bool isAllowedToEdit = PagePermission.HasPermission(User.Identity.Name, PageID, 1);
            if (!isAllowedToEdit) return Json(new { Success = false, ErrorMessage = "You are not allowed to edit information! contact system admin!" }, JsonRequestBehavior.AllowGet);
            ModelState.Remove("IdentificationNumber");
            if (ModelState.IsValid)
            {
                if (vm_pfLoan.LoanAmount == 0 || vm_pfLoan.TermMonth == 0)
                {
                    return Json(new { Success = false, ErrorMessage = "Fill all information..." }, JsonRequestBehavior.AllowGet);
                }
                tbl_PFLoan tblPfLoan;
                bool isRecordExist = unitOfWork.PFLoanRepository.IsExist(w => w.EmpID == vm_pfLoan.EmpID && w.PFLoanID == vm_pfLoan.PFLoanID);

                if (!isRecordExist)
                {
                    //tbl_pfLoan = new tbl_PFLoan();
                    tblPfLoan = _dpPfLoan.tbl_PFLoan(vm_pfLoan);
                    tblPfLoan.IsApproved = 0;
                    tblPfLoan.EditUser = unitOfWork.CustomRepository.GetUserID(User.Identity.Name);
                    tblPfLoan.EditDate = DateTime.Now;
                    tblPfLoan.EditUserName = User.Identity.Name;
                    tblPfLoan.RuleUsed = RuleID;
                    //tbl_pfLoan.PayableAmount = PayableAmount;
                    //double b = (double)((tbl_pfLoan.TermMonth) / 12);
                    //decimal c = (decimal)(10 / 100);
                    //double a = (double)tbl_pfLoan.LoanAmount;
                    tblPfLoan.PayableAmount = _mvcApplication.GetNumber((decimal)((double)tblPfLoan.LoanAmount + ((double)((double)tblPfLoan.LoanAmount * 0.1) * (double)((tblPfLoan.TermMonth) / 12))));
                    tblPfLoan.OCode = oCode;
                    unitOfWork.PFLoanRepository.Insert(tblPfLoan);
                }
                else
                {
                    tblPfLoan = unitOfWork.PFLoanRepository.Get(w => w.EmpID == vm_pfLoan.EmpID && w.PFLoanID == vm_pfLoan.PFLoanID).Single();
                    //if loan already approved it should be allowed for edit.
                    if (tblPfLoan.IsApproved == 1)
                    {
                        return Json(new { Success = false, ErrorMessage = "This loan already approved, and cannot be edited." }, JsonRequestBehavior.AllowGet);
                    }
                    tblPfLoan.Installment = vm_pfLoan.Installment;
                    tblPfLoan.Interest = _mvcApplication.GetNumber(vm_pfLoan.Interest);
                    tblPfLoan.LoanAmount = _mvcApplication.GetNumber(vm_pfLoan.LoanAmount);
                    tblPfLoan.PFLoanID = vm_pfLoan.PFLoanID;
                    tblPfLoan.StartDate = vm_pfLoan.StartDate ?? DateTime.Now;
                    tblPfLoan.TermMonth = vm_pfLoan.TermMonth;
                    tblPfLoan.IsApproved = 0;
                    tblPfLoan.EditUser = unitOfWork.CustomRepository.GetUserID(User.Identity.Name);
                    tblPfLoan.EditDate = DateTime.Now;
                    tblPfLoan.OCode = oCode;
                    unitOfWork.PFLoanRepository.Update(tblPfLoan);
                }
                try
                {
                    //delete loan payment detail if exist...[be careful]
                    var v = unitOfWork.AmortizationRepository.Get().Where(e => e.EmpID == vm_pfLoan.EmpID && e.PFLoanID == vm_pfLoan.PFLoanID);
                    foreach (tbl_PFL_Amortization item in v)
                    {
                        unitOfWork.AmortizationRepository.Delete(item);
                    }
                    unitOfWork.Save();
                    return Json(new { Success = true, Message = "Loan information saved for employee" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception x)
                {
                    return Json(new { Success = false, ErrorMessage = "Check the error:\n" + x.Message }, JsonRequestBehavior.AllowGet);
                }
            }
            var errorList = (from item in ModelState.Values
                             from error in item.Errors
                             select error.ErrorMessage).ToList();
            return Json(JsonResponseFactory.ErrorResponse(errorList), JsonRequestBehavior.DenyGet);
        }

        public ActionResult LoanHistory(int empID)
        {
            DateTime empPfActivation = unitOfWork.EmployeesRepository.GetByID(empID).PFActivationDate;


            int months = ((DateTime.Now.Year - empPfActivation.Year) * 12) + (DateTime.Now.Month - empPfActivation.Month);


            double totalyears = Math.Round(months / 12d,2);

            //int year = Convert.ToInt32(Math.Ceiling(totalyears));

            ViewBag.year = totalyears;
            return PartialView("LoanHistory");
        }

        #region SELECT LOAN HISTORY
        [GridAction]
        public ActionResult _SelectLoanHistory(int empID = 0)
        {
            if (empID > 0)
            {
                return View(new GridModel(GetEmployeesLoanHistory(empID)));
            }
            return View(new GridModel<tbl_PFLoan>
            {
                Data = Enumerable.Empty<tbl_PFLoan>()
            });
        }


        private IEnumerable<VM_PFLoan> GetEmployeesLoanHistory(int empID)
        {
            var result = unitOfWork.CustomRepository.GetPFLoanHistoryByEmpID().Where(x => x.EmpID == empID).ToList();
            return result;
        }
        #endregion

        #region SELECT PAYMENT DETAIL
        [GridAction]
        public ActionResult _SelectPaymentDetail(string empID = "", string pfLoanID = "")
        {
            if (!string.IsNullOrEmpty(empID) && !string.IsNullOrEmpty(pfLoanID))
            {
                return View(new GridModel(GetPaymentDetail(empID, pfLoanID)));
            }
            return View(new GridModel<VM_Amortization>
            {
                Data = Enumerable.Empty<VM_Amortization>()
            });
        }


        private IEnumerable<VM_Amortization> GetPaymentDetail(string empID, string pfLoanID)
        {
            List<VM_Amortization> listVmAmortization = unitOfWork.CustomRepository.GetAmortizationDetail(pfLoanID);
            return listVmAmortization;
        }
        #endregion

        public ActionResult GetMonthlyPayment(double _loanAmount, double _loanTenor, double _intRate)
        {
            try
            {
                _mvcApplication = new MvcApplication();
                int oCode = ((int?)Session["OCode"]) ?? 0;


                var loanRule = unitOfWork.LoanRulesRepository.Get(x => x.OCode == oCode && x.EffectiveFrom <= DateTime.Now).ToList();
                decimal interestRate = 0;
                int temp = 0;
                foreach (var item in loanRule)
                {
                    if (_loanTenor <=(double)item.InstallmentNoumber && _loanTenor > temp)
                    {
                        interestRate = item.IntarestRate ?? 0;
                        _intRate = (double)interestRate;
                        break;
                    }
                    temp = (Int16)item.InstallmentNoumber;

                }

                var tmpk = 1 / (1 + (_intRate / 100) * 1 / 12);

                var tmpgp = (Math.Pow(tmpk, _loanTenor) - 1) / (tmpk - 1) * tmpk;

                var pHlemi = _loanAmount / tmpgp / 1;

                double pHlemiRound = (double)_mvcApplication.GetNumber((decimal)pHlemi);
                //End
                return Json(new { Success = true, Result = pHlemiRound }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { Success = false, Result = 0 }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetInterest(int _loanTenor)
        {
            try
            {
                _mvcApplication = new MvcApplication();
                int oCode = ((int?)Session["OCode"]) ?? 0;

                var loanRule = unitOfWork.LoanRulesRepository.Get(x => x.OCode == oCode && x.EffectiveFrom <= DateTime.Now).ToList();
                decimal interestRate = 0;
                int temp = 0;
                foreach (var item in loanRule)
                {
                    if (_loanTenor <= item.InstallmentNoumber && _loanTenor >temp)
                    {
                       interestRate = item.IntarestRate??0;
                       break;
                    }
                    temp =(Int16)item.InstallmentNoumber;
                   
                }

                //double pHlemiRound = (double)_mvcApplication.GetNumber((decimal)pHlemi);
                //End
                return Json(new { Success = true, Result = interestRate }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { Success = false, Result = 0 }, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult LoanPaymentDetail(string empID = "", string pfLoanID = "")
        {
            ViewBag.LoanNumber = pfLoanID;
            return PartialView("_LoanPaymentDetail");
        }

        public ActionResult DeleteLoanPossible(int empID, string pfLoanID)
        {
            bool b = PagePermission.HasPermission(User.Identity.Name, PageID, 2);
            if (!b)
            {
                return Json(new { Success = false, ErrorMessage = "You are not authorized to delete information!" }, JsonRequestBehavior.AllowGet);
            }

            var v = unitOfWork.PFLoanRepository.Get(w => w.EmpID == empID && w.PFLoanID == pfLoanID).Single();
            if (v.IsApproved == 1)
            {
                return Json(new { Success = false, ErrorMessage = "This loan already approved, and cannot be deleted!!!" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { Success = true, Message = "Are you sure deleting this Loan: " + pfLoanID + "?" }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DeleteLoanConfirm(int empID, string pfLoanID)
        {
            //Added By Avishek Date:Jan-19-2016
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End
            var v = unitOfWork.PFLoanRepository.Get(w => w.EmpID == empID && w.PFLoanID == pfLoanID).Single();
            if (v.IsApproved == 1)
            {
                return Json(new { Success = false, ErrorMessage = "This loan already approved, and cannot be deleted!!!" }, JsonRequestBehavior.AllowGet);
            }
            if (v.IsApproved == 0)
            {
                unitOfWork.PFLoanRepository.Delete(v);
                try
                {
                    //delete loan payment detail if exist...[be careful]
                    var paymentDetail = unitOfWork.AmortizationRepository.Get(e => e.EmpID == empID && e.PFLoanID == pfLoanID);
                    if (paymentDetail != null)
                    {
                        foreach (tbl_PFL_Amortization item in paymentDetail)
                        {
                            unitOfWork.AmortizationRepository.Delete(item);
                        }
                    }
                    unitOfWork.Save();
                    return Json(new { Success = true, Message = "Loan Successfully Deleted!!!" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception x)
                {
                    return Json(new { Success = false, ErrorMessage = "Check error : " + x.Message }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { Success = false, ErrorMessage = "Problem found while deleting" }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Payables the loan amount.
        /// </summary>
        /// <param name="empID">The emp identifier.</param>
        /// <param name="ruleID">The rule identifier.</param>
        /// <param name="ruleDetail">The rule detail.</param>
        /// <returns>Decimal</returns>
        /// <ModifiedBy>Avishek</ModifiedBy>
        /// <DateofModification>Date:Feb-11-2016</DateofModification>
        public decimal PayableLoanAmount(int empID, ref int ruleID, ref string ruleDetail)
        {
            try
            {
                var employee = unitOfWork.EmployeesRepository.Get(f => f.EmpID == empID).FirstOrDefault();
                _mvcApplication = new MvcApplication();
                if (employee != null)
                {
                    int oCode = ((int?)Session["OCode"]) ?? 0;
                    //var pfActivationDate = employee.PFActivationDate;
                    //var duration = DateTime.Today - pfActivationDate; //Commentout by Avishek Date:Feb-23-2015

                    //get sp_GetMemberFinancialSummary
                    decimal selfCon, empCon, selfProfit, empProfit, loanAmount, loanAmountPaid, payableAmount;
                    //selfCon = empCon = selfProfit = empProfit = loanAmount = loanAmountPaid = 0;

                    unitOfWork.CustomRepository.sp_GetMemberFinancialSummary(empID, out selfCon, out empCon, out selfProfit, out empProfit, out loanAmount, out loanAmountPaid);

                    //Added by Avishek Date Feb-22-2015 (It's can be update feature for faster issue)
                    //Start
                    var employeDetail = unitOfWork.EmployeesRepository.Get(x => x.EmpID == empID).FirstOrDefault();
                    //var remainimgLoanAmount = unitOfWork.AmortizationRepository.Get().Where(x => x.EmpID == empID).OrderBy(x=>x.InstallmentNumber).Select(x =>x.Balance).LastOrDefault();
                    
                    /// --- Added By Kamrul for Calculating based on Joining date--
                    
                    var pfDuration = ((DateTime.Now.Year - employeDetail.PFActivationDate.Year) * 12) + DateTime.Now.Month - employeDetail.PFActivationDate.Month;
                    if (ApplicationSetting.JoiningDate == true)
                    {
                        pfDuration = ((DateTime.Now.Year - employeDetail.JoiningDate.Value.Year) * 12) + DateTime.Now.Month - employeDetail.JoiningDate.Value.Month;
                    }
                    

                    /// End Kamrul
                    //get all rule by descending order of working duration
                    var loanRule = unitOfWork.LoanRulesRepository.Get(x => x.OCode == oCode && x.EffectiveFrom <= DateTime.Now).ToList();

                    decimal isOwnPartPayable = 0;
                    decimal isEmpPartPayable = 0;
                    decimal isOwnProfitPayable = 0;
                    decimal isEmpProfitPayable = 0;
                    decimal empContrebution = (decimal)(employeDetail.opOwnContribution + selfCon);
                    decimal ownContrebution = (decimal)(employeDetail.opEmpContribution + empCon);
                    //decimal profitContrebution = (decimal)(employeDetail.opProfit + selfProfit + empProfit);
                    decimal ownProfit = (decimal)(employeDetail.opProfit/2 + selfProfit); //Added by Fahim
                    decimal _EmpProfit = (decimal)(employeDetail.opProfit/2 + empProfit);  //Added by Fahim

                    foreach (var item in loanRule.OrderBy(x => x.WorkingDurationInMonth))
                    {
                        if (pfDuration >= item.WorkingDurationInMonth)
                        {
                            isOwnPartPayable = item.OwnPartPayable;
                            isEmpPartPayable = item.EmpPartPayable;
                            isOwnProfitPayable = item.OwnProfitPartPayable;
                            isEmpProfitPayable = item.EmpProfitPartPayable;
                            ruleID = item.ROWID;
                        }
                    }
                    if (isOwnPartPayable != 0 || isOwnPartPayable != null)
                    {
                        empContrebution = (empContrebution * isOwnPartPayable) / 100;
                    }
                    if (isEmpPartPayable != 0 || isEmpPartPayable != null)
                    {
                        ownContrebution = (ownContrebution * isEmpPartPayable) / 100;
                    }
                    if (isOwnProfitPayable != 0 || isOwnProfitPayable != null)
                    {
                        ownProfit = (ownProfit * isOwnProfitPayable) / 100;
                    }
                    if (isEmpProfitPayable != 0 || isEmpProfitPayable != null)
                    {
                        _EmpProfit = (_EmpProfit * isEmpProfitPayable) / 100;
                    }
                    payableAmount = _mvcApplication.GetNumber(empContrebution + ownContrebution + ownProfit + _EmpProfit - loanAmount);
                    ruleDetail = "Self " + isOwnPartPayable + ", Emp " + isEmpPartPayable + ", Profit Self " + isOwnProfitPayable + ", Profit Emp " + isEmpProfitPayable;
                    return payableAmount;
                }
                return 0;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public JsonResult AutocompleteSuggestions(string term)
        {
            try
            {
                int oCode = ((int?)Session["OCode"]) ?? 0;
                var suggestions = unitOfWork.EmployeesRepository.Get(w => w.IdentificationNumber.ToLower().Trim().Contains(term.ToLower().Trim()) && w.OCode == oCode && w.PFStatus != 2).Select(s => new { value = s.EmpName, label = s.IdentificationNumber }).ToList();
                return Json(suggestions, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public JsonResult AutocompleteByEmployeeName(string term)
        {
            try
            {
                int oCode = ((int?)Session["OCode"]) ?? 0;
                var suggestions = unitOfWork.EmployeesRepository.Get(w => w.EmpName.ToLower().Trim().Contains(term.ToLower().Trim()) && w.OCode == oCode && w.PFStatus != 2).Select(s => new { value = s.IdentificationNumber, label = s.EmpName }).ToList();
                return Json(suggestions, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public JsonResult GetEmpId(string identificationNo)
        {
            try
            {
                int suggestions = unitOfWork.CustomRepository.GetEmployeeByIdentificationNumber(identificationNo.Trim()).Select(s => s.EmpID).FirstOrDefault();
                return Json(suggestions);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


    }
}
