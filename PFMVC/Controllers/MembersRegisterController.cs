using DLL;
using DLL.Repository;
using DLL.Utility;
using DLL.ViewModel;
using PFMVC.common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;

namespace PFMVC.Controllers
{
    public class MembersRegisterController : Controller
    {
        int _pageId = 5;
        private PFTMEntities db = new PFTMEntities();
        UnitOfWork _unitOfWork = new UnitOfWork();
        MvcApplication _mvcApplication;

        /// <summary>
        /// Indexes this instance.
        /// </summary>
        /// <returns>view</returns>
        /// <CreatedBy>Avishek</CreatedBy>
        /// <CreatedDate>1-NOV-2015</CreatedDate>
        [Authorize]
        public ActionResult Index()
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                RedirectToAction("Login", "Account");
            }
            bool b = PagePermission.HasPermission(User.Identity.Name, _pageId, 0);
            if (b)
            {
                return View();
            }
            else
            {
                ViewBag.PageName = "Member Register";
                return View("Unauthorized");
            }
        }


        /// <summary>
        /// Gets all members register.
        /// </summary>
        /// <param name="toDate">To date.</param>
        /// <param name="fromDate">From date.</param>
        /// <returns>List</returns>
        /// <CreatedBy>Avishek</CreatedBy>
        /// <CreatedDate>NOV-1-2015</CreatedDate>
        /// Edited by Fahim 27/10/2015
        public ActionResult GetAllMembersRegister(DateTime toDate, DateTime fromDate)
        {
            ViewBag.branchList = db.LU_tbl_Branch;
            //DateTime endDate = toDate;
            //DateTime startDate = fromDate;
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                RedirectToAction("Login", "Account");
            }
            _mvcApplication = new MvcApplication();
            //double distributedAmount = _unitOfWork.CustomRepository.GetAllDistributedAmount();

            //Ediited by Izab Ahmed Date Nov-26-2018

            //var employeeDetail = _unitOfWork.CustomRepository.GetAllEmployee(oCode).ToList();
            //var employeeDetail = _unitOfWork.CustomRepository.GetAllEmployee(oCode).Where(x => x.JoiningDate <= toDate).ToList();
            var employeeDetail = _unitOfWork.CustomRepository.GetAllEmployee(oCode).Where(x => x.PFStatusID == 1 && x.PFActivationDate <= toDate || x.PFDeactivationDate >= fromDate).ToList();

            if (DLL.Utility.ApplicationSetting.Branch == true)
            {
                employeeDetail = _unitOfWork.CustomRepository.GetAllEmployeeByBranch(oCode).Where(x => x.JoiningDate <= toDate).ToList();

            }
            //End by Izab Ahmed Date Nov-26-2018

            //var employeeDetail = _unitOfWork.CustomRepository.GetAllEmployee(oCode).Where(x => x.PFStatusID == 1 || x.PFDeactivationDate >= fromDate & x.PFDeactivationDate<=toDate).ToList();
            //var employeeDetail = _unitOfWork.CustomRepository.GetAllEmployee(oCode).Where(x => x.PFStatusID == 1 || x.PFDeactivationDate >= fromDate).ToList();

            List<LU_tbl_Branch> branch_detail = _unitOfWork.BranchRepository.Get(x => x.OCode == oCode).ToList();
            List<tbl_Contribution> contribution = _unitOfWork.ContributionRepository.Get(x => x.OCode == oCode).Where(x => x.ContributionDate <= toDate.AddDays(1).AddSeconds(-1)).ToList();


            ////List<tbl_Contribution> contribution = _unitOfWork.CustomRepository.ValidContributionDetail().Where(f => f.OCode == oCode && f.ContributionDate <= toDate.AddDays(1).AddSeconds(-1)).ToList();

            var profit = _unitOfWork.ProfitDistributionDetailRepository.Get(f => f.OCode == oCode).Where(f => f.TransactionDate <= toDate.AddDays(1).AddSeconds(-1)).ToList();
            //get all settlement voucher
            var settlementVoucher = _unitOfWork.ACC_VoucherEntryRepository.Get().Where(g => g.OCode == oCode && g.ActionName == "Settlement" && g.TransactionDate <= toDate.AddDays(1).AddSeconds(-1)).Select(s => s.VoucherID).ToList();

            var transferVoucher = _unitOfWork.ACC_VoucherEntryRepository.Get().Where(g => g.OCode == oCode && g.ActionName == "Transfer" && g.TransactionDate <= toDate.AddDays(1).AddSeconds(-1)).Select(s => s.VoucherID).ToList();

            //get forfeiture, members fund, loan ledger id
            Guid forfeitureLedgerID = _unitOfWork.ChartofAccountMapingRepository.Get(x => x.MIS_Id == 6).Select(x => x.Ledger_Id).FirstOrDefault();
            
            Guid transferLedgerID = _unitOfWork.ChartofAccountMapingRepository.Get(x => x.MIS_Id == 4).Select(x => x.Ledger_Id).FirstOrDefault();

            Guid distributedInterestIncomeID = _unitOfWork.ChartofAccountMapingRepository.Get(x => x.MIS_Id == 26).Select(x => x.Ledger_Id).FirstOrDefault();


            var forfeitureAmount = _unitOfWork.ACC_VoucherDetailRepository.Get().Where(f => settlementVoucher.Contains(f.VoucherID) && f.LedgerID == forfeitureLedgerID).Where(f => f.TransactionDate <= toDate.AddDays(1).AddSeconds(-1)).ToList();

            var transferAmount = _unitOfWork.ACC_VoucherDetailRepository.Get().Where(f => transferVoucher.Contains(f.VoucherID) && f.LedgerID == transferLedgerID).Where(f => f.TransactionDate <= toDate.AddDays(1).AddSeconds(-1)).ToList();

            var withdrawalAmount = _unitOfWork.ACC_VoucherDetailRepository.Get().Where(f => settlementVoucher.Contains(f.VoucherID) && f.Credit != 0.00M).Where(f => f.TransactionDate <= toDate.AddDays(1).AddSeconds(-1)).ToList();

            var distributedInterestIncomeAmount = _unitOfWork.ACC_VoucherDetailRepository.Get().Where(f => settlementVoucher.Contains(f.VoucherID) && f.LedgerID == distributedInterestIncomeID).Where(f => f.TransactionDate <= toDate.AddDays(1).AddSeconds(-1)).ToList();

            //Checked Brfore this
            List<VM_Employee> list = new List<VM_Employee>();
            string empId = "";
            var contributionBeforeDate = contribution.Where(f => f.OCode == oCode && f.ContributionDate < fromDate).GroupBy(g => g.EmpID).Select(s => new { OwnCont = s.Sum(w => w.SelfContribution), EmpCont = s.Sum(w => w.EmpContribution), EmpID = s.Key }).ToList();
            var contributionBetweenDate = contribution.Where(f => f.OCode == oCode && f.ContributionDate >= fromDate.AddSeconds(-1)).GroupBy(g => g.EmpID).Select(s => new { OwnCont = s.Sum(w => w.SelfContribution), EmpCont = s.Sum(w => w.EmpContribution), EmpID = s.Key }).ToList();
            var profitBeforeDate = profit.Where(x => x.OCode == oCode && x.TransactionDate < fromDate).GroupBy(g => g.EmpID).Select(s => new { OwnProfit = s.Sum(w => w.SelfProfit), EmpProfit = s.Sum(w => w.EmpProfit), EmpID = s.Key }).ToList();
            var forfeitureAmountBeforeDate = forfeitureAmount.Where(x => x.TransactionDate < fromDate).ToList();
            var forfeitureAmountBetweenDate = forfeitureAmount.Where(x => x.TransactionDate >= fromDate).ToList();
            var transferAmountBetweenDate = transferAmount.Where(x => x.TransactionDate >= fromDate).ToList();

            var withdrawalAmountBeforeDate = withdrawalAmount.Where(x => x.TransactionDate < fromDate).ToList();
            var withdrawalAmountBetweenDate = withdrawalAmount.Where(x => x.TransactionDate >= fromDate).ToList();
            foreach (var item in employeeDetail)
            {
                //Ediited by Avishek Date Sep-21-2015
                //Start
                var employee = new VM_Employee();
                empId = item.EmpID.ToString();
                employee.IdentificationNumber = item.IdentificationNumber;
                employee.EmpName = item.EmpName;
                employee.BranchName = item.BranchName;
                decimal openingOwn = contributionBeforeDate.Where(f => f.EmpID == item.EmpID).Sum(s => s.OwnCont);
                decimal openingEmp = contributionBeforeDate.Where(f => f.EmpID == item.EmpID).Sum(s => s.EmpCont);
                decimal selfProfitBeforeDateById = profitBeforeDate.Where(f => f.EmpID == item.EmpID).Select(x => x.OwnProfit).SingleOrDefault();
                decimal empfProfitBeforeDateById = profitBeforeDate.Where(f => f.EmpID == item.EmpID).Select(x => x.EmpProfit).SingleOrDefault();
                
                /// Modified By Kamrul for Showing transfer in amount in Member Register 2019-03-24 
                DateTime? calculatedate = DateTime.Now;

                calculatedate = item.PFActivationDate;

                
                    employee.OpeningBalance = _mvcApplication.GetNumber(item.opOwnContribution + item.opEmpContribution + item.opProfit +
                                                                    openingOwn + openingEmp +
                                                                    selfProfitBeforeDateById + empfProfitBeforeDateById);

                    employee.OpeningBalance = employee.OpeningBalance - _mvcApplication.GetNumber((withdrawalAmountBeforeDate.Where(f => f.PFMemberID == item.EmpID).Sum(s => s.Credit ?? 0)));
               
                //End by Izab Ahmed Date Nov-26-2018


                employee.PFDeactivationDate = item.PFDeactivationDate;
                employee.opDesignationName = item.opDesignationName ?? "";
                employee.opDepartmentName = item.opDepartmentName ?? "";
                employee.OwnCont = _mvcApplication.GetNumber(contributionBetweenDate.Where(f => f.EmpID == item.EmpID).Select(s => s.OwnCont).SingleOrDefault());
                employee.EmpCont = _mvcApplication.GetNumber(contributionBetweenDate.Where(f => f.EmpID == item.EmpID).Select(s => s.EmpCont).SingleOrDefault());
                employee.Forfeiture = _mvcApplication.GetNumber((decimal)forfeitureAmountBetweenDate.Where(x => x.PFMemberID == item.EmpID).Sum(x => x.Credit));

                
                employee.OwnProfit = _mvcApplication.GetNumber(profit.Where(x => x.EmpID == item.EmpID && x.TransactionDate >= fromDate).Sum(x => x.SelfProfit));
                employee.EmpProfit = _mvcApplication.GetNumber(profit.Where(x => x.EmpID == item.EmpID && x.TransactionDate >= fromDate).Sum(x => x.EmpProfit));

                //Asif end
                var forf = forfeitureAmountBetweenDate.Where(x => x.PFMemberID == item.EmpID).Sum(x => x.Credit);
                var with = withdrawalAmountBetweenDate.Where(f => f.PFMemberID == item.EmpID).Sum(s => s.Credit ?? 0);
                employee.distributedInterestIncomeProfit = distributedInterestIncomeAmount.Where(f => f.PFMemberID == item.EmpID).Sum(s => s.Debit ?? 0);
               
                employee.TransferCompanyAmount = _mvcApplication.GetNumber(transferAmountBetweenDate.Where(f => f.PFMemberID == item.EmpID).Sum(s => s.Debit ?? 0));
                
                if (employee.TransferCompanyAmount != 0)
                {
                    employee.Withdrawal = employee.TransferCompanyAmount;
                }
                else {
                    employee.Withdrawal = (_mvcApplication.GetNumber(withdrawalAmountBetweenDate.Where(f => f.PFMemberID == item.EmpID).Sum(s => s.Credit ?? 0)) - employee.Forfeiture);
                
                }
               
                employee.ShowSummaryBalance = _mvcApplication.GetNumber(Convert.ToDecimal(employee.OpeningBalance + employee.TransferAmount + employee.OwnCont + employee.EmpCont + +employee.OwnProfit + employee.EmpProfit + employee.distributedInterestIncomeProfit - employee.Withdrawal - employee.Forfeiture, new CultureInfo("en-US")));


                //Editted by Izab Ahmed Date Nov-25-2018

                if (employee.OpeningBalance == 0 && employee.TransferAmount == 0 && employee.OwnCont == 0 && employee.EmpCont == 0 && employee.OwnProfit == 0 && employee.EmpProfit == 0 && employee.Withdrawal == 0
                    && employee.Forfeiture == 0 && employee.ShowSummaryBalance == 0)
                {

                }
                else
                {
                    list.Add(employee);
                }
                //list.Add(employee);

                //End by Izab Ahmed Date Nov-25-2018

            }
            List<tbl_ProfitDistributionSummary> prof = _unitOfWork.ProfitDistributionSummaryRepository.Get().Where(f => f.FromDate >= fromDate.AddSeconds(-1) && f.ToDate >= toDate.AddSeconds(-1)).ToList();
            
            decimal selfprof = 0;
            decimal empprof = 0;
            if (prof.Count == 0)
            {
                 selfprof = 0;
                 empprof = 0;
            ViewBag.OwnProfit = 0;
            ViewBag.EmpProfit =0;
            }
            else
	        {
                 selfprof = prof.Select(p => p.SelfTotalProfit).FirstOrDefault().Value;
                 empprof = prof.Select(p => p.EmpTotalProfit).FirstOrDefault().Value;
                ViewBag.OwnProfit = prof.Select(p => p.SelfTotalProfit).FirstOrDefault();
                ViewBag.EmpProfit = prof.Select(p => p.EmpTotalProfit).FirstOrDefault();
	        }
            ViewBag.OpeningBalance = _mvcApplication.GetNumber(list.Sum(s => s.OpeningBalance));
            ViewBag.TransferAmount = _mvcApplication.GetNumber(list.Sum(s => s.TransferAmount));
            ViewBag.OwnCont = _mvcApplication.GetNumber(list.Sum(s => s.OwnCont));
            ViewBag.EmpCont = _mvcApplication.GetNumber(list.Sum(s => s.EmpCont));
            ViewBag.distributedInterestIncomeProfit = _mvcApplication.GetNumber(list.Sum(s => s.distributedInterestIncomeProfit));
            ViewBag.Withdrawal = _mvcApplication.GetNumber(list.Sum(s => s.Withdrawal));
            ViewBag.Forfeiture = _mvcApplication.GetNumber(list.Sum(s => s.Forfeiture));
            ViewBag.LoanAdjustment = _mvcApplication.GetNumber(list.Sum(s => s.LoanAdjustment));
           
            ViewBag.ShowSummaryBalance = _mvcApplication.GetNumber(list.Sum(s => s.ShowSummaryBalance) + selfprof + selfprof);
            //ViewBag.DistributedAmount = _mvcApplication.GetNumber((decimal)(distributedAmount == 0 ? 1 : distributedAmount));
            //End

            return PartialView("_AllMemberRegister", list);
        }

        /// <summary>
        /// Gets all members register.
        /// </summary>
        /// <param name="toDate">To date.</param>
        /// <param name="fromDate">From date.</param>
        /// <returns>List</returns>
        /// <EditedBy>Avishek</EditedBy>
        /// <EditedDate>Jun-23-2016</EditedDate>
        public ActionResult SingleMembersRegister(int employeeId, DateTime toDate, DateTime fromDate)
        {
            _mvcApplication = new MvcApplication();
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                RedirectToAction("Login", "Account");
            }
            var employeeDetail = _unitOfWork.EmployeesRepository.Get(f => f.OCode == oCode && f.EmpID == employeeId).SingleOrDefault();
            if (employeeDetail == null)
            {
                return Json(new { Success = false, ErrorMessage = "Invalid Employee ID..." }, JsonRequestBehavior.AllowGet);
            }
            //var contributionDetail = _unitOfWork.CustomRepository.ValidContributionDetail(employeeId).Where(f => f.ContributionDate <= toDate.AddDays(1).AddSeconds(-1)).Select(s => new { OwnCont = s.SelfContribution, EmpCont = s.EmpContribution, EmpID = s.EmpID, TransactionDate = s.ContributionDate }).ToList();
            var contributionDetail = _unitOfWork.CustomRepository.ValidContributionDetailWithoutVoucher(employeeId).Where(f => f.ContributionDate <= toDate.AddDays(1).AddSeconds(-1)).Select(s => new { OwnCont = s.SelfContribution, EmpCont = s.EmpContribution, EmpID = s.EmpID, TransactionDate = s.ContributionDate }).ToList();
            var profit = _unitOfWork.ProfitDistributionDetailRepository.Get(f => f.OCode == oCode && f.EmpID == employeeId).Where(f => f.TransactionDate <= toDate.AddDays(1).AddSeconds(-1)).Select(s => new { SelfProfit = s.SelfProfit, EmpProfit = s.EmpProfit, EmpID = s.EmpID, TransactionDate = s.TransactionDate }).ToList();
            //get all settlement voucher
            var settlementVoucher = _unitOfWork.ACC_VoucherEntryRepository.Get(g => g.OCode == oCode && g.ActionName == "Settlement" && g.EmpID == employeeId).Select(s => s.VoucherID).FirstOrDefault();
            //get forfeiture, members fund, loan ledger id
            Guid forfeitureLedgerId = _unitOfWork.ChartofAccountMapingRepository.Get(x => x.MIS_Id == 6).Select(x => x.Ledger_Id).FirstOrDefault();
            Guid distributedInterestIncomeID = _unitOfWork.ChartofAccountMapingRepository.Get(x => x.MIS_Id == 26).Select(x => x.Ledger_Id).FirstOrDefault();
            
            
            decimal forfeitureAmount = _unitOfWork.CustomRepository.GetForfeitureAmount(forfeitureLedgerId, settlementVoucher);
            var withdrawalAmount = _unitOfWork.ACC_VoucherDetailRepository.Get().Where(f => f.VoucherID == settlementVoucher && f.Credit != 0.00M && f.TransactionDate <= toDate.AddDays(1).AddSeconds(-1) && f.PFMemberID == employeeId && f.LedgerID != forfeitureLedgerId).ToList();
            var distributedInterestIncomeAmount = _unitOfWork.ACC_VoucherDetailRepository.Get().Where(f => f.VoucherID == settlementVoucher && f.LedgerID == distributedInterestIncomeID).Where(f => f.TransactionDate <= toDate.AddDays(1).AddSeconds(-1)).ToList();
            
            List<VM_Employee> list = new List<VM_Employee>();
            String empId;
            var employee = new VM_Employee();
            var loan = _unitOfWork.CustomRepository.GetPaymentofEmployee(employeeDetail.EmpID, "").FirstOrDefault(x => x.Processed == 0);
            decimal loanAmount = loan == null ? 0 : loan.Amount + loan.Interest;
            empId = employeeDetail.EmpID.ToString();
            employee.IdentificationNumber = employeeDetail.IdentificationNumber;
            employee.EmpName = employeeDetail.EmpName;
            employee.opDesignationName = employeeDetail.opDesignationName;
            employee.OpeningBalance = _mvcApplication.GetNumber(
                contributionDetail.Where(f => f.EmpID == employeeDetail.EmpID && f.TransactionDate < fromDate).GroupBy(g => g.EmpID).Select(s => s.Sum(o => o.OwnCont)).FirstOrDefault())
                + contributionDetail.Where(f => f.EmpID == employeeDetail.EmpID && f.TransactionDate < fromDate).GroupBy(g => g.EmpID).Select(s => s.Sum(e => e.EmpCont)).FirstOrDefault()
                + profit.Where(f => f.EmpID == employeeDetail.EmpID && f.TransactionDate < fromDate).GroupBy(g => g.EmpID).Select(s => s.Sum(d => d.SelfProfit) == null ? 0 : s.Sum(d => d.SelfProfit)).SingleOrDefault()
                + profit.Where(f => f.EmpID == employeeDetail.EmpID && f.TransactionDate < fromDate).GroupBy(g => g.EmpID).Select(s => s.Sum(d => d.EmpProfit) == null ? 0 : s.Sum(d => d.EmpProfit)).SingleOrDefault()
                + Convert.ToDecimal(employeeDetail.opEmpContribution ?? 0)
                + Convert.ToDecimal(employeeDetail.opOwnContribution ?? 0)
                + Convert.ToDecimal(employeeDetail.opProfit ?? 0)
                - Convert.ToDecimal(employeeDetail.opLoan ?? 0);
            employee.LoanAdjustment = _mvcApplication.GetNumber(loanAmount);
            employee.OwnCont = _mvcApplication.GetNumber(contributionDetail.Where(f => f.EmpID == employeeDetail.EmpID && f.TransactionDate >= fromDate).GroupBy(g => g.EmpID).Select(s => s.Sum(o => o.OwnCont)).FirstOrDefault());
            employee.EmpCont = _mvcApplication.GetNumber(contributionDetail.Count == 0 ? 0 : contributionDetail.Where(f => f.EmpID == employeeDetail.EmpID && f.TransactionDate >= fromDate).GroupBy(g => g.EmpID).Select(s => s.Sum(e => e.EmpCont)).FirstOrDefault());

            //Edited by asif 22 Aug 2016

            //if (employee.EmpCont != 0 && employee.OwnCont != 0)
            //{
            //    employee.OwnProfit = _mvcApplication.GetNumber(profit.Where(x=>x.TransactionDate > fromDate).Sum(x => x.SelfProfit));
            //    employee.EmpProfit = _mvcApplication.GetNumber(profit.Where(x => x.TransactionDate > fromDate).Sum(x => x.EmpProfit));
            //}
            //else
            //{
            //    employee.OwnProfit = 0;
            //    employee.EmpProfit = 0;
            //}

            employee.OwnProfit = _mvcApplication.GetNumber(profit.Where(x => x.TransactionDate > fromDate).Sum(x => x.SelfProfit));
            employee.EmpProfit = _mvcApplication.GetNumber(profit.Where(x => x.TransactionDate > fromDate).Sum(x => x.EmpProfit));

            //asif end
            employee.distributedInterestIncomeProfit = distributedInterestIncomeAmount.Where(f => f.PFMemberID == employeeId).Sum(s => s.Debit ?? 0);


            employee.Withdrawal = withdrawalAmount.Sum(s => s.Credit) ?? 0;
            employee.Forfeiture = forfeitureAmount;
            employee.ShowSummaryBalance = _mvcApplication.GetNumber(employee.OpeningBalance +
                                                                    employee.OwnCont +
                                                                    employee.EmpCont +
                                                                    employee.OwnProfit +
                                                                    employee.EmpProfit+
                                                                    employee.distributedInterestIncomeProfit -
                                                                    employee.Withdrawal -
                                                                    employee.Forfeiture -
                                                                    employee.LoanAdjustment);
            list.Add(employee);
            list = list.OrderBy(o => o.TracsactionDate).ToList();
            ViewBag.IdentificationNumber = employeeDetail.IdentificationNumber;
            ViewBag.EmpName = employeeDetail.EmpName;
            ViewBag.Designation = employeeDetail.Designation ?? employeeDetail.opDesignationName;
            ViewBag.Department = employeeDetail.Department ?? employeeDetail.opDepartmentName;
            return PartialView("_SingleMemberRegister", list);
        }

        /// <summary>
        /// Autocompletes the suggestions.
        /// </summary>
        /// <param name="term">The term.</param>
        /// <returns>list</returns>
        /// <CreatyedBy>Avishek</CreatyedBy>
        /// <CreatedDate>Nov-3-2015</CreatedDate>
        public JsonResult AutocompleteSuggestionsName(string term)
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            var suggestions = _unitOfWork.EmployeesRepository.Get().Where(w => w.EmpName.ToLower().Trim().Contains(term.ToLower().Trim()) && w.OCode == oCode).Select(s => new { value = s.IdentificationNumber, label = s.EmpName }).ToList();
            return Json(suggestions, JsonRequestBehavior.AllowGet);
        }

        //Added by Fahim 22/11/2015
        public JsonResult AutocompleteSuggestions(string term)
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            var suggestions = _unitOfWork.EmployeesRepository.Get().Where(w => w.IdentificationNumber.ToLower().Trim().Contains(term.ToLower().Trim()) && w.OCode == oCode).Select(s => new { value = s.EmpName, label = s.IdentificationNumber }).ToList();
            return Json(suggestions, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetEmpId(string identificationNumber)
        {
            try
            {
                int oCode = ((int?)Session["OCode"]) ?? 0;
                if (oCode == 0)
                {
                    RedirectToAction("Login", "Account");
                }
                var suggestions = _unitOfWork.CustomRepository.GetEmployeeByIdentificationNo(identificationNumber).Select(x => x.EmpID).FirstOrDefault();
                return Json(suggestions, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
