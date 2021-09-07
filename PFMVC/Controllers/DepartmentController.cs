using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using CustomJsonResponse;
using DLL;
using DLL.DataPrepare;
using DLL.Repository;
using DLL.ViewModel;
using PFMVC.common;
using Telerik.Web.Mvc;

namespace PFMVC.Controllers
{
    public class DepartmentController : Controller
    {
        int PageID = 2;

        private UnitOfWork unitOfWork = new UnitOfWork();
        DP_Department dp_Department = new DP_Department();
        [Authorize]
        public ActionResult Index()
        {
            ViewBag.PageName = "Department Information";
            int OCode = ((int?)Session["OCode"]) ?? 0;
            if (OCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            bool b = PagePermission.HasPermission(User.Identity.Name, PageID, 0);
            if (b)
            {
                return View();
            }
           ViewBag.PageName = "Settings";
           return View("Unauthorized");
        }

        [GridAction]
        public ActionResult _SelectDepartments()
        {
            return View(new GridModel(GetDepartments()));
        }


        private IEnumerable<VM_Department> GetDepartments()
        {
            var result = unitOfWork.DepartmentRepository.Get().OrderBy(c => c.DepartmentName).ToList();
            List<VM_Department> departmentList = new List<VM_Department>();
            VM_Department r;
            foreach (var item in result)
            {
                r = new VM_Department();
                r.DepartmentID = item.DepartmentID;
                r.DepartmentName = item.DepartmentName;

                departmentList.Add(r);
            }
            return departmentList;
        }

        public ActionResult DepartmentForm(string id = null)
        {
            VM_Department v;

            if (string.IsNullOrEmpty(id))
            {
                v = new VM_Department();
                return PartialView("_DepartmentForm", v);
            }
            else
            {
                LU_tbl_Department t = unitOfWork.DepartmentRepository.GetByID(id);
                v = dp_Department.vm_Department(t);
                return PartialView("_DepartmentForm", v);
            }

        }

        [HttpPost]
        public ActionResult DepartmentForm(VM_Department v)
        {
            int OCode = ((int?)Session["OCode"]) ?? 0;
            if (OCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            bool IsAllowedToEdit = PagePermission.HasPermission(User.Identity.Name, PageID, 1);
            if (!IsAllowedToEdit) return Json(new { Success = false, ErrorMessage = "You are not allowed to edit information! contact system admin!" }, JsonRequestBehavior.AllowGet);

            if (ModelState.IsValid)
            {
                string _message = "";
                bool b = true;
                if (!string.IsNullOrEmpty(v.DepartmentID))
                {
                    b = unitOfWork.DepartmentRepository.IsExist(filter: c => c.DepartmentName == v.DepartmentName && c.DepartmentID != v.DepartmentID);
                }
                else
                {
                    b = unitOfWork.DepartmentRepository.IsExist(filter: c => c.DepartmentName == v.DepartmentName);
                }
                if (!b)
                {
                    LU_tbl_Department s;
                    if (string.IsNullOrEmpty(v.DepartmentID))
                    {
                        s = dp_Department.tbl_Department(v);
                        s.DepartmentID = GetMaxID();
                        s.EditDate = System.DateTime.Now;
                        s.EditUser = unitOfWork.CustomRepository.GetUserID(User.Identity.Name);
                        unitOfWork.DepartmentRepository.Insert(s);
                        _message = "New department inserted!";
                    }
                    else
                    {
                        s = dp_Department.tbl_Department(v);
                        s.EditDate = System.DateTime.Now;
                        s.EditUser = unitOfWork.CustomRepository.GetUserID(User.Identity.Name);
                        unitOfWork.DepartmentRepository.Update(s);
                        _message = "Department information updated";
                    }
                    try
                    {
                        unitOfWork.Save();
                        return Json(new { Success = true, Message = _message }, JsonRequestBehavior.AllowGet);

                    }
                    catch (DataException x)
                    {
                        return Json(new { Success = false, ErrorMessage = x.Message + " \n=> " + x.InnerException }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "This Department Name Already Exist In Database!");
                }
            }
            var errorList = (from item in ModelState.Values
                             from error in item.Errors
                             select error.ErrorMessage).ToList();

            return Json(JsonResponseFactory.ErrorResponse(errorList), JsonRequestBehavior.DenyGet);
        }



        public string GetMaxID()
        {
            var query = "SELECT isnull(MAX(convert(int, DepartmentID)),0) FROM [PFTM].[dbo].[LU_tbl_Department]";
            var data = unitOfWork.CountryRepository.GetRowCount(query);
            string s = (Convert.ToInt16(data) + 1) + "";
            return s.Trim().PadLeft(4, '0');
        }


        public ActionResult DeletePossible(string id)
        {
            int OCode = ((int?)Session["OCode"]) ?? 0;
            if (OCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            bool b = PagePermission.HasPermission(User.Identity.Name, PageID, 2);
            if (!b)
            {
                return Json(new { Success = false, ErrorMessage = "You are not authorized to delete information!" }, JsonRequestBehavior.AllowGet);
            }
            bool a = true;// unitOfWork.RequisitionRepository.IsExist(i => i.DepartmentID == id);
            if (!a)
            {
                return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { Success = false, ErrorMessage = "This department record is used AND cannot be deleted!" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult DeleteConfirm(string id)
        {

            unitOfWork.DepartmentRepository.Delete(id);
            try
            {
                unitOfWork.Save();
                return Json(new { Success = true, Message = "Successfully Deleted!" }, JsonRequestBehavior.DenyGet);
            }
            catch (Exception x)
            {
                return Json(new { Success = true, ErrorMessage = "Problem While deleting depertment., \nDetails:" + x.Message }, JsonRequestBehavior.DenyGet);
            }
        }

    }
}
