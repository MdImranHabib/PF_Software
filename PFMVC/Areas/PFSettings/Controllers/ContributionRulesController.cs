using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using DLL;
using DLL.Repository;
using DLL.ViewModel;
using PFMVC.common;
using Telerik.Web.Mvc;

namespace PFMVC.Areas.PFSettings.Controllers
{
    public class ContributionRulesController : Controller
    {
        int PageID = 5;
        private UnitOfWork unitOfWork = new UnitOfWork();

        /// <summary>
        /// Pfs the contribution rules.
        /// </summary>
        /// <returns></returns>
        /// <ModifiedBy>Avishek</ModifiedBy>
        /// <ModificationDate>Dec-15-2015</ModificationDate>
        public ActionResult PFContributionRules()
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
                return View("ContributionRules");
            }
            ViewBag.PageName = "PF Core";
            return View("Unauthorized");
        }

        [GridAction]
        public ActionResult _SelectPFRules()
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            return View(new GridModel(GetPFRules(oCode)));
        }

        private IEnumerable<VM_PFRules> GetPFRules(int oCode)
        {
            var result = unitOfWork.PFRulesRepository.Get().Where(x=>x.OCode == oCode);
            List<VM_PFRules> lstVmPfRules = new List<VM_PFRules>();
            foreach (var item in result)
            {
                var vmPfRules = new VM_PFRules();
                vmPfRules.ROWID = item.ROWID;
                vmPfRules.EffectiveFrom = item.EffectiveFrom;
                vmPfRules.EmployeeContribution = item.EmployeeContribution;
                vmPfRules.EmployerContribution = item.EmployerContribution;
                vmPfRules.IsActive = item.IsActive == 1 ? true : false;
                vmPfRules.WorkingDurationInMonth = item.WorkingDurationInMonth;
                lstVmPfRules.Add(vmPfRules);
            }
            return lstVmPfRules;
        }

        /// <summary>
        /// Pfs the rules form.
        /// </summary>
        /// <param name="rowid">The rowid.</param>
        /// <returns></returns>
        /// <ModifiedBy>Avishek</ModifiedBy>
        /// <ModificationDate>Dec-15-2015</ModificationDate>
        public ActionResult PFRulesForm(int rowid = 0)
        {
            //Added By Avishek Date:Jan-19-2016
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End
            VM_PFRules vmPfRules = new VM_PFRules();
            if (rowid > 0)
            {
                var v = unitOfWork.PFRulesRepository.Get(w => w.ROWID == rowid).SingleOrDefault();
                if (v.IsActive == 0)
                {
                    return Json(new { Success = false, ErrorMessage = "Deactivated rule cannot be edited..." }, JsonRequestBehavior.AllowGet);
                }
                bool isThisRuleUsed = unitOfWork.ContributionRepository.IsExist(i => i.PFRulesID == v.ROWID);
                if (isThisRuleUsed)
                {
                    return Json(new { Success = false, ErrorMessage = "This rule has been used in contribution process and cannot be edited..." }, JsonRequestBehavior.AllowGet);
                }
                vmPfRules.ROWID = v.ROWID;
                vmPfRules.WorkingDurationInMonth = v.WorkingDurationInMonth;
                vmPfRules.EmployeeContribution = v.EmployeeContribution;
                vmPfRules.EmployerContribution = v.EmployerContribution;
                vmPfRules.IsActive = v.IsActive == 1 ? true : false;
                vmPfRules.EffectiveFrom = v.EffectiveFrom;
            }
            return View("_PFRulesForm", vmPfRules);
        }

        /// <summary>
        /// Pfs the rules form.
        /// </summary>
        /// <param name="v">The v.</param>
        /// <returns></returns>
        /// <ModifiedBy>Avishek</ModifiedBy>
        /// <ModificationDate>Dec-15-2015</ModificationDate>
        [HttpPost]
        public ActionResult PFRulesForm(VM_PFRules v)
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
            
            if (ModelState.IsValid)
            {
                //if edit operation check if this rule already in use
                bool isThisRuleUsed = unitOfWork.ContributionRepository.IsExist(i => i.PFRulesID == v.ROWID);
                if (isThisRuleUsed)
                {
                    return Json(new { Success = false, ErrorMessage = "This rule has been used in contribution process and cannot be edited..." }, JsonRequestBehavior.AllowGet);
                }
                //check if rule with same working duration exist
                var isWorkingDurationRedundant = unitOfWork.PFRulesRepository.Get(w => w.WorkingDurationInMonth == v.WorkingDurationInMonth && w.OCode == oCode && w.IsActive == 1 && w.ROWID != v.ROWID).SingleOrDefault();
                if (isWorkingDurationRedundant != null)
                {
                    return Json(new { Success = false, ErrorMessage = "This rule previously exist!!!" }, JsonRequestBehavior.AllowGet);
                }
                if (v.ROWID == 0)
                {
                    LU_tbl_PFRules luTblPfRules = new LU_tbl_PFRules();
                    luTblPfRules.ROWID = v.ROWID;
                    luTblPfRules.WorkingDurationInMonth = v.WorkingDurationInMonth;
                    luTblPfRules.EmployeeContribution = v.EmployeeContribution;
                    luTblPfRules.EmployerContribution = v.EmployerContribution;
                    luTblPfRules.EditDate = System.DateTime.Now;
                    luTblPfRules.EditUser = unitOfWork.CustomRepository.GetUserID(User.Identity.Name);
                    luTblPfRules.IsActive = v.IsActive == true ? (byte)1 : (byte)0;
                    luTblPfRules.EffectiveFrom = v.EffectiveFrom;
                    luTblPfRules.OCode = oCode;
                    unitOfWork.PFRulesRepository.Insert(luTblPfRules);
                }
                else if (v.ROWID > 0)
                {
                    LU_tbl_PFRules luTblPfRules = unitOfWork.PFRulesRepository.GetByID(v.ROWID);
                    luTblPfRules.ROWID = v.ROWID;
                    luTblPfRules.WorkingDurationInMonth = v.WorkingDurationInMonth;
                    luTblPfRules.EmployeeContribution = v.EmployeeContribution;
                    luTblPfRules.EmployerContribution = v.EmployerContribution;
                    luTblPfRules.EditDate = System.DateTime.Now;
                    luTblPfRules.EditUser = unitOfWork.CustomRepository.GetUserID(User.Identity.Name);
                    luTblPfRules.IsActive = v.IsActive == true ? (byte)1 : (byte)0;
                    luTblPfRules.EffectiveFrom = v.EffectiveFrom;
                    luTblPfRules.OCode = oCode;
                    unitOfWork.PFRulesRepository.Update(luTblPfRules);
                }
                else
                {
                    return Json(new { Success = false, ErrorMessage = "Undefined row id! cannot process!" }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(new { Success = false, ErrorMessage = "Model state not valid" }, JsonRequestBehavior.AllowGet);
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

        /// <summary>
        /// Pfs the rule delete possible.
        /// </summary>
        /// <param name="rowid">The rowid.</param>
        /// <returns></returns>
        /// <ModifiedBy>Avishek</ModifiedBy>
        /// <ModificationDate>Dec-15-2015</ModificationDate>
        public ActionResult PFRuleDeletePossible(int rowid)
        {
            bool b = PagePermission.HasPermission(User.Identity.Name, PageID, 0);
            if (!b)
            {
                return Json(new { Success = false, ErrorMessage = "You are not authorized to delete information!" }, JsonRequestBehavior.AllowGet);
            }
            var v = unitOfWork.PFRulesRepository.Get(w => w.ROWID == rowid).SingleOrDefault();
            if (v.IsActive == 0)
            {
                var isUsedInContribution = unitOfWork.ContributionRepository.Get(w => w.PFRulesID == rowid).FirstOrDefault();
                if (isUsedInContribution == null)
                {
                    return Json(new { Success = true, Message = "This rule already de-activated and will deleted! Proceed?" }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { Success = false, ErrorMessage = "This rule already de-activated!" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { Success = true, Message = "Sure de-activate this rule?" }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Pfs the rule delete confirm.
        /// </summary>
        /// <param name="rowid">The rowid.</param>
        /// <returns></returns>
        /// <ModifiedBy>Avishek</ModifiedBy>
        /// <ModificationDate>Dec-15-2015</ModificationDate>
        [HttpPost]
        public ActionResult PFRuleDeleteConfirm(int rowid)
        {
            //Added By Avishek Date:Jan-19-2016
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End
            string message = "";
            bool isAllowedToEdit = PagePermission.HasPermission(User.Identity.Name, PageID, 1);
            if (!isAllowedToEdit) return Json(new { Success = false, ErrorMessage = "You are not allowed to edit information! contact system admin!" }, JsonRequestBehavior.AllowGet);

            var isUsedInContribution = unitOfWork.ContributionRepository.Get(w => w.PFRulesID == rowid).FirstOrDefault();
            if (isUsedInContribution == null)
            {
                unitOfWork.PFRulesRepository.Delete(rowid);
                message = "deleted";
            }
            else
            {
                var v = unitOfWork.PFRulesRepository.Get(w => w.ROWID == rowid).SingleOrDefault();
                v.IsActive = 0;
                unitOfWork.PFRulesRepository.Update(v);
                message = "de-activated";
            }
            try
            {
                unitOfWork.Save();
                return Json(new { Success = true, Message = "Rule " + message + "!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception x)
            {
                return Json(new { Success = false, Message = "Check error: " + x.Message }, JsonRequestBehavior.AllowGet);
            }

        }

    }
}
