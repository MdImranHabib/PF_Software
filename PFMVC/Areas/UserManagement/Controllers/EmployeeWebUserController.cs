using System;
using System.Linq;
using System.Web.Mvc;
using DLL;
using DLL.Repository;
using PFMVC.common;

namespace PFMVC.Areas.UserManagement.Controllers
{
    public class EmployeeWebUserController : Controller
    {
        int _pageId = 1;
        private UnitOfWork unitOfWork = new UnitOfWork();

        /// <summary>
        /// Indexes this instance.
        /// </summary>
        /// <returns>View</returns>
        ///  <ModifyBy>Avishek</ModifyBy>
        /// <modificationDate>Mar-6-2016</modificationDate>
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Gets the employee.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        ///  <ModifyBy>Avishek</ModifyBy>
        /// <modificationDate>Mar-6-2016</modificationDate>
        public ActionResult GetEmployee(string id, string name)
        {
            var v = unitOfWork.CustomRepository.GetEmployeeWebUser(id, name).ToList();
            return PartialView("_EmployeeWebUser", v);
        }


        /// <summary>
        /// Autocompletes the suggestions for emp identifier.
        /// </summary>
        /// <param name="term">The term.</param>
        /// <returns>List</returns>
        ///  <ModifyBy>Avishek</ModifyBy>
        /// <modificationDate>Mar-6-2016</modificationDate>
        public JsonResult AutocompleteSuggestionsForEmpId(string term)
        {
            try
            {
                var suggestions = unitOfWork.EmployeesRepository.Get(x => x.IdentificationNumber.ToLower().Trim().Contains(term.ToLower().Trim())).Select(s => new
                {
                    value = s.EmpName,
                    label = s.IdentificationNumber
                }).ToList();
                return Json(suggestions, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Autocompletes the name of the suggestions.
        /// </summary>
        /// <param name="term">The term.</param>
        /// <returns>List</returns>
        ///  <ModifyBy>Avishek</ModifyBy>
        /// <modificationDate>Mar-6-2016</modificationDate>
        public JsonResult AutocompleteSuggestionsName(string term)
        {
            try
            {
                var suggestions = unitOfWork.EmployeesRepository.Get(x => x.EmpName.ToLower().Trim().Contains(term.ToLower().Trim())).Select(s => new
                {
                    value = s.IdentificationNumber,
                    label = s.EmpName
                }).ToList();
                return Json(suggestions, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        /// <summary>
        /// Emps the web access.
        /// </summary>
        /// <param name="empid">The empid.</param>
        /// <returns>Bool</returns>
        /// <ModifyBy>Avishek</ModifyBy>
        /// <modificationDate>Mar-6-2016</modificationDate>
        [HttpPost]
        public ActionResult EmpWebAccess(int empid)
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            string membershipStatus = "";
            var v = unitOfWork.UserProfileRepository.Get(w => w.EmpID == empid).SingleOrDefault();
            if (v != null)
            {
                //Previously exist
                if (v.IsActive == 0)
                {
                    v.IsActive = 1;
                    v.RoleID = 2;
                    membershipStatus = "Enable";
                }
                else if(v.IsActive == 1)
                {
                    v.IsActive = 0;
                    v.RoleID = 0;
                    membershipStatus = "Disable";
                }
            }
            else
            {
                //new member
                var employee = unitOfWork.EmployeesRepository.Get(w => w.EmpID == empid).SingleOrDefault();
                Guid EditUser = unitOfWork.CustomRepository.GetUserID(User.Identity.Name);
                //if employee record found push it to user table.
                if (employee != null)
                {
                    tbl_User user = new tbl_User();
                    user.UserID = Guid.NewGuid();
                    user.UserFullName = employee.EmpName;
                    user.EditDate = DateTime.Now;
                    user.EditUser = EditUser ;
                    user.IsActive = 1;
                    user.RoleID = 2;
                    user.EmpID = employee.EmpID;
                    user.OCode = oCode;
                    //let's check if user with employee identification number already exist in database!
                    bool isUserWithEmpIdentificationNumberExist = unitOfWork.UserProfileRepository.IsExist(w => w.LoginName == employee.IdentificationNumber);
                    if (!isUserWithEmpIdentificationNumberExist)
                    {
                        user.LoginName = employee.IdentificationNumber;
                    }
                    else
                    {
                        return Json(new { Success = false, ErrorMessage = "A user with this employee identification number already exist! This is unexpected error! Contact system admin!" }, JsonRequestBehavior.DenyGet);
                    }
                    //puss the user model to context
                    unitOfWork.UserProfileRepository.Insert(user);

                    //prepare password model
                    tbl_UserPassword pass = new tbl_UserPassword();
                    pass.EditDate = DateTime.Now;
                    pass.EditUser = EditUser;
                    pass.Password = GetMD5HashPassword.GetMd5Hash(employee.IdentificationNumber);
                    pass.UserID = user.UserID;
                    unitOfWork.UserPasswordRepository.Insert(pass);
                    membershipStatus = "Enable";
                }
                else
                {
                    return Json(new { Success = false, ErrorMessage = "Employee record not found!" }, JsonRequestBehavior.DenyGet);
                }
            }
            try
            {   
                unitOfWork.Save();
                return Json(new { Success = true, MembershipStatus = membershipStatus, OperationMessage = "Web access " + membershipStatus + " for this user!" }, JsonRequestBehavior.DenyGet);
            }
            catch (Exception x)
            {
                return Json(new { Success = false, ErrorMessage = "Error Occured:" + x.Message }, JsonRequestBehavior.DenyGet);
            }
        }

        

    }
}
