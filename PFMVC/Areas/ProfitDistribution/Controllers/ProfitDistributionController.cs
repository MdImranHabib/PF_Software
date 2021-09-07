using DLL;
using DLL.Repository;
using DLL.ViewModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;

namespace PFMVC.Areas.ProfitDistribution.Controllers
{
    public class ProfitDistributionController : Controller
    {
        UnitOfWork unitOfWork = new UnitOfWork();
        MvcApplication _mvcApplication;

        /// <summary>
        /// Indexes this instance.
        /// </summary>
        /// <returns>View</returns>
        /// <ModifiedBy>Avishek</ModifiedBy>
        /// <ModificationDate>24-Feb-2016</ModificationDate>
        public ActionResult Index()
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode != 0)
            {
                return View();
            }
            return RedirectToAction("Login", "Account", new { area = "" });
        }

        /// <summary>
        /// Previouslies the distributed.
        /// </summary>
        /// <returns>View</returns>
        /// <ModifiedBy>Avishek</ModifiedBy>
        /// <ModificationDate>24-Feb-2016</ModificationDate>
        public ActionResult PreviouslyDistributed()
        {
            List<tbl_ProfitDistributionSummary> v = unitOfWork.ProfitDistributionSummaryRepository.Get().ToList();
            return PartialView("_PreviouslyDistributed", v);
        }

        /// <summary>
        /// Processes the calculation.
        /// </summary>
        /// <param name="fromDate">From date.</param>
        /// <param name="toDate">To date.</param>
        /// <param name="amount">The amount.</param>
        /// <returns>View</returns>
        /// <ModifiedBy>Avishek</ModifiedBy>
        /// <ModificationDate>Dec-15-2015</ModificationDate>
        public ActionResult ProcessCalculation(DateTime fromDate, DateTime toDate, decimal amount = 0)
        {
            decimal ShowSummaryBalance = 0;
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            CultureInfo info = new CultureInfo("en-IN");
            _mvcApplication = new MvcApplication();
            if (amount != 0)
            {
                amount = _mvcApplication.GetNumber(amount);
            }
            ViewBag.DistributedAmount = _mvcApplication.GetNumber(amount).ToString("N", info);
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;

            if (fromDate == null || toDate == null)
            {
                ViewBag.ErrorMessage = "You must enter valid date range!";
                return PartialView("_ProfitDistribution");
            }

            //if date valid
            DateTime f = (DateTime) fromDate;
            DateTime t = (DateTime) toDate;
            List<VM_acc_VoucherDetail> expense = unitOfWork.AccountingRepository.GenerateBalanceBook(null, 3, null, f, t.AddHours(23).AddMinutes(59).AddSeconds(59), oCode); // 3 for exp
            List<VM_acc_VoucherDetail> revenue = unitOfWork.AccountingRepository.GenerateBalanceBook(null, 4, null, f, t.AddHours(23).AddMinutes(59).AddSeconds(59), oCode); // 4 for rev

            decimal cCreditRev = 0;
            decimal cDebitRev = 0;
            decimal cCreditExp = 0;
            decimal cDebitExp = 0;

            foreach (var item in revenue)
            {
                cCreditRev += item.Credit;
                cDebitRev += item.Debit;
            }

            foreach (var item in expense)
            {
                cCreditExp += item.Credit;
                cDebitExp += item.Debit;
            }

            decimal cBalRev = cCreditRev - cDebitRev;
            decimal cBalExp = cCreditExp - cDebitExp;

            //Profit Or Loss
            decimal pl = cBalRev + cBalExp;

            if (pl < 0)
            {
                ViewBag.PL = "Net Loss within selected date range is: " + ((-1) * pl).ToString("N", info) + "/=. Loss cannot be distributed!";
                return PartialView("_ProfitDistribution");
            }
            if (_mvcApplication.GetNumber(pl) < _mvcApplication.GetNumber(amount))
            {
                ViewBag.PL = "Amount " + _mvcApplication.GetNumber(amount).ToString("N", info) + " is more than Profit " + _mvcApplication.GetNumber(pl).ToString("N", info) + ". Please revise your decision!";
                return PartialView("_ProfitDistribution");
            }
            
            ViewBag.Amount = _mvcApplication.GetNumber(pl).ToString("N", info);
            ViewBag.PL = "Net Profit from " + f.ToString("dd/MMM/yyyy") + " to " + t.ToString("dd/MMM/yyyy") + " is: " + _mvcApplication.GetNumber(pl).ToString("N", info) + "/=";
            List<VM_Contribution> contributionDetail = unitOfWork.CustomRepository.sp_GetTotalContributionProfit(fromDate, toDate).ToList();
            
            if (contributionDetail.Any())
            {
                decimal totalContribution = 0;
                decimal PreviousTotalContribution = 0;
                foreach (var item in contributionDetail)
                {
                    decimal totalDay = 0;
                    //decimal SelftotalContribution = 0;
                    decimal totalDiffDays = Convert.ToDecimal((toDate - fromDate).TotalDays + 1);
                    if (item.PFDeactivationDate == null)
                    {
                        var totalDiff = (toDate - fromDate);

                        totalDay = Convert.ToDecimal(Math.Round(totalDiff.TotalDays) + 1);
                    }
                    else
                    {
                        var totalDiff = (item.PFDeactivationDate.Value - fromDate);
                        totalDay = Convert.ToDecimal(Math.Round(totalDiff.TotalDays));

                    }
                    if (totalDay == 366)
                    {
                        totalDay = 365;
                    }
                    if (totalDiffDays == 366)
                    {
                        totalDiffDays = 365;
                    }
                    decimal selfTotalContribution = ((item.SelfContribution / totalDiffDays) * totalDay) + item.opOwnContribution;
                    decimal empTotalContribution = ((item.EmpContribution / totalDiffDays) * totalDay) + item.opEmpContribution;
                    totalContribution = PreviousTotalContribution + selfTotalContribution + empTotalContribution;

                    PreviousTotalContribution = totalContribution;
                }
                //var totalContribution = contributionDetail.Sum(x => x.opEmpContribution) +
                //                            contributionDetail.Sum(x => x.opOwnContribution) +
                //                            contributionDetail.Sum(x => x.opProfit) +
                //                            contributionDetail.Sum(x => x.SelfContribution) +
                //                            contributionDetail.Sum(x => x.EmpContribution) +
                //                            contributionDetail.Sum(x => x.SelfProfit) +
                //                            contributionDetail.Sum(x => x.EmpProfit);
                if (amount > 0)
                {
                    ViewBag.DistributableAmount = _mvcApplication.GetNumber(pl).ToString("#,##,##,##0.00");
                    pl = amount;
                }
                else
                {
                    ViewBag.DistributableAmount = _mvcApplication.GetNumber(pl).ToString("#,##,###,##0.00");
                }
                ViewBag.DistributedAmount = _mvcApplication.GetNumber(pl).ToString("#,##,##,##0.00");


                _mvcApplication = new MvcApplication();
                //double distributedAmount = _unitOfWork.CustomRepository.GetAllDistributedAmount();
                var employeeDetail = unitOfWork.CustomRepository.GetAllEmployee(oCode).ToList();

                List<tbl_Contribution> contribution = unitOfWork.ContributionRepository.Get(x => x.OCode == oCode).Where(x => x.ContributionDate <= toDate.AddDays(1).AddSeconds(-1)).ToList();

                var profit = unitOfWork.ProfitDistributionDetailRepository.Get(x => x.OCode == oCode).Where(x => x.TransactionDate <= toDate.AddDays(1).AddSeconds(-1)).ToList();
                //get all settlement voucher
                var settlementVoucher = unitOfWork.ACC_VoucherEntryRepository.Get().Where(g => g.OCode == oCode && g.ActionName == "Settlement" && g.TransactionDate <= toDate.AddDays(1).AddSeconds(-1)).Select(s => s.VoucherID).ToList();
                //get forfeiture, members fund, loan ledger id
                Guid forfeitureLedgerId = unitOfWork.ChartofAccountMapingRepository.Get(x => x.MIS_Id == 6).Select(x => x.Ledger_Id).FirstOrDefault();
                var forfeitureAmount = unitOfWork.ACC_VoucherDetailRepository.Get(x => settlementVoucher.Contains(x.VoucherID) && x.LedgerID == forfeitureLedgerId).Where(x => x.TransactionDate <= toDate.AddDays(1).AddSeconds(-1)).ToList();
                var withdrawalAmount = unitOfWork.ACC_VoucherDetailRepository.Get().Where(x => settlementVoucher.Contains(x.VoucherID) && x.TransactionDate <= toDate.AddDays(1).AddSeconds(-1) && x.Credit != 0.00M).ToList();
                //Checked Brfore this
                List<VM_Employee> list = new List<VM_Employee>();
                string empId = "";
                var contributionBeforeDate = contribution.Where(x => x.OCode == oCode && x.ContributionDate < fromDate).GroupBy(g => g.EmpID).Select(s => new { OwnCont = s.Sum(w => w.SelfContribution), EmpCont = s.Sum(w => w.EmpContribution), EmpID = s.Key }).ToList();
                var contributionBetweenDate = contribution.Where(x => x.OCode == oCode && x.ContributionDate >= fromDate.AddSeconds(-1)).GroupBy(g => g.EmpID).Select(s => new { OwnCont = s.Sum(w => w.SelfContribution), EmpCont = s.Sum(w => w.EmpContribution), EmpID = s.Key }).ToList();
                var profitBeforeDate = profit.Where(x => x.OCode == oCode && x.TransactionDate < fromDate).GroupBy(g => g.EmpID).Select(s => new { OwnProfit = s.Sum(w => w.SelfProfit), EmpProfit = s.Sum(w => w.EmpProfit), EmpID = s.Key }).ToList();
                var forfeitureAmountBeforeDate = forfeitureAmount.Where(x => x.TransactionDate < fromDate).ToList();
                var forfeitureAmountBetweenDate = forfeitureAmount.Where(x => x.TransactionDate >= fromDate).ToList();
                var withdrawalAmountBeforeDate = withdrawalAmount.Where(x => x.TransactionDate < fromDate).ToList();
                var withdrawalAmountBetweenDate = withdrawalAmount.Where(x => x.TransactionDate >= fromDate).ToList();
                foreach (var item in employeeDetail)
                {
                    //Ediited by Avishek Date Sep-21-2015
                    //Start
                    var employee = new VM_Employee();
                    empId = item.EmpID.ToString();
                    employee.EmpID = Int32.Parse(empId);
                    employee.IdentificationNumber = item.IdentificationNumber;
                    employee.EmpName = item.EmpName;
                    decimal openingOwn = contributionBeforeDate.Where(x => x.EmpID == item.EmpID).Select(s => s.OwnCont).SingleOrDefault();
                    decimal openingEmp = contributionBeforeDate.Where(x => x.EmpID == item.EmpID).Select(s => s.EmpCont).SingleOrDefault();
                    decimal selfProfitBeforeDateById = profitBeforeDate.Where(x => x.EmpID == item.EmpID).Select(x => x.OwnProfit).SingleOrDefault();
                    decimal empfProfitBeforeDateById = profitBeforeDate.Where(x => x.EmpID == item.EmpID).Select(x => x.EmpProfit).SingleOrDefault();
                    //decimal forfeitureAmountBeforeDateById = forfeitureAmountBeforeDate.Where(x => x.PFMemberID == item.EmpID).Sum(x => x.Credit).GetValueOrDefault();
                    //decimal withdrawalAmountBeforeDateById = withdrawalAmountBeforeDate.Where(f => f.PFMemberID == item.EmpID).Sum(x=>x.Credit).GetValueOrDefault();

                    employee.OpeningBalance = _mvcApplication.GetNumber(item.opOwnContribution + item.opEmpContribution + item.opProfit +
                                                                     openingOwn + openingEmp + selfProfitBeforeDateById + empfProfitBeforeDateById);
                    
                    employee.opDesignationName = item.opDesignationName ?? "";
                    employee.opDepartmentName = item.opDepartmentName ?? "";
                    employee.OwnCont = _mvcApplication.GetNumber(contributionBetweenDate.Where(x => x.EmpID == item.EmpID).Select(s => s.OwnCont).SingleOrDefault());
                    employee.EmpCont = _mvcApplication.GetNumber(contributionBetweenDate.Where(x => x.EmpID == item.EmpID).Select(s => s.EmpCont).SingleOrDefault());
                    employee.Forfeiture = _mvcApplication.GetNumber(forfeitureAmountBetweenDate.Where(x => x.PFMemberID == item.EmpID).Sum(x => x.Credit).GetValueOrDefault());
                    //Edited by fahim 28/10/2015
                    if (employee.EmpCont != 0 && employee.OwnCont != 0)
                    {
                        employee.OwnProfit = _mvcApplication.GetNumber(selfProfitBeforeDateById);
                        employee.EmpProfit = _mvcApplication.GetNumber(empfProfitBeforeDateById);
                    }
                    else
                    {
                        employee.OwnProfit = 0;
                        employee.EmpProfit = 0;
                    }
                    //Fahim end
                    employee.Withdrawal = _mvcApplication.GetNumber((withdrawalAmountBetweenDate.Where(x => x.PFMemberID == item.EmpID).Sum(s => s.Credit ?? 0)) - forfeitureAmountBetweenDate.Where(x => x.PFMemberID == item.EmpID).Sum(x => x.Credit).GetValueOrDefault());
                    employee.ShowSummaryBalance = _mvcApplication.GetNumber(Convert.ToDecimal(employee.OpeningBalance + employee.OwnCont + employee.EmpCont + employee.OwnProfit + employee.EmpProfit - employee.Withdrawal - employee.Forfeiture, new CultureInfo("en-US")));
                    list.Add(employee);
                    ShowSummaryBalance += employee.ShowSummaryBalance;
                }

                var creditLedgerList = unitOfWork.AccountingRepository.sp_GetLedgerSummary(Convert.ToDateTime("01/01/1800"), toDate.AddDays(1).AddSeconds(-1), 2, oCode);
                decimal _balance = 0;
                foreach (var item in creditLedgerList)
                {
                    if (item.BalanceType == 1 && item.GroupID ==12) // 1 for credit
                    {
                        _balance = _mvcApplication.GetNumber(item.InitialBalance ?? 0) + _mvcApplication.GetNumber(item.Credit) - _mvcApplication.GetNumber(item.Debit);
                    }
                }

               // ShowSummaryBalance = _mvcApplication.GetNumber(list.Sum(s => s.ShowSummaryBalance));

                foreach (var item in contributionDetail)
                {
                    try
                    {
                        decimal totalDay = 0;
                       decimal  totalDiffDays =Convert.ToDecimal((toDate - fromDate).TotalDays + 1);
                        var v = list.Where(x => x.EmpID == item.EmpID).Select(
                            a => new
                            {
                                a.ShowSummaryBalance
                            }).FirstOrDefault();
                        if (item.PFDeactivationDate== null)
                        {
                            var totalDiff = (toDate - fromDate);

                             totalDay = Convert.ToDecimal(Math.Round(totalDiff.TotalDays) + 1);
                        }
                        else
                        {
                            var totalDiff = (item.PFDeactivationDate.Value - fromDate);

                             totalDay = Convert.ToDecimal(Math.Round(totalDiff.TotalDays) + 1);
                        }
                        ////Fahim end
                        //Withdrawal = _mvcApplication.GetNumber((withdrawalAmountBetweenDate.Where(x => x.PFMemberID == item.EmpID).Sum(s => s.Credit ?? 0)) - forfeitureAmountBetweenDate.Where(x => x.PFMemberID == item.EmpID).Sum(x => x.Credit).GetValueOrDefault());
                        decimal selfContributionOfPeriod = item.SelfContribution + item.SelfProfit + item.opOwnContribution + (item.opProfit / 2);
                        decimal empContributionOfPeriod = item.EmpContribution + item.EmpProfit + item.opEmpContribution + (item.opProfit / 2);
                        decimal mBalance = selfContributionOfPeriod + empContributionOfPeriod;
                        
                        // End of Modification
                        if (totalDay == 366)
                        {
                            totalDay = 365;
                        }
                        if (totalDiffDays == 366)
                        {
                            totalDiffDays = 365;
                        }

                        decimal selfTotalContribution = ((item.SelfContribution / totalDiffDays) * totalDay) + item.opOwnContribution;
                        decimal empTotalContribution = ((item.EmpContribution / totalDiffDays) * totalDay) + item.opEmpContribution;

                        //Previous code
                        item.IdentificationNumber = item.IdentificationNumber;
                        item.EmpName = item.EmpName;
                        decimal rate = Convert.ToDecimal(((pl) / (_balance*2)));
                        rate = Math.Round(rate, 8);

                        item.SeProfit = Convert.ToDouble(((pl) / (_balance * 2)) * selfTotalContribution);

                        //item.SeProfit = Convert.ToDouble(((pl) / totalContribution) * selfTotalContribution);
                        item.EmProfit = Convert.ToDouble(((pl) / totalContribution) * empTotalContribution);

                        //item.SelfProfit = (decimal)item.SeProfit;
                        decimal _Profit = (mBalance * rate)/2;
                        item.SelfProfit = Math.Round(_Profit, 2);

                        decimal total_contribution = Convert.ToDecimal( v.ShowSummaryBalance);

                        item.EmpProfit = Math.Round(_Profit, 2);
                        item.SelfContribution = _mvcApplication.GetNumber(selfContributionOfPeriod);
                        item.EmpContribution = _mvcApplication.GetNumber(empContributionOfPeriod);
                    }
                    catch
                    {
                        item.DistributedAmount = 0;
                    }
                }
            }
            return PartialView("_ProfitDistribution", contributionDetail);
        }

        /// <summary>
        /// Profits the distribution confirm.
        /// </summary>
        /// <param name="v">The v.</param>
        /// <param name="_fromDate">The _from date.</param>
        /// <param name="_toDate">The _to date.</param>
        /// <param name="_distributableAmount">The _distributable amount.</param>
        /// <param name="_distributedAmount">The _distributed amount.</param>
        /// <returns>bool</returns>
        /// <ModifiedBy>Avishek</ModifiedBy>
        /// <ModificationDate>Dec-15-2015</ModificationDate>
        [HttpPost]
        public ActionResult ProfitDistributionConfirm(List<VM_Contribution> v, DateTime? _fromDate, DateTime? _toDate, string _distributableAmount = "", string _distributedAmount = "")
        {
            int voucherId = 0;
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            var checkDateRedundancy = unitOfWork.ProfitDistributionSummaryRepository.Get(f => f.FromDate <= _fromDate && f.ToDate >= _fromDate && f.OCode == oCode).FirstOrDefault();
            if (checkDateRedundancy != null)
            {
                return Json(new { Success = false, ErrorMessage = _fromDate + " - This date is already taken between " + checkDateRedundancy.FromDate + "-" + checkDateRedundancy.ToDate + "." }, JsonRequestBehavior.DenyGet);
            }
            checkDateRedundancy = unitOfWork.ProfitDistributionSummaryRepository.Get(f => f.FromDate <= _toDate && f.ToDate >= _toDate && f.OCode == oCode).FirstOrDefault();
            if (checkDateRedundancy != null)
            {
                return Json(new { Success = false, ErrorMessage = _toDate + " - This date is already taken between " + checkDateRedundancy.FromDate + "-" + checkDateRedundancy.ToDate + "." }, JsonRequestBehavior.DenyGet);
            }
            int processId = (unitOfWork.ProfitDistributionSummaryRepository.Get(x => x.OCode == oCode).Max(m => (int?)m.ProcessID) ?? 0) + 1;
            tbl_ProfitDistributionSummary profitSummary = new tbl_ProfitDistributionSummary();
            profitSummary.ProcessID = processId;
            profitSummary.FromDate = _fromDate;
            profitSummary.ToDate = _toDate;
            profitSummary.Message = "";
            profitSummary.DistributableAmount = Convert.ToDecimal(_distributableAmount);
            profitSummary.DistributedAmount = Convert.ToDecimal(_distributedAmount);
            profitSummary.SelfTotalProfit = v.Sum(x => x.SelfProfit);
            profitSummary.EmpTotalProfit = v.Sum(x => x.EmpProfit);
            profitSummary.EditTime = _toDate;
            profitSummary.EditUserName = User.Identity.Name;
            profitSummary.EditUser = unitOfWork.CustomRepository.GetUserID(User.Identity.Name);
            profitSummary.OCode = oCode;

            unitOfWork.ProfitDistributionSummaryRepository.Insert(profitSummary);

            List<tbl_ProfitDistributionDetail> listProfitDetail = new List<tbl_ProfitDistributionDetail>();
            foreach (var item in v)
            {
                //if (item.DistributedAmount > 0)
                if (item.SelfProfit > 0)
                {
                    var profitDetail = new tbl_ProfitDistributionDetail();
                    profitDetail.DistributedAmount = item.EmpProfit + item.SelfProfit;
                    profitDetail.EmpID = item.EmpID;
                    profitDetail.IdentificationNumber = item.IdentificationNumber;
                    profitDetail.ProcessID = processId;
                    profitDetail.TransactionDate = _toDate;
                    profitDetail.SelfContribution = item.SelfContribution;
                    profitDetail.EmpContribution = item.EmpContribution;
                    profitDetail.SelfProfit = item.SelfProfit;
                    profitDetail.EmpProfit = item.EmpProfit;
                    profitDetail.OCode = oCode;
                    listProfitDetail.Add(profitDetail);
                }
            }

            listProfitDetail.ForEach(f => unitOfWork.ProfitDistributionDetailRepository.Insert(f));
            unitOfWork.Save();
            if (listProfitDetail.Count > 0)
            {
                string refMessage = "";
                #region create profit distribution ledger

                //List<string> LedgerNameList = new List<string>();
                List<Guid> ledgerIdList = new List<Guid>();
                List<decimal> credit = new List<decimal>();
                List<decimal> debit = new List<decimal>();
                List<string> chqNumber = new List<string>();
                List<string> pfLoanId = new List<string>();
                List<string> pfMemberId = new List<string>();

                List<acc_Chart_of_Account_Maping> accChartOfAccountMaping = unitOfWork.ChartofAccountMapingRepository.Get().ToList();

                //LedgerNameList.Add("Profit Distribution"); 
                ledgerIdList.Add(accChartOfAccountMaping.Where(x => x.MIS_Id == 7 && x.OCode == oCode).Select(x => x.Ledger_Id).FirstOrDefault());
                debit.Add(Convert.ToDecimal(v.Sum(x => x.SelfProfit) + v.Sum(x => x.EmpProfit)));
                credit.Add(0);
                chqNumber.Add("");
                pfMemberId.Add("");
                pfLoanId.Add("");

                //LedgerNameList.Add("Members Fund");
                ledgerIdList.Add(accChartOfAccountMaping.Where(x => x.MIS_Id == 3 && x.OCode == oCode).Select(x => x.Ledger_Id).FirstOrDefault());
                credit.Add(Convert.ToDecimal(v.Sum(x => x.SelfProfit)));
                debit.Add(0);
                chqNumber.Add("");
                pfMemberId.Add("");
                pfLoanId.Add("");

                ledgerIdList.Add(accChartOfAccountMaping.Where(x => x.MIS_Id == 4 && x.OCode == oCode).Select(x => x.Ledger_Id).FirstOrDefault());
                credit.Add(Convert.ToDecimal(v.Sum(x => x.EmpProfit)));
                debit.Add(0);
                chqNumber.Add("");
                pfMemberId.Add("");
                pfLoanId.Add("");

                //bool isOperationSuccess = unitOfWork.AccountingRepository.DualEntryVoucher(0, 5, Convert.ToDateTime(_toDate), ref voucherID, "Profit Distribution from " + _fromDate + " to " + _toDate, LedgerNameList, Debit, Credit, ChqNumber, ref refMessage, User.Identity.Name, unitOfWork.CustomRepository.GetUserID(User.Identity.Name), PFMemberID, "", "", "", processID, PFLoanID, OCode, "Profit Distribution");
                bool isOperationSuccess = unitOfWork.AccountingRepository.DualEntryVoucherById(0, 5, Convert.ToDateTime(_toDate), ref voucherId, "Profit Distribution from " + _fromDate + " to " + _toDate, ledgerIdList, debit, credit, chqNumber, ref refMessage, User.Identity.Name, unitOfWork.CustomRepository.GetUserID(User.Identity.Name), pfMemberId, "", "", "", processId, pfLoanId, oCode, "Profit Distribution");
                //Above line toDate will be deleated by DtaeTime.Now after Back Dated data entry 
                if (isOperationSuccess)
                {
                    try
                    {
                        unitOfWork.Save();
                        return Json(new { Success = true, Message = "Transction Sucessfull and status updated!" }, JsonRequestBehavior.DenyGet);
                    }
                    catch (Exception x)
                    {
                        return Json(new { Success = false, ErrorMessage = "Transction Sucessfull BUT STATUS UPDATE FAILED WITH FOLLOWING ERROR: " + x.Message + " PLEASE CONTACT SYS ADMIN!" }, JsonRequestBehavior.DenyGet);
                    }
                }
                return Json(new { Success = false, ErrorMessage = "Transaction Failded with following error : " + refMessage }, JsonRequestBehavior.DenyGet);

                #endregion
            }
            return Json(new { Success = false, ErrorMessage = "No data found to save!" }, JsonRequestBehavior.DenyGet);
        }

        public ActionResult DeleteProfitDistributionPossible(int id)
        {
            return Json(new { Success = true, Message = "Process can be deleted! continue?" }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Deletes the profit distribution confirm.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>String</returns>
        /// <ModifiedBy>Avishek</ModifiedBy>
        /// <ModificationDate>Dec-15-2015</ModificationDate>
        /// <ReViewDate>24-Feb-2016</ReViewDate>
        [HttpPost]
        public ActionResult DeleteProfitDistributionConfirm(int id)
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            var listProfitDetail = unitOfWork.ProfitDistributionDetailRepository.Get(f => f.ProcessID == id).ToList();
            listProfitDetail.ForEach(f => unitOfWork.ProfitDistributionDetailRepository.Delete(f));
            var profitSummary = unitOfWork.ProfitDistributionSummaryRepository.Get(f => f.ProcessID == id).Single();
            unitOfWork.ProfitDistributionSummaryRepository.Delete(profitSummary);

            var vEntry = unitOfWork.ACC_VoucherEntryRepository.Get(f => f.ProfitDistributionProcessID == id).SingleOrDefault();
            if (vEntry != null)
            {
                var vDetail = unitOfWork.ACC_VoucherDetailRepository.Get(f => f.VoucherID == vEntry.VoucherID).ToList();
                vDetail.ForEach(f => unitOfWork.ACC_VoucherDetailRepository.Delete(f));
                unitOfWork.ACC_VoucherEntryRepository.Delete(vEntry);
            }
            else
            {
                return Json(new { Success = false, ErrorMessage = "Accounting entry not found!" }, JsonRequestBehavior.DenyGet);
            }

            try
            {
                unitOfWork.Save();
                return Json(new { Success = true, Message = "Process Successfully Deleted!" }, JsonRequestBehavior.DenyGet);
            }
            catch (Exception x)
            {
                return Json(new { Success = false, ErrorMessage = "Operation Unsuccessful: " + x.Message }, JsonRequestBehavior.DenyGet);
            }
        }

        /// <summary>
        /// Views the profit distribution detail.
        /// </summary>
        /// <param name="processID">The process identifier.</param>
        /// <returns>view</returns>
        /// <ModifiedBy>Avishek</ModifiedBy>
        /// <ModificationDate>Dec-15-2015</ModificationDate>
        /// <ReViewDate>24-Feb-2016</ReViewDate>
        public ActionResult ViewProfitDistributionDetail(int processID)
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            CultureInfo cInfo = new CultureInfo("en-IN");
            var summary = unitOfWork.ProfitDistributionSummaryRepository.GetByID(processID);
            ViewBag.FromDate = summary.FromDate;
            ViewBag.ToDate = summary.ToDate;
            ViewBag.DistributableAmount = Convert.ToDecimal(summary.DistributableAmount).ToString("N", cInfo);
            ViewBag.DistributedAmount = Convert.ToDecimal(summary.SelfTotalProfit + summary.EmpTotalProfit).ToString("N", cInfo);
            ViewBag.EditUserName = summary.EditUserName;

            List<VM_Contribution> detail = unitOfWork.CustomRepository.sp_GetProfitDistributionDetail(processID).ToList();
            return PartialView("_ViewProfitDistributionDetail", detail);
        }
    }
}
