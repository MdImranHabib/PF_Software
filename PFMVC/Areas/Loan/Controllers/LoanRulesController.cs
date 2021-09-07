using DLL.Repository;
using PFMVC.common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Telerik.Web.Mvc;
using DLL.ViewModel;

namespace PFMVC.Areas.Loan.Controllers
{
    public class LoanRulesController : Controller
    {

        #region Loan Rules

        int PageID = 5;
        private UnitOfWork unitOfWork = new UnitOfWork();

        /// <summary>
        /// Indexes this instance.
        /// </summary>
        /// <returns></returns>
        /// <ModifiedBy>Avishek</ModifiedBy>
        /// <ModificationDate>Mar-6-2016</ModificationDate>
        public ActionResult Index()
        {
            //Added By Avishek Date:Jan-19-2016
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End
            bool b = true;//PagePermission.HasPermission(User.Identity.Name, PageID, 0);
            if (b)
            {
                return View();
            }
            ViewBag.PageName = "Loan Rules";
            return View("Unauthorized");
        }

        /// <summary>
        /// _s the select loan rules.
        /// </summary>
        /// <returns></returns>
        /// <ModifiedBy>Avishek</ModifiedBy>
        /// <ModificationDate>Mar-6-2016</ModificationDate>
        [GridAction]
        public ActionResult _SelectLoanRules()
        {
            return View(new GridModel(GetLoanRules()));
        }

        private IEnumerable<VM_LoanRules> GetLoanRules()
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            var result = unitOfWork.LoanRulesRepository.Get().Where(x=>x.OCode == oCode).ToList();
            List<VM_LoanRules> listLoanRules = new List<VM_LoanRules>();
            foreach (var item in result)
            {
                VM_LoanRules loanRules = new VM_LoanRules();
                loanRules.ROWID = item.ROWID;
                loanRules.EffectiveFrom = item.EffectiveFrom ?? DateTime.Now;
                loanRules.OwnPartPayable = item.OwnPartPayable;
                loanRules.EmpPartPayable = item.EmpPartPayable;
                loanRules.OwnProfitPartPayable = item.OwnProfitPartPayable;
                loanRules.EmpProfitPartPayable = item.EmpProfitPartPayable;
                loanRules.IsActive = item.IsActive == 1 ? true : false;
                loanRules.WorkingDurationInMonth = item.WorkingDurationInMonth;
                loanRules.IntarestRate = (decimal)item.IntarestRate;
                loanRules.Installment = (decimal)item.InstallmentNoumber;
                loanRules.OCode = oCode;
                listLoanRules.Add(loanRules);
            }
            return listLoanRules;
        }

        /// <summary>
        /// Loans the rules form.
        /// </summary>
        /// <param name="rowid">The rowid.</param>
        /// <returns></returns>
        /// <ModifiedBy>Avishek</ModifiedBy>
        /// <ModificationDate>Mar-6-2016</ModificationDate>
        public ActionResult LoanRulesForm(int rowid = 0)
        {
            //Added By Avishek Date:Jan-19-2016
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End
            VM_LoanRules vmLoanRules = new VM_LoanRules();
            vmLoanRules.EffectiveFrom = DateTime.Now;
            if (rowid > 0)
            {
                var v = unitOfWork.LoanRulesRepository.Get(w => w.ROWID == rowid).Single();
                if (v.IsActive == 0)
                {
                    return Json(new { Success = false, ErrorMessage = "Deactivated rule cannot be edited..." }, JsonRequestBehavior.AllowGet);
                }
                //bool isThisRuleUsed = unitOfWork.ContributionRepository.IsExist(i => i.PFRulesID == v.ROWID);
                //if (isThisRuleUsed)
                //{
                //    return Json(new { Success = false, ErrorMessage = "This rule has been used in contribution process and cannot be edited..." }, JsonRequestBehavior.AllowGet);
                //}
                vmLoanRules.ROWID = v.ROWID;
                vmLoanRules.WorkingDurationInMonth = v.WorkingDurationInMonth;
                vmLoanRules.OwnPartPayable = v.OwnPartPayable;
                vmLoanRules.EmpPartPayable = v.EmpPartPayable;
                vmLoanRules.EmpProfitPartPayable = v.EmpProfitPartPayable;
                vmLoanRules.OwnProfitPartPayable = v.OwnProfitPartPayable;
                //vm_MembershipClosingRules.IsActive = v.IsActive == 1 ? true : false;
                vmLoanRules.EffectiveFrom = v.EffectiveFrom ?? DateTime.Now;
            }
            return View("_LoanRulesForm", vmLoanRules);
        }

        /// <summary>
        /// Loans the rules form.
        /// </summary>
        /// <param name="v">The v.</param>
        /// <returns></returns>
        /// <ModifiedBy>Avishek</ModifiedBy>
        /// <ModificationDate>Mar-6-2016</ModificationDate>
        [HttpPost]
        public ActionResult LoanRulesForm(VM_LoanRules v)
        {
            //Added By Avishek Date:Jan-19-2016
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End
            DLL.LU_tbl_LoanRules loanRules = new DLL.LU_tbl_LoanRules();
            if (ModelState.IsValid)
            {
                if (v.ROWID > 0)
                {
                    loanRules = unitOfWork.LoanRulesRepository.Get(w => w.ROWID == v.ROWID).Single();
                }

                var isWorkingDurationRedundant = unitOfWork.LoanRulesRepository.Get(w => w.WorkingDurationInMonth == v.WorkingDurationInMonth && w.IsActive == 1 && w.ROWID != v.ROWID).SingleOrDefault();
                if (isWorkingDurationRedundant != null)
                {
                    return Json(new { Success = false, ErrorMessage = "This rule previously exist!!!" }, JsonRequestBehavior.AllowGet);
                }

                loanRules.ROWID = v.ROWID;
                loanRules.WorkingDurationInMonth = v.WorkingDurationInMonth;
                loanRules.OwnPartPayable = v.OwnPartPayable;
                loanRules.EmpPartPayable = v.EmpPartPayable;
                loanRules.OwnProfitPartPayable = v.OwnProfitPartPayable;
                loanRules.EmpProfitPartPayable = v.EmpProfitPartPayable;
                loanRules.EditDate = System.DateTime.Now;
                loanRules.EditUser = unitOfWork.CustomRepository.GetUserID(User.Identity.Name);
                loanRules.OCode = oCode;
                loanRules.EffectiveFrom = v.EffectiveFrom;
                loanRules.InstallmentNoumber = v.Installment;
                loanRules.IntarestRate = v.IntarestRate;
            }
            else
            {
                return Json(new { Success = false, ErrorMessage = "Model state not valid" }, JsonRequestBehavior.AllowGet);
            }
            if (v.ROWID > 0)
            {
                unitOfWork.LoanRulesRepository.Update(loanRules);
            }
            else
            {
                loanRules.IsActive = (byte)1;
                unitOfWork.LoanRulesRepository.Insert(loanRules);
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
        /// Loans the rule delete possible.
        /// </summary>
        /// <param name="rowid">The rowid.</param>
        /// <returns></returns>
        /// <ModifiedBy>Avishek</ModifiedBy>
        /// <ModificationDate>Mar-6-2016</ModificationDate>
        public ActionResult LoanRuleDeletePossible(int rowid)
        {
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
            var isRuleUsed = unitOfWork.PFLoanRepository.Get(w => w.RuleUsed == rowid).FirstOrDefault();
            if (isRuleUsed != null)
            {
                return Json(new { Success = true, Message = "This rule has been used to disburse loan. This rule cannot be edit or delete possible but will be deactivated! Continue?" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { Success = true, Message = "This de-activation rule will be deleted! Continue?" }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Loans the rule delete confirm.
        /// </summary>
        /// <param name="rowid">The rowid.</param>
        /// <returns></returns>
        /// <ModifiedBy>Avishek</ModifiedBy>
        /// <ModificationDate>Mar-6-2016</ModificationDate>
        [HttpPost]
        public ActionResult LoanRuleDeleteConfirm(int rowid)
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

            var isRuleUsed = unitOfWork.PFLoanRepository.Get(w => w.RuleUsed == rowid).FirstOrDefault();
            if (isRuleUsed != null)
            {
                var v = unitOfWork.LoanRulesRepository.Get(w => w.ROWID == rowid).SingleOrDefault();
                v.IsActive = 0;
                unitOfWork.LoanRulesRepository.Update(v);
                message = "De-activated";
            }
            else
            {
                unitOfWork.LoanRulesRepository.Delete(rowid);
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
