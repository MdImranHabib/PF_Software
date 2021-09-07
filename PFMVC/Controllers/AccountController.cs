using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using Microsoft.Web.WebPages.OAuth;
using PFMVC.Models;
using DLL;
using DLL.Repository;
using System.Net;
using PFMVC.DB;
using PFMVC.common;
using System.Management;

namespace PFMVC.Controllers
{
    [Authorize]
    //[InitializeSimpleMembership]
    public class AccountController : Controller
    {
        //
        // GET: /Account/Login

        [AllowAnonymous]
        public ActionResult Login()
        {
            PFTMEntities entities = new PFTMEntities();
            var companyInfo = (from a in entities.LU_tbl_CompanyInformation.Where(w => w.OCode == 1) select a).FirstOrDefault().CompanyName;
            ViewBag.CompanyName = companyInfo;
            ViewBag.Message = TempData["Message"] as string;
            // ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel model, string returnUrl)
        {
            // Lets first check if the Model is valid or not
            if (ModelState.IsValid)
            {
                //Added By Avishek Date Oct-4-2015
                string processorId = GetProcessorId();       
                //if (processorID != "BFEBFBFF000206A7")
                //if (processorId != "BFEBFBFF000306A9")//KDS Server
                //{
                //    ModelState.AddModelError("", "You are 'Illigully' use this softwre");
                //    return View(model);
                //}
                //End
                using (PFTMEntities entities = new PFTMEntities())
                {
                    string username = model.UserName;
                    Session["userName"] = username.ToString();
                    string password = GetMD5HashPassword.GetMd5Hash(model.Password);
                    string companyName = model.Company;

                    if (!entities.Database.Exists())
                    {
                        ModelState.AddModelError("", "DB server is down!");
                        goto A;
                    }
                    //This is written for avoid more than 9 company for Emergine
                    double count = entities.LU_tbl_CompanyInformation.Count();
                    if (count >= 9)
                    {
                        RedirectToAction("Login", new { model = model, returnUrl = returnUrl });
                    }
                    //end
                    var isUserExist = (from a in entities.tbl_User.Where(w => w.IsActive == 1)
                                       join b in entities.tbl_UserPassword
                                       on a.UserID equals b.UserID
                                       join c in entities.LU_tbl_CompanyInformation on a.OCode equals c.OCode
                                       where a.LoginName == username && b.Password == password && c.CompanyName == companyName
                                       select a).SingleOrDefault();
                    int userCount = entities.tbl_User.Count();
                    //bool userValid = entities.tbl_User.Any(user => user.LoginName == username && user. == password);

                    // User found in the database
                    if (isUserExist != null)
                    {
                        //log the user
                        try
                        {
                            using (UnitOfWork unitOfWork = new UnitOfWork())
                            {

                                tbl_LoginHistory v = new tbl_LoginHistory();
                                v.UserName = model.UserName;
                                v.LoginTime = System.DateTime.Now;
                                try
                                {
                                    v.Terminal = Request.ServerVariables["REMOTE_ADDR"];
                                }
                                catch
                                {
                                    v.Terminal = System.Web.HttpContext.Current.Request.UserHostAddress;
                                }
                                v.HostName = Dns.GetHostName();

                                unitOfWork.LoginHistoryRepository.Insert(v);
                                unitOfWork.Save();
                            }
                        }
                        catch
                        {
                            //should not catch
                        }

                        FormsAuthentication.SetAuthCookie(username, false);
                        if (isUserExist.OCode > 0)
                        {
                            Session["OCode"] = isUserExist.OCode;
                            Session["EmpId"] = null;
                        }
                        else
                        {
                            Session["OCode"] = null;
                            Session["EmpId"] = null;
                        }

                        if (isUserExist.EmpID > 0 && (isUserExist.RoleID != 1 || isUserExist.RoleID == null ))
                        {
                            Session["EmpId"] = isUserExist.EmpID;
                            //return RedirectToAction("Index", "WebUserReport", new { Area = "Report" });
                            return RedirectToAction("Contact", "Home");

                        }
                        if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                            && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                        {
                            return Redirect(returnUrl);
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }

                    }
                    else if (userCount == 0)
                    {
                        //this means database in initial state
                        if (model.UserName == "initial" && model.Password == "initial")
                        {
                            string message = "";
                            if (DBFeed.InitialFeed(ref message))
                            {
                                RedirectToAction("Login", new { model = model, returnUrl = returnUrl });
                            }
                            else
                            {
                                ModelState.AddModelError("", "Error while initializing database..." + message);
                            }
                        }
                        else
                        {
                            ModelState.AddModelError("", "Please Initialize Database...contact developer!");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Username or password is incorrect.");
                    }
                }
            }
        A:
            // If we got this far, something failed, redisplay form
            return View(model);
        }

        /// <summary>
        /// Gets the processor identifier.
        /// </summary>
        /// <returns>string</returns>
        /// <CreatredBy>Avishek</CreatredBy>
        /// <CreatedDate>Oct-4-2015</CreatedDate>
        public String GetProcessorId()
        {
            String processorId = "";
            try
            {
                ManagementObjectSearcher searcherProcessor = new ManagementObjectSearcher("root\\CIMV2","SELECT * FROM Win32_Processor");
                foreach (ManagementObject queryObj in searcherProcessor.Get())
                {
                    processorId = queryObj["ProcessorId"].ToString();
                }
                return processorId;
            }
            catch (Exception) { return processorId; }
        }


        //[HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            try
            {
                using (UnitOfWork unitOfWork = new UnitOfWork())
                {
                    var v = unitOfWork.LoginHistoryRepository.Get().Where(u => u.UserName.ToLower() == User.Identity.Name.ToLower()).OrderByDescending(o => o.rowid).FirstOrDefault();
                    if (v != null)
                    {
                        v.SignOut = System.DateTime.Now;
                    }
                    unitOfWork.Save();
                    Session["OCode"] = null;
                    Session["EmpId"] = null;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "Account", new { area = "" });
        }

        //
        // GET: /Account/Register

        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                // Attempt to register the user
                try
                {
                    return RedirectToAction("Index", "Home");
                }
                catch (MembershipCreateUserException e)
                {
                    ModelState.AddModelError("", ErrorCodeToString(e.StatusCode));
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }



        #region Helpers
        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
        }

        internal class ExternalLoginResult : ActionResult
        {
            public ExternalLoginResult(string provider, string returnUrl)
            {
                Provider = provider;
                ReturnUrl = returnUrl;
            }

            public string Provider { get; private set; }
            public string ReturnUrl { get; private set; }

            public override void ExecuteResult(ControllerContext context)
            {
                OAuthWebSecurity.RequestAuthentication(Provider, ReturnUrl);
            }
        }

        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "User name already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A user name for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }
        #endregion
    }
}
