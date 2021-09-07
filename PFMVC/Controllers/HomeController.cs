using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DLL;

namespace PFMVC.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            using (PFTMEntities context = new PFTMEntities())
            {
                if (!context.Database.Exists())
                {
                    return View("ConnectionFailed");
                }

                var user = context.tbl_User.Where(w => w.LoginName == User.Identity.Name).SingleOrDefault();
                if (user != null)
                {
                    if (user.EmpID > 0 && (user.RoleID != 1 || user.RoleID == null))
                    {
                        return RedirectToAction("Index", "WebUserReport", new { Area = "Report" });
                    }
                }
                return View();
            }
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        /// <summary>
        /// Changes the theme.
        /// </summary>
        /// <param name="themename">The themename.</param>
        /// <param name="returnUrl">The return URL.</param>
        /// <returns></returns>
        /// <ModifiedBy>Avishek</ModifiedBy>
        /// <ModificationDate>Mar-6-2016</ModificationDate>
        public ActionResult ChangeTheme(string themename = "", string returnUrl = "")
        {
            HttpCookie cookie = new HttpCookie("Theme");
            cookie.Values["ThemeName"] = themename;
            cookie.Expires = DateTime.Now.AddDays(365);
            Response.Cookies.Add(cookie);
            if (string.IsNullOrEmpty(returnUrl))
            {
                return RedirectToAction("Index", "Home");
            }
            return Redirect(returnUrl);
        }

    }
}
