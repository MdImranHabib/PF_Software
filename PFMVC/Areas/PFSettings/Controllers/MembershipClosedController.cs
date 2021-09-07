using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using DLL.Repository;
using DLL.ViewModel;
using PFMVC.common;
using Telerik.Web.Mvc;
using DLL;

namespace PFMVC.Areas.PFSettings.Controllers
{
    public class MembershipClosedController : Controller
    {
        #region PFMembershipClosed Rules

        int PageID = 5;
        private UnitOfWork unitOfWork = new UnitOfWork();

        /// <summary>
        /// Indexes this instance.
        /// </summary>
        /// <returns></returns>
        /// <ModifiedBy>Avishek</ModifiedBy>
        /// <ModificationDate>Dec-15-2015</ModificationDate>
        public ActionResult Index()
        {
            //Added By Avishek Date:Jan-19-2016
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End
            bool b = PagePermission.HasPermission(User.Identity.Name, PageID, 0);
            if (b)
            {
                return View();
            }
            ViewBag.PageName = "PF Core";
            return View("Unauthorized");
        }

        [GridAction]
        public ActionResult _SelectPFMembershipClosingRules()
        {
            return View(new GridModel(GetPFRules()));
        }

        private IEnumerable<VM_MembershipClosingRules> GetPFRules()
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            var result = unitOfWork.MembershipClosingRulesRepository.Get().Where(x=>x.OCode == oCode);
            List<VM_MembershipClosingRules> lstVmMembershipClosingRules = new List<VM_MembershipClosingRules>();
            foreach (var item in result)
            {
                var vmMembershipClosingRules = new VM_MembershipClosingRules();
                vmMembershipClosingRules.ROWID = item.ROWID;
                vmMembershipClosingRules.EffectiveFrom = item.EffectiveFrom;
                vmMembershipClosingRules.OwnPartPercent = item.OwnPartPayable == null ? 0 : item.OwnPartPayable;
                vmMembershipClosingRules.EmployerPartPercent = item.EmployerPartPayable == null ? 0 : item.EmployerPartPayable;
                vmMembershipClosingRules.EmpProfitPercent = Convert.ToInt32(item.EmpProfitPercent) == null ? 0 : Convert.ToInt32(item.EmpProfitPercent);
                vmMembershipClosingRules.OwnProfitPercent = Convert.ToInt32(item.OwnProfitPercent) == null ? 0 : Convert.ToInt32(item.OwnProfitPercent);
                vmMembershipClosingRules.IsActive = item.IsActive == 1 ? true : false;
                vmMembershipClosingRules.PFDurationInMonth = item.PFDurationInMonth;
                if (item.OwnProfitPercent == null)
                {
                    vmMembershipClosingRules.OwnProfitPercent = 0;
                }
                else {
                    vmMembershipClosingRules.OwnProfitPercent = (decimal)item.OwnProfitPercent;
                }
                lstVmMembershipClosingRules.Add(vmMembershipClosingRules);
            }
            return lstVmMembershipClosingRules;
        }

        public ActionResult MembershipClosingRulesForm(int rowid = 0)
        {
            //Added By Avishek Date:Jan-19-2016
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End
            VM_MembershipClosingRules vmMembershipClosingRules = new VM_MembershipClosingRules();
            vmMembershipClosingRules.EffectiveFrom = DateTime.Now;
            if (rowid > 0)
            {
                var v = unitOfWork.MembershipClosingRulesRepository.Get(w => w.ROWID == rowid).Single();
                if (v.IsActive == 0)
                {
                    return Json(new { Success = false, ErrorMessage = "Deactivated rule cannot be edited..." }, JsonRequestBehavior.AllowGet);
                }
                vmMembershipClosingRules.ROWID = v.ROWID;
                vmMembershipClosingRules.PFDurationInMonth = v.PFDurationInMonth;
                vmMembershipClosingRules.EmployerPartPercent = v.EmployerPartPayable == null ? 0 : v.EmployerPartPayable;
                vmMembershipClosingRules.OwnPartPercent = v.OwnPartPayable == null ? 0 : v.OwnPartPayable;
                vmMembershipClosingRules.OwnProfitPercent = Convert.ToInt32(v.OwnProfitPercent)== null ? 0 :Convert.ToInt32(v.OwnProfitPercent);
                vmMembershipClosingRules.EmpProfitPercent = Convert.ToInt32(v.EmpProfitPercent) == null ? 0 : Convert.ToInt32(v.EmpProfitPercent);
                vmMembershipClosingRules.EffectiveFrom = v.EffectiveFrom;
            }
            return View("_PFMembershipClosedRulesForm", vmMembershipClosingRules);
        }

        [HttpPost]
        public ActionResult MembershipClosingRulesForm(VM_MembershipClosingRules v)
        {
            //Added By Avishek Date:Jan-19-2016
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End
            bool isAllowedToEdit = PagePermission.HasPermission(User.Identity.Name, PageID, 1);
            if (!isAllowedToEdit) return Json(new { Success = false, ErrorMessage = "You are not allowed to edit information! contact system admin!" }, JsonRequestBehavior.AllowGet);
            LU_tbl_MembershipClosingRules luTblMembershipClosingRules = new LU_tbl_MembershipClosingRules();
            if (ModelState.IsValid)
            {
                if (v.ROWID > 0)
                {
                    luTblMembershipClosingRules = unitOfWork.MembershipClosingRulesRepository.Get(w => w.ROWID == v.ROWID).Single();
                }

                var isWorkingDurationRedundant = unitOfWork.MembershipClosingRulesRepository.Get(w => w.PFDurationInMonth == v.PFDurationInMonth && w.OCode == oCode && w.IsActive == 1 && w.ROWID != v.ROWID).SingleOrDefault();
                if (isWorkingDurationRedundant != null)
                {
                    return Json(new { Success = false, ErrorMessage = "This rule previously exist!!!" }, JsonRequestBehavior.AllowGet);
                }

                luTblMembershipClosingRules.ROWID = v.ROWID;
                luTblMembershipClosingRules.PFDurationInMonth = v.PFDurationInMonth;
                luTblMembershipClosingRules.OwnPartPayable = Convert.ToInt32(v.OwnPartPercent) == null ? 0 : Convert.ToInt32(v.OwnPartPercent);
                luTblMembershipClosingRules.EmployerPartPayable = Convert.ToInt32(v.EmployerPartPercent) == null ? 0 : Convert.ToInt32(v.EmployerPartPercent);
                luTblMembershipClosingRules.EmpProfitPercent = Convert.ToInt32(v.EmpProfitPercent) == null ? 0 : Convert.ToInt32(v.EmpProfitPercent);
                luTblMembershipClosingRules.OwnProfitPercent = Convert.ToInt32(v.OwnProfitPercent) == null ? 0 : Convert.ToInt32(v.OwnProfitPercent);
                luTblMembershipClosingRules.EditDate = System.DateTime.Now;
                luTblMembershipClosingRules.EditUser = unitOfWork.CustomRepository.GetUserID(User.Identity.Name);
                luTblMembershipClosingRules.OCode = oCode;
                luTblMembershipClosingRules.EffectiveFrom = v.EffectiveFrom;
            }
            else
            {
                return Json(new { Success = false, ErrorMessage = "Model state not valid" }, JsonRequestBehavior.AllowGet);
            }
            if (v.ROWID > 0)
            {
                unitOfWork.MembershipClosingRulesRepository.Update(luTblMembershipClosingRules);
            }
            else
            {
                luTblMembershipClosingRules.IsActive = 1;
                unitOfWork.MembershipClosingRulesRepository.Insert(luTblMembershipClosingRules);
            }
            try
            {
                unitOfWork.Save();
                return Json(new { Success = true, Message = "Rules information updated!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception x)
            {
                return Json(new { Success = false, ErrorMessage = "Check following error:\n" + x.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult PFMembershipClosingRuleDeletePossible(int rowid)
        {
            //Added By Avishek Date:Jan-19-2016
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End
            //logic
            //if rule has been used to deactivate a user in employee table
            //then this rule cannot be deleted!
            //rather it will be de-activated!
            //if this rule has never been used it will be de-activated!!!
            bool b = PagePermission.HasPermission(User.Identity.Name, PageID, 0);
            if (!b)
            {
                return Json(new { Success = false, ErrorMessage = "You are not authorized to delete information!" }, JsonRequestBehavior.AllowGet);
            }
            var isRuleUsed = unitOfWork.EmployeesRepository.Get(w => w.PFRuleUsedForDeactivation == rowid).FirstOrDefault();
            if (isRuleUsed != null)
            {
                return Json(new { Success = true, Message = "This This de-activation rule has been used to deactivate a user - "+isRuleUsed.EmpName+". This rule cannot be edit or delete possible but will be deactivated! Continue?" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { Success = true, Message = "This de-activation rule will be deleted! Continue?" }, JsonRequestBehavior.AllowGet);
            
        }

        [HttpPost]
        public ActionResult PFMembershipClosingRuleDeleteConfirm(int rowid)
        {
            //Added By Avishek Date:Jan-19-2016
            int OCode = ((int?)Session["OCode"]) ?? 0;
            if (OCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End
            string message = "";
            bool isAllowedToEdit = PagePermission.HasPermission(User.Identity.Name, PageID, 1);
            if (!isAllowedToEdit) return Json(new { Success = false, ErrorMessage = "You are not allowed to edit information! contact system admin!" }, JsonRequestBehavior.AllowGet);

            var isRuleUsed = unitOfWork.EmployeesRepository.Get(w => w.PFRuleUsedForDeactivation == rowid).FirstOrDefault();
            if (isRuleUsed != null)
            {
                var v = unitOfWork.MembershipClosingRulesRepository.Get(w => w.ROWID == rowid).SingleOrDefault();
                v.IsActive = 0;
                unitOfWork.MembershipClosingRulesRepository.Update(v);
                message = "De-activated";
            }
            else
            {   
                unitOfWork.MembershipClosingRulesRepository.Delete(rowid);
                message = "Deleted!";
            }
            try
            {
                unitOfWork.Save();
                return Json(new { Success = true, Message = "Rule " + message + "!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception x)
            {
                return Json(new { Success = false, ErrorMessage = "Check error: " + x.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        #endregion

    }
}
