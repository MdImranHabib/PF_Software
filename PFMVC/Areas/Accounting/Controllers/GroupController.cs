using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using CustomJsonResponse;
using DLL;
using DLL.ViewModel;
using Microsoft.Reporting.WebForms;
using PFMVC.common;

namespace PFMVC.Areas.Accounting.Controllers
{
    public class GroupController : BaseController
    {
        ReportDataSource rd;
        List<GroupTree> finalGroupList = new List<GroupTree>();
        int PageID = 15;

        [Authorize]
        public ActionResult GroupIndex()
        {
            //Added By Avishek Date:Jan-19-2016
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            bool b = PagePermission.HasPermission(User.Identity.Name, PageID, 0);
            if (!b)
            {
                ViewBag.PageName = "Employee Setup";
                return View("Unauthorized");
            }
            //End
            return View();
        }

        public ActionResult GroupForm(int GroupID = 0)
        {
            //Added By Avishek Date:Jan-19-2016
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End
            if (GroupID == 0)
            {
                ViewData["Nature"] = new SelectList(unitOfWork.ACC_NatureRepository.Get(), "NatureID", "NatureName");
                return PartialView("_GroupForm");
            }
            if (GroupID > 0)
            {
                var v = unitOfWork.AccountingRepository.GetGroup().Where(w => w.GroupID == GroupID).SingleOrDefault();
                if (v != null)
                {
                    ViewData["Nature"] = new SelectList(unitOfWork.ACC_NatureRepository.Get(), "NatureID", "NatureName", v.NatureID);
                    if (v.ParentGroupID > 0)
                    {
                        v.ParentGroupName = unitOfWork.ACC_GroupRepository.Get().Where(w => w.GroupID == v.ParentGroupID).Select(s => s.GroupName).Single();
                    }
                    ViewBag.User = "This record was modified by " + unitOfWork.UserProfileRepository.Get(w => w.UserID == v.EditUser).SingleOrDefault().UserFullName + " at " + v.EditDate;
                    return PartialView("_GroupForm", v);
                }
                return Json(new { Success = false, ErrorMessage = "Data not found! Unexpected error" }, JsonRequestBehavior.DenyGet);
            }
            return Json(new { Success = false, ErrorMessage = "No group record selected..." }, JsonRequestBehavior.DenyGet);
        }


        [HttpPost]
        public ActionResult GroupForm(VM_acc_group v)
        {
            //Added By Avishek Date:Jan-19-2016
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            bool b = PagePermission.HasPermission(User.Identity.Name, PageID, 1);
            if (!b)
            {
                return Json(new { Success = false, ErrorMessage = "You are not authorized in this section." }, JsonRequestBehavior.DenyGet);
            }
            //End
            if (ModelState.IsValid)
            {
                if (oCode == 0)
                {
                    return Json(new { Success = false, ErrorMessage = "To create a group you must be under a compnay!" }, JsonRequestBehavior.DenyGet);
                }
                bool isGroupNameCodeExist = unitOfWork.ACC_GroupRepository.IsExist(w => (w.GroupName == v.GroupName || w.GroupCode == v.GroupCode) && (w.GroupID != v.GroupID && w.OCode == oCode));
                if (!isGroupNameCodeExist)
                {
                    if (v.ParentGroupID > 0 && v.ParentGroupID == v.GroupID)
                    {
                        return Json(new { Success = false, ErrorMessage = "Group name and parent group name shouldn't be same!" }, JsonRequestBehavior.DenyGet);
                    }
                    if (v.GroupID == 0)
                    {
                        acc_Group acc_group = new acc_Group();
                        acc_group.GroupID = GetGroupMaxID() + 1;
                        acc_group.GroupName = v.GroupName;
                        acc_group.ParentGroupID = v.ParentGroupID;
                        acc_group.NatureID = v.NatureID == 0 ? null : (Nullable<int>)v.NatureID;
                        acc_group.OCode = oCode;
                        acc_group.GroupCode = v.GroupCode;
                        acc_group.EditDate = DateTime.Now;
                        acc_group.EditUser = unitOfWork.CustomRepository.GetUserID(User.Identity.Name);
                        unitOfWork.ACC_GroupRepository.Insert(acc_group);
                    }
                    else if (v.GroupID > 0)
                    {
                        acc_Group acc_group = unitOfWork.ACC_GroupRepository.GetByID(v.GroupID);
                        if (acc_group != null)
                        {
                            if (acc_group.IsSystemDefault == true)
                            {
                                return Json(new { Success = false, ErrorMessage = "This is default group and cannot be edit or delete." }, JsonRequestBehavior.DenyGet);
                            }
                            acc_group.GroupName = v.GroupName;
                            acc_group.ParentGroupID = v.ParentGroupID;
                            acc_group.NatureID = v.NatureID == 0 ? null : (Nullable<int>)v.NatureID;
                            acc_group.OCode = oCode;
                            acc_group.GroupCode = v.GroupCode;
                            acc_group.EditDate = DateTime.Now;
                            acc_group.EditUser = unitOfWork.CustomRepository.GetUserID(User.Identity.Name);
                            unitOfWork.ACC_GroupRepository.Update(acc_group);
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
                return Json(new { Success = false, ErrorMessage = "Group name or Code exist. Try another..." }, JsonRequestBehavior.DenyGet);
            }
            var errorList = (from item in ModelState.Values
                             from error in item.Errors
                             select error.ErrorMessage).ToList();

            return Json(JsonResponseFactory.ErrorResponse(errorList), JsonRequestBehavior.DenyGet);
        }

        public int GetGroupMaxID()
        {
            //we must ensure that group data is added before. new group data should be add after 50
            int data;
            try
            {
                data = unitOfWork.ACC_GroupRepository.Get().Select(m => m.GroupID).Max();
            }
            catch
            {
                data = 50;
            }

            if (data < 51)
            {
                data = 50;
            }
            return data;
        }

        public ActionResult DeleteGroupPossible(int GroupID = 0)
        {
            if (GroupID > 0)
            {
                //first check if RestrictDelete enabled for this item
                var isDeletePossible = unitOfWork.ACC_GroupRepository.GetByID(GroupID);
                if (isDeletePossible != null)
                {
                    if (isDeletePossible.RestrictDelete == true)
                    {
                        return Json(new { Success = false, ErrorMessage = "Restrict delete enabled for this item, cannot be deleted!" }, JsonRequestBehavior.AllowGet);
                    }
                }
                //dont't need this logic.
                //if (0 < GroupID && GroupID < 51)
                //{
                //    return Json(new { Success = false, ErrorMessage = "This is default group and cannot be edit or delete." }, JsonRequestBehavior.AllowGet);
                //}
                bool isExistInLedger = unitOfWork.ACC_LedgerRepository.IsExist(w => w.GroupID == GroupID);
                if (!isExistInLedger)
                {
                    var isUsedAsParentId = unitOfWork.ACC_GroupRepository.Get(w => w.ParentGroupID == GroupID).FirstOrDefault();
                    if (isUsedAsParentId == null)
                    {
                        return Json(new { Success = true, Message = "Sure deleting Group record?" }, JsonRequestBehavior.AllowGet);
                    }
                    return Json(new { Success = false, ErrorMessage = "This group is used as parent for " + isUsedAsParentId.GroupName }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { Success = false, ErrorMessage = "This group is used in Ledger. Cannot be deleted!" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { Success = false, ErrorMessage = "GroupID not found!" }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DeleteGroupConfirm(int GroupID = 0)
        {
            //Added By Avishek Date:Jan-19-2016
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End
            bool b = PagePermission.HasPermission(User.Identity.Name, PageID, 2);
            if (!b)
            {
                return Json(new { Success = false, ErrorMessage = "You are not authorized in this section." }, JsonRequestBehavior.DenyGet);
            }

            if (GroupID > 0)
            {
                var isDeletePossible = unitOfWork.ACC_GroupRepository.GetByID(GroupID);
                if (isDeletePossible != null)
                {
                    if (isDeletePossible.RestrictDelete == true)
                    {
                        return Json(new { Success = false, ErrorMessage = "Restrict delete enabled for this item, cannot be deleted!" }, JsonRequestBehavior.AllowGet);
                    }
                }
                unitOfWork.ACC_GroupRepository.Delete(GroupID);
                try
                {
                    unitOfWork.Save();
                    return Json(new { Success = true, Message = "Group deleted..." }, JsonRequestBehavior.DenyGet);
                }
                catch (Exception x)
                {
                    return Json(new { Success = false, ErrorMessage = "Error! " + x.Message }, JsonRequestBehavior.DenyGet);
                }
            }
            return Json(new { Success = false, ErrorMessage = "GroupID not found!" }, JsonRequestBehavior.DenyGet);
        }


        public ActionResult GetGroupName(int filter = 0, string name = "")
        {
            //prepare ocode
            //Added By Avishek Date:Jan-19-2016
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End
            if (filter > 0)
            {
                ViewBag.LegendName = "Group List: " + name;
                var v = unitOfWork.AccountingRepository.GetGroup().Where(w => w.NatureID == filter && (w.OCode == null || w.OCode == oCode)).ToList();
                ViewBag.Group = v;
                return PartialView("_GroupNameList");
            }
            else
            {
                ViewBag.LegendName = "Group List: All";
                var v = unitOfWork.AccountingRepository.GetGroup().Where(w => (w.OCode == null || w.OCode == oCode)).ToList();
                ViewBag.Group = v;
                return PartialView("_GroupNameList");
            }
        }

        public ActionResult GetGroupNameJson(int filter = 0, string term = "")
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (filter > 0 && !string.IsNullOrEmpty(term))
            {
                var v = unitOfWork.AccountingRepository.GetGroup().Where(w => w.NatureID == filter && w.GroupName.Contains(term) && (w.OCode == null || w.OCode == oCode)).Select(s => new { value = s.GroupID, label = s.GroupName }).ToList();
                return Json(v, JsonRequestBehavior.AllowGet);
            }
            if (filter == 0 && !string.IsNullOrEmpty(term))
            {
                var v = unitOfWork.AccountingRepository.GetGroup().Where(w => w.GroupName.Contains(term) && (w.OCode == null || w.OCode == oCode)).Select(s => new { value = s.GroupID, label = s.GroupName }).ToList();
                return Json(v, JsonRequestBehavior.AllowGet);
            }
            return Json(null, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetJQFile()
        {
            return PartialView("_GroupJQ");
        }

        public ActionResult ReportGroupTree()
        {
            //Added By Avishek Date:Jan-19-2016
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End
            LocalReport lr = new LocalReport();
            string path = Path.Combine(Server.MapPath("~/Areas/Accounting/AccountingReport"), "GroupTree.rdlc");
            if (System.IO.File.Exists(path))
            {
                lr.ReportPath = path;
            }
            else
            {
                return View("Report/ReportPF/Index");
            }

            var v = unitOfWork.AccountingRepository.sp_GroupTree();
            //now fileter group tree by OCode
            v = v.Where(f => f.OCode == null || f.OCode == oCode).ToList();

            rd = new ReportDataSource("DataSet1", v);
            lr.DataSources.Add(rd);

            string reportType = "PDF";
            string mimeType;
            string encoding;
            string fileNameExtension;
            string deviceInfo =

            @"<DeviceInfo>" +
            "  <OutputFormat>PDF</OutputFormat>" +
            "</DeviceInfo>";

            Warning[] warnings;
            string[] streams;
            byte[] renderedBytes;

            renderedBytes = lr.Render(
                reportType,
                deviceInfo,
                out mimeType,
                out encoding,
                out fileNameExtension,
                out streams,
                out warnings);


            return File(renderedBytes, mimeType);
        }

        public ActionResult LedgerTreeView()
        {
            //Added By Avishek Date:Jan-19-2016
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End
            GroupTree cat;
            List<GroupTree> ledgerList = new List<GroupTree>();
            var result = unitOfWork.AccountingRepository.sp_GroupTree();
            //filter result by group
            result = result.Where(f => f.OCode == null || f.OCode == oCode).ToList();
            foreach (var item in result)
            {
                cat = new GroupTree();
                cat.GroupID = item.GroupID;
                cat.ParentGroupID = item.ParentGroupID ?? 0;
                cat.GroupName = item.GroupName;
                cat.ParentGroupName = item.ParentGroupName;
                ledgerList.Add(cat);
            }
            var president = ledgerList.Where(x => x.ParentGroupID == 0).ToList();
            foreach (var item in president)
            {
                SetChildren(item, ledgerList);
                finalGroupList.Add(item);
            }
            return PartialView("CATree1", finalGroupList);
        }

        private void SetChildren(GroupTree model, List<GroupTree> ledgerList)
        {
            var childs = ledgerList.Where(x => x.ParentGroupID == model.GroupID).ToList();
            if (childs.Count > 0)
            {
                foreach (var child in childs)
                {
                    SetChildren(child, ledgerList);
                    model.GTs.Add(child);
                }
            }
        }
        
    }

   public class GroupTree
    {
        public int GroupID { set; get; }
        public int ParentGroupID { set; get; }
        public string GroupName { set; get; }
        public string ParentGroupName { set; get; }
        
        public IList<GroupTree> GTs { set; get; }
        public GroupTree()
        {
            GTs = new List<GroupTree>();
        }
    }
}
