using CustomJsonResponse;
using DLL;
using DLL.Repository;
using DLL.Utility;
using DLL.ViewModel;
using PFMVC.common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Telerik.Web.Mvc;

namespace PFMVC.Areas.UserManagement.Controllers
{
    public class AuditLogController : Controller
    {
        private UnitOfWork unitOfWork = new UnitOfWork();
        //
        // GET: /UserManagement/AuditLog/

        public ActionResult Index()
        {
            return View();
            //ViewBag.PageName = "Audit Log Information";
        }

        [GridAction]
        public ActionResult _SelectAuditLog()
        {
            return View(new GridModel(GetAuditLogList()));
        }
        /// <summary>
        /// For showing Db BackUp Path and View
        /// </summary>
        /// <returns> View path of BackUp Location</returns>
        public ActionResult _DBBackup()
        {
            ViewBag.DbPath = ApplicationSetting.DbBackUpPath;
            return View();
        }
        /// <summary>
        /// For creating a BackUp on local PC
        /// </summary>
        /// <param name="name">Name of File</param>
        /// <param name="path">Drive location of Local PC</param>
        /// <returns></returns>
        public JsonResult ExecuteBackup(string name, string path)
        {
            //processFactory = new ProcessFactory();
            try
            {


                unitOfWork.CustomRepository.DBBackupProcess(name, path);


                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { uccess = false,errorMessage = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }
        private IEnumerable<VM_AuditLog> GetAuditLogList()
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            var result = unitOfWork.CustomRepository.GetAuditLogList(oCode).ToList().OrderByDescending(x=>x.LastAuditDate);

            return result;
        }

        public ActionResult AddNewAudit()
        {
            //var d = unitOfWork.BranchRepository.Get().ToList();
            //ViewData["BranchOptions"] = new SelectList(d, "BranchID", "BranchName", "");
            //DepartmentOptions();
            //CompanyOptions();
            //EmployeOption();
            ViewData["CompanyId"] = ((int?)Session["OCode"]) ?? 0;
            return PartialView("_CreateNewAuditLock");
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
        public ActionResult AddNewAudit(VM_AuditLog model)
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            //bool b = PagePermission.HasPermission(User.Identity.Name, _pageId, 0);
            //if (!b)
            //{
            //    return Json(new { Success = false, ErrorMessage = "Sorry, you are not authorized to register a new member." }, JsonRequestBehavior.AllowGet);
            //}

            try
            {
                tbl_FinancialAuditLog auditLog = new tbl_FinancialAuditLog();

                //auditLog.LogID = model.LogID;
                auditLog.LastAuditDate = model.LastAuditDate;
                auditLog.LogDate = DateTime.Now;
                auditLog.OCode = oCode;
                auditLog.EditUser = unitOfWork.CustomRepository.GetUserID(User.Identity.Name);
                unitOfWork.AuditLogRepository.Insert(auditLog);
                unitOfWork.Save();
                return Json(new { Success = true, Message = "Successfully added new user!" }, JsonRequestBehavior.DenyGet);

            }
            catch (Exception ex)
            {
                return Json(new { Success = false, ErrorMessage = "Error: " + ex.Message }, JsonRequestBehavior.DenyGet);
            }
        }

        public ActionResult GetAuditLockInfo(int id)
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            VM_AuditLog v = unitOfWork.CustomRepository.GetAuditLogList(oCode).FirstOrDefault(m => m.LogID == id);
            //unitOfWork.CustomRepository.GetSystemUserList(oCode).FirstOrDefault(m => m.UserId == id);
            //DepartmentOptions(v.DepartmentID);
            //var d = unitOfWork.BranchRepository.Get().ToList();
            //ViewData["BranchOptions"] = new SelectList(d, "BranchID", "BranchName", v.BranchID ?? "");
            //CompanyOptions(v.CompanyID + "");
            return PartialView("_AuditLockInfo", v);
        }

        [HttpPost]
        public ActionResult GetAuditLockInfo(VM_AuditLog v)
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;

            tbl_FinancialAuditLog fal = new tbl_FinancialAuditLog();
            fal = unitOfWork.AuditLogRepository.Get(w => w.LogID == v.LogID).Single();
            fal.LastAuditDate = v.LastAuditDate;
            fal.EditUser = unitOfWork.CustomRepository.GetUserID(User.Identity.Name);
            fal.OCode = oCode;
            unitOfWork.AuditLogRepository.Update(fal);
            unitOfWork.Save();
            return Json(new { Success = true, Message = "Audit information updated successfully." }, JsonRequestBehavior.DenyGet);
        }

        public ActionResult AuditLogDeletePossible(int id)
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
           
            bool a = unitOfWork.AuditLogRepository.IsExist(i => i.LogID == id);
            if (a)

            {
                return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { Success = false, ErrorMessage = "This MIS is not here..." }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DeleteConfirm(int id)
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            bool a = unitOfWork.AuditLogRepository.IsExist(i => i.LogID == id);
            if (a)
            {
                var objEmp = unitOfWork.AuditLogRepository.Get(e => e.LogID == id).SingleOrDefault();
                try
                {
                    unitOfWork.AuditLogRepository.Delete(objEmp);
                    unitOfWork.Save();
                    return Json(new { Success = true, Message = "Delete Successfully" }, JsonRequestBehavior.DenyGet);
                }
                catch (Exception x)
                {
                    return Json(new { Success = false, ErrorMessage = "Problem While deleting record., \nDetails:" + x.Message }, JsonRequestBehavior.DenyGet);
                }
            }
            return Json(new { Success = false, ErrorMessage = "Cannot be deleted" }, JsonRequestBehavior.AllowGet);
        }
    }
}
