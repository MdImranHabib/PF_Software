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
    public class LoanListController : Controller
    {

        MvcApplication _mvcApplication;/// Added By: Avishek  Created Date: Sep-20-2015
                                        

        int PageID = 7;// 7 for loan management

        private UnitOfWork unitOfWork = new UnitOfWork();



        /// <summary>
        /// Indexes this instance.
        /// </summary>
        /// <returns></returns>
        /// <ModifiedBy>Avishek</ModifiedBy>
        /// <ModificationDate>Dec-15-2015</ModificationDate>
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
            ViewBag.PageName = "Loan List";

            bool b = PagePermission.HasPermission(User.Identity.Name, PageID, 0);
            if (b)
            {
                return View();
            }
            ViewBag.PageName = "Import Loan Payment";
            return View("Unauthorized");
        }

        #region SELECT LOAN HISTORY
        [GridAction]
        public ActionResult _SelectLoanHistory(int empID =0 , string loanID = "")
        {
            return View(new GridModel(GetEmployeesLoanHistory(empID, loanID)));
        }


        /// <summary>
        /// Gets the employees loan history.
        /// </summary>
        /// <param name="empID">The emp identifier.</param>
        /// <param name="loanID">The loan identifier.</param>
        /// <returns>list</returns>
        /// <CreatedBy>Avishek</CreatedBy>
        /// <CreatedDate>Sep-20-2015</CreatedDate>
        private IEnumerable<VM_PFLoan> GetEmployeesLoanHistory(int empID, string loanID)
        {
            _mvcApplication = new MvcApplication();
            var result = unitOfWork.CustomRepository.GetPFLoanHistoryByEmpID();
            if (empID>0)
            {
                result = result.Where(w => w.EmpID == empID);
            }
            if (!string.IsNullOrEmpty(loanID))
            {
                result = result.Where(w => w.PFLoanID == loanID);
            }
            List<VM_PFLoan> _VM_PFLoan = new List<VM_PFLoan>();
            foreach (var item in result)
            {
                VM_PFLoan aVM_PFLoan = new VM_PFLoan();
                item.Installment = _mvcApplication.GetNumber(item.Installment);
                _VM_PFLoan.Add(item);
            }
            return _VM_PFLoan.OrderBy(x => x.PFLoanID);
        }

        #endregion

        /// <summary>
        /// Loans the closed.
        /// </summary>
        /// <returns></returns>
        /// <ModifiedBy>Avishek</ModifiedBy>
        /// <ModificationDate>Dec-15-2015</ModificationDate>
        public ActionResult LoanClosed()
        {
            //Added By Avishek Date:Jan-19-2016
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End
            return View("Closed");
        }
        
        #region SELECT LOAN HISTORY
        [GridAction]
        public ActionResult _SelectClosedLoan(string empID = "", string loanID = "") //int change to string reason that here get identification no which is string in DB mofidied by Avishek Date:Jun-30-2015 
        {
            return View(new GridModel(GetClosedLoan(empID, loanID)));
        }


        private IEnumerable<VM_PFLoan> GetClosedLoan(string empID = "", string loanID = "")
        {
            //Avoeding unusefull data send empId & loanId mofidied by Avishek Date:Jun-30-2015
            //start
            _mvcApplication = new MvcApplication();
            int oCode = ((int?)Session["OCode"]) ?? 0;
            var result = unitOfWork.CustomRepository.GetClosedLoan(oCode,empID,loanID).OrderBy(x=>x.PFLoanID);

            //if (empID>0)
            //{
            //    result = result.Where(w => w.EmpID == empID);
            //}
            //if (!string.IsNullOrEmpty(loanID))
            //{
            //    result = result.Where(w => w.PFLoanID == loanID);
            //}
            //end
            return result;
        }

        /// <summary>
        /// Autocompletes the suggestions for emp.
        /// </summary>
        /// <param name="term">The term.</param>
        /// <returns>List</returns>
        /// <CrteatedBy>Avishek</CrteatedBy>
        /// <createdDate>Jun-30-2015</createdDate>
        public JsonResult AutocompleteSuggestionsForEmp(string term)
        {
            string loanNo = "";
            var suggestions = unitOfWork.CustomRepository.EmpWithLoanAutoComplete(term.Trim(), loanNo).Select(s => new
            {
                value = s.EmpName,
                label = s.IdentificationNumber
            }).OrderBy(x=>x.label);
            return Json(suggestions, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Autocompletes the suggestions for loan.
        /// </summary>
        /// <param name="term">The term.</param>
        /// <returns>List</returns>
        /// <CrteatedBy>Avishek</CrteatedBy>
        /// <createdDate>Jun-30-2015</createdDate>
        public JsonResult AutocompleteSuggestionsForLoan(string term)
        {
            string empNo = "";
            if (Session["EmpId"] =="")
            {
                var suggestions = unitOfWork.CustomRepository.EmpWithLoanAutoComplete(empNo, term).Select(s => new
                {
                    value = s.IdentificationNumber,
                    label = s.PFLoanID
                }).OrderBy(x => x.label);
                return Json(suggestions, JsonRequestBehavior.AllowGet);
            }else{
                int empID = Convert.ToInt32(Session["EmpId"]);
                var suggestions = unitOfWork.CustomRepository.EmpWithLoanAutoComplete(empNo, term).Where(x => x.EmpID == empID).Select(s => new
                {
                    value = s.IdentificationNumber,
                    label = s.PFLoanID
                }).OrderBy(x => x.label);
                return Json(suggestions, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Gets the name of the emp.
        /// </summary>
        /// <param name="identificationNo">The identification no.</param>
        /// <param name="loanID">The loan identifier.</param>
        /// <returns>List</returns>
        /// <CreatedBy>Avishek</CreatedBy>
        /// <DateofCreated>Nov-11-2015</DateofCreated>
        public JsonResult GetEmpName(string identificationNo, string loanID)
        {
            try
            {
                string suggestions = "";
                suggestions = identificationNo != "" ? unitOfWork.CustomRepository.GetEmployeeByIdentificationNumber(identificationNo.Trim()).Select(s => s.EmpName).FirstOrDefault() : unitOfWork.CustomRepository.GetLoanByID(loanID).Select(s=>s.EmpName).FirstOrDefault();
                return Json(suggestions);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Autocompletes the name of the suggestions by.
        /// </summary>
        /// <param name="term">The term.</param>
        /// <returns>List</returns>
        /// <CreatedBy>Avishek</CreatedBy>
        /// <DateofCreated>Nov-11-2015</DateofCreated>
        public JsonResult AutocompleteSuggestionsByName(string term)
        {
            var suggestions = unitOfWork.CustomRepository.GetEmployeeByName(term).Select(s => new
            {
                value = s.IdentificationNumber,
                label = s.EmpName
            }).OrderBy(x => x.label);
            return Json(suggestions, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Sets the emp identifier.
        /// </summary>
        /// <param name="identificationNo">The identification no.</param>
        /// <returns>Message</returns>
        /// <CreatedBy>Avishek</CreatedBy>
        /// <DateofCreated>Nov-11-2015</DateofCreated>
        public JsonResult SetEmpID(string identificationNo)
        {
            try
            {
                if (identificationNo == "")
                {
                    Session["EmpId"] = "";
                }
                else
                {
                    tbl_Employees vmEmployee = unitOfWork.CustomRepository.GetEmployeeByIdentificationNo(identificationNo).FirstOrDefault();
                    Session["EmpId"] = vmEmployee.EmpID.ToString();
                }
                return Json("OK");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        /// <summary>
        /// Autocompletes the suggestions for emp.
        /// </summary>
        /// <param name="term">The term.</param>
        /// <returns>List</returns>
        /// <CrteatedBy>Avishek</CrteatedBy>
        /// <createdDate>Jun-30-2015</createdDate>
        public JsonResult GetEmpIDName(string term)
        {
            string loanNo = "";
            var suggestions = unitOfWork.CustomRepository.GetLoanByIdNo(term.Trim(), loanNo).Select(s => new
            {
                value = s.EmpName,
                label = s.IdentificationNumber
            }).OrderBy(x => x.label);
            return Json(suggestions, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetLoanD(string term)
        {
            string idNo = "";
            var suggestions = unitOfWork.CustomRepository.GetLoanByIdNo(idNo, term.Trim()).Select(s => new
            {
                value = s.IdentificationNumber,
                label = s.PFLoanID
            }).OrderBy(x => x.label);
            return Json(suggestions, JsonRequestBehavior.AllowGet);
        }
        #endregion

    }
}
