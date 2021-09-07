using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using DLL;
using DLL.Repository;
using DLL.ViewModel;
using System.Net;

namespace PFMVC.Areas.MemberInOut.Controllers
{
    public class MemberInOutController : Controller
    {
        readonly UnitOfWork _unitOfWork = new UnitOfWork();

        /// <summary>
        /// Indexes this instance.
        /// </summary>
        /// <returns>view</returns>
        /// <CreatedBy>Avishek</CreatedBy>
        /// <CreatedDate>Mar-7-2015</CreatedDate>
        public ActionResult Index()
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode != 0)
                return View();
            return RedirectToAction("Login", "Account");
        }

        /// <summary>
        /// Autocompletes the suggestions.
        /// </summary>
        /// <param name="term">The term.</param>
        /// <returns>list</returns>
        /// <CreatedBy>Avishek</CreatedBy>
        /// <createdDate>Mar-9-2015</createdDate>
        public JsonResult AutocompleteSuggestions(string term)
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            var suggestions = _unitOfWork.EmployeesRepository.Get(w => w.IdentificationNumber.Contains(term) && w.OCode == oCode).Select(s => new
            {
                value = s.EmpID,
                label = s.IdentificationNumber
            }).ToList();
            return Json(suggestions, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Autocompletes the suggestions.
        /// </summary>
        /// <param name="term">The term.</param>
        /// <returns>list</returns>
        /// <CreatedBy>Avishek</CreatedBy>
        /// <createdDate>Mar-9-2015</createdDate>
        public JsonResult AutocompleteLedgerName(string term)
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            var suggestions = _unitOfWork.ACC_LedgerRepository.Get(w => w.LedgerName.ToLower().Contains(term.ToLower()) && w.OCode == oCode).Select(s => new
            {
                value = s.LedgerID,
                label = s.LedgerName
            }).ToList();
            return Json(suggestions, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Systems the voucher.
        /// </summary>
        /// <param name="VoucherID">The voucher identifier.</param>
        /// <returns>view</returns>
        /// <CreatedBy>Avishek<CreatedBy>
        /// <Date>Mar-09-2015</Date>
        public ActionResult GetEmployee(int id = 0)
        {

            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            tbl_Employees employee = _unitOfWork.EmployeesRepository.GetByID(id);
            List<tbl_Contribution> tblContribution = _unitOfWork.CustomRepository.ValidContributionDetail(employee.EmpID);
            List<tbl_ProfitDistributionDetail> tblProfitDistributionDetail = _unitOfWork.CustomRepository.GetProfitDistributionByEmpId(employee.EmpID);
            List<tbl_PFL_Amortization> tblPflAmortization = _unitOfWork.CustomRepository.GetPaymentofEmployee(employee.EmpID, "").Where(x => x.Processed != 1).OrderBy(x => x.InstallmentNumber).ToList();
            VM_Employee vmEmployee = new VM_Employee();
            vmEmployee.IdentificationNumber = employee.IdentificationNumber;
            vmEmployee.EmpName = employee.EmpName;
            vmEmployee.OwnCont = (decimal)employee.opOwnContribution + tblContribution.Where(x => x.EmpID == employee.EmpID).Sum(x => x.SelfContribution);
            vmEmployee.EmpCont = (decimal)employee.opEmpContribution + tblContribution.Where(x => x.EmpID == employee.EmpID).Sum(x => x.EmpContribution);
            vmEmployee.Profit = (decimal)employee.opProfit + (decimal)tblProfitDistributionDetail.Where(x => x.EmpID == employee.EmpID).Sum(x => x.DistributedAmount);
            vmEmployee.opLoan = (decimal)employee.opLoan + tblPflAmortization.Where(x => x.EmpID == employee.EmpID).Select(x => x.Amount).FirstOrDefault();
            vmEmployee.SummaryBalance = vmEmployee.OwnCont + vmEmployee.EmpCont + vmEmployee.Profit - vmEmployee.opLoan;

            Session["IdentificationNumber"] = vmEmployee.IdentificationNumber;
            Session["OwnCont"] = vmEmployee.OwnCont;
            Session["EmpCont"] = vmEmployee.EmpCont;
            Session["Profit"] = vmEmployee.Profit;
            Session["opLoan"] = vmEmployee.opLoan;
            Session["SummaryBalance"] = vmEmployee.SummaryBalance;

            return Json(vmEmployee);
        }

        /// <summary>
        /// Saves the member out.
        /// </summary>
        /// <param name="empId">The emp identifier.</param>
        /// <param name="debitLedgerID">The debit ledger identifier.</param>
        /// <param name="debitAmount">The debit amount.</param>
        /// <param name="creditLedgerID">The credit ledger identifier.</param>
        /// <param name="creditAmount">The credit amount.</param>
        /// <returns></returns>
        /// <CreatedBy>Avishek<CreatedBy>
        /// <Date>Mar-09-2015</Date>
        public JsonResult SaveMemberOut(int empId, Guid debitLedgerID, decimal debitAmount, Guid creditLedgerID, decimal creditAmount, string Comment, DateTime transactionDate)
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return Json(new
                {
                    Success = false,
                    ErrorMessage = "Session out please login again"
                }, JsonRequestBehavior.DenyGet);
            }
            tbl_Employees tbl_Employees = _unitOfWork.EmployeesRepository.GetByID(empId);

            if (tbl_Employees == null)
            {
                return Json(new
                {
                    Success = false,
                    ErrorMessage = "This employee is already Transfared"
                }, JsonRequestBehavior.DenyGet);
            }
            int voucherId = 0;
            try
            {
                List<string> ledgerNameList = new List<string>();
                List<decimal> credit = new List<decimal>();
                List<decimal> debit = new List<decimal>();
                List<string> chqNumber = new List<string>();
                List<string> pfLoanId = new List<string>();
                List<string> pfMemberId = new List<string>();

                string refMessage = "";

                //decimal totalDebit = ((decimal?)Session["OwnCont"]) ?? 0 + ((decimal?)Session["EmpCont"]) ?? 0 + ((decimal?)Session["Profit"]) ?? 0;
                string debitledgerName = _unitOfWork.ACC_LedgerRepository.GetByID(debitLedgerID).LedgerName;
                ledgerNameList.Add(debitledgerName);
                debit.Add(debitAmount);
                credit.Add(0);
                chqNumber.Add("");
                pfMemberId.Add(empId + "");
                pfLoanId.Add("");


                //decimal debit = ((decimal?)Session["SummaryBalance"]) ?? 0;
                string creditledgerName = _unitOfWork.ACC_LedgerRepository.GetByID(creditLedgerID).LedgerName;
                ledgerNameList.Add(creditledgerName);
                credit.Add(creditAmount);
                debit.Add(0);
                chqNumber.Add("");
                pfMemberId.Add(empId + "");
                pfLoanId.Add("");

                bool isOperationSuccess = _unitOfWork.AccountingRepository.DualEntryVoucher(empId, 5, transactionDate, ref voucherId, "Member  Transfer to other Company", ledgerNameList, debit, credit, chqNumber, ref refMessage, User.Identity.Name, _unitOfWork.CustomRepository.GetUserID(User.Identity.Name), pfMemberId, "Member  Transfer to other Company", "", "", null, pfLoanId, oCode, "Transfer");

                var tblEmployees = _unitOfWork.EmployeesRepository.GetByID(empId);
                tblEmployees.EditDate = DateTime.Now;
                tblEmployees.PFStatus = 2;
                tblEmployees.Branch = 1;
                tblEmployees.Comment += Comment;
                tblEmployees.PFDeactivationDate = transactionDate;
                _unitOfWork.EmployeesRepository.Update(tblEmployees);
                _unitOfWork.Save();
                return Json(isOperationSuccess ? "Sucess" : "Error ocur in Save");
            }
            catch (Exception)
            {
                return Json(new
                {
                    Success = false,
                    ErrorMessage = "Error ocur in Save"
                }, JsonRequestBehavior.DenyGet);
            }
        }

        /// <summary>
        /// Saves the member in.
        /// </summary>
        /// <param name="empId">The emp identifier.</param>
        /// <param name="empName">Name of the emp.</param>
        /// <param name="total">The total.</param>
        /// <param name="debitLedgerID">The debit ledger identifier.</param>
        /// <param name="debitAmount">The debit amount.</param>
        /// <param name="creditLedgerID">The credit ledger identifier.</param>
        /// <param name="creditAmount">The credit amount.</param>
        /// <returns></returns>
        /// <CreatedBy>Avishek<CreatedBy>
        /// <Date>Mar-09-2015</Date>
        public JsonResult SaveMemberIn(string empId, string empName, string total, Guid debitLedgerID, decimal debitAmount, Guid creditLedgerID, decimal creditAmount, DateTime transactionDate)
        {

            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return Json(new
                {
                    Success = false,
                    ErrorMessage = "Session out please login again"
                }, JsonRequestBehavior.DenyGet);
            }

            string[] words = total.Split('/');

            tbl_Employees tblEmployee;
            int empID = GetMaxID() + 1;
            var editUser = _unitOfWork.CustomRepository.GetUserID(User.Identity.Name);

            List<tbl_Employees> tblEmployees = _unitOfWork.EmployeesRepository.Get(x => x.OCode == oCode).ToList();

            tblEmployee = new tbl_Employees();
            bool hasId = tblEmployees.FirstOrDefault(x => x.IdentificationNumber == empId) == null ? true : false;
            if (!hasId)
            {
                return Json(new { Success = false, Message = "Employee Exist of This ID!" }, JsonRequestBehavior.DenyGet);
                
            }
            else
            {
                
                tblEmployee.EmpID = empID;
                tblEmployee.EmpName = empName;
                tblEmployee.IdentificationNumber = empId;
                tblEmployee.JoiningDate = transactionDate;
                //tblEmployee.PFStatus = 1;
                tblEmployee.Branch = 1;
                tblEmployee.opOwnContribution = words[0] == "" ? 0 : Convert.ToDecimal(words[0]);
                tblEmployee.opEmpContribution = words[1] == "" ? 0 : Convert.ToDecimal(words[1]);
                tblEmployee.opLoan = words[3] == "" ? 0 : Convert.ToDecimal(words[3]);
                tblEmployee.opProfit = words[2] == "" ? 0 : Convert.ToDecimal(words[2]);
                tblEmployee.EditDate = DateTime.Now;
                tblEmployee.EditUser = editUser;
                tblEmployee.EditUserName = User.Identity.Name;
                tblEmployee.PFActivationDate = DateTime.Now;
                tblEmployee.PFStatus = 1;
                tblEmployee.Comment = "[Excel Upload]";
                tblEmployee.PassVoucher = false;
                tblEmployee.OCode = oCode;
                tblEmployee.PassVoucher = true;
                _unitOfWork.EmployeesRepository.Insert(tblEmployee);
                if (tblEmployee.opLoan > 0)
                {
                    SaveLoan(tblEmployee);
                }

                try
                {
                    _unitOfWork.Save();

                    int voucherId = 0;
                    List<string> ledgerNameList = new List<string>();
                    List<decimal> credit = new List<decimal>();
                    List<decimal> debit = new List<decimal>();
                    List<string> chqNumber = new List<string>();
                    List<string> pfLoanId = new List<string>();
                    List<string> pfMemberId = new List<string>();

                    string refMessage = "";
                    string debitledgerName = _unitOfWork.ACC_LedgerRepository.GetByID(debitLedgerID).LedgerName;
                    ledgerNameList.Add(debitledgerName);
                    debit.Add(debitAmount);
                    credit.Add(0);
                    chqNumber.Add("");
                    pfMemberId.Add(empId + "");
                    pfLoanId.Add("");

                    string creditledgerName = _unitOfWork.ACC_LedgerRepository.GetByID(creditLedgerID).LedgerName;
                    ledgerNameList.Add(creditledgerName);
                    credit.Add(creditAmount);
                    debit.Add(0);
                    chqNumber.Add("");
                    pfMemberId.Add(empId + "");
                    pfLoanId.Add("");

                    _unitOfWork.AccountingRepository.DualEntryVoucher(empID, 5, transactionDate, ref voucherId, "Member  Transfer in Company", ledgerNameList, debit, credit, chqNumber, ref refMessage, User.Identity.Name, _unitOfWork.CustomRepository.GetUserID(User.Identity.Name), pfMemberId, "Member  Transfer to other Company", "", "", null, pfLoanId, oCode, "Transfer In");

                    return Json(new { Success = true, Message = "Inserted Successfully!" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception x)
                {
                    return Json(new
                    {
                        Success = false,
                        ErrorMessage = "Error: " + x.Message,
                        InnerException = "Detail Error: " + x.InnerException
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            //return Json("Sucess");
        }

        /// <summary>
        /// Gets the maximum identifier.
        /// </summary>
        /// <returns>int tpe</returns>
        /// <CreatedBy>Avishek</CreatedBy>
        /// <CreatedeDate>Mar-10-2015</CreatedeDate>
        public int GetMaxID()
        {
            int data = _unitOfWork.EmployeesRepository.Get().Select(s => s.EmpID).DefaultIfEmpty().Max();
            return data;
        }

        /// <summary>
        /// Saves the loan.
        /// </summary>
        /// <param name="tbl_employee">The tbl_employee.</param>
        /// <CreatedBy>Avishek</CreatedBy>
        /// <CreatedeDate>Mar-10-2015</CreatedeDate>
        private void SaveLoan(tbl_Employees tbl_employee)
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode != 0)
            {
                try
                {
                    string loanid = "";
                    var v = _unitOfWork.PFLoanRepository.Get().Select(s => new
                    {
                        PFLoanID = s.PFLoanID
                    }).OrderBy(o => o.PFLoanID).LastOrDefault();
                    if (v != null)
                    {
                        int i = Convert.ToInt32(v.PFLoanID);
                        i = i + 1;
                        loanid = i.ToString().PadLeft(6, '0');
                    }
                    else
                    {
                        loanid = "000001";
                    }

                    var pfDuration = ((DateTime.Now.Year - tbl_employee.PFActivationDate.Year) * 12) + DateTime.Now.Month - tbl_employee.PFActivationDate.Month;
                    if (DLL.Utility.ApplicationSetting.JoiningDate == true)

                    {
                        pfDuration = ((DateTime.Now.Year - tbl_employee.JoiningDate.Value.Year) * 12) + DateTime.Now.Month - tbl_employee.JoiningDate.Value.Month;
                    }
                    var loanRule = _unitOfWork.LoanRulesRepository.Get().ToList();

                    decimal ruleID = 0;
                    foreach (var item in loanRule.OrderBy(x => x.WorkingDurationInMonth))
                    {
                        if (pfDuration >= item.WorkingDurationInMonth)
                        {
                            ruleID = item.ROWID;
                        }
                    }

                    if (tbl_employee != null)
                    {
                        tbl_PFLoan tbl_pfLoan = new tbl_PFLoan();
                        tbl_pfLoan.EmpID = tbl_employee.EmpID;
                        tbl_pfLoan.PFLoanID = loanid;
                        tbl_pfLoan.LoanAmount = (decimal)tbl_employee.opLoan;
                        tbl_pfLoan.TermMonth = (int)loanRule.Where(x => x.ROWID == ruleID).Select(x => x.InstallmentNoumber).FirstOrDefault();
                        tbl_pfLoan.Interest = (int)loanRule.Where(x => x.ROWID == ruleID).Select(x => x.IntarestRate).FirstOrDefault();
                        tbl_pfLoan.StartDate = DateTime.Now;
                        tbl_pfLoan.EditUser = _unitOfWork.CustomRepository.GetUserID(User.Identity.Name);
                        tbl_pfLoan.EditDate = System.DateTime.Now;
                        tbl_pfLoan.IsApproved = 1;
                        tbl_pfLoan.ApprovedBy = _unitOfWork.CustomRepository.GetUserID(User.Identity.Name);
                        tbl_pfLoan.EditUserName = User.Identity.Name;
                        tbl_pfLoan.RuleUsed = (int)ruleID;
                        tbl_pfLoan.OCode = oCode;
                        _unitOfWork.PFLoanRepository.Insert(tbl_pfLoan);

                        int voucherId = 0;
                        List<string> ledgerNameList = new List<string>();
                        List<decimal> debit = new List<decimal>();

                        ledgerNameList.Add("Member Loan");
                        debit.Add(tbl_pfLoan.LoanAmount);

                        _unitOfWork.AccountingRepository.SingleEntryVoucher(tbl_pfLoan.EmpID, 5, DateTime.Now, ref voucherId, "Loan approve ", ledgerNameList, debit, User.Identity.Name, _unitOfWork.CustomRepository.GetUserID(User.Identity.Name), tbl_pfLoan.EmpID.ToString(), "Loan approved", "", "", null, tbl_pfLoan.PFLoanID, oCode, "Approve Loan");
                        GenerateAmortization(tbl_pfLoan, tbl_pfLoan.EmpID, tbl_pfLoan.PFLoanID, tbl_pfLoan.StartDate);
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        /// <summary>
        /// Generates the amortization.
        /// </summary>
        /// <param name="tbl_pfLoan">The TBL_PF loan.</param>
        /// <param name="empID">The emp identifier.</param>
        /// <param name="pfLoanID">The pf loan identifier.</param>
        /// <param name="startDate">The start date.</param>
        /// <CreatedBy>Avishek</CreatedBy>
        /// <CreatedeDate>Mar-10-2015</CreatedeDate>
        private void GenerateAmortization(tbl_PFLoan tbl_pfLoan, int empID, string pfLoanID, DateTime startDate)
        {
            int i = 1;
            tbl_PFL_Amortization tblPflAmortization;
            List<tbl_PFL_Amortization> lstAmortization = new List<tbl_PFL_Amortization>();
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode != 0)
            {
                double intRate = Convert.ToDouble(tbl_pfLoan.Interest / 100);
                double loanTenor = Convert.ToDouble(tbl_pfLoan.TermMonth);
                double loanAmount = Convert.ToDouble(tbl_pfLoan.LoanAmount);
                double pHlemi;
                double tmpgp;
                double tmpk;
                tmpk = 1 / (1 + intRate * 1 / 12);
                tmpgp = (Math.Pow(tmpk, loanTenor) - 1) / (tmpk - 1) * tmpk;
                pHlemi = loanAmount / tmpgp / 1;
                double pHlemiRound = Math.Round(pHlemi, 4);
                if (loanAmount > 0 && loanTenor > 0)
                {
                    decimal Balance = Convert.ToDecimal(loanAmount);
                    startDate = startDate.AddMonths(-1);
                    while (Balance > 0)
                    {
                        tblPflAmortization = new tbl_PFL_Amortization();
                        tblPflAmortization.InstallmentNumber = i;
                        tblPflAmortization.Amount = Math.Round(Balance, 4);
                        tblPflAmortization.Interest = Math.Round(tblPflAmortization.Amount * Convert.ToDecimal(intRate) / 12, 4);
                        tblPflAmortization.Principal = Math.Round(Convert.ToDecimal(pHlemi) - tblPflAmortization.Interest, 4);
                        tblPflAmortization.Balance = Math.Round(tblPflAmortization.Amount - tblPflAmortization.Principal, 4);
                        tblPflAmortization.Processed = 0;
                        startDate = startDate.AddMonths(1);
                        string year = startDate.Year + "";
                        string month = startDate.Month.ToString().PadLeft(2, '0');
                        tblPflAmortization.ConMonth = month;
                        tblPflAmortization.ConYear = year;
                        if (tblPflAmortization.Amount < Convert.ToDecimal(pHlemiRound))
                        {
                            Balance = 0;
                            tblPflAmortization.Balance = 0;
                        }
                        else
                        {
                            Balance = tblPflAmortization.Balance;
                        }
                        lstAmortization.Add(tblPflAmortization);
                        i++;
                    }
                    Guid userID = _unitOfWork.CustomRepository.GetUserID(User.Identity.Name);
                    foreach (tbl_PFL_Amortization item in lstAmortization)
                    {
                        item.EditUser = userID;
                        item.EditDate = System.DateTime.Now;
                        item.ReScheduleID = 1;
                        item.EmpID = empID;
                        item.PFLoanID = pfLoanID;
                        item.OCode = oCode;
                        _unitOfWork.AmortizationRepository.Insert(item);
                    }
                    try
                    {
                        _unitOfWork.Save();
                    }
                    catch (Exception x)
                    {
                        ErrorGenerate(x);
                    }
                }
            }
        }

        /// <summary>
        /// Errors the generate.
        /// </summary>
        /// <param name="ex">The ex.</param>
        /// <CreatedBy>Avishek</CreatedBy>
        /// <CreatedeDate>Mar-10-2015</CreatedeDate>
        private void ErrorGenerate(Exception ex)
        {
            tbl_ErrorLog error_log = new tbl_ErrorLog();
            error_log.Message = ex.Message;
            error_log.InnerException = ex.InnerException + "";
            error_log.UserName = User.Identity.Name;
            error_log.Time = DateTime.Now;
            try
            {
                error_log.Terminal = Request.ServerVariables["REMOTE_ADDR"];
            }
            catch
            {
                error_log.Terminal = System.Web.HttpContext.Current.Request.UserHostAddress;
            }
            error_log.HostName = Dns.GetHostName();
            try
            {
                _unitOfWork.ErrorLogRepository.Insert(error_log);
                _unitOfWork.Save();
            }
            catch
            {

            }
        }



    }
}
