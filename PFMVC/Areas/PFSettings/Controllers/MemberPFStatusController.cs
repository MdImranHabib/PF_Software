using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using DLL.Repository;
using DLL.ViewModel;
using PFMVC.common;
using Telerik.Web.Mvc;
using System.Globalization;
using DLL;

namespace PFMVC.Areas.PFSettings.Controllers
{
    public class MemberPFStatusController : Controller
    {

        int PageID = 5;
        private UnitOfWork unitOfWork = new UnitOfWork();
        MvcApplication _mvcApplication;

        /// <summary>
        /// Members the pf status.
        /// </summary>
        /// <returns></returns>
        /// <ModifiedBy>Avishek</ModifiedBy>
        /// <ModificationDate>Dec-15-2015</ModificationDate>
        [Authorize]
        public ActionResult MemberPFStatus()
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            bool b = PagePermission.HasPermission(User.Identity.Name, PageID, 0);
            if (b)
            {
                var result = unitOfWork.EmployeesRepository.Get().Select(s => new { s.EmpID });
                ViewBag.Employee = new SelectList(result, "EmpID", "EmpID");
                return View("MemberPFStatus");
            }
            ViewBag.PageName = "Employee";
            return View("Unauthorized");
        }

        public ActionResult SelectPFStatus()
        {
            return PartialView("_EmployeePFMonthlyStatus");
        }

        /// <summary>
        /// _s the select pf status.
        /// </summary>
        /// <param name="empID">The emp identifier.</param>
        /// <param name="fromDate">From date.</param>
        /// <param name="toDate">To date.</param>
        /// <returns></returns>
        /// <ModifiedBy>Avishek</ModifiedBy>
        /// <ModificationDate>Dec-15-2015</ModificationDate>
        [GridAction]
        public ActionResult _SelectPFStatus(string empID, DateTime? fromDate, DateTime? toDate)
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            DateTime dateFrom = DateTime.MinValue;
            DateTime dateTo = DateTime.MaxValue;
            dateFrom = fromDate == null ? DateTime.MinValue : fromDate.GetValueOrDefault();
            if (toDate == null)
            {
                dateTo = DateTime.MaxValue;
            }
            else
            {
                dateTo = toDate.GetValueOrDefault();
                dateTo = dateTo.AddDays(1).AddSeconds(-1);
            }

            //var _empID = unitOfWork.CustomRepository.GetEmployeeByIdentificationNo(empID).FirstOrDefault().EmpID;
            if (empID != "")
            {
                return View(new GridModel(GetEmployeesPFStatus(empID, dateFrom, dateTo)));
            }
            return View(new GridModel<VM_PFMonthlyStatus>
            {
                Data = Enumerable.Empty<VM_PFMonthlyStatus>()
            });
        }

        /// <summary>
        /// Gets the employees pf status.
        /// </summary>
        /// <param name="empID">The emp identifier.</param>
        /// <param name="fromDate">From date.</param>
        /// <param name="toDate">To date.</param>
        /// <returns>list</returns>
        /// <ModifiedBy>Avishek</ModifiedBy>
        /// <ModificationDate>Dec-19-2015</ModificationDate>
        private IEnumerable<VM_PFMonthlyStatus> GetEmployeesPFStatus(string empID, DateTime? fromDate, DateTime? toDate)
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            List<VM_PFMonthlyStatus> result;
            //Added By Avishek Date:Feb-18-2015
            //result = unitOfWork.CustomRepository.EmpPFMonthlyStatus(OCode).Where(w => w.EmpID == empID).ToList();
            CultureInfo cInfo = new CultureInfo("en-IN");
            var v = unitOfWork.EmployeesRepository.Get(x => x.IdentificationNumber == empID).FirstOrDefault();
            _mvcApplication = new MvcApplication();
            // now result = unitOfWork.CustomRepository.EmpPFMonthlyStatus(OCode, v.EmpID).Where(w => w.ProcessRunDate >= fromDate && w.ProcessRunDate <= toDate).ToList();
            //End


            var allContrebution = unitOfWork.CustomRepository.EmpPFMonthlyStatus(oCode, v.EmpID).Where(x => x.SelfContribution != 0).ToList();
            var previousContribution = allContrebution.Where(x => x.ProcessRunDate < fromDate).ToList();
            var _empID = unitOfWork.CustomRepository.GetEmployeeByIdentificationNo(empID).FirstOrDefault().EmpID;//Added by Izab Date: Jun-24-2020

           // List<tbl_ProfitDistributionDetail> profit = unitOfWork.CustomRepository.GetProfitDistributionByEmpId(Convert.ToInt16(empID)).ToList();//Added by Avishek Date: Oct-4-2015
            List<tbl_ProfitDistributionDetail> profit = unitOfWork.CustomRepository.GetProfitDistributionByEmpId(_empID).ToList();//Added by Izab Date: Jun-24-2020

            result = allContrebution.Where(w => w.ProcessRunDate >= fromDate && w.ProcessRunDate <= toDate).ToList();
            if (previousContribution.Count > 0 || previousContribution != null)
            {
                VM_PFMonthlyStatus _VM_PFMonthlyStatus = new VM_PFMonthlyStatus();
                for (int i = 0; i < 1; i++)
                {
                    _VM_PFMonthlyStatus.MonthYear = "Opening Balance";
                    _VM_PFMonthlyStatus.SelfContribution = (decimal)(previousContribution.Sum(x => x.SelfContribution) + v.opOwnContribution);
                    _VM_PFMonthlyStatus.EmpContribution = (decimal)(previousContribution.Sum(x => x.EmpContribution) + v.opEmpContribution);
                    //_VM_PFMonthlyStatus.SCInterest = (decimal)(previousContribution.Sum(x => x.SCInterest) + v.opProfit);//CommentOut by Avishek Date: Oct-4-2015
                    _VM_PFMonthlyStatus.SCInterest = _mvcApplication.GetNumber(profit.Where(x => x.TransactionDate < fromDate).Sum(x => x.DistributedAmount) ?? 0);//Added by Avishek Date: Oct-4-2015
                    _VM_PFMonthlyStatus.ProcessRunDate = DateTime.MinValue;
                    result.Add(_VM_PFMonthlyStatus);
                }
            }

            decimal cumulativeEmpContribution = 0;
            decimal cumulativeSelfContribution = 0;
            foreach (var item in result.OrderBy(x => x.ProcessRunDate))
            {
                cumulativeSelfContribution = _mvcApplication.GetNumber(cumulativeSelfContribution) + _mvcApplication.GetNumber(item.SelfContribution);
                cumulativeEmpContribution = _mvcApplication.GetNumber(cumulativeEmpContribution) + _mvcApplication.GetNumber(item.EmpContribution);
                item.CEC = _mvcApplication.GetNumber(cumulativeEmpContribution).ToString("N", cInfo);
                item.CSC = _mvcApplication.GetNumber(cumulativeSelfContribution).ToString("N", cInfo);
                item.EMP = _mvcApplication.GetNumber(item.EmpContribution).ToString("N", cInfo);
                item.Self = _mvcApplication.GetNumber(item.SelfContribution).ToString("N", cInfo);
                item.SUM = _mvcApplication.GetNumber(item.Total).ToString("N", cInfo);
                //item.Profit = _mvcApplication.GetNumber(item.SCInterest).ToString("N", CInfo);//CommentOut by Avishek Date: Oct-4-2015
                item.Profit = _mvcApplication.GetNumber(Convert.ToDecimal(profit.Where(x => x.TransactionDate.Value.Month == Convert.ToInt32(item.Month) && x.TransactionDate.Value.Year == Convert.ToInt32(item.Year)).Select(x => x.DistributedAmount).FirstOrDefault())).ToString();//Added by Avishek Date: Oct-4-2015
                item.MonthYear = item.ContrebutionDate == DateTime.MinValue ? "Opening Balance" : item.ContrebutionDate.ToString("MMMM, yyyy");
            }

            int countProfitWithContribution = result.Count(x => x.Profit != "0.00");

            //if (countProfitWithContribution > 0)
            //{
            if (countProfitWithContribution < profit.Count)
            {
                int countProfitNo = profit.Count;
                VM_PFMonthlyStatus atblProfitDistributionDetail = new VM_PFMonthlyStatus();
                atblProfitDistributionDetail.Self = 0.ToString();
                atblProfitDistributionDetail.EMP = 0.ToString();
                atblProfitDistributionDetail.SUM = 0.ToString();
                atblProfitDistributionDetail.CSC = 0.ToString();
                atblProfitDistributionDetail.CEC = 0.ToString();
                atblProfitDistributionDetail.ProcessRunDate = profit[profit.Count - 1].TransactionDate.GetValueOrDefault();
                DateTime date = profit[countProfitNo - 1].TransactionDate.GetValueOrDefault();
                atblProfitDistributionDetail.Year = date.Year.ToString();
                atblProfitDistributionDetail.Month = date.Month.ToString();
                atblProfitDistributionDetail.MonthYear = profit[countProfitNo - 1].TransactionDate.GetValueOrDefault().ToString("MMMM, yyyy");
                atblProfitDistributionDetail.Profit = _mvcApplication.GetNumber(Convert.ToDecimal(profit.Where(x => x.TransactionDate.Value.Month == Convert.ToInt32(date.Month.ToString()) && x.TransactionDate.Value.Year == Convert.ToInt32(date.Year.ToString())).Select(x => x.DistributedAmount).FirstOrDefault())).ToString();
                result.Add(atblProfitDistributionDetail);
            }
            //}

            return result.OrderBy(x => x.ProcessRunDate);
        }

        public JsonResult AutocompleteSuggestions(string term)
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            //Commented by Fahim 07/12/2015
            //var suggestions = unitOfWork.EmployeesRepository.Get().Where(w => w.IdentificationNumber.Contains(term) && w.OCode == OCode && w.PFStatus != 2).Select(s => new {  value = s.EmpID, label = s.IdentificationNumber }).ToList();
            var suggestions = unitOfWork.EmployeesRepository.Get().Where(w => w.IdentificationNumber.ToLower().Trim().Contains(term.ToLower().Trim()) && w.OCode == oCode).Select(s => new { value = s.EmpName, label = s.IdentificationNumber }).ToList();
            return Json(suggestions, JsonRequestBehavior.AllowGet);
        }

        //Added by Fahim 07/12/2015
        public JsonResult AutocompleteSuggestionsName(string term)
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            term = term.ToLower();
            var suggestions = unitOfWork.EmployeesRepository.Get().Where(w => w.EmpName.ToLower().Contains(term.ToLower().Trim()) && w.OCode == oCode).Select(s => new { value = s.IdentificationNumber, label = s.EmpName }).ToList();
            return Json(suggestions, JsonRequestBehavior.AllowGet);
        }
        //End Fahim

        /// <summary>
        /// Gets the employee by identifier.
        /// </summary>
        /// <param name="empID">The emp identifier.</param>
        /// <returns></returns>
        /// <ModifiedBy>Avishek</ModifiedBy>
        /// <ModificationDate>Dec-15-2015</ModificationDate>
        public ActionResult GetEmployeeByID(string empID)
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                RedirectToAction("Login", "Account");
            }
            CultureInfo cInfo = new CultureInfo("en-IN");
            var v = unitOfWork.CustomRepository.GetEmployeeByIdentificationNo(empID).FirstOrDefault();
            ViewBag.EmpID = empID;
            ViewBag.EditUserName = v.EditUserName + "-" + v.EditDate + "-" + v.Comment;
            tbl_Employees tblEmployees = unitOfWork.CustomRepository.GetEmployeeById(v.EmpID).FirstOrDefault();
            List<tbl_Contribution> tblContribution = unitOfWork.CustomRepository.GetCurrentContributions(v.EmpID).ToList();
            VM_Employee vmEmployee = new VM_Employee();
            vmEmployee.EmpName = tblEmployees.EmpName;
            vmEmployee.opDesignationName = tblEmployees.opDesignationName;
            vmEmployee.opDepartmentName = tblEmployees.opDepartmentName;
            vmEmployee.ContactNumber = tblEmployees.ContactNumber;
            vmEmployee.Email = tblEmployees.Email;
            vmEmployee.JoiningDate = tblEmployees.JoiningDate;
            vmEmployee.PFActivationDate = tblEmployees.PFActivationDate;
            vmEmployee.PresentAddress = tblEmployees.PresentAddress;
            vmEmployee.opOwnContribution = tblEmployees.opOwnContribution.GetValueOrDefault();
            vmEmployee.opEmpContribution = tblEmployees.opEmpContribution.GetValueOrDefault();
            vmEmployee.opLoan = tblEmployees.opLoan.GetValueOrDefault();
            vmEmployee.opLoan = vmEmployee.opLoan == 0 ? 0 : _mvcApplication.GetNumber(vmEmployee.opLoan);
            vmEmployee.opProfit = tblEmployees.opProfit.GetValueOrDefault();
            vmEmployee.PFStatusID = Convert.ToInt32(tblEmployees.PFStatus);
            ViewBag.CurrentOwnCont = (tblContribution.Sum(x => x.EmpContribution)).ToString("N", cInfo);
            ViewBag.CurrentEmpCont = (tblContribution.Sum(x => x.EmpContribution)).ToString("N", cInfo);
            return PartialView("_EmployeeDetail", vmEmployee);
        }

    }
}
