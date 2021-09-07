using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using CustomJsonResponse;
using DLL;
using DLL.DataPrepare;
using DLL.Repository;
using DLL.ViewModel;
using PFMVC.common;
using Telerik.Web.Mvc;
using System.Collections;
using System.Globalization;
using Microsoft.Reporting.WebForms;
//using DLL.PayRollAccess.Repository;
namespace PFMVC.Areas.Accounting.Controllers
{
    public class CashFlowController : Controller
    {
        private PFTMEntities context;
        UnitOfWork unitOfWork = new UnitOfWork();
        //
        // GET: /Accounting/CashFlow/

        public ActionResult Index()
        {
            //Added By Shohid Date:Oct-9-2016
            int OCode = ((int?)Session["OCode"]) ?? 0;
            if (OCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End
            return View();
        }

        public ActionResult CashFlowGroupIndex()
        {
            //Added By Shohid Date:Oct-9-2016
            int OCode = ((int?)Session["OCode"]) ?? 0;
            if (OCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End
            return View();
        }


        public ActionResult CashFlowGroupForm(int GroupID = 0)
        {
            //Added By Avishek Date:Jan-19-2016
            int OCode = ((int?)Session["OCode"]) ?? 0;
            if (OCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End
            if (GroupID == 0)
            {
                //  ViewData["Nature"] = new SelectList(unitOfWork.ACC_NatureRepository.Get(), "NatureID", "NatureName");
                return PartialView("CashFlow_GroupForm");
            }
            else if (GroupID > 0)
            {

                var v = unitOfWork.ACC_CashFlowGroup.Get().Where(x => x.CashFlowGroup_Id == GroupID).FirstOrDefault();
                if (v != null)
                {
                    VM_acc_Cash_Flow_Group a = new VM_acc_Cash_Flow_Group();
                    a.CashFlowGroup_Id = v.CashFlowGroup_Id;
                    a.CashFlow_Group = v.CashFlow_Group;

                    //ViewData["Nature"] = new SelectList(unitOfWork.ACC_NatureRepository.Get(), "NatureID", "NatureName", v.NatureID);
                    //if (v.CashFlowGroup_Id > 0)
                    //{
                    //v.CashFlow_Group
                    //}
                    //ViewBag.User = "This record was modified by " + unitOfWork.ACC_CashFlowGroup.Get().Where(w => w. == v.EditUser).SingleOrDefault().UserFullName + " at " + v.EditDate;
                    return PartialView("CashFlow_GroupForm", a);
                }
                else
                {
                    return Json(new { Success = false, ErrorMessage = "Data not found! Unexpected error" }, JsonRequestBehavior.DenyGet);
                }
            }
            else
            {
                return Json(new { Success = false, ErrorMessage = "No group record selected..." }, JsonRequestBehavior.DenyGet);
            }
        }

        [HttpPost]
        public ActionResult CashFlowGroupForm(VM_acc_Cash_Flow_Group v)
        {
            //Added By Avishek Date:Jan-19-2016
            int OCode = ((int?)Session["OCode"]) ?? 0;
            if (OCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End
            if (ModelState.IsValid)
            {
                if (OCode == 0)
                {
                    return Json(new { Success = false, ErrorMessage = "To create a group you must be under a compnay!" }, JsonRequestBehavior.DenyGet);
                }
                // bool isGroupNameCodeExist = unitOfWork.ACC_GroupRepository.IsExist(w => (w.GroupName == v.GroupName || w.GroupCode == v.GroupCode) && (w.GroupID != v.GroupID && w.OCode == OCode));
                bool isGroupNameCodeExist = unitOfWork.ACC_CashFlowGroup.IsExist(w => w.CashFlow_Group == v.CashFlow_Group);
                if (!isGroupNameCodeExist)
                {
                    //if (v.ParentGroupID > 0 && v.ParentGroupID == v.GroupID)
                    //{
                    //    return Json(new { Success = false, ErrorMessage = "Group name and parent group name shouldn't be same!" }, JsonRequestBehavior.DenyGet);
                    //}
                    if (v.CashFlowGroup_Id == 0)
                    {
                        //     = new acc_Group();
                        acc_CashFlowGroup acc_cfg = new acc_CashFlowGroup();
                        acc_cfg.CashFlow_Group = v.CashFlow_Group;
                        acc_cfg.EditDate = DateTime.Now;
                        acc_cfg.EditUser = unitOfWork.CustomRepository.GetUserID(User.Identity.Name);
                        acc_cfg.OCode = OCode.ToString();

                        unitOfWork.ACC_CashFlowGroup.Insert(acc_cfg);
                    }
                    else if (v.CashFlowGroup_Id > 0)
                    {
                        acc_CashFlowGroup acc_group = unitOfWork.ACC_CashFlowGroup.Get().Where(x => x.CashFlowGroup_Id == v.CashFlowGroup_Id).FirstOrDefault();
                        if (acc_group != null)
                        {

                            acc_group.CashFlow_Group = v.CashFlow_Group;
                            acc_group.CashFlowGroup_Id = v.CashFlowGroup_Id;
                            acc_group.OCode = OCode.ToString();
                            acc_group.EditDate = DateTime.Now;
                            acc_group.EditUser = unitOfWork.CustomRepository.GetUserID(User.Identity.Name);
                            unitOfWork.ACC_CashFlowGroup.Update(acc_group);
                        }
                        else
                        {
                            return Json(new { Success = false, ErrorMessage = "Null object found!" }, JsonRequestBehavior.DenyGet);
                        }
                    }
                    try
                    {
                        unitOfWork.Save();
                        return Json(new { Success = true, Message = "Successfully saved!" }, JsonRequestBehavior.DenyGet);
                    }
                    catch (Exception x)
                    {
                        return Json(new { Success = false, ErrorMessage = "Error while saving. " + x.Message }, JsonRequestBehavior.DenyGet);
                    }
                }
                else
                {
                    return Json(new { Success = false, ErrorMessage = "Group name or Code exist. Try another..." }, JsonRequestBehavior.DenyGet);
                }

            }
            var errorList = (from item in ModelState.Values
                             from error in item.Errors
                             select error.ErrorMessage).ToList();

            return Json(JsonResponseFactory.ErrorResponse(errorList), JsonRequestBehavior.DenyGet);
        }


        public ActionResult GetGroupJQFile()
        {
            return PartialView("CashFlow_GroupJQ");
        }

        public ActionResult GetGroupName(int filter = 0, string name = "")
        {
            //prepare ocode
            //Added By Avishek Date:Jan-19-2016
            int OCode = ((int?)Session["OCode"]) ?? 0;
            if (OCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End

            if (filter > 0)
            {
                ViewBag.LegendName = "Cash Flow Group List: " + name;
                var v = unitOfWork.ACC_CashFlowGroup.Get().Where(w => w.CashFlowGroup_Id == filter && (w.OCode == null || w.OCode == OCode.ToString())).ToList();
                // var v = unitOfWork.ACC_CashFlowGroup.GetGroup().Where(w => w.NatureID == filter && (w.OCode == null || w.OCode == OCode)).ToList();

                ViewBag.Group = v;
                return PartialView("CashFlow_GroupNameList");
            }
            else
            {
                ViewBag.LegendName = "Group List: All";
                var v = unitOfWork.ACC_CashFlowGroup.Get().Where(w => (w.OCode == null || w.OCode == OCode.ToString())).ToList();
                ViewBag.Group = v;
                return PartialView("CashFlow_GroupNameList");
            }
        }

        public ActionResult CashFlowTypeIndex()
        {
            //Added By Shohid Date:Oct-9-2016
            int OCode = ((int?)Session["OCode"]) ?? 0;
            if (OCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End
            return View();
        }

        public ActionResult CashFlowTypeForm(int TypeID = 0)
        {
            //Added By Avishek Date:Jan-19-2016
            int OCode = ((int?)Session["OCode"]) ?? 0;
            if (OCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End
            if (TypeID == 0)
            {
                //IEnumerable<SelectListItem> basetypes = unitOfWork.ACC_CashFlowGroup.Get().Select(b => new SelectListItem { Value = b.CashFlowGroup_Id.ToString(), Text = b.CashFlow_Group });
                ViewData["Group"] = new SelectList(unitOfWork.ACC_CashFlowGroup.Get(), "CashFlowGroup_Id", "CashFlow_Group");
                //ViewData["Group"] = basetypes;
                return PartialView("CashFlow_TypeForm");
            }
            else if (TypeID > 0)
            {

                var v = unitOfWork.ACC_CashFlowType.Get().Where(x => x.CashFlowType_Id == TypeID).FirstOrDefault();
                ViewData["Group"] = new SelectList(unitOfWork.ACC_CashFlowGroup.Get(), "CashFlowGroup_Id", "CashFlow_Group");
                if (v != null)
                {
                    VM_acc_Cash_Flow_Type t = new VM_acc_Cash_Flow_Type();
                    t.CashFlow_Type = v.CashFlow_Type;
                    t.CashFlowGroup_Id = v.CashFlowGroup_Id ?? 0;
                    t.CashFlowType_Id = v.CashFlowType_Id;
                    return PartialView("CashFlow_TypeForm", t);
                }
                else
                {
                    return Json(new { Success = false, ErrorMessage = "Data not found! Unexpected error" }, JsonRequestBehavior.DenyGet);
                }
            }
            else
            {
                return Json(new { Success = false, ErrorMessage = "No group record selected..." }, JsonRequestBehavior.DenyGet);
            }
        }

        [HttpPost]
        public ActionResult CashFlowTypeForm(VM_acc_Cash_Flow_Type v)
        {
            //Added By Avishek Date:Jan-19-2016
            int OCode = ((int?)Session["OCode"]) ?? 0;
            if (OCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End
            //if (ModelState.IsValid)
            //{
            if (OCode == 0)
            {
                return Json(new { Success = false, ErrorMessage = "To create a group you must be under a compnay!" }, JsonRequestBehavior.DenyGet);
            }
            // bool isGroupNameCodeExist = unitOfWork.ACC_GroupRepository.IsExist(w => (w.GroupName == v.GroupName || w.GroupCode == v.GroupCode) && (w.GroupID != v.GroupID && w.OCode == OCode));
            bool isTypeNameCodeExist = unitOfWork.ACC_CashFlowType.IsExist(w => w.CashFlow_Type == v.CashFlow_Type && w.CashFlowGroup_Id == v.CashFlowGroup_Id);
            if (!isTypeNameCodeExist)
            {

                if (v.CashFlowType_Id == 0)
                {

                    acc_CashFlow_Type acc_cft = new acc_CashFlow_Type();
                    acc_cft.CashFlow_Type = v.CashFlow_Type;
                    acc_cft.CashFlowGroup_Id = v.CashFlowGroup_Id;
                    acc_cft.EditDate = DateTime.Now;
                    acc_cft.EditUser = unitOfWork.CustomRepository.GetUserID(User.Identity.Name);
                    acc_cft.OCode = OCode.ToString();

                    unitOfWork.ACC_CashFlowType.Insert(acc_cft);
                }
                else if (v.CashFlowType_Id > 0)
                {

                    acc_CashFlow_Type acc_cft = new acc_CashFlow_Type();
                    acc_cft.CashFlow_Type = v.CashFlow_Type;
                    acc_cft.CashFlowGroup_Id = v.CashFlowGroup_Id;
                    acc_cft.EditDate = DateTime.Now;
                    acc_cft.CashFlowType_Id = v.CashFlowType_Id;
                    acc_cft.EditUser = unitOfWork.CustomRepository.GetUserID(User.Identity.Name);
                    acc_cft.OCode = OCode.ToString();
                    unitOfWork.ACC_CashFlowType.Update(acc_cft);
                }
                else
                {
                    return Json(new { Success = false, ErrorMessage = "Null object found!" }, JsonRequestBehavior.DenyGet);
                }

                try
                {
                    unitOfWork.Save();
                    return Json(new { Success = true, Message = "Successfully saved!" }, JsonRequestBehavior.DenyGet);
                }
                catch (Exception x)
                {
                    return Json(new { Success = false, ErrorMessage = "Error while saving. " + x.Message }, JsonRequestBehavior.DenyGet);
                }
            }
            else
            {
                return Json(new { Success = false, ErrorMessage = "Group name or Code exist. Try another..." }, JsonRequestBehavior.DenyGet);
            }

            // }
            //var errorList = (from item in ModelState.Values
            //                 from error in item.Errors
            //                 select error.ErrorMessage).ToList();

            // return Json(JsonResponseFactory.ErrorResponse(errorList), JsonRequestBehavior.DenyGet);
        }


        public ActionResult GetTypeJQFile()
        {
            return PartialView("CashFlow_TypeJQ");
        }

        public ActionResult GetTypeName(int filter = 0, string name = "")
        {
            //prepare ocode
            //Added By Avishek Date:Jan-19-2016
            int OCode = ((int?)Session["OCode"]) ?? 0;
            if (OCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End

            if (filter > 0)
            {
                ViewBag.LegendName = "Cash Flow Type List: " + name;
                var v = unitOfWork.ACC_CashFlowType.Get().Where(w => w.CashFlowGroup_Id == filter && (w.OCode == null || w.OCode == OCode.ToString())).ToList();
                // var v = unitOfWork.ACC_CashFlowGroup.GetGroup().Where(w => w.NatureID == filter && (w.OCode == null || w.OCode == OCode)).ToList();

                ViewBag.Type = v;
                return PartialView("CashFlow_TypeNameList");
            }
            else
            {
                ViewBag.LegendName = "Type List: All";
                var v = unitOfWork.ACC_CashFlowType.Get().Where(w => (w.OCode == null || w.OCode == OCode.ToString())).ToList();
                ViewBag.Type = v;
                return PartialView("CashFlow_TypeNameList");
            }
        }


        public ActionResult CashFlowMappingIndex()
        {
            //Added By Shohid Date:Oct-9-2016
            int OCode = ((int?)Session["OCode"]) ?? 0;
            if (OCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End
            return View();
        }

        public ActionResult CashFlowMappingForm(int MappingID = 0)
        {
            //Added By Avishek Date:Jan-19-2016
            int OCode = ((int?)Session["OCode"]) ?? 0;
            if (OCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End
            if (MappingID == 0)
            {
                //IEnumerable<SelectListItem> basetypes = unitOfWork.ACC_CashFlowGroup.Get().Select(b => new SelectListItem { Value = b.CashFlowGroup_Id.ToString(), Text = b.CashFlow_Group });
                ViewData["Ledger"] = new SelectList(unitOfWork.ACC_LedgerRepository.Get(), "LedgerID", "LedgerName");
                //ViewData["Group"] = basetypes;
                return PartialView("CashFlow_MappingForm");
            }
            else if (MappingID > 0)
            {

                var v = unitOfWork.ACC_CashFlowMapping.Get().Where(x => x.CashFlowMapping_Id == MappingID).FirstOrDefault();
                var s = unitOfWork.ACC_CashFlowType.Get().Where(y => y.CashFlowType_Id == v.CashFlowType_Id).FirstOrDefault();
                ViewData["Ledger"] = new SelectList(unitOfWork.ACC_LedgerRepository.Get(), "LedgerID", "LedgerName");
                if (v != null)
                {
                    VM_acc_Cash_Flow_Mapping m = new VM_acc_Cash_Flow_Mapping();
                    m.CashFlowType_Id = v.CashFlowType_Id;
                    m.CashFlowType = s.CashFlow_Type;
                    m.CashFlowType_Id = v.CashFlowType_Id;
                    return PartialView("CashFlow_MappingForm", m);
                }
                else
                {
                    return Json(new { Success = false, ErrorMessage = "Data not found! Unexpected error" }, JsonRequestBehavior.DenyGet);
                }
            }
            else
            {
                return Json(new { Success = false, ErrorMessage = "No group record selected..." }, JsonRequestBehavior.DenyGet);
            }
        }

        [HttpPost]
        public ActionResult CashFlowMappingForm(VM_acc_Cash_Flow_Mapping v)
        {
            //Added By Avishek Date:Jan-19-2016
            int OCode = ((int?)Session["OCode"]) ?? 0;
            if (OCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End
            //if (ModelState.IsValid)
            //{
            if (OCode == 0)
            {
                return Json(new { Success = false, ErrorMessage = "To create a group you must be under a compnay!" }, JsonRequestBehavior.DenyGet);
            }
            // bool isGroupNameCodeExist = unitOfWork.ACC_GroupRepository.IsExist(w => (w.GroupName == v.GroupName || w.GroupCode == v.GroupCode) && (w.GroupID != v.GroupID && w.OCode == OCode));
            bool isTypeNameCodeExist = unitOfWork.ACC_CashFlowMapping.IsExist(w => w.CashFlowMapping_Id == v.CashFlowMapping_Id && w.CashFlowType_Id == v.CashFlowType_Id && w.LedgerID == v.LedgerID);
            if (!isTypeNameCodeExist)
            {

                if (v.CashFlowMapping_Id == 0)
                {

                    acc_CashFlowMapping cfm = new acc_CashFlowMapping();
                    cfm.CashFlowType_Id = v.CashFlowType_Id;
                    cfm.LedgerID = v.LedgerID;
                    cfm.EditDate = DateTime.Now;
                    cfm.EditUser = unitOfWork.CustomRepository.GetUserID(User.Identity.Name);
                    cfm.OCode = OCode.ToString();

                    unitOfWork.ACC_CashFlowMapping.Insert(cfm);
                }
                else if (v.CashFlowMapping_Id > 0)
                {

                    acc_CashFlowMapping cfm = new acc_CashFlowMapping();
                    cfm.CashFlowType_Id = v.CashFlowType_Id;
                    cfm.LedgerID = v.LedgerID;
                    cfm.EditDate = DateTime.Now;
                    cfm.EditUser = unitOfWork.CustomRepository.GetUserID(User.Identity.Name);
                    cfm.OCode = OCode.ToString();
                    cfm.CashFlowMapping_Id = v.CashFlowMapping_Id;
                    unitOfWork.ACC_CashFlowMapping.Update(cfm);
                }
                else
                {
                    return Json(new { Success = false, ErrorMessage = "Null object found!" }, JsonRequestBehavior.DenyGet);
                }

                try
                {
                    unitOfWork.Save();
                    return Json(new { Success = true, Message = "Successfully saved!" }, JsonRequestBehavior.DenyGet);
                }
                catch (Exception x)
                {
                    return Json(new { Success = false, ErrorMessage = "Error while saving. " + x.Message }, JsonRequestBehavior.DenyGet);
                }
            }
            else
            {
                return Json(new { Success = false, ErrorMessage = "Group name or Code exist. Try another..." }, JsonRequestBehavior.DenyGet);
            }

            //}
            //var errorList = (from item in ModelState.Values
            //                 from error in item.Errors
            //                 select error.ErrorMessage).ToList();

            //return Json(JsonResponseFactory.ErrorResponse(errorList), JsonRequestBehavior.DenyGet);
        }


        public ActionResult GetMappingJQFile()
        {
            return PartialView("CashFlow_MappingJQ");
        }


        public ActionResult GetMappingTypeName(int filter = 0, string name = "")
        {
            //prepare ocode
            //Added By Avishek Date:Jan-19-2016
            int OCode = ((int?)Session["OCode"]) ?? 0;
            if (OCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End

            if (filter > 0)
            {
                ViewBag.LegendName = "Cash Flow Type List: " + name;
                var v = unitOfWork.ACC_CashFlowType.Get().Where(w => w.CashFlowGroup_Id == filter && (w.OCode == null || w.OCode == OCode.ToString())).ToList();
                // var v = unitOfWork.ACC_CashFlowGroup.GetGroup().Where(w => w.NatureID == filter && (w.OCode == null || w.OCode == OCode)).ToList();

                ViewBag.Type = v;
                return PartialView("CashFlow_MappingTypeList");
            }
            else
            {
                ViewBag.LegendName = "Type List: All";
                var v = unitOfWork.ACC_CashFlowType.Get().Where(w => (w.OCode == null || w.OCode == OCode.ToString())).ToList();
                ViewBag.Type = v;
                return PartialView("CashFlow_MappingTypeList");
            }
        }


        [GridAction]
        public ActionResult _SelectAllMapping(int page, int size, string orderBy)
        {
            int oCode = ((int?)Session["oCode"]) ?? 0;
            if (oCode == 0)
            {
                RedirectToAction("Login", "Account");
            }
            return View(new GridModel(GetAllMapping(oCode.ToString())));
        }

        private IQueryable<VM_acc_Cash_Flow_Mapping> GetAllMapping(string oCode)
        {
            context = new PFTMEntities();
            //var result = unitOfWork.ACC_CashFlowMapping.Get().Where(x => x.OCode == oCode.ToString()).ToList();
            var result = from a in context.acc_CashFlowMapping.Where(x => x.OCode == oCode)
                         join b in context.acc_CashFlow_Type on a.CashFlowType_Id equals b.CashFlowType_Id
                         join c in context.acc_CashFlowGroup on b.CashFlowGroup_Id equals c.CashFlowGroup_Id
                         join d in context.acc_Ledger on a.LedgerID equals d.LedgerID


                         select new VM_acc_Cash_Flow_Mapping
                         {
                             CashFlowMapping_Id = a.CashFlowMapping_Id,
                             LedgerID = a.LedgerID,
                             CashFlowType_Id = a.CashFlowType_Id,
                             CashFlowType = b.CashFlow_Type,
                             CashFlow_Group = c.CashFlow_Group,
                             LedgerName = d.LedgerName,

                         };


            return result;
        }

        [HttpPost]
        public ActionResult MappingDeleteConfirm(int id)
        {
            int oCode = ((int?)Session["oCode"]) ?? 0;
            if (oCode == 0)
            {
                RedirectToAction("Login", "Account");//Added by Avishek Date:Dec-20-2015
            }

            try
            {
                unitOfWork.ACC_CashFlowMapping.Delete(id);
                unitOfWork.Save();
                TempData["Message"] = "All related data successfully deleted!";
                return Json(new { Success = true, Message = TempData["Message"] as string }, JsonRequestBehavior.DenyGet);
            }
            catch (Exception x)
            {
                return Json(new { Success = false, ErrorMessage = "Problem While deleting record., \nDetails:" + x.Message }, JsonRequestBehavior.DenyGet);
            }


        }


        public ActionResult DeletePossible(int id)
        {
            return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
        }

    }
}
