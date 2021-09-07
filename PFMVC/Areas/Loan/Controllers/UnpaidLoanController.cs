using System;
using System.Web.Mvc;
using DLL.Repository;
using System.Globalization;

namespace PFMVC.Areas.Loan.Controllers
{
    public class UnpaidLoanController : Controller
    {

        int PageID = 3;
        MvcApplication _mvcApplication = new MvcApplication();
        CultureInfo _info = new CultureInfo("en-IN");
        private UnitOfWork unitOfWork = new UnitOfWork();
       
        /// <summary>
        /// Indexes this instance.
        /// </summary>
        /// <returns>view</returns>
        /// <ModifiedBy>Avishek</ModifiedBy>
        /// <ModificationDate>Sep-19-2015</ModificationDate>
        public ActionResult Index()
        {
            //Added By Avishek Date:Jan-19-2016
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End
            int year = DateTime.Now.Year;
            int month = DateTime.Now.Month;
            var v = unitOfWork.CustomRepository.UnpaidLoan(year, month, oCode);
            foreach (var item in v) 
            {
                item.Amount = _mvcApplication.GetNumber(item.Amount);
            }
            ViewBag.Message = "Unpaid loan till today...";
            return View(v);
        }

    }
}
