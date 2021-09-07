using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DLL;
using DLL.Repository;
using DLL.ViewModel;
using Newtonsoft.Json;
using PFMVC.common;

namespace PFMVC.Areas.Contribution.Controllers
{
    public class MissingContributionController : Controller
    {
        int PageID = 6;
        public ActionResult Index()
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                RedirectToAction("Login", "Account");
            }
            bool b = PagePermission.HasPermission(User.Identity.Name, PageID, 0);
            if (!b)
            {
                return View("Unauthorized");
            }
            return View();
        }
      
        /// <summary>
        /// Autocompletes the suggestions.
        /// </summary>
        /// <param name="term">The term.</param>
        /// <returns>list</returns>
        /// <CreatedBy>Avishek</CreatedBy>
        /// <createdDate>Aug-1-2015</createdDate>
        public JsonResult AutocompleteSuggestionsForEmpId(string term)
        {
            try
            {
                UnitOfWork unitOfWork = new UnitOfWork();
                int OCode = ((int?)Session["OCode"]) ?? 0;
                var suggestions = unitOfWork.EmployeesRepository.Get().Where(x => x.IdentificationNumber.ToLower().Contains(term.ToLower()) && x.OCode == OCode).Select(s => new
                {
                    value = s.EmpName,
                    label = s.IdentificationNumber
                }).GroupBy(x => x.label).Select(g => g.FirstOrDefault()).ToList();
                return Json(suggestions, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        /// <summary>
        /// Autocompletes the suggestions.
        /// </summary>
        /// <param name="term">The term.</param>
        /// <returns>list</returns>
        /// <CreatedBy>Avishek</CreatedBy>
        /// <createdDate>Aug-1-2015</createdDate>
        public JsonResult AutocompleteSuggestionsName(string term)
        {
            try
            {
                UnitOfWork unitOfWork = new UnitOfWork();
                int OCode = ((int?)Session["OCode"]) ?? 0;
                var suggestions = unitOfWork.EmployeesRepository.Get().Where(x => x.EmpName.ToLower().Contains(term.ToLower()) && x.OCode == OCode).Select(s => new
                {
                    value = s.IdentificationNumber,
                    label = s.EmpName
                }).ToList();
                return Json(suggestions, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        /// <summary>
        /// Autocompletes the suggestions.
        /// </summary>
        /// <param name="term">The term.</param>
        /// <returns>list</returns>
        /// <CreatedBy>Avishek</CreatedBy>
        /// <createdDate>Aug-1-2015</createdDate>
        public JsonResult GetEmpId(string identificationNo)
        {
            try
            {
                UnitOfWork unitOfWork = new UnitOfWork();
                int OCode = ((int?)Session["OCode"]) ?? 0;
                int suggestions = unitOfWork.CustomRepository.GetEmployeeByIdentificationNumber(identificationNo.Trim()).Select(s => s.EmpID).FirstOrDefault();
                Session["EmpId"] = suggestions.ToString();
                return Json(suggestions);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public JsonResult SaveContribution(String jsonObject)
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                RedirectToAction("Login", "Account");
            }
            VM_Contribution VM_MissingContributions = JsonConvert.DeserializeObject<VM_Contribution>(jsonObject);
            int totalMonths = VM_MissingContributions.ContributionToDate.Month - VM_MissingContributions.ContributionFromDate.Month + 12 * (VM_MissingContributions.ContributionToDate.Year - VM_MissingContributions.ContributionFromDate.Year);
            totalMonths = totalMonths + 1;                  
            if (totalMonths <= 0)
            {
                return Json(new { Success = false, ErrorMessage = "Date range Error!" }, JsonRequestBehavior.DenyGet);
            }
            decimal monthlyEmpContribution = VM_MissingContributions.EmpContribution / totalMonths;
            decimal monthlySelfContribution = VM_MissingContributions.SelfContribution / totalMonths;
            DateTime tempContributionDate = VM_MissingContributions.ContributionFromDate;

            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                string _ConMonth = VM_MissingContributions.ContributionFromDate.Month.ToString();
                string _ConYear = VM_MissingContributions.ContributionFromDate.Year.ToString();
                bool isEntryExist = unitOfWork.ContributionRepository.IsExist(w => w.ConMonth == _ConMonth
                    && w.ConYear == _ConYear && w.EmpID == VM_MissingContributions.EmpID);
                if (isEntryExist)
                {
                    return Json(new { Success = false, ErrorMessage = "Data already exist!" }, JsonRequestBehavior.DenyGet);
                }

                string curUserName = User.Identity.Name;
                Guid curUserId = unitOfWork.CustomRepository.GetUserID(User.Identity.Name);
                for (int i = 1; i <= totalMonths; i++)
                {

                    tbl_Contribution missingContribution = new tbl_Contribution();
                    missingContribution.EmpID = VM_MissingContributions.EmpID;
                    missingContribution.EmpContribution = monthlyEmpContribution;
                    missingContribution.SelfContribution = monthlySelfContribution;
                    missingContribution.ContributionDate = Convert.ToDateTime("15-" + tempContributionDate.Month + "-" + tempContributionDate.Year); //Special date create for report view with order by
                    missingContribution.OCode = oCode;
                    missingContribution.ConMonth = tempContributionDate.Month.ToString();
                    missingContribution.ConYear = tempContributionDate.Year.ToString();
                    missingContribution.PFRulesID = 1;
                    missingContribution.ProcessDate = Convert.ToDateTime("15-" + tempContributionDate.Month + "-" + tempContributionDate.Year); //Special date create for report view with order by
                    missingContribution.EditDate = DateTime.Now;
                    missingContribution.EditUser = curUserId;
                    unitOfWork.ContributionRepository.Insert(missingContribution);

                    //--- Modified By Kamrul Hasan for Matching Month both Contribution and ContributionMonthRecord Table. 2019-02-17

                tbl_ContributionMonthRecord tblContributionMonthRecord = new tbl_ContributionMonthRecord();
                tblContributionMonthRecord.OCode = oCode;
                tblContributionMonthRecord.ConMonth = tempContributionDate.Month.ToString();
                tblContributionMonthRecord.ConYear = tempContributionDate.Year.ToString();
                tblContributionMonthRecord.PassVoucher = true;
                tblContributionMonthRecord.PassVoucherMessage = "Missing Contribution";
                tblContributionMonthRecord.EditDate = DateTime.Now;
                tblContributionMonthRecord.EditUser = curUserId;
                tblContributionMonthRecord.EditUserName = curUserName;
                tblContributionMonthRecord.TotalEmpCont = VM_MissingContributions.EmpContribution;
                tblContributionMonthRecord.TotalSelfCont = VM_MissingContributions.SelfContribution;
                tblContributionMonthRecord.ContributionDate = Convert.ToDateTime("15-" + tempContributionDate.Month + "-" + tempContributionDate.Year);
                unitOfWork.ContributionMonthRecordRepository.Insert(tblContributionMonthRecord);
                tempContributionDate = tempContributionDate.AddMonths(1);
                }
                int voucherId = 0;
                string refMessage = "";

                List<Guid> ledgerIdList = new List<Guid>();
                List<decimal> credit = new List<decimal>();
                List<decimal> debit = new List<decimal>();
                List<string> chqNumber = new List<string>();
                List<string> pfLoanId = new List<string>();
                List<string> pfMemberId = new List<string>();

                //remember diff btwn empID and identification number
                //Account should be exist with Each Identification Number 
                //LedgerNameList.Add("Members Fund"); //this is convention
                //members fund should be credited!
                List<acc_Chart_of_Account_Maping> accChartOfAccountMaping = unitOfWork.ChartofAccountMapingRepository.Get().ToList();
                Guid ledgerIdforOwnConLiability = accChartOfAccountMaping.Where(x => x.MIS_Id == 3).Select(x => x.Ledger_Id).FirstOrDefault();
                ledgerIdList.Add(ledgerIdforOwnConLiability);
                credit.Add(VM_MissingContributions.SelfContribution);
                debit.Add(0);
                chqNumber.Add("");
                pfMemberId.Add("");
                pfLoanId.Add("");

                Guid ledgerIdforOwnConAsset = accChartOfAccountMaping.Where(x => x.MIS_Id == 1).Select(x => x.Ledger_Id).FirstOrDefault();
                ledgerIdList.Add(ledgerIdforOwnConAsset);
                credit.Add(0);
                debit.Add(VM_MissingContributions.SelfContribution);
                chqNumber.Add("");
                pfMemberId.Add("");
                pfLoanId.Add("");
                //Edited by Suman

                Guid ledgerIdforEmpConLiability = accChartOfAccountMaping.Where(x => x.MIS_Id == 4).Select(x => x.Ledger_Id).FirstOrDefault();
                ledgerIdList.Add(ledgerIdforEmpConLiability);
                credit.Add(VM_MissingContributions.EmpContribution);
                debit.Add(0);
                chqNumber.Add("");
                pfMemberId.Add("");
                pfLoanId.Add("");

                Guid ledgerIdforEmpConAsset = accChartOfAccountMaping.Where(x => x.MIS_Id == 2).Select(x => x.Ledger_Id).FirstOrDefault();
                ledgerIdList.Add(ledgerIdforEmpConAsset);
                credit.Add(0);
                debit.Add(VM_MissingContributions.EmpContribution);
                chqNumber.Add("");
                pfMemberId.Add("");
                pfLoanId.Add("");

                string narration = "Missing Contribution from the month: " + VM_MissingContributions.ContributionFromDate.ToString("dd/MMM/yyyy") + " to: " + VM_MissingContributions.ContributionToDate.ToString("dd/MMM/yyyy") + " for the employee Id: " + VM_MissingContributions.IdentificationNumber;

                bool isOperationSuccess = unitOfWork.AccountingRepository.DualEntryVoucherById(VM_MissingContributions.EmpID, 5, VM_MissingContributions.ContributionDate, ref voucherId, narration, ledgerIdList, debit, credit, chqNumber, ref refMessage, curUserName, curUserId, pfMemberId, "Missing Contribution", "", "", null, pfLoanId, oCode, "Missing Contribution");

                if (isOperationSuccess)
                {
                    try
                    {
                        unitOfWork.Save();
                        return Json(new { Success = true, Message = "Successfully saved the missing contribution! Transction Sucessfull!" }, JsonRequestBehavior.DenyGet);
                    }
                    catch (Exception x)
                    {
                        return Json(new { Success = false, ErrorMessage = "Transaction Failded with following error: " + x.Message + " PLEASE CONTACT SYS ADMIN!" }, JsonRequestBehavior.DenyGet);
                    }
                }
                return Json(new { Success = false, ErrorMessage = "Transaction Failded with following error : " + refMessage }, JsonRequestBehavior.DenyGet);
            }
        }

    }
}
