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
    public class LedgerController : BaseController
    {
        ReportDataSource rd;
        List<ChartAccountTree> finalLedgerList = new List<ChartAccountTree>();
        int PageID = 16;

        public ActionResult LedgerIndex()
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


        public ActionResult LedgerForm(Guid LedgerID)
        {
            //Added By Avishek Date:Jan-19-2016
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End
            List<SelectListItem> items = new List<SelectListItem>();
            items.Add(new SelectListItem() { Text = "Cr.", Value = "1", Selected = false });
            items.Add(new SelectListItem() { Text = "Dr.", Value = "2", Selected = false });
            if (LedgerID == Guid.Empty)
            {
                ViewData["Balance"] = new SelectList(items, "Value", "Text");
                return PartialView("_LedgerForm");
            }
            var v = unitOfWork.AccountingRepository.GetLedger().SingleOrDefault(w => w.LedgerID == LedgerID);
            if (v != null)
            {
                ViewData["Balance"] = new SelectList(items, "Value", "Text", v.BalanceType);
                ViewBag.User = "This record was modified by " + unitOfWork.UserProfileRepository.Get(w => w.UserID == v.EditUser).SingleOrDefault().UserFullName + " at " + v.EditDate;
                return PartialView("_LedgerForm", v);
            }
            return Json(new { Success = false, ErrorMessage = "Data not found! Unexpected error" }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LedgerForm(VM_acc_ledger v)
        {
            //Added By Avishek Date:Jan-19-2016
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End
            bool b = PagePermission.HasPermission(User.Identity.Name, PageID, 1);  
            if (!b)
            {
                return Json(new { Success = false, ErrorMessage = "You donot have permission in this section." }, JsonRequestBehavior.DenyGet);
            }

            //if (ModelState.IsValid)  
            //{
                string message = "";
                if (!IsLedgerEditOrDeletePossible(v.LedgerID, ref message))
                {
                    return Json(new { Success = false, ErrorMessage = "This is system generated Ledger, cannot be edit or deleted! "+message }, JsonRequestBehavior.DenyGet);
                }
                if (v.InitialBalance <0)
                {
                    return Json(new { Success = false, ErrorMessage = "Negative value not accepted!" }, JsonRequestBehavior.DenyGet);
                }

                if (v.InitialBalance > 0 && (v.BalanceType == null || v.BalanceType == 0))
                {
                    return Json(new { Success = false, ErrorMessage = "This this ledger has initial balance then you must define balance type." }, JsonRequestBehavior.DenyGet);
                }
                bool isLedgerNameExist = unitOfWork.ACC_LedgerRepository.IsExist(w => (w.LedgerName == v.LedgerName || w.LedgerCode == v.LedgerCode) && (w.LedgerID != v.LedgerID && w.OCode == oCode));
                if (!isLedgerNameExist)
                {
                    if (v.LedgerID == Guid.Empty)
                    {
                        acc_Ledger accLedger = new acc_Ledger();
                        accLedger.GroupID = v.GroupID;
                        accLedger.LedgerName = v.LedgerName;
                        accLedger.LedgerID = Guid.NewGuid();
                        accLedger.InitialBalance = v.InitialBalance;
                        accLedger.BalanceType = v.BalanceType;
                        accLedger.EditDate = DateTime.Now;
                        accLedger.EditUser = unitOfWork.CustomRepository.GetUserID(User.Identity.Name);
                        accLedger.OCode = oCode;
                        accLedger.LedgerCode = v.LedgerCode;
                        unitOfWork.ACC_LedgerRepository.Insert(accLedger);
                    }
                    else
                    {
                        acc_Ledger accLedger = unitOfWork.ACC_LedgerRepository.GetByID(v.LedgerID);
                        if (accLedger != null)
                        {
                            accLedger.GroupID = v.GroupID;
                            accLedger.LedgerName = v.LedgerName;
                            accLedger.InitialBalance = v.InitialBalance;
                            accLedger.BalanceType = v.BalanceType;
                            accLedger.EditDate = DateTime.Now;
                            accLedger.EditUser = unitOfWork.CustomRepository.GetUserID(User.Identity.Name);
                            accLedger.OCode = oCode;
                            accLedger.LedgerCode = v.LedgerCode;
                            unitOfWork.ACC_LedgerRepository.Update(accLedger);
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
                return Json(new { Success = false, ErrorMessage = "Ledger name or code exist. Try another..." }, JsonRequestBehavior.DenyGet);
            //}
            var errorList = (from item in ModelState.Values
                             from error in item.Errors
                             select error.ErrorMessage).ToList();

            return Json(JsonResponseFactory.ErrorResponse(errorList), JsonRequestBehavior.DenyGet);
        }

        public bool IsLedgerEditOrDeletePossible(Guid ledgerID, ref string s)
        {   
            try
            {
                var isDeletePossible = unitOfWork.ACC_LedgerRepository.GetByID(ledgerID);
                if (isDeletePossible != null)
                {
                    if (isDeletePossible.RestrictDelete == true)
                    {
                        return false;
                    }
                }
                return true;
            }
            catch (Exception x)
            {
                s = x.Message;
                return false;
            }
        }

        public ActionResult DeleteLedgerPossible(Guid LedgerID)
        {
            if (LedgerID != Guid.Empty)
            {
                string message = "";
                if (!IsLedgerEditOrDeletePossible(LedgerID,  ref message))
                {
                    return Json(new { Success = false , ErrorMessage = "Delete restriction enabled for this Ledger, cannot be deleted!"+message }, JsonRequestBehavior.AllowGet);
                }
                var isLedgerUsedInVoucher = unitOfWork.ACC_VoucherDetailRepository.Get(w => w.LedgerID == LedgerID).FirstOrDefault();
                if (isLedgerUsedInVoucher == null)
                {
                    return Json(new { Success = true, Message = "Sure deleting this ledger?" }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { Success = false, ErrorMessage = "This ledger used in voucher " }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { Success = false, ErrorMessage = "Ledger object not found!" }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DeleteLedgerConfirm(Guid LedgerID)
        {
            //Added By Avishek Date:Jan-19-2016
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            bool b = PagePermission.HasPermission(User.Identity.Name, PageID, 2);
            if (!b)
            {
                return Json(new { Success = false, ErrorMessage = "You donot have permission in this section." }, JsonRequestBehavior.DenyGet);
            }
            //End
            if (LedgerID != Guid.Empty)
            {
                string message = "";
                if (!IsLedgerEditOrDeletePossible(LedgerID, ref message))
                {
                    return Json(new { Success = false, ErrorMessage = "This is system generated Ledger, cannot be deleted! "+message }, JsonRequestBehavior.AllowGet);
                }
                unitOfWork.ACC_LedgerRepository.Delete(LedgerID);
                try
                {
                    unitOfWork.Save();
                    return Json(new { Success = true, Message = "Ledger data deleted!" }, JsonRequestBehavior.DenyGet);
                }
                catch (Exception x)
                {
                    return Json(new { Success = false, ErrorMessage = "Error! " + x.Message }, JsonRequestBehavior.DenyGet);
                }
            }
            return Json(new { Success = false, ErrorMessage = "Ledger object not found!" }, JsonRequestBehavior.DenyGet);
        }

        public ActionResult GetLedgerNameList()
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            ViewBag.LegendName = "Ledger List";
            var v = unitOfWork.ACC_LedgerRepository.Get(w => w.ParentLedgerID == null && (w.OCode == null || w.OCode == oCode)).ToList(); //.Select(s => new { LedgerID = s.LedgerID, LedgerName = s.LedgerName })
            ViewBag.Ledger = v;
            return PartialView("_LedgerNameList");
        }

        public ActionResult GetJQFile()
        {
            return PartialView("_LedgerJQ");
        }

        public ActionResult ReportChartOfAccount()
        {
            LocalReport lr = new LocalReport();
            //Added By Avishek Date:Jan-19-2016
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End

            string path = Path.Combine(Server.MapPath("~/Areas/Accounting/AccountingReport"), "ChartAccount.rdlc");
            if (System.IO.File.Exists(path))
            {
                lr.ReportPath = path;
            }
            else
            {
                return View("Report/ReportPF/Index");
            }

            var v = unitOfWork.AccountingRepository.sp_ChartOfAccount(oCode).ToList();
            //filter chart of account by this user OCode
            //v = v.Where(f => f.OCode == null || f.OCode == OCode).ToList();
            var getCompany = unitOfWork.CompanyInformationRepository.GetByID(oCode);
            ReportParameterCollection reportParameters = new ReportParameterCollection();
            reportParameters.Add(new ReportParameter("rpCompanyName", getCompany.CompanyName + ""));
            reportParameters.Add(new ReportParameter("rpCompanyAddress", getCompany.CompanyAddress + ""));
            lr.SetParameters(reportParameters);
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
            ChartAccountTree cat;
            List<ChartAccountTree> ledgerList = new List<ChartAccountTree>();
            int oCode = ((int?)Session["OCode"]) ?? 0;
            var result = unitOfWork.AccountingRepository.sp_ChartOfAccount(oCode);

            //now filter by ocode
            result = result.Where(f => f.OCode == 0 || f.OCode == oCode).ToList();

            foreach (var item in result)
            {
                cat = new ChartAccountTree();
                cat.GroupID = item.GroupID;
                cat.ParentGroupID = item.ParentGroupID;
                cat.GroupName = item.GroupName;
                cat.ParentGroupName = item.ParentGroup;
                cat.LedgerName = item.LedgerName;
                ledgerList.Add(cat);
            }
            var president = ledgerList.Where(x => x.ParentGroupID == 0).ToList();
            foreach (var item in president)
            {
                SetChildren(item, ledgerList);
                finalLedgerList.Add(item);
            }
            return PartialView("CATree1", finalLedgerList);
        }

        private void SetChildren(ChartAccountTree model, List<ChartAccountTree> ledgerList)
        {
            var childs = ledgerList.
                            Where(x => x.ParentGroupID == model.GroupID).ToList();
            if (childs.Count > 0)
            {
                foreach (var child in childs)
                {
                    SetChildren(child, ledgerList);
                    model.CATs.Add(child);
                }
            }
        }

        public ActionResult LedgerListByGroup(int groupID)
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            List<string> ledgerList = new List<string>();
            //ledger filtered by this user ledger
            ledgerList = unitOfWork.ACC_LedgerRepository.Get().Where(w => w.GroupID == groupID && (w.OCode == null || w.OCode == oCode)).Select(s => s.LedgerName).ToList();
            if (ledgerList.Count > 0)
            {
                return Json(new { Success = true, Data = ledgerList, Message = ledgerList.Count+ " Ledger Found!" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { Success = false, Message = "No Ledger Found!" }, JsonRequestBehavior.AllowGet);
        }
    }

    public class ChartAccountTree
    {
        public int GroupID { set; get; }
        public int ParentGroupID { set; get; }
        public string GroupName { set; get; }
        public string ParentGroupName { set; get; }
        public string LedgerName {get;set;}
        public IList<ChartAccountTree> CATs { set; get; }
        public ChartAccountTree()
        {
            CATs = new List<ChartAccountTree>();
        }
    }
}
