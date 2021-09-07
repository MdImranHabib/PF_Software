using System;
using System.Linq;
using System.Web.Mvc;
using DLL.Repository;
using PFMVC.common;

namespace PFMVC.Controllers
{
    public class UnfeedMyDBController : Controller
    {
        int PageID = 10;
        UnitOfWork unitOfWork = new UnitOfWork();


        //Very careful.. this method only for developer!
        //before publishing just comment the save() method!!!

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(string passcode)
        {

            DateTime today = DateTime.Now;
             //int OCode = ((int?)Session["OCode"]) ?? 0;

             //if (OCode == 0)
             //{
             //    return Json(new { Success = false, ErrorMessage = "You must be under a compnay!" }, JsonRequestBehavior.DenyGet);
             //}

            string code = ((today.Year+today.Month)-today.Day)+"";
            if (passcode == code)
            {
                //delete from contribution
                var contributionList = unitOfWork.ContributionRepository.Get().ToList();
                contributionList.ForEach(f => unitOfWork.ContributionRepository.Delete(f));

                //delete from contributionMonth
                var contributionMonthList = unitOfWork.ContributionMonthRecordRepository.Get().ToList();
                contributionMonthList.ForEach(f => unitOfWork.ContributionMonthRecordRepository.Delete(f));

                //delete amortization
                var amortizationList = unitOfWork.AmortizationRepository.Get().ToList();
                amortizationList.ForEach(f => unitOfWork.AmortizationRepository.Delete(f));

                //delete from loan
                var loanList = unitOfWork.PFLoanRepository.Get().ToList();
                loanList.ForEach(f => unitOfWork.PFLoanRepository.Delete(f));

                var v = unitOfWork.EmployeesRepository.Get().ToList();
                v.ForEach(e => unitOfWork.EmployeesRepository.Delete(e));

                //delete voucherdetail
                var voucherDetailList = unitOfWork.ACC_VoucherDetailRepository.Get().ToList();
                voucherDetailList.ForEach(f => unitOfWork.ACC_VoucherDetailRepository.Delete(f));

                //delete voucher entry
                var voucherentryList = unitOfWork.ACC_VoucherEntryRepository.Get().ToList();
                voucherentryList.ForEach(f => unitOfWork.ACC_VoucherEntryRepository.Delete(f));

                //delete ledger list
                var ledgerList = unitOfWork.ACC_LedgerRepository.Get().Where(w => w.IsSystemDefault != true).ToList();
                ledgerList.ForEach(f => unitOfWork.ACC_LedgerRepository.Delete(f));

                //reset ledger valu
                var ledgerListToResetInitValue = unitOfWork.ACC_LedgerRepository.Get().ToList();
                foreach (var item in ledgerListToResetInitValue)
                {
                    item.InitialBalance = null;
                    item.BalanceType = null;
                }

                //delete groupList 
                var groupList = unitOfWork.ACC_GroupRepository.Get().Where(w => w.IsSystemDefault != true).ToList();
                groupList.ForEach(f => unitOfWork.ACC_GroupRepository.Delete(f));

                var instrumentList = unitOfWork.InstrumentRepository.Get().ToList();
                instrumentList.ForEach(f => unitOfWork.InstrumentRepository.Delete(f));

                //profit detail table
                var profitDistributionDetail = unitOfWork.ProfitDistributionDetailRepository.Get().ToList();
                profitDistributionDetail.ForEach(f => unitOfWork.ProfitDistributionDetailRepository.Delete(f));

                //profit summary table
                var profitDistributionSummary = unitOfWork.ProfitDistributionSummaryRepository.Get().ToList();
                profitDistributionSummary.ForEach(f => unitOfWork.ProfitDistributionSummaryRepository.Delete(f));

                try
                {
                    unitOfWork.Save();
                    return Content("Successfully Deleted!!!");
                }
                catch (Exception x)
                {
                    return Content("Something went wrong!!! Error Message: " + x.Message + "\nInnerException: " + x.InnerException);
                }
            }
            return Content("Pass code not correct!");
        }

        public ActionResult ResetAccountingCompany()
        {
            //visit 0
            //edit 1
            //delete 2
            //execute 3
            bool b = PagePermission.HasPermission(User.Identity.Name, PageID, 3);
            if (b)
            {
                ViewData["CompanyOptions"] = new SelectList(unitOfWork.CompanyInformationRepository.Get(), "CompanyID", "CompanyName", "");
                return View();
            }
            else
            {
                ViewBag.PageName = "Employee Setup";
                return View("Unauthorized");
            }
            
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetCompany(int companyID, string passcode)
        {
            
            DateTime today = DateTime.Now;

            string code = ((today.Year+today.Month)-today.Day)+"";
            if (passcode == code)
            {

                //delete voucherdetail
                var voucherDetailList = unitOfWork.ACC_VoucherDetailRepository.Get().Where(f => f.OCode == companyID).ToList();
                voucherDetailList.ForEach(f => unitOfWork.ACC_VoucherDetailRepository.Delete(f));

                //delete voucher entry
                var voucherentryList = unitOfWork.ACC_VoucherEntryRepository.Get().Where(f => f.OCode == companyID).ToList();
                voucherentryList.ForEach(f => unitOfWork.ACC_VoucherEntryRepository.Delete(f));

                //delete ledger list
                var ledgerList = unitOfWork.ACC_LedgerRepository.Get().Where(w => w.IsSystemDefault != true && w.OCode == companyID).ToList();
                ledgerList.ForEach(f => unitOfWork.ACC_LedgerRepository.Delete(f));


                //delete groupList 
                var groupList = unitOfWork.ACC_GroupRepository.Get().Where(w => w.IsSystemDefault != true && w.OCode == companyID).ToList();
                groupList.ForEach(f => unitOfWork.ACC_GroupRepository.Delete(f));

                var instrumentList = unitOfWork.InstrumentRepository.Get().Where(f => f.OCode == companyID).ToList();
                instrumentList.ForEach(f => unitOfWork.InstrumentRepository.Delete(f));
            }
            else
            {
                return Content("Pass code not correct!");
            }
            try
            {
                unitOfWork.Save();
                return Content("Successfully Deleted!!!");
            }
            catch (Exception x)
            {
                return Content("Something went wrong!!! Error Message: " + x.Message + "\nInnerException: " + x.InnerException);
            }
        }

    }
}
