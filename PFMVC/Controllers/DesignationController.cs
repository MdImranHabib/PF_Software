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
    public class DesignationController : Controller
    {
        int PageID = 2;

        private UnitOfWork unitOfWork = new UnitOfWork();
        DP_Designation dp_Designation = new DP_Designation();
        [Authorize]
        public ActionResult Index()
        {

            ViewBag.PageName = "Designation Information";

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
        public ActionResult _SelectDesignation()
        {
            return View(new GridModel(GetDesignation()));
        }


        private IEnumerable<VM_Designation> GetDesignation()
        {
            var result = unitOfWork.DesignationRepository.Get().OrderBy(c => c.DesignationName).ToList();
            List<VM_Designation> designationList = new List<VM_Designation>();
            VM_Designation r;
            foreach (var item in result)
            {
                r = new VM_Designation();
                r.DesignationID = item.DesignationID;
                r.DesignationName = item.DesignationName;

                designationList.Add(r);
            }
            return designationList;
        }

        public ActionResult DesignationForm(string id = null)
        {
            VM_Designation v;

            if (string.IsNullOrEmpty(id))
            {
                v = new VM_Designation();
                return PartialView("_DesignationForm", v);
            }
            else
            {
                LU_tbl_Designation t = unitOfWork.DesignationRepository.GetByID(id);
                v = dp_Designation.vm_Designation(t);
                return PartialView("_DesignationForm", v);
            }
        }

        [HttpPost]
        public ActionResult DesignationForm(VM_Designation v)
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
                if (!string.IsNullOrEmpty(v.DesignationID))
                {
                    b = unitOfWork.DesignationRepository.IsExist(filter: c => c.DesignationName == v.DesignationName && c.DesignationID != v.DesignationID);
                }
                else
                {
                    b = unitOfWork.DesignationRepository.IsExist(filter: c => c.DesignationName == v.DesignationName);
                }
                if (!b)
                {
                    LU_tbl_Designation s;
                    if (string.IsNullOrEmpty(v.DesignationID))
                    {
                        s = dp_Designation.tbl_Designation(v);
                        s.DesignationID = GetMaxID();
                        s.EditDate = System.DateTime.Now;
                        s.EditUser = unitOfWork.CustomRepository.GetUserID(User.Identity.Name);
                        unitOfWork.DesignationRepository.Insert(s);
                        _message = "New designation inserted!";
                    }
                    else
                    {
                        s = dp_Designation.tbl_Designation(v);
                        s.EditDate = System.DateTime.Now;
                        s.EditUser = unitOfWork.CustomRepository.GetUserID(User.Identity.Name);
                        unitOfWork.DesignationRepository.Update(s);
                        _message = "Designation information updated!";
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
                    ModelState.AddModelError("", "This Designation Already Exist In Database!");
                }
            }
            var errorList = (from item in ModelState.Values
                             from error in item.Errors
                             select error.ErrorMessage).ToList();

            return Json(JsonResponseFactory.ErrorResponse(errorList), JsonRequestBehavior.DenyGet);
        }



        public string GetMaxID()
        {
            var query = "SELECT isnull(MAX(convert(int, DesignationID)),0) FROM [PFTM].[dbo].[LU_tbl_Designation]";
            var data = unitOfWork.CountryRepository.GetRowCount(query);
            string s = (Convert.ToInt16(data) + 1) + "";
            return s.Trim().PadLeft(4, '0');
        }


        public ActionResult DeletePossible(int id)
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
            //bool a = unitOfWork.SupplierRepository.GetByID(supplierId)
            //if (!a)
            //{
            return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
            //}
            //else
            //{
            //    return Json(new { Success = false }, JsonRequestBehavior.AllowGet);
            //}
        }

        [HttpPost]
        public ActionResult DeleteConfirm(string id)
        {

            unitOfWork.DesignationRepository.Delete(id);
            try
            {
                unitOfWork.Save();
                return Json(new { Success = true, Message = "Successfully Deleted!" }, JsonRequestBehavior.DenyGet);
            }
            catch (Exception x)
            {
                return Json(new { Success = true, ErrorMessage = "Problem While deleting country., \nDetails:" + x.Message }, JsonRequestBehavior.DenyGet);
            }
        }
    }
}
