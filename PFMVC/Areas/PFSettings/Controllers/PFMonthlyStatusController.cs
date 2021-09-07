using System;
using System.Linq;
using System.Web.Mvc;
using DLL.Repository;
using PFMVC.common;
using Telerik.Web.Mvc;
using System.Globalization;

namespace PFMVC.Areas.PFSettings.Controllers
{
    public class PFMonthlyStatusController : Controller
    {

        int PageID = 5;
        private UnitOfWork unitOfWork = new UnitOfWork();
        MvcApplication _MvcApplication;

        [Authorize]
        public ActionResult PFMonthlyStatus()
        {
            //Added By Avishek Date:Jan-19-2016
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End
            bool b = PagePermission.HasPermission(User.Identity.Name, PageID, 0);
            if (b)
            {
                return View("PFMonthlyStatus");
            }
            ViewBag.PageName = "PF Core";
            return View("Unauthorized");
        }

        /// <summary>
        /// Pfs the month hierarchy ajax.
        /// </summary>
        /// <returns>View</returns>
        /// <ModifiedBy>Avishek</ModifiedBy>
        /// <ModifiedDate>Sep-28-2015</ModifiedDate>
        [GridAction]
        public ActionResult PFMonthHierarchyAjax()
        {
            _MvcApplication = new MvcApplication();
            int oCode = ((int?)Session["OCode"]) ?? 0;
            CultureInfo cInfo = new CultureInfo("en-IN");
            var v = unitOfWork.CustomRepository.PFMonthlyStatus(oCode).OrderBy(x=>x.ProcessRunDate).ToList();
            decimal totalCon = 0;
            decimal totalSelfCon = 0;
            decimal totalEmpCon = 0;
            foreach(var item in v){
                item.SUM = _MvcApplication.GetNumber(item.Total).ToString("N", cInfo);
                item.Self = _MvcApplication.GetNumber(item.SelfContribution).ToString("N", cInfo);
                item.EMP = _MvcApplication.GetNumber(item.EmpContribution).ToString("N", cInfo);
                totalCon += _MvcApplication.GetNumber(item.Total);
                totalSelfCon += _MvcApplication.GetNumber(item.SelfContribution);
                totalEmpCon += _MvcApplication.GetNumber(item.EmpContribution);
                item.MonthYear = item.ContrebutionDate.ToString("MMMM, yyyy");
            }
            ViewBag.Total = "Total Self Contrebution: " + totalSelfCon.ToString("N", cInfo) + "  Total Emp Contrebution: " + totalEmpCon.ToString("N", cInfo) + "  Total: " + totalCon.ToString("N", cInfo);
            return View(new GridModel(v));
        }

        /// <summary>
        /// _s the employees for pf month hierarchy ajax.
        /// </summary>
        /// <param name="month">The month.</param>
        /// <returns>View</returns>
        /// <ModifiedBy>Avishek</ModifiedBy>
        [GridAction]
        public ActionResult _EmployeesForPFMonthHierarchyAjax(string month)
        {
            DateTime datetime;
            DateTime.TryParse(month, out datetime);
            new CultureInfo("en-IN");
            var employees = unitOfWork.CustomRepository.GetContributionDetail().Where(w => w.ConMonth == datetime.Month + "" && w.ConYear == datetime.Year + "");
            return View(new GridModel(employees));
        }

    }
}
