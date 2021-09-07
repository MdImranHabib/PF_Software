using CustomJsonResponse;
using DLL;
using DLL.Repository;
using DLL.ViewModel;
using PFMVC.common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using Telerik.Web.Mvc;

namespace PFMVC.Controllers
{
    public class AccountMappingController : Controller
    {
        int PageID = 3;
        private UnitOfWork unitOfWork = new UnitOfWork();

        /// <summary>
        /// Indexes this instance.
        /// </summary>
        /// <returns>View</returns>
        /// <CreatedBy>Avishek</CreatedBy>
        /// <CreatedDate>Mar-6-2016</CreatedDate>
        public ActionResult Index()
        {
            ViewBag.PageName = "Index";
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            bool b = PagePermission.HasPermission(User.Identity.Name, PageID, 0);
            if (b)
            {
                return View();
            }
            ViewBag.PageName = "Employee Setup";
            return View("Unauthorized");
        }

        /// <summary>
        /// _s the select mis.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="size">The size.</param>
        /// <param name="orderBy">The order by.</param>
        /// <returns></returns>
        /// <CreatedBy>Avishek</CreatedBy>
        /// <CreatedDate>Mar-6-2016</CreatedDate>
        [GridAction]
        public ActionResult _SelectMIS(int page, int size, string orderBy)
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            var result = unitOfWork.CustomRepository.GetChartAccountMappingList();
            return View(new GridModel(result));
        }

        /// <summary>
        /// Autocompletes the name of the by ledger.
        /// </summary>
        /// <param name="term">The term.</param>
        /// <returns>List</returns>
        /// <CreatedBy>Avishek</CreatedBy>
        /// <CreatedDate>Mar-6-2016</CreatedDate>
        public JsonResult AutocompleteByLedgerName(string term)
        {
            var suggestions = unitOfWork.CustomRepository.GetLedgerList().Where(w => w.LedgerName.ToLower().Trim().Contains(term.ToLower().Trim())).Select(s => new
            {
                value = s.LedgerID,
                label = s.LedgerName
            });
            return Json(suggestions, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Mises the form.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>List</returns>
        /// <CreatedBy>Avishek</CreatedBy>
        /// <CreatedDate>Mar-6-2016</CreatedDate>
        public ActionResult MISForm(int id = 0)
        {
            List<VM_MIS> mis = new List<VM_MIS>();
            List<VM_acc_ledger> ledger = new List<VM_acc_ledger>();
            mis = unitOfWork.CustomRepository.GetMISList();
            ledger = unitOfWork.CustomRepository.GetLedgerList();
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            if (id == 0)
            {
                VM_acc_Chart_Of_Account_Mapping accChartOfAccountMapping = new VM_acc_Chart_Of_Account_Mapping();
                ViewBag.MisName = new SelectList(mis, "id", "MISName");
                ViewBag.LedgerName = new SelectList(ledger, "LedgerID", "LedgerName");
                return PartialView("_ChartOfAccountMapping", accChartOfAccountMapping);
            }
            else
            {
                VM_acc_Chart_Of_Account_Mapping accChartOfAccountMapping = new VM_acc_Chart_Of_Account_Mapping();
                accChartOfAccountMapping = unitOfWork.CustomRepository.GetChartAccountMappingList(id);
                ViewBag.MisName = new SelectList(mis, "id", "MISName");
                ViewBag.LedgerName = new SelectList(ledger, "LedgerID", "LedgerName");
                return PartialView("_ChartOfAccountMapping", accChartOfAccountMapping);
            }
        }

        /// <summary>
        /// Mises the form.
        /// </summary>
        /// <param name="v">The v.</param>
        /// <returns>Bool</returns>
        /// <CreatedBy>Avishek</CreatedBy>
        /// <CreatedDate>Mar-6-2016</CreatedDate>
        [HttpPost]
        public ActionResult MISForm(VM_acc_Chart_Of_Account_Mapping v)
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            string message = "";
            bool isExist = false;

            if (v.MISName == null)
            {
                return Json(new { Success = false, Message = "First" }, JsonRequestBehavior.AllowGet);
            }

            bool isAllowedToEdit = PagePermission.HasPermission(User.Identity.Name, PageID, 1);
            if (!isAllowedToEdit)
                return Json(new { Success = false, ErrorMessage = "You are not allowed to edit information! contact system admin!" }, JsonRequestBehavior.AllowGet);

            if (ModelState.IsValid)
            {
                acc_Chart_of_Account_Maping s;
                if (true)
                {
                    isExist = unitOfWork.ChartofAccountMapingRepository.IsExist(filter: c => (c.id == v.id));
                    //if (isExist)
                    if (isExist == true)
                    {
                        s = unitOfWork.CustomRepository.Get_acc_Chart_of_Account_Mapping(v);
                        s.OCode = oCode;
                        s.DateOfEntry = System.DateTime.Now;
                        s.EntryBy = unitOfWork.CustomRepository.GetUserID(User.Identity.Name);
                        s.Ledger_Id = v.Ledger_Id;
                        s.MIS_Id = Convert.ToInt32(v.MISName);
                        s.id = v.id;
                        unitOfWork.ChartofAccountMapingRepository.Update(s);
                        message = "MIS Information updated";
                    }
                    else
                    {
                        int id = Convert.ToInt32(v.MISName);
                        bool iexist = unitOfWork.ChartofAccountMapingRepository.IsExist(filter: c => c.MIS_Id == id && c.Ledger_Id == v.Ledger_Id);
                        if (!iexist)
                        {
                            s = unitOfWork.CustomRepository.Get_acc_Chart_of_Account_Mapping(v);
                            s.DateOfEntry = System.DateTime.Now;
                            s.EntryBy = unitOfWork.CustomRepository.GetUserID(User.Identity.Name);
                            s.OCode = oCode;
                            s.MIS_Id = Convert.ToInt32(v.MISName);
                            s.Ledger_Id = v.Ledger_Id;
                            unitOfWork.ChartofAccountMapingRepository.Insert(s);
                            message = "MIS Information inserted!";
                        }
                    }
                    unitOfWork.Save();
                    return Json(new { Success = true, Message = message }, JsonRequestBehavior.AllowGet);
                }
            }
            var errorList = (from item in ModelState.Values
                             from error in item.Errors
                             select error.ErrorMessage).ToList();

            return Json(JsonResponseFactory.ErrorResponse(errorList), JsonRequestBehavior.DenyGet);
        }
       
        /// <summary>
        /// Deletes the confirm.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>Bool</returns>
        /// <CreatedBy>Avishek</CreatedBy>
        /// <CreatedDate>Mar-6-2016</CreatedDate>
        public ActionResult DeletePossible(int id)
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            bool b = PagePermission.HasPermission(User.Identity.Name, PageID, 2);
            if (!b)
            {
                return Json(new { Success = false, ErrorMessage = "You are not authorized to delete information!" }, JsonRequestBehavior.AllowGet);
            }
            bool a = unitOfWork.ChartofAccountMapingRepository.IsExist(i => i.id == id);
            if (a)
            {
                return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { Success = false, ErrorMessage = "This MIS is not here..." }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Deletes the confirm.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>Bool</returns>
        /// <CreatedBy>Avishek</CreatedBy>
        /// <CreatedDate>Mar-6-2016</CreatedDate>
        [HttpPost]
        public ActionResult DeleteConfirm(int id)
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            bool a = unitOfWork.ChartofAccountMapingRepository.IsExist(i => i.id == id);
            if (a)
            {
                var objEmp = unitOfWork.ChartofAccountMapingRepository.Get(e => e.id == id).SingleOrDefault();
                try
                {
                    unitOfWork.ChartofAccountMapingRepository.Delete(objEmp);
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
