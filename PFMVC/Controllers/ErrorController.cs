using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using DLL;
using DLL.Repository;

namespace PFMVC.Controllers
{
    public class ErrorController : Controller
    {

        UnitOfWork unitOfWork = new UnitOfWork();
        
        public ActionResult Index(string excep, string innerExcep)
        {
            ViewBag.Exception = excep;
            ViewBag.InnerException = innerExcep;
            return View();
        }

        [Authorize]
        public ActionResult ErrorList()
        {
            var v = unitOfWork.ErrorLogRepository.Get().OrderByDescending(o => o.ID).ToList();
            return View(v);
        }

        /// <summary>
        /// Logs the application error.
        /// </summary>
        /// <param name="Message">The message.</param>
        /// <param name="InnerException">The inner exception.</param>
        /// <ModifiedBy>Avishek</ModifiedBy>
        /// <ModificationDate>Mar-6-2016</ModificationDate>
        public void LogApplicationError(string Message, string InnerException)
        {
            tbl_ErrorLog errorLog = new tbl_ErrorLog();
            errorLog.Message = Message;
            errorLog.InnerException = InnerException + "";
            errorLog.UserName = User.Identity.Name;
            errorLog.Time = DateTime.Now;
            errorLog.Type = "Application Error";
            try
            {
                errorLog.Terminal = Request.ServerVariables["REMOTE_ADDR"];
            }
            catch
            {
                errorLog.Terminal = System.Web.HttpContext.Current.Request.UserHostAddress;
            }
            errorLog.HostName = Dns.GetHostName();
            try
            {
                unitOfWork.ErrorLogRepository.Insert(errorLog);
                unitOfWork.Save();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
