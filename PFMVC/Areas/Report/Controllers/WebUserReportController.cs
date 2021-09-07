using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using DLL.Repository;
using Telerik.Web.Mvc;
using DLL.ViewModel;
using DLL;
using PFMVC.common;
using CustomJsonResponse;
using DLL.DataPrepare;

namespace PFMVC.Areas.Report.Controllers
{
    public class WebUserReportController : Controller
    {
        UnitOfWork unitOfWork = new UnitOfWork();
        MvcApplication _MvcApplication;
        int PageID = 7;
        DP_PFLoan dp_pfLoan = new DP_PFLoan();

        [Authorize]
        public ActionResult Index()
        {
            //Added By Avishek Date:Jan-19-2016
            int OCode = ((int?)Session["OCode"]) ?? 0;
            if (OCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End
            var v = unitOfWork.UserProfileRepository.Get().Where( w => w.LoginName.ToLower() == User.Identity.Name.ToLower()).SingleOrDefault();
            if (v != null)
            {
                var employee = unitOfWork.EmployeesRepository.GetByID(v.EmpID);
                ViewBag.EmployeeID = employee.EmpID;
                Session["empId"] = employee.EmpID.ToString();
                return View();
            }
            ViewBag.Message = "We dont found your record. contact system admin.";
            return View("Error");
        }

        #region SELECT LOAN HISTORY
        [GridAction]
        public ActionResult _SelectLoanHistory(int empID = 0)
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                var user = unitOfWork.UserProfileRepository.Get().Where(w => w.LoginName == User.Identity.Name).SingleOrDefault();
                if (user.EmpID > 0)
                {
                    empID = user.EmpID ?? 0;
                }
            }

            if (empID > 0)
            {
                return View(new GridModel(GetEmployeesLoanHistory(empID)));
            }
            else
            {
                return View(new GridModel<tbl_PFLoan>
                {
                    Data = Enumerable.Empty<tbl_PFLoan>()
                });
            }
        }


        private IEnumerable<VM_PFLoan> GetEmployeesLoanHistory(int empID)
        {
            var result = unitOfWork.CustomRepository.GetPFLoanHistoryByEmpID().Where(c => c.EmpID == empID).ToList();
            return result;
        }
        #endregion

        /// <summary>
        /// Resets the password.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="currentPassword">The current password.</param>
        /// <param name="newPassword">The new password.</param>
        /// <param name="confirmPassword">The confirm password.</param>
        /// <returns>bool</returns>
        /// <CreatedBy>Avishek</CreatedBy>
        /// <CreatedDate>Oct-3-2015</CreatedDate>
        [HttpPost]
        public ActionResult ResetPassword(string userName, string currentPassword, string newPassword, string confirmPassword)
        {
            //Added By Avishek Date:Jan-19-2016
            int OCode = ((int?)Session["OCode"]) ?? 0;
            userName = (string)Session["UserName"];
            if (OCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End
            if (userName != User.Identity.Name) {
                return Json(new { Success = false, ErrorMessage = "Currently one user cannot change other user password except ADMIN." }, JsonRequestBehavior.DenyGet);
            }
            else
            {
                int empId = Convert.ToInt32(Session["empId"] ?? 0);
                VM_EmployeeWebUser _VM_EmployeeWebUser = unitOfWork.CustomRepository.GetUser(empId, userName).FirstOrDefault();
                var userModel = unitOfWork.UserPasswordRepository.GetByID(_VM_EmployeeWebUser.UserId);
                string md5OldPassword = userModel.Password;
                string md5NewPassword = GetMD5HashPassword.GetMd5Hash(currentPassword);
                if (newPassword == confirmPassword)
                {
                    if (md5OldPassword == md5NewPassword)
                    {
                        userModel.Password = GetMD5HashPassword.GetMd5Hash(newPassword);
                        userModel.EditDate = DateTime.Now;
                        try
                        {
                            unitOfWork.Save();
                            return Json(new { Success = true, Message = "Password successfully altered! next time you have to login with your new password." }, JsonRequestBehavior.DenyGet);
                        }
                        catch (Exception x)
                        {
                            return Json(new { Success = false, ErrorMessage = "Error:" + x.Message }, JsonRequestBehavior.DenyGet);
                        }
                    }
                    else
                    {
                        return Json(new { Success = false, ErrorMessage = "Stored current password and your input current password are not same!" }, JsonRequestBehavior.DenyGet);
                    }
                }
                else
                {
                    return Json(new { Success = false, ErrorMessage = "New password and confirm new password are not same!" }, JsonRequestBehavior.DenyGet);
                }
            }
        }


        /// <summary>
        /// Autocompletes the suggestions loan.
        /// </summary>
        /// <param name="term">The term.</param>
        /// <returns>list</returns>
        /// <modifiedBy>Avishek</modifiedBy>
        /// <ModifiedDate>Sep-30-2015</ModifiedDate>
        public JsonResult AutocompleteSuggestionsForEmpId(string term)
        {
            try
            {
                int OCode = ((int?)Session["OCode"]) ?? 0;
                int empId = Convert.ToInt32(Session["empId"] ?? 0);
                var suggestions = unitOfWork.CustomRepository.EmployeeWithLoan(OCode, term).Where(x=>x.EmpID == empId).Select(s => new
                {
                    value = s.LoanID,
                    label = s.LoanID
                }).GroupBy(x => x.label).Select(g => g.FirstOrDefault()).ToList();
                return Json(suggestions, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public ActionResult LoanForm(decimal loanAmount, decimal termMonth, decimal interest, decimal installment, DateTime startDate)
        {
            //Added By Avishek Date:Jan-19-2016
            int OCode = ((int?)Session["OCode"]) ?? 0;
            if (OCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End
            _MvcApplication = new MvcApplication();
            int empId = Convert.ToInt32(Session["empId"] ?? 0);
            
            if (loanAmount == 0 || termMonth == 0)
            {
                return Json(new { Success = false, ErrorMessage = "Fill all information..." }, JsonRequestBehavior.AllowGet);
            }
            string pFLoanID = "";
            var v = unitOfWork.PFLoanRepository.Get().Select(s => new { PFLoanID = s.PFLoanID }).OrderBy(o => o.PFLoanID).LastOrDefault();
            if (v != null)
            {
                int i = Convert.ToInt32(v.PFLoanID);
                i = i + 1;
                pFLoanID = i.ToString().PadLeft(6, '0');
            }
            else
            {
                pFLoanID = "000001";
            }

            tbl_PFLoan tbl_pfLoan;
            int ruleId = 1;
            //VM_PFLoan
            bool isRecordExist = unitOfWork.PFLoanRepository.IsExist(w => w.EmpID == empId && w.PFLoanID == pFLoanID);

                if (!isRecordExist)
                {
                    ViewBag.RuleID = ruleId;
                    var loanRule = unitOfWork.LoanRulesRepository.Get(X => X.ROWID == ruleId).FirstOrDefault();
                    //vmPfLoan.TermMonth = (int)loanRule.InstallmentNoumber;
                    //vmPfLoan.Interest = (decimal)loanRule.IntarestRate == null ? 0 : (decimal)loanRule.IntarestRate;
                    tbl_pfLoan = new tbl_PFLoan();
                    tbl_pfLoan.PFLoanID = pFLoanID;
                    tbl_pfLoan.EmpID = empId; 

                    tbl_pfLoan.LoanAmount = loanAmount;
                    tbl_pfLoan.TermMonth = Convert.ToInt16(termMonth);
                    //tbl_pfLoan.Interest = interest;
                    tbl_pfLoan.Interest = (decimal)loanRule.IntarestRate == null ? 0 : (decimal)loanRule.IntarestRate;
                    tbl_pfLoan.Installment = installment;
                    tbl_pfLoan.StartDate = startDate;
                    tbl_pfLoan.IsApproved = 0;
                    tbl_pfLoan.EditUser = unitOfWork.CustomRepository.GetUserID(User.Identity.Name);
                    tbl_pfLoan.EditDate = System.DateTime.Now;
                    tbl_pfLoan.EditUserName = User.Identity.Name;
                    tbl_pfLoan.RuleUsed = unitOfWork.PFRulesRepository.Get().Select(x=>x.ROWID).FirstOrDefault();
                    tbl_pfLoan.PayableAmount = _MvcApplication.GetNumber((decimal)((double)tbl_pfLoan.LoanAmount + ((double)((double)tbl_pfLoan.LoanAmount * 0.1) * (double)((tbl_pfLoan.TermMonth) / 12))));
                    tbl_pfLoan.OCode = OCode;
                    unitOfWork.PFLoanRepository.Insert(tbl_pfLoan);
                }
                else
                {
                    tbl_pfLoan = unitOfWork.PFLoanRepository.Get().Where(w => w.EmpID == empId && w.PFLoanID == pFLoanID).Single();
                    if (tbl_pfLoan.IsApproved == 1)
                    {
                        return Json(new { Success = false, ErrorMessage = "This loan already approved, and cannot be edited." }, JsonRequestBehavior.AllowGet);
                    }
                    tbl_pfLoan.Installment = installment;
                    tbl_pfLoan.Interest = _MvcApplication.GetNumber(interest);
                    tbl_pfLoan.LoanAmount = _MvcApplication.GetNumber(loanAmount);
                    tbl_pfLoan.PFLoanID = pFLoanID;
                    tbl_pfLoan.StartDate = startDate;
                    tbl_pfLoan.TermMonth = Convert.ToInt16(termMonth);
                    tbl_pfLoan.IsApproved = 0;
                    tbl_pfLoan.EditUser = unitOfWork.CustomRepository.GetUserID(User.Identity.Name);
                    tbl_pfLoan.EditDate = System.DateTime.Now;
                    tbl_pfLoan.OCode = OCode;
                    unitOfWork.PFLoanRepository.Update(tbl_pfLoan);
                }
                try
                {
                    var v1 = unitOfWork.AmortizationRepository.Get().Where(e => e.EmpID == empId && e.PFLoanID == pFLoanID);
                    if (v1 != null)
                    {
                        foreach (tbl_PFL_Amortization item in v1)
                        {
                            unitOfWork.AmortizationRepository.Delete(item);
                        }
                    }
                    unitOfWork.Save();
                    return Json(new { Success = true, Message = "Loan information saved for employee" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception x)
                {
                    return Json(new { Success = false, ErrorMessage = "Check the error:\n" + x.Message }, JsonRequestBehavior.AllowGet);
                }
                        var errorList = (from item in ModelState.Values
                             from error in item.Errors
                             select error.ErrorMessage).ToList();
            return Json(JsonResponseFactory.ErrorResponse(errorList), JsonRequestBehavior.DenyGet);
            }

    }
}
