using CustomJsonResponse;
using DLL;
using DLL.DataPrepare;
using DLL.Repository;
using DLL.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using Telerik.Web.Mvc;
using PFMVC.common;

namespace PFMVC.Controllers
{
    public class BranchController : Controller
    {
        
        //visit 0
        //edit 1
        //delete 2
        //execute 3

        int PageID = 2;
        private UnitOfWork unitOfWork = new UnitOfWork();
        DP_Branch dp_Branch = new DP_Branch();

        [Authorize]
        public ActionResult Index()
        {
            ViewBag.PageName = "Branch Information";
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
        public ActionResult _SelectBranches()
        {
            return View(new GridModel(GetBranches()));
        }


        private IEnumerable<VM_Branch> GetBranches()
        {
            var result = unitOfWork.BranchRepository.Get().OrderBy(c => c.BranchName).ToList();
            List<VM_Branch> branchList = new List<VM_Branch>();
            VM_Branch r;
            foreach (var item in result)
            {
                r = new VM_Branch();
                r.BranchID = item.BranchID;
                r.BranchName = item.BranchName;
                r.BranchLocation = item.BranchLocation;
                branchList.Add(r);
            }
            return branchList;
        }


        public ActionResult InsertBranchForm()
        {
            VM_Branch v;

            v = new VM_Branch();
            return PartialView("_BranchForm", v);
            
            
        }
        public ActionResult BranchForm(int id)
        { 
            VM_Branch v;
            //if (string.IsNullOrEmpty(id))
            //{
            //    v = new VM_Branch();
            //    return PartialView("_BranchForm", v);
            //}
            //else
            //{
                LU_tbl_Branch t = unitOfWork.BranchRepository.GetByID(id);
                v = dp_Branch.vm_Branch(t);
                return PartialView("_BranchForm", v);
            //}
        }

        [HttpPost]
        public ActionResult BranchForm(VM_Branch v)
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
                bool b = true;
                //if (!string.IsNullOrEmpty(v.BranchID))
                    if (v.BranchID!=0)
                {
                    b = unitOfWork.BranchRepository.IsExist(filter: s => s.BranchName == v.BranchName && v.BranchID != v.BranchID);
                }
                else
                {
                    b = unitOfWork.BranchRepository.IsExist(filter: s => s.BranchName == v.BranchName);
                }
                if (!b)
                {
                    LU_tbl_Branch s;
                    //if (string.IsNullOrEmpty(v.BranchID))
                        if (v.BranchID == 0)
                    {
                        s = dp_Branch.tbl_Branch(v);
                        //s.BranchID = GetMaxID();
                        s.EditDate = System.DateTime.Now;
                        s.OCode = 1;
                        s.EditUser = unitOfWork.CustomRepository.GetUserID(User.Identity.Name);
                        unitOfWork.BranchRepository.Insert(s);
                    }
                    else
                    {
                        s = dp_Branch.tbl_Branch(v);
                        s.EditDate = System.DateTime.Now;
                        s.EditUser = unitOfWork.CustomRepository.GetUserID(User.Identity.Name);
                        unitOfWork.BranchRepository.Update(s);
                    }
                    try
                    {
                        unitOfWork.Save();
                        return Json(new { Success = true, Message = "Successfully Saved!" }, JsonRequestBehavior.AllowGet);

                    }
                    catch (DataException x)
                    {
                        return Json(new { Success = false, ErrorMessage = x.Message + " \n=> " + x.InnerException }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "This Branch Name Already Exist In Database!");
                }
            }
            var errorList = (from item in ModelState.Values
                             from error in item.Errors
                             select error.ErrorMessage).ToList();

            return Json(JsonResponseFactory.ErrorResponse(errorList), JsonRequestBehavior.DenyGet);
        }



        public int GetMaxID()
        {
            //var query = "SELECT isnull(MAX(convert(int, BranchID)),0) FROM [PFTM].[dbo].[LU_tbl_Branch]";
            var query = "SELECT isnull(MAX(BranchID) FROM [PFTM].[dbo].[LU_tbl_Branch]";

            var data = unitOfWork.CountryRepository.GetRowCount(query);
            //string s = (Convert.ToInt16(data) + 1) + "";
            //return s.Trim().PadLeft(4, '0');
            return 0;

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

            bool a = unitOfWork.UserProfileRepository.IsExist(c => c.BranchID == id);
            if (!a)
            {
                return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { Success = false, ErrorMessage = "This information is used and cannot be deleted!" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult DeleteConfirm(int id)
        {
            bool b = unitOfWork.EmployeesRepository.IsExist(i => i.Branch == id);
            if (!b)
            {
                unitOfWork.BranchRepository.Delete(id);
                
                try
                {
                    unitOfWork.Save();
                    return Json(new { Success = true, Message = "Successfully Deleted!" }, JsonRequestBehavior.DenyGet);
                }
                catch (Exception x)
                {
                    //return Json(new { Success = false, ErrorMessage = "You cannot Delete Branch because it Related to Employee Information, \nDetails:" + x.Message }, JsonRequestBehavior.DenyGet);
                    return Json(new { Success = false, ErrorMessage = "You cannot Delete Branch because it Related to Employee Information" }, JsonRequestBehavior.DenyGet);

                }
            }
            return Json(new { Success = false, ErrorMessage = "You cannot Delete Branch because it Related to Employee Information" }, JsonRequestBehavior.AllowGet);

        }
    }
}
