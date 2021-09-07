using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CustomJsonResponse;
using DLL;
using DLL.Repository;
using DLL.ViewModel;
using PFMVC.common;
using PFMVC.Models;
using Telerik.Web.Mvc;

namespace PFMVC.Areas.UserManagement.Controllers
{
    public class UserManagementController : Controller
    {
        int _pageId = 1;
        private UnitOfWork unitOfWork = new UnitOfWork();


        /// <summary>
        /// Indexes this instance.
        /// </summary>
        /// <returns>View</returns>
        /// <ModifyBy>Avishek</ModifyBy>
        /// <modificationDate>Mar-6-2016</modificationDate>
        [Authorize]
        public ActionResult Index()
        {
            bool b = PagePermission.HasPermission(User.Identity.Name, _pageId, 0);
            //if valid go to UserManagement Page
            if (b)
            {
                return View();
            }
            ViewBag.PageName = "User Management";
            return View("Unauthorized");
        }

        //public ActionResult FinancialAuditHistory()
        //{
        //    bool b = PagePermission.HasPermission(User.Identity.Name, _pageId, 0);
        //    if (b)
        //    {
        //        return View();
        //    }
        //    ViewBag.PageName = "";
        //    return View("Unauthorized");
        //}


        /// <summary>
        /// _s the select users login history.
        /// </summary>
        /// <returns></returns>
        ///  <ModifyBy>Avishek</ModifyBy>
        /// <modificationDate>Mar-6-2016</modificationDate>
        [GridAction]
        public ActionResult _SelectUsersLoginHistory()
        {
            return View(new GridModel(GetUserLoginHistory()));
        }


        private IEnumerable<VM_UserLoginHistory> GetUserLoginHistory()
        {
            var result = unitOfWork.CustomRepository.UserLoginHistory().ToList();
            return result;
        }


        [GridAction]
        public ActionResult _SelectUsers()
        {
            return View(new GridModel(GetSystemUserList()));
        }


        private IEnumerable<VM_UserInfo> GetSystemUserList()
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            var result = unitOfWork.CustomRepository.GetSystemUserList(oCode).ToList();
            return result;
        }


        /// <summary>
        /// Gets the role list.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <returns></returns>
        /// <ModifyBy>Avishek</ModifyBy>
        /// <modificationDate>Mar-6-2016</modificationDate>
        public ActionResult GetRoleList(string userName)
        {
            string[] allRoles = GetRoles.GetAllRoles();
            if (allRoles != null)
            {
                List<VM_UserRole> v = new List<VM_UserRole>();
                VM_UserRole r;
                //List<ArunimaInventoryV2.Booking> list = result.ToList();
                string roleForUser = GetRoles.GetRoleForUser(userName);
                string[] relatedRecords = new string[allRoles.Count()];
                string[] checkedRecords = new string[roleForUser.Count()];

                int j = 0;
                for (int i = 0; i < allRoles.Count(); i++)
                {
                    relatedRecords[i] = allRoles[i];
                    if (roleForUser.Contains(allRoles[i]))
                    {
                        checkedRecords[j] = allRoles[i];
                        j++;
                    }
                    r = new VM_UserRole();
                    r.RoleName = allRoles[i];
                    v.Add(r);
                }
                ViewData["relatedRecords"] = relatedRecords;
                ViewData["checkedRecords"] = checkedRecords;
                ViewData["userName"] = userName;
                return PartialView("_UserRoles", v);
            }
            return PartialView("_UserRoles", null);
        }

        /// <summary>
        /// Gets the role list.
        /// </summary>
        /// <param name="checkedRecords">The checked records.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="relatedRecords">The related records.</param>
        /// <returns></returns>
        /// <ModifyBy>Avishek</ModifyBy>
        /// <modificationDate>Mar-6-2016</modificationDate>
        [HttpPost]
        public ActionResult GetRoleList(string[] checkedRecords, string userName, string[] relatedRecords)
        {
            bool b = PagePermission.HasPermission(User.Identity.Name, _pageId, 1);
            if (!b)
            {
                return Json(new { Success = false, ErrorMessage = "Sorry, you are not authorized to modify role management." }, JsonRequestBehavior.AllowGet);
            }
            if (checkedRecords != null)
            {
                if (checkedRecords.Count() > 1)
                {
                    return Json(new { Success = false, ErrorMessage = "Every user should be assign to maximum one role..." }, JsonRequestBehavior.AllowGet);
                }

                string curentRoles = GetRoles.GetRoleForUser(userName);//System.Web.Security.Roles.GetRolesForUser(userName);
                foreach (var item in relatedRecords.Where(item => checkedRecords.Contains(item)))
                {
                    if (curentRoles.Contains(item))
                    {

                    }
                    else
                    {
                        //Roles.AddUserToRole(userName, item);
                        if (!Roles.AddUserToRole(userName, item))
                        {
                            return Json(new { Success = false, Message = "Error occured! check Roles method..." }, JsonRequestBehavior.AllowGet);
                        }
                        break; // each user relates with only single role...
                    }
                }
            }
            else
            {
                if (!Roles.RemoveUserFromRole(userName))
                {
                    return Json(new { Success = false, Message = "Error occured! check Roles method..." }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { Success = true, Message = "Roles Updated" }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Gets the user information.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        /// <ModifyBy>Avishek</ModifyBy>
        /// <modificationDate>Mar-6-2016</modificationDate>
        public ActionResult GetUserInfo(Guid id)
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            VM_UserInfo v = unitOfWork.CustomRepository.GetSystemUserList(oCode).FirstOrDefault(m => m.UserId == id);
            //DepartmentOptions(v.DepartmentID);
            //var d = unitOfWork.BranchRepository.Get().ToList();
            //ViewData["BranchOptions"] = new SelectList(d, "BranchID", "BranchName", v.BranchID ?? "");
            CompanyOptions(v.CompanyID + "");
            return PartialView("_UserInfo", v);
        }

        /// <summary>
        /// Gets the user information.
        /// </summary>
        /// <param name="v">The v.</param>
        /// <returns></returns>
        /// <ModifyBy>Avishek</ModifyBy>
        /// <modificationDate>Mar-6-2016</modificationDate>
        [HttpPost]
        public ActionResult GetUserInfo(VM_UserInfo v)
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            bool b = PagePermission.HasPermission(User.Identity.Name, _pageId, 2);
            if (!b)
            {
                return Json(new { Success = false, ErrorMessage = "Sorry, you are not authorized to modify user information." }, JsonRequestBehavior.AllowGet);
            }
            ModelState.Remove("ConfirmPassword");
            ModelState.Remove("Password");
            if (ModelState.IsValid)
            {
                bool isUserNameExist = unitOfWork.UserProfileRepository.IsExist(filter: s => s.LoginName == v.UserName && s.UserID != v.UserId);
                if (isUserNameExist)
                {
                    return Json(new { Success = false, ErrorMessage = "This username already exist!!! Try another one..." }, JsonRequestBehavior.DenyGet);
                }
                tbl_User u = new tbl_User();
                u = unitOfWork.UserProfileRepository.Get(w => w.UserID == v.UserId).Single();
                u.BranchID = v.BranchID;
                u.DepartmentID = v.DepartmentID;
                u.Email = v.Email;
                u.EmailNotificationActive = v.EmailNotificationActive ? (byte)1 : (byte)0;
                u.IsActive = v.IsActive == true ? (byte)1 : (byte)0;
                u.Phone = v.Phone;
                u.UserFullName = v.FullName;
                //u.OCode = v.CompanyID;
                u.EditDate = DateTime.Now;
                u.EditUser = unitOfWork.CustomRepository.GetUserID(User.Identity.Name);
                u.OCode = oCode;
                unitOfWork.UserProfileRepository.Update(u);
                try
                {
                    unitOfWork.Save();
                }
                catch (Exception x)
                {
                    return Json(new { Success = false, ErrorMessage = "Problem while updating user info." + x.Message }, JsonRequestBehavior.DenyGet);
                }

                return Json(new { Success = true, Message = "User information updated successfully." }, JsonRequestBehavior.DenyGet);
            }
            var errorList = (from item in ModelState.Values
                             from error in item.Errors
                             select error.ErrorMessage).ToList();

            return Json(JsonResponseFactory.ErrorResponse(errorList), JsonRequestBehavior.DenyGet);
        }

        /// <summary>
        /// Gets the module list.
        /// </summary>
        /// <param name="roleName">Name of the role.</param>
        /// <returns></returns>
        /// <ModifyBy>Avishek</ModifyBy>
        /// <modificationDate>Mar-6-2016</modificationDate>
        public ActionResult GetModuleList(string roleName) // Jobno is unique so no problem if I search with jobNo.
        {
            int roleId = Roles.GetRoleID(roleName);
            var result = unitOfWork.CustomRepository.GetModuleList(roleId).OrderBy(f => f.ModuleName).ToList();

            return PartialView("_RoleWiseModuleAccess", result);
        }

        /// <summary>
        /// Gets the module list.
        /// </summary>
        /// <param name="v">The v.</param>
        /// <returns></returns>
        /// <ModifyBy>Avishek</ModifyBy>
        /// <modificationDate>Mar-6-2016</modificationDate>
        [HttpPost]
        public ActionResult GetModuleList(List<VM_RolesInModules> v)
        {
            bool b = PagePermission.HasPermission(User.Identity.Name, _pageId, 1);
            if (!b)
            {
                return Json(new { Success = false, ErrorMessage = "Sorry, you are not authorized to modify module management." }, JsonRequestBehavior.AllowGet);
            }
            int roleId = 0;
            int pageId = 0;
            if (v.Count > 0)
            {
                roleId = v[0].RoleID;
                pageId = v[0].PageID;
            }
            else
            {
                return Json(new { Success = false, Message = "No data saved!!!" }, JsonRequestBehavior.AllowGet);
            }

            SA_tbl_PagePermission sa_tbl_pagePermission;
            List<SA_tbl_PagePermission> lst_sa_pagePermission = unitOfWork.PagePermissionRepository.Get().Where(w => w.RoleID == roleId).ToList();

            DateTime datetime = System.DateTime.Now;
            Guid user = unitOfWork.CustomRepository.GetUserID(User.Identity.Name);

            foreach (var item in v)
            {
                if (lst_sa_pagePermission.Any(w => w.RoleID == item.RoleID && w.PageID == item.PageID))
                {
                    sa_tbl_pagePermission = unitOfWork.PagePermissionRepository.Get(w => w.RoleID == item.RoleID && w.PageID == item.PageID).Single();
                    sa_tbl_pagePermission.PageID = item.PageID;
                    sa_tbl_pagePermission.RoleID = item.RoleID;
                    sa_tbl_pagePermission.CanVisit = item.CanVisit;
                    sa_tbl_pagePermission.CanEdit = item.CanEdit;
                    sa_tbl_pagePermission.CanDelete = item.CanDelete;
                    sa_tbl_pagePermission.CanExecute = item.CanExecute;
                    sa_tbl_pagePermission.EditDate = datetime;
                    sa_tbl_pagePermission.EditUser = user;
                    unitOfWork.PagePermissionRepository.Update(sa_tbl_pagePermission);
                }
                else
                {
                    sa_tbl_pagePermission = new SA_tbl_PagePermission();
                    sa_tbl_pagePermission.PageID = item.PageID;
                    sa_tbl_pagePermission.RoleID = item.RoleID;
                    sa_tbl_pagePermission.CanVisit = item.CanVisit;
                    sa_tbl_pagePermission.CanEdit = item.CanEdit;
                    sa_tbl_pagePermission.CanDelete = item.CanDelete;
                    sa_tbl_pagePermission.CanExecute = item.CanExecute;
                    sa_tbl_pagePermission.EditDate = datetime;
                    sa_tbl_pagePermission.EditUser = user;
                    unitOfWork.PagePermissionRepository.Insert(sa_tbl_pagePermission);
                }
            }
            try
            {
                unitOfWork.Save();
                return Json(new { Success = true, Message = "Roles Updated..." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception x)
            {
                return Json(new { Success = false, Message = "Error: " + x.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Registers this instance.
        /// </summary>
        /// <returns></returns>
        /// <ModifyBy>Avishek</ModifyBy>
        /// <modificationDate>Mar-6-2016</modificationDate>
        //public int[] CurrentModuleAccessList(int roleID)
        //{
        //    var result = unitOfWork.CustomRepository.GetModuleList(roleID);
        //    int[] currentModuleAccessList;
        //    if (result != null)
        //    {
        //        List<VM_RolesInModules> list = result.ToList();
        //        currentModuleAccessList = new int[list.Count];

        //        for (int i = 0; i < list.Count; i++)
        //        {
        //            currentModuleAccessList[i] = list[i].RoleID == 0 ? 0 : list[i].ModuleID;
        //        }
        //    }
        //    else
        //    {
        //        currentModuleAccessList = new int[0];
        //    }
        //    return currentModuleAccessList;
        //}

        public ActionResult Register()
        {
            //var d = unitOfWork.BranchRepository.Get().ToList();
            //ViewData["BranchOptions"] = new SelectList(d, "BranchID", "BranchName", "");
            //DepartmentOptions();
            //CompanyOptions();
            EmployeOption();
            ViewData["CompanyId"] = ((int?)Session["OCode"]) ?? 0;
            return PartialView("_CreateNewUser");
        }

        /// <summary>
        /// Registers the specified model.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        /// <ModifyBy>Avishek</ModifyBy>
        /// <modificationDate>Mar-6-2016</modificationDate>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(VM_UserInfo model)
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            bool b = PagePermission.HasPermission(User.Identity.Name, _pageId, 0);
            if (!b)
            {
                return Json(new { Success = false, ErrorMessage = "Sorry, you are not authorized to register a new member." }, JsonRequestBehavior.AllowGet);
            }
            if (ModelState.IsValid)
            {
                try
                {
                    bool isUserNameExist = unitOfWork.UserProfileRepository.IsExist(filter: s => s.LoginName == model.UserName);
                     string EmpID = model.IdentificationNumber;
                    if (!isUserNameExist)
                    {
                        //DbTransaction tran = db.Database.Connection.BeginTransaction();
                        try
                        {
                            tbl_User userProfile = new tbl_User();
                            userProfile.UserID = Guid.NewGuid();
                            userProfile.LoginName = model.UserName;
                            userProfile.UserFullName = model.FullName;
                            userProfile.Phone = model.Phone;
                            userProfile.IsActive = 1;
                            userProfile.RoleID = 2;
                            userProfile.EmailNotificationActive = 0;
                            userProfile.Email = model.Email;
                            userProfile.OCode = model.CompanyID;
                            userProfile.EditDate = System.DateTime.Now;
                            userProfile.EditUser = unitOfWork.CustomRepository.GetUserID(User.Identity.Name);
                            userProfile.OCode = oCode;
                            //userProfile.EmpID = unitOfWork.EmployeesRepository.Get().Where(x => x.IdentificationNumber == model.IdentificationNumber).Select(x => x.EmpID).FirstOrDefault();
                            userProfile.EmpID = Convert.ToInt32(EmpID);

                            unitOfWork.UserProfileRepository.Insert(userProfile);

                            tbl_UserPassword membership = new tbl_UserPassword();
                            membership.UserID = userProfile.UserID;
                            membership.Password = GetMD5HashPassword.GetMd5Hash(model.Password);
                            membership.EditDate = System.DateTime.Now;
                            membership.EditUser = unitOfWork.CustomRepository.GetUserID(User.Identity.Name);
                            unitOfWork.UserPasswordRepository.Insert(membership);
                            unitOfWork.Save();
                            return Json(new { Success = true, Message = "Successfully added new user!" }, JsonRequestBehavior.DenyGet);
                        }
                        catch (Exception e)
                        {
                            //tran.Rollback();
                            return Json(new { Success = false, ErrorMessage = "Error: " + e.Message }, JsonRequestBehavior.DenyGet);
                        }
                    }
                    return Json(new { Success = false, ErrorMessage = "This user name " + model.UserName + " already exist, try with a differennt one!" }, JsonRequestBehavior.DenyGet);
                }
                catch (Exception e)
                {
                    return Json(new { Success = false, ErrorMessage = "Error: " + e.Message }, JsonRequestBehavior.DenyGet);
                }
            }
            var errorList = (from item in ModelState.Values
                             from error in item.Errors
                             select error.ErrorMessage).ToList();

            return Json(JsonResponseFactory.ErrorResponse(errorList), JsonRequestBehavior.DenyGet);

            // If we got this far, something failed, redisplay form
            //return View(model);
        }

        ///<ModifyBy>Avishek</ModifyBy>
        /// <modificationDate>Mar-6-2016</modificationDate>
        //public string GetMd5Hash(string value)
        //{
        //    var md5Hasher = MD5.Create();
        //    var data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(value));
        //    var sBuilder = new StringBuilder();
        //    for (var i = 0; i < data.Length; i++)
        //    {
        //        sBuilder.Append(data[i].ToString("x2"));
        //    }
        //    return sBuilder.ToString();
        //}


        //user delete possible
        //public ActionResult UserDeletePossible(int userID)
        //{
        //    string s = "";
        //    bool userHasRecord = false;
        //    userHasRecord = unitOfWork.BranchRepository.IsExist(c => c.EditUser == userID);
        //    if (userHasRecord)
        //    {
        //        s = "This user has data in branch database.";
        //        goto A;
        //    }

        //    userHasRecord = unitOfWork.CategoryRepository.IsExist(c => c.EditUser == userID);
        //    if (userHasRecord)
        //    {
        //        s = "This user has data in Category database.";
        //        goto A;
        //    }

        //    userHasRecord = unitOfWork.CountryRepository.IsExist(c => c.EditUser == userID);
        //    if (userHasRecord)
        //    {
        //        s = "This user has data in country database.";
        //        goto A;
        //    }

        //    userHasRecord = unitOfWork.DepartmentRepository.IsExist(c => c.EditUser == userID);
        //    if (userHasRecord)
        //    {
        //        s = "This user has data in Department database.";
        //        goto A;
        //    }
        //    userHasRecord = unitOfWork.DesignationRepository.IsExist(c => c.EditUser == userID);
        //    if (userHasRecord)
        //    {
        //        s = "This user has data in Designation database.";
        //        goto A;
        //    }

        //    userHasRecord = unitOfWork.ExtCostRepository.IsExist(c => c.EditUser == userID);
        //    if (userHasRecord)
        //    {
        //        s = "This user has data in Extended Cost database.";
        //        goto A;
        //    }

        //    userHasRecord = unitOfWork.ItemCostRepository.IsExist(c => c.EditUser == userID);
        //    if (userHasRecord)
        //    {
        //        s = "This user has data in Item cost database.";
        //        goto A;
        //    }

        //    userHasRecord = unitOfWork.ItemRepository.IsExist(c => c.EditUser == userID);
        //    if (userHasRecord)
        //    {
        //        s = "This user has data in Item database.";
        //        goto A;
        //    }

        //    userHasRecord = unitOfWork.OrderRepository.IsExist(c => c.EditUser == userID);
        //    if (userHasRecord)
        //    {
        //        s = "This user has data in Order database.";
        //        goto A;
        //    }

        //    userHasRecord = unitOfWork.ProductRepository.IsExist(c => c.EditUser == userID);
        //    if (userHasRecord)
        //    {
        //        s = "This user has data in Product database.";
        //        goto A;
        //    }

        //    userHasRecord = unitOfWork.CategoryRepository.IsExist(c => c.EditUser == userID);
        //    if (userHasRecord)
        //    {
        //        s = "This user has data in Category database.";
        //        goto A;
        //    }

        //    userHasRecord = unitOfWork.RequisitionRepository.IsExist(c => c.EditUser == userID);
        //    if (userHasRecord)
        //    {
        //        s = "This user has data in Requisition database.";
        //        goto A;
        //    }

        //    userHasRecord = unitOfWork.StockRepository.IsExist(c => c.EditUser == userID);
        //    if (userHasRecord)
        //    {
        //        s = "This user has data in Stock database.";
        //        goto A;
        //    }

        //    userHasRecord = unitOfWork.SupplierRepository.IsExist(c => c.EditUser == userID);
        //    if (userHasRecord)
        //    {
        //        s = "This user has data in Supplier database.";
        //        goto A;
        //    }

        //    userHasRecord = unitOfWork.TransTypeRepository.IsExist(c => c.EditUser == userID);
        //    if (userHasRecord)
        //    {
        //        s = "This user has data in Transaction Type database.";
        //        goto A;
        //    }

        //    userHasRecord = unitOfWork.UnitTypeRepository.IsExist(c => c.EditUser == userID);
        //    if (userHasRecord)
        //    {
        //        s = "This user has data in Unit Type database.";
        //        goto A;
        //    }



        //    if (!userHasRecord)
        //    {
        //        return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
        //    }
        //A: return Json(new { Success = false, ErrorMessage = s + "\nCannot be deleted" }, JsonRequestBehavior.AllowGet);

        //}

        ////user delete confirm
        //[HttpPost]
        //public ActionResult UserDeleteConfirm(string username)
        //{
        //    string[] RoleList = ICMSWeb.Common.GetRoles.GetRolesForUser(username);

        //    if (RoleList.Length > 0)
        //    {
        //        Roles.RemoveUsersFromRoles(username, RoleList);
        //    }

        //    System.Web.Security.Membership.DeleteUser(username);
        //    try
        //    {
        //        unitOfWork.Save();
        //        return Json(new { Success = true }, JsonRequestBehavior.DenyGet);
        //    }
        //    catch (Exception x)
        //    {
        //        return Json(new { Success = true, ErrorMessage = "Problem While deleting country., \nDetails:" + x.Message }, JsonRequestBehavior.DenyGet);
        //    }
        //}

        public void DepartmentOptions(string n = "")
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            var v = unitOfWork.DepartmentRepository.Get().Where(x => x.OCode == oCode).ToList();
            ViewData["DepartmentOptions"] = new SelectList(v, "DepartmentID", "DepartmentName", n);
        }

        public void CompanyOptions(string n = "")
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            var v = unitOfWork.CompanyInformationRepository.Get().Where(x => x.OCode == oCode).ToList();
            ViewData["CompanyOptions"] = new SelectList(v, "CompanyID", "CompanyName", n);
        }

        public void EmployeOption(string n = "")
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            var v = unitOfWork.EmployeesRepository.Get().Where(x => x.OCode == oCode).ToList();
            ViewData["EmployeOption"] = new SelectList(v, "EmpID", "IdentificationNumber", n);
        }

        public ActionResult ResetPassword(Guid userID, string userName)
        {
            if (userName == User.Identity.Name || User.Identity.Name == "admin")
            {
                ResetPassword v = new ResetPassword();
                v.UserID = userID;
                v.UserName = userName;
                return PartialView("_ResetPassword");
            }
            return Json(new { Success = false, ErrorMessage = "Currently one user cannot change other user password except ADMIN." }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ResetPassword(ResetPassword v)
        {
            if (v.UserName == User.Identity.Name || User.Identity.Name == "admin") { }
            else
            {
                return Json(new { Success = false, ErrorMessage = "Currently one user cannot change other user password except ADMIN." }, JsonRequestBehavior.DenyGet);
            }

            var userModel = unitOfWork.UserPasswordRepository.GetByID(v.UserID);

            string md5OldPassword = userModel.Password;
            string md5NewPassword = GetMD5HashPassword.GetMd5Hash(v.CurrentPassword);

            if (v.NewPassword == v.ConfirmPassword)
            {
                if (md5OldPassword == md5NewPassword)
                {
                    userModel.Password = GetMD5HashPassword.GetMd5Hash(v.NewPassword);
                    userModel.EditUser = v.UserID;
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
                return Json(new { Success = false, ErrorMessage = "Stored current password and your input current password are not same!" }, JsonRequestBehavior.DenyGet);
            }
            return Json(new { Success = false, ErrorMessage = "New password and confirm new password are not same!" }, JsonRequestBehavior.DenyGet);
        }

        [Authorize]
        public ActionResult UserLoginHistory()
        {
            try
            {
                ViewBag.ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["PFTMEntities"].ConnectionString;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return View();
        }

    }
}
