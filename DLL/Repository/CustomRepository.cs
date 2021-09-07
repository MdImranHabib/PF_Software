using System;
using System.Collections.Generic;
using System.Linq;
using DLL.ViewModel;
using System.Data.SqlClient;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Configuration;
using DLL.Utility;

namespace DLL.Repository
{
    public class CustomRepository : IDisposable
    {
        private PFTMEntities context;
        public UnitOfWork unitOfWork = new UnitOfWork();

        Cheque_BankInfo_DAL aCheque_BankInfo_DAL = new Cheque_BankInfo_DAL();

        //internal List<ChequeR> GetChequePrint(int empId, int OCODE)
        //{
        //    return aCheque_BankInfo_DAL.GetAc_Rpt_ChequePrint(empId, OCODE);
        //}

        public List<ChequeR> GetChequePrint(int? EmpId, int OCODE)
        {
            PFTMEntities context = new PFTMEntities();
            SqlParameter empId = new SqlParameter("@EmpId", EmpId);
            SqlParameter O_CODE = new SqlParameter("@OCODE", OCODE);
            var result = context.Database.SqlQuery<ChequeR>("Ac_Rpt_ChequePrint @EmpId, @OCODE", empId, O_CODE).ToList();

            return result;
        }

        // Added By Kamrul 2019-05-04
        public IEnumerable<VM_LoanSummery> sp_GenerateLoanBalance(int fromMonth, int fromYear, int toMonth, int toYear)
        {

            SqlParameter fm = new SqlParameter("@fromMonth", fromMonth);
            SqlParameter fy = new SqlParameter("@fromYear", fromYear);
            SqlParameter tm = new SqlParameter("@toMonth", toMonth);
            SqlParameter ty = new SqlParameter("@toYear", toYear);

            var result = context.Database.SqlQuery<VM_LoanSummery>("rsp_GetLoanSummary @fromMonth, @fromYear, @toMonth, @toYear", fm, fy, tm, ty).ToList();
            //var result = context.Database.SqlQuery<VM_acc_VoucherDetail>("EXEC sp_GetLedgerTransactionDetail").ToList();
            return result;
        }

        #region Review and Modify By Avishek Date:Mar-7-2016
        public int GetCurrentContribution(int empID, ref decimal aa, ref decimal b)
        {
            var v = (from a in context.tbl_Contribution.Where(f => f.EmpID == empID)
                     group a by a.EmpID into grp
                     select new
                     {
                         oc = grp.Sum(g => g.SelfContribution),
                         ec = grp.Sum(g => g.EmpContribution)
                     });
            return 0;
        }

        #region Get Member Financial Summary
        /// <summary>
        /// SP_s the get member financial summary.
        /// </summary>
        /// <param name="employeeID">The employee identifier.</param>
        /// <param name="selfCon">The self con.</param>
        /// <param name="empCon">The emp con.</param>
        /// <param name="selfProfit">The self profit.</param>
        /// <param name="empProfit">The emp profit.</param>
        /// <param name="loanAmount">The loan amount.</param>
        /// <param name="loanAmountPaid">The loan amount paid.</param>
        /// <ModifiedBy>Avishek</ModifiedBy>
        /// <DateofModification>Date:Feb-11-2016</DateofModification>
        public void sp_GetMemberFinancialSummary(int employeeID, out decimal selfCon, out decimal empCon, out decimal selfProfit, out decimal empProfit, out decimal loanAmount, out decimal loanAmountPaid)
        {
            SqlParameter empID = new SqlParameter("@empId", employeeID);

            SqlParameter oSelfCon = new SqlParameter("oSelfContribution", SqlDbType.Decimal)
            {
                Direction = System.Data.ParameterDirection.Output
            };

            SqlParameter oEmpCon = new SqlParameter("oEmpContribution", SqlDbType.Decimal)
            {
                Direction = System.Data.ParameterDirection.Output
            };
            SqlParameter oSelfProfit = new SqlParameter("oSelfProfit", SqlDbType.Decimal)
            {
                Direction = System.Data.ParameterDirection.Output
            };
            SqlParameter oEmpfProfit = new SqlParameter("oEmpfProfit", SqlDbType.Decimal)
            {
                Direction = System.Data.ParameterDirection.Output
            };
            SqlParameter oLoanAmount = new SqlParameter("oLoanAmount", SqlDbType.Decimal)
            {
                Direction = System.Data.ParameterDirection.Output
            };
            SqlParameter oLoanAmountPaid = new SqlParameter("oLoanAmountPaid", SqlDbType.Decimal)
            {
                Direction = System.Data.ParameterDirection.Output
            };
            context.Database.ExecuteSqlCommand("sp_GetMemberFinancialSummary @empId, @oSelfContribution out, @oEmpContribution out, @oSelfProfit out,@oEmpfProfit out, @oLoanAmount out, @oLoanAmountPaid out", empID, oSelfCon, oEmpCon, oSelfProfit, oEmpfProfit, oLoanAmount, oLoanAmountPaid);
            selfCon = (decimal)oSelfCon.Value;
            empCon = (decimal)oEmpCon.Value;
            selfProfit = (decimal)oSelfProfit.Value;
            empProfit = (decimal)oEmpfProfit.Value;
            loanAmount = (decimal)oLoanAmount.Value;
            loanAmountPaid = (decimal)oLoanAmountPaid.Value;
        }
        #endregion

        public CustomRepository(PFTMEntities context)
        {
            this.context = context;
        }

        public Guid GetUserID(string userName)
        {
            Guid result = context.tbl_User.Where(u => u.LoginName == userName).Select(s => s.UserID).FirstOrDefault();
            return result;
        }

        public string GetUserName(Guid userID)
        {
            string result = context.tbl_User.Where(u => u.UserID == userID).Select(s => s.LoginName).FirstOrDefault();
            return result;
        }

        public IEnumerable<VM_Contribution> sp_GetProfitDistributionDetail(int processID)
        {
            SqlParameter pID = new SqlParameter("@processID", processID);
            var result = context.Database.SqlQuery<VM_Contribution>("sp_GetProfitDistributionDetail @processID", pID).ToList();
            return result;
        }

        /// <summary>
        /// SP_s the get total contribution.
        /// </summary>
        /// <param name="fromDate">From date.</param>
        /// <param name="toDate">To date.</param>
        /// <returns>list</returns>
        /// <CreteadBy>Avishek</CreteadBy>
        /// <CreatedDate>Apr-19-2015</CreatedDate>
        public IEnumerable<VM_Contribution> sp_GetTotalContribution(DateTime fromDate, DateTime toDate)
        {
            SqlParameter fDate = new SqlParameter("@fromDate", fromDate);
            SqlParameter tDate = new SqlParameter("@toDate", toDate);
            var result = context.Database.SqlQuery<VM_Contribution>("sp_GetTotalContribution @fromDate, @toDate", fDate, tDate).ToList();

            return result;
        }
        public IEnumerable<VM_Contribution> sp_GetTotalContributionProfit(DateTime fromDate, DateTime toDate)
        {
            SqlParameter fDate = new SqlParameter("@fromDate", fromDate);
            SqlParameter tDate = new SqlParameter("@toDate", toDate);
            var result = context.Database.SqlQuery<VM_Contribution>("sp_GetTotalContributionProfit @fromDate, @toDate", fDate, tDate).ToList();

            return result;
        }
        //End

        /// <summary>
        /// Gets all employee.
        /// </summary>
        /// <param name="oCode">The o code.</param>
        /// <returns>list</returns>
        /// <ModifiedBy>Avishek</ModifiedBy>
        /// <DateofModification>Date:Feb-11-2016</DateofModification>
        /// 
        public IQueryable<VM_Employee> GetAllEmployee(int oCode)
        {
            var result = from a in context.tbl_Employees.Where(o => o.OCode == oCode)
                         //join b in context.LU_tbl_Branch on a.Branch equals b.BranchID
                         // d in context.LU_tbl_Designation on a.Designation equals d.DesignationID into leftJoinDsg
                         // from dd in leftJoinDsg.DefaultIfEmpty()

                         select new VM_Employee
                         {
                             BirthDate = a.BirthDate,
                             ContactNumber = a.ContactNumber,
                             DepartmentID = a.Department,
                             // DesignationName = dd.DesignationName,
                             //BranchID = a.Branch ?? 0,
                             //BranchName = b.BranchName,
                             DesignationID = a.Designation,
                             EditDate = a.EditDate,
                             EditUser = a.EditUser,
                             EditUserName = a.EditUserName,
                             Email = a.Email,
                             EmpID = a.EmpID,
                             IdentificationNumber = a.IdentificationNumber,
                             EmpName = a.EmpName,
                             Gender = a.Gender == null ? "" : (a.Gender == "0" ? "Female" : "Male"),
                             JoiningDate = a.JoiningDate,
                             PresentAddress = a.PresentAddress,
                             NID = a.NID,
                             PFStatusID = a.PFStatus ?? 0,
                             EmpImg = a.EmpImg,
                             PFActivationDate = a.PFActivationDate,
                             PFDeactivationDate = a.PFDeactivationDate,
                             opOwnContribution = a.opOwnContribution ?? 0,
                             opEmpContribution = a.opEmpContribution ?? 0,
                             opLoan = a.opLoan ?? 0,
                             opProfit = a.opProfit ?? 0,
                             opDepartmentName = a.opDepartmentName,
                             opDesignationName = a.opDesignationName,
                             Comment = a.Comment,
                             PassVoucher = a.PassVoucher,
                             PassVoucherMessage = a.PassVoucherMessage,
                             PFDeactivationVoucherID = a.PFDeactivationVoucherID ?? 0,
                             OCode = a.OCode ?? 0
                         };

            return result;
        }

        public IQueryable<VM_Employee> GetAllEmployeeByBranch(int oCode)
        {
            var result = from a in context.tbl_Employees.Where(o => o.OCode == oCode)
                         join b in context.LU_tbl_Branch on a.Branch equals b.BranchID
                         // d in context.LU_tbl_Designation on a.Designation equals d.DesignationID into leftJoinDsg
                         // from dd in leftJoinDsg.DefaultIfEmpty()

                         select new VM_Employee
                         {
                             BirthDate = a.BirthDate,
                             ContactNumber = a.ContactNumber,
                             DepartmentID = a.Department,
                             // DesignationName = dd.DesignationName,
                             BranchID = a.Branch ?? 0,
                             BranchName=b.BranchName ?? "",
                             DesignationID = a.Designation,
                             EditDate = a.EditDate,
                             EditUser = a.EditUser,
                             EditUserName = a.EditUserName,
                             Email = a.Email,
                             EmpID = a.EmpID,
                             IdentificationNumber = a.IdentificationNumber,
                             EmpName = a.EmpName,
                             Gender = a.Gender == null ? "" : (a.Gender == "0" ? "Female" : "Male"),
                             JoiningDate = a.JoiningDate,
                             PresentAddress = a.PresentAddress,
                             NID = a.NID,
                             PFStatusID = a.PFStatus ?? 0,
                             EmpImg = a.EmpImg,
                             PFActivationDate = a.PFActivationDate,
                             PFDeactivationDate = a.PFDeactivationDate,
                             opOwnContribution = a.opOwnContribution ?? 0,
                             opEmpContribution = a.opEmpContribution ?? 0,
                             opLoan = a.opLoan ?? 0,
                             opProfit = a.opProfit ?? 0,
                             opDepartmentName = a.opDepartmentName,
                             opDesignationName = a.opDesignationName,
                             Comment = a.Comment,
                             PassVoucher = a.PassVoucher,
                             PassVoucherMessage = a.PassVoucherMessage,
                             PFDeactivationVoucherID = a.PFDeactivationVoucherID ?? 0,
                             OCode = a.OCode ?? 0
                         };

            return result;
        }

        public IQueryable<VM_Employee> GetDeactivatedPFMemberlist()
        {
            var result = from a in context.tbl_Employees.Where(w => w.PFStatus == 2) // 02 for deactivated memberlist
                         join c in context.tbl_User on a.EditUser equals c.UserID
                         select new VM_Employee
                         {
                             BirthDate = a.BirthDate,
                             ContactNumber = a.ContactNumber,
                             PFDeactivatedBy = c.LoginName,
                             EditDate = a.EditDate,
                             EditUser = a.EditUser,
                             Email = a.Email,
                             EmpID = a.EmpID,
                             EmpName = a.EmpName,
                             JoiningDate = a.JoiningDate,
                             PresentAddress = a.PresentAddress,
                             NID = a.NID,
                             PFActivationDate = a.PFActivationDate,
                             PFDeactivationDate = a.PFDeactivationDate
                         };

            return result;
        }

        public IQueryable<VM_Contribution> GetPendingContributionList()
        {
            //DateTime lastDate = unitOfWork.ContributionRepository.Get().Select(x => x.ProcessDate).Max();



            var result = from c in context.tbl_Contribution
                         join a in context.tbl_Employees on c.EmpID equals a.EmpID
                         where ((DateTime.Now.Year - c.ProcessDate.Year) * 12 + DateTime.Now.Month - c.ProcessDate.Month) > 3 && a.PFStatus == 1
                         //group c by c.EmpID into g

                         select new VM_Contribution
                         {
                             EmpID = c.EmpID,
                             IdentificationNumber = a.IdentificationNumber,
                             EmpName = a.EmpName,
                             ProcessDate = c.ProcessDate,
                             TotalPendingMonth = (DateTime.Now.Year - c.ProcessDate.Year) * 12 + DateTime.Now.Month - c.ProcessDate.Month
                         };

            return result;
        }


        //public int GetContributionPendingMonth()
        //{
        //    try
        //    {
        //        IObjectContextAdapter dbcontextadapter = (IObjectContextAdapter)context;
        //        dbcontextadapter.ObjectContext.CommandTimeout = 10000;

        //        var result = (from c in context.tbl_Contribution
        //                      join a in context.tbl_Employees on c.EmpID equals a.EmpID

        //                      where ((DateTime.Now.Year - c.ProcessDate.Year) * 12 + DateTime.Now.Month - c.ProcessDate.Month) > 3 && a.PFStatus == 1
        //                      group c by c.EmpID into g
        //                      select g);

        //        return result.Count();
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }


        //}

        public List<VM_tbl_Loan_Receivable> GetLoanReceivableList(string conMonth, string conYear)
        {

            List<VM_tbl_Loan_Receivable> v = (from a in context.tbl_PFL_Amortization
                                              join b in context.tbl_Employees on a.EmpID equals b.EmpID
                                              where a.ConMonth == conMonth && a.ConYear == conYear && a.Processed == 0
                                              select new VM_tbl_Loan_Receivable
                                              {
                                                  IdentificationNumber = b.IdentificationNumber,
                                                  EmpName = b.EmpName,
                                                  Loan_id = a.PFLoanID,
                                                  Principal = a.Principal,
                                                  interest = a.Interest,
                                                  PF_Month = a.ConMonth,
                                                  PF_Year = a.ConYear
                                              }).ToList();

            return v;
        }

        public List<VM_Employee> GetDeactivatedPFMemberReport(DateTime? fromDate, DateTime? toDate)
        {
            PFTMEntities context = new PFTMEntities();
            SqlParameter fDate = new SqlParameter("@fromDate", fromDate);
            SqlParameter tDate = new SqlParameter("@toDate", toDate);

            var data = context.Database.SqlQuery<VM_Employee>("sp_GetDeactivatedMemberList @fromDate, @toDate", fDate, tDate).ToList();

            return data;
        }

        public IQueryable<VM_Contribution> GetContributionDetail()
        {
            try
            {
                var v = from a in context.tbl_Contribution
                        join b in context.tbl_Employees on a.EmpID equals b.EmpID
                        //join br in context.LU_tbl_Branch on b.Branch equals br.BranchID
                        join c in context.tbl_ProfitDistributionDetail on new { a.EmpID, a.ContributionDate.Value.Month, a.ContributionDate.Value.Year } equals new { c.EmpID, c.TransactionDate.Value.Month, c.TransactionDate.Value.Year } into pr
                        from c in pr.DefaultIfEmpty()
                        select new VM_Contribution
                        {
                            ConMonth = a.ConMonth,
                            ConYear = a.ConYear,
                            ECPercentage = a.ECPercentage,
                            //Branch=br.BranchName ?? "",
                            EmpID = a.EmpID,
                            IdentificationNumber = b.IdentificationNumber ?? "",
                            EmpContribution = a.EmpContribution,
                            EmpName = b.EmpName,
                            Deginecation = b.opDesignationName ?? "",
                            Department = b.opDepartmentName ?? "",
                            JoiningDate = b.JoiningDate ?? DateTime.MinValue,
                            ProcessDate = a.ProcessDate,
                            SCPercentage = a.SCPercentage,
                            SelfContribution = a.SelfContribution,
                            Salary = a.Salary ?? 0,
                            SCInterest = a.SCInterest ?? 0,
                            ECInterest = a.ECInterest ?? 0,
                            InterestRate = a.InterestRate,
                            OCode = a.OCode ?? 0,
                            SelfProfit = c.SelfProfit == null ? 0 : c.SelfProfit,
                            EmpProfit = c.EmpProfit == null ? 0 : c.EmpProfit
                        };
                return v;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        //For Calculating Contribution according to branch Added By Kamrul
        public IQueryable<VM_Contribution> GetContributionDetailWithBranch()
        {
            try
            {
                var v = from a in context.tbl_Contribution
                        join b in context.tbl_Employees on a.EmpID equals b.EmpID
                        join br in context.LU_tbl_Branch on b.Branch equals br.BranchID
                        join c in context.tbl_ProfitDistributionDetail on new { a.EmpID, a.ContributionDate.Value.Month, a.ContributionDate.Value.Year } equals new { c.EmpID, c.TransactionDate.Value.Month, c.TransactionDate.Value.Year } into pr
                        from c in pr.DefaultIfEmpty()
                        select new VM_Contribution
                        {
                            ConMonth = a.ConMonth,
                            ConYear = a.ConYear,
                            ECPercentage = a.ECPercentage,
                            Branch = br.BranchName ?? "",
                            EmpID = a.EmpID,
                            IdentificationNumber = b.IdentificationNumber ?? "",
                            EmpContribution = a.EmpContribution,
                            EmpName = b.EmpName,
                            Deginecation = b.opDesignationName ?? "",
                            Department = b.opDepartmentName ?? "",
                            JoiningDate = b.JoiningDate ?? DateTime.MinValue,
                            ProcessDate = a.ProcessDate,
                            SCPercentage = a.SCPercentage,
                            SelfContribution = a.SelfContribution,
                            Salary = a.Salary ?? 0,
                            SCInterest = a.SCInterest ?? 0,
                            ECInterest = a.ECInterest ?? 0,
                            InterestRate = a.InterestRate,
                            OCode = a.OCode ?? 0,
                            SelfProfit = c.SelfProfit == null ? 0 : c.SelfProfit,
                            EmpProfit = c.EmpProfit == null ? 0 : c.EmpProfit
                        };
                return v;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        //this method needs to tune
        public IEnumerable<VM_MonthlyPFInterest> GetMonthlyPFInterest(string month, string year)
        {
            int _month = Convert.ToInt32(month);
            int _year = Convert.ToInt32(year);

            var v = context.tbl_Contribution.Where(w => w.ConMonth == month && w.ConYear == year).Select(s => s.EmpID).ToList();
            var totalFund = from a in context.tbl_Contribution.Where(x => v.Contains(x.EmpID)).ToList()
                            where Convert.ToInt32(a.ConMonth) < _month && Convert.ToInt32(a.ConYear) <= _year// && v.Contains(a.EmpID)
                            group a by a.EmpID into grp
                            select new VM_MonthlyPFInterest
                            {
                                EmpID = grp.Key,
                                SelfContributionTillNow = grp.Sum(s => s.SelfContribution),
                                EmpContributionTillNow = grp.Sum(t => t.EmpContribution)
                            };

            return totalFund;
        }

        public IQueryable<VM_PFMonthlyStatus> PFMonthlyStatus(int oCode)
        {
            var v = from a in context.tbl_Contribution
                    where a.OCode == oCode
                    group a by new { a.ContributionDate } into grp
                    select new VM_PFMonthlyStatus
                    {
                        ContrebutionDate = (DateTime)grp.Key.ContributionDate,
                        SelfContribution = grp.Sum(a => a.SelfContribution),
                        EmpContribution = grp.Sum(b => b.EmpContribution),
                        ProcessRunDate = grp.Max(c => c.ProcessDate)
                    };
            return v;
        }

        public IQueryable<VM_PFMonthlyStatus> EmpPFMonthlyStatus(int empId)
        {
            var v = from a in context.tbl_Contribution
                    join b in context.tbl_Employees.Where(x => x.EmpID == empId) on a.EmpID equals b.EmpID
                    select new VM_PFMonthlyStatus
                    {
                        EmpID = a.EmpID,
                        IdentificationNumber = b.IdentificationNumber,
                        EmpName = b.EmpName,
                        Designation = b.Designation,
                        ProcessRunDate = a.ProcessDate,
                        EmpContribution = a.EmpContribution,
                        Month = a.ConMonth, //DateTime.ParseExact("13/" + a.ConMonth + "/" + a.ConYear, "dd/MM/yyyy", CultureInfo.InvariantCulture)+"",
                        Year = a.ConYear,
                        SelfContribution = a.SelfContribution,
                        SCInterest = a.SCInterest ?? 0,
                        ECInterest = a.ECInterest ?? 0
                    };
            return v;
        }

        public IQueryable<VM_PFMonthlyStatus> EmpPFMonthlyStatus(int oCode, int empId)
        {
            var v = from a in context.tbl_Contribution.Where(x => x.OCode == oCode)
                    join b in context.tbl_Employees.Where(x => x.EmpID == empId) on a.EmpID equals b.EmpID //Added By Avishek Date:Feb-18-2015
                    select new VM_PFMonthlyStatus
                    {
                        EmpID = a.EmpID,
                        IdentificationNumber = b.IdentificationNumber,//Modified by Avishek Date:Feb-18-2015 Reason that show IdentificationNumber in report IdentificationNumber
                        ProcessRunDate = a.ProcessDate,
                        ContrebutionDate = (DateTime)a.ContributionDate,
                        EmpContribution = a.EmpContribution,
                        Month = a.ConMonth, //DateTime.ParseExact("13/" + a.ConMonth + "/" + a.ConYear, "dd/MM/yyyy", CultureInfo.InvariantCulture)+"",
                        Year = a.ConYear,
                        SelfContribution = a.SelfContribution,
                        SCInterest = a.SCInterest ?? 0,
                        ECInterest = a.ECInterest ?? 0
                    };
            return v;
        }

        /// <summary>
        /// Gets the system user list.
        /// </summary>
        /// <param name="oCode">The o code.</param>
        /// <returns></returns>
        /// <ModifiedBy>Avishek</ModifiedBy>
        /// <DateofModification>Date:Feb-11-2016</DateofModification>
        public IQueryable<VM_UserInfo> GetSystemUserListByBranch(int oCode)
        {
            var v = from a in context.tbl_User.Where(w => w.EmpID != null && w.OCode == oCode)
                    join b in context.LU_tbl_Branch on a.BranchID equals b.BranchID into leftJoinGroup
                    from c in leftJoinGroup.DefaultIfEmpty()
                    join d in context.LU_tbl_Department on a.DepartmentID equals d.DepartmentID into leftJoinDepartmentId
                    join e in context.LU_tbl_CompanyInformation on a.OCode equals e.CompanyID into leftJoinCompany
                    from ee in leftJoinCompany.DefaultIfEmpty()
                    from s in leftJoinDepartmentId.DefaultIfEmpty()
                    select new VM_UserInfo
                    {
                        UserId = a.UserID,
                        UserName = a.LoginName,
                        FullName = a.UserFullName,
                        IsActive = a.IsActive == 1 ? true : false,
                        BranchID = c.BranchID,
                        BranchName = c.BranchName,
                        EmailNotificationActive = a.EmailNotificationActive == 1 ? true : false,
                        Email = a.Email,
                        Phone = a.Phone,
                        DepartmentID = s.DepartmentID,
                        DepartmentName = s.DepartmentName,
                        CompanyID = a.OCode,
                        CompanyName = ee.CompanyName
                    };
            return v;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="oCode"></param>
        /// <returns></returns>
         public IQueryable<VM_UserInfo> GetSystemUserList(int oCode)
        {
            var v = from a in context.tbl_User.Where(w => w.EmpID != null && w.OCode == oCode)
                    //join 
                    //b in context.LU_tbl_Branch on a.BranchID equals b.BranchID into leftJoinGroup
                    //from c in leftJoinGroup.DefaultIfEmpty()
                    join d in context.LU_tbl_Department on a.DepartmentID equals d.DepartmentID into leftJoinDepartmentId

                    join e in context.LU_tbl_CompanyInformation on a.OCode equals e.CompanyID into leftJoinCompany
                    from ee in leftJoinCompany.DefaultIfEmpty()
                    from s in leftJoinDepartmentId.DefaultIfEmpty()
                    select new VM_UserInfo
                    {
                        UserId = a.UserID,
                        UserName = a.LoginName,
                        FullName = a.UserFullName,
                        IsActive = a.IsActive == 1 ? true : false,
                        //BranchID = c.BranchID,
                        //BranchName = c.BranchName,
                        EmailNotificationActive = a.EmailNotificationActive == 1 ? true : false,
                        Email = a.Email,
                        Phone = a.Phone,
                        DepartmentID = s.DepartmentID,
                        DepartmentName = s.DepartmentName,
                        CompanyID = a.OCode,
                        CompanyName = ee.CompanyName
                    };
            return v;
        }

        public IQueryable<VM_AuditLog> GetAuditLogList(int oCode)
        {
            var v = from al in context.tbl_FinancialAuditLog.Where(x => x.OCode == oCode)
                    join user in context.tbl_User on al.EditUser equals user.UserID
                    select new VM_AuditLog
                    {
                        LogID = al.LogID,
                        LastAuditDate = (DateTime)al.LastAuditDate,
                        EditUserName = user.UserFullName,
                        OCode = al.OCode,
                        LogDate = (DateTime)al.LogDate
                    };
            return v;
        }

        /// <summary>
        /// For Executing Backup storeprocedure Added By Kamrul Hasan 2019-01-10
        /// </summary>
        /// <param name="name">File Name</param>
        /// <param name="path">Static Path of Local PC</param>
        public void DBBackupProcess(string name, string path)
        {
            
            // for Test : string cs = "data source=192.168.0.17;initial catalog=YWCAPF;user id=sa;password=ssl@1234";
            string cs = "";

            if (DLL.Utility.ApplicationSetting.DbBackUpConnection != null)
            {
                cs = ApplicationSetting.DbBackUpConnection;
            }
            //List<VM_forfeiture> forfeitures = new List<VM_forfeiture>();
            using (SqlConnection con = new SqlConnection(cs))
            
                using (SqlCommand cmd = new SqlCommand("psp_db_backup", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@name", SqlDbType.VarChar).Value = name;
                    cmd.Parameters.Add("@path", SqlDbType.VarChar).Value = path;

                    con.Open();
                    cmd.CommandTimeout = 3600;
                    cmd.ExecuteNonQuery();
                    con.Close();
                }
                
        }

        /// <summary>
        /// Added by Shohid
        /// </summary>
        /// <param name="ledgerId"></param>
        /// <param name="ocode"></param>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <returns></returns>
        public List<VM_acc_VoucherDetail> GetVoucherDetailsByLedgerId(Guid ledgerId, int ocode, DateTime fromDate, DateTime toDate)
        {
            List<VM_acc_VoucherDetail> v = new List<VM_acc_VoucherDetail>();
            v = (from a in context.acc_VoucherDetail
                 join b in context.acc_VoucherEntry on a.VoucherID equals b.VoucherID
                 where a.LedgerID == ledgerId && a.OCode == ocode && a.TransactionDate >= fromDate && a.TransactionDate <= toDate
                 select new VM_acc_VoucherDetail
                 {
                     VoucherID = a.VoucherID
                 }
                ).ToList();
            return v;
        }

        /// <summary>
        /// Added by Shohid
        /// </summary>
        /// <param name="voucherID"></param>
        /// <param name="ocode"></param>
        /// <returns></returns>
        public List<VM_acc_VoucherDetail> GetVoucherDetailsByVoucherId(int voucherID, int ocode)
        {
            List<VM_acc_VoucherDetail> v = new List<VM_acc_VoucherDetail>();
            v = (from a in context.acc_VoucherDetail
                 join b in context.acc_VoucherEntry on a.VoucherID equals b.VoucherID
                 join c in context.acc_Ledger on a.LedgerID equals c.LedgerID
                 where a.OCode == ocode && a.VoucherID == voucherID
                 select new VM_acc_VoucherDetail
                 {
                     VNumber = b.VNumber,
                     Debit = a.Debit ?? 0,
                     Credit = a.Credit ?? 0,
                     LedgerName = c.LedgerName,
                     TransactionDate = a.TransactionDate,
                     ChequeNumber = a.ChequeNumber
                 }
                ).OrderBy(o => o.TransactionDate).ToList();
            return v;
        }

        public IQueryable<VM_UserLoginHistory> UserLoginHistory()
        {
            var v = from a in context.tbl_User
                    join b in context.tbl_LoginHistory on a.LoginName equals b.UserName
                    orderby b.LoginTime descending
                    select new VM_UserLoginHistory
                    {
                        UserName = b.UserName,
                        LoginTime = b.LoginTime,
                        SignOut = b.SignOut,
                        UserFullName = a.UserFullName,
                        Host = b.HostName,
                        Terminal = b.Terminal
                    };
            return v;
        }

        /// <summary>
        /// Gets the module list.
        /// </summary>
        /// <param name="roleId">The role identifier.</param>
        /// <returns></returns>
        /// <ModifiedBy>Avishek</ModifiedBy>
        /// <DateofModification>Date:Feb-11-2016</DateofModification>
        public IQueryable<VM_RolesInModules> GetModuleList(int roleId)
        {
            var v = from a in context.SA_tbl_Module
                    join b in context.SA_tbl_Page on a.id equals b.ModuleID
                    join c in context.SA_tbl_PagePermission.Where(w => w.RoleID == roleId) on b.PageID equals c.PageID into leftJoinPage
                    from cc in leftJoinPage.DefaultIfEmpty()
                    select new VM_RolesInModules
                    {
                        ModuleName = a.name,
                        PageID = b.PageID,
                        PageName = b.PageName,
                        RoleID = roleId,
                        CanDelete = cc != null && cc.CanDelete,
                        CanEdit = cc != null && cc.CanEdit,
                        CanExecute = cc != null && cc.CanExecute,
                        CanVisit = cc != null && cc.CanVisit
                    };
            return v;
        }

        public IQueryable<VM_PFLoan> GetPFLoanHistoryByEmpID()
        {
            var v = from a in context.tbl_PFLoan
                    join c in context.tbl_Employees on a.EmpID equals c.EmpID
                    join b in context.tbl_User on a.ApprovedBy equals b.UserID into leftJoinApprover
                    from bb in leftJoinApprover.DefaultIfEmpty()
                    select new VM_PFLoan
                    {
                        ApprovedById = bb == null ? Guid.Empty : bb.UserID,
                        ApprovedByName = bb == null ? "" : bb.LoginName,
                        IsApproved = a.IsApproved,
                        EmpID = a.EmpID,
                        Installment = a.Installment,
                        Interest = a.Interest,
                        LoanAmount = a.LoanAmount,
                        PFLoanID = a.PFLoanID,
                        StartDate = a.StartDate,
                        TermMonth = a.TermMonth,
                        IdentificationNumber = c.IdentificationNumber,
                        PayableAmount = a.PayableAmount ?? 0,
                        RuleUsed = a.RuleUsed ?? 0
                    };
            return v;
        }

        public IQueryable<VM_PFLoan> GetPFLoanHistoryByEmpID(int ocode)
        {
            var v = from a in context.tbl_PFLoan
                    where a.OCode == ocode
                    join c in context.tbl_Employees on a.EmpID equals c.EmpID
                    join b in context.tbl_User on a.ApprovedBy equals b.UserID into leftJoinApprover
                    from bb in leftJoinApprover.DefaultIfEmpty()
                    select new VM_PFLoan
                    {
                        ApprovedById = bb == null ? Guid.Empty : bb.UserID,
                        ApprovedByName = bb == null ? "" : bb.LoginName,
                        IsApproved = a.IsApproved,
                        EmpID = a.EmpID,
                        EmpName = c.EmpName,
                        EmpDesignation = c.Designation,
                        EmpDepartment = c.Department,
                        Installment = a.Installment,
                        Interest = a.Interest,
                        LoanAmount = a.LoanAmount,
                        PFLoanID = a.PFLoanID,
                        StartDate = a.StartDate,
                        TermMonth = a.TermMonth,
                        IdentificationNumber = c.IdentificationNumber,
                        PayableAmount = a.PayableAmount ?? 0,
                        RuleUsed = a.RuleUsed ?? 0
                    };
            return v.OrderBy(s=>s.PFLoanID);
        }

        //get closed loan method calculates if there is unprocessed payment in particular loan
        public List<VM_PFLoan> GetClosedLoan(int OCode, string identificationNo, string loanNo)
        {
            List<VM_PFLoan> v = new List<VM_PFLoan>();
            if (identificationNo == "" || loanNo == "")
            {
                v = (from b in context.tbl_PFLoan
                     join e in context.tbl_Employees on b.EmpID equals e.EmpID
                     join d in context.tbl_User on b.ApprovedBy equals d.UserID
                     join c in
                         (
                              from a in context.tbl_PFL_Amortization
                              group a by a.PFLoanID into grp
                              where grp.Count(c => c.Processed == 0) < 1
                              select new
                              {
                                  PFLoanID = grp.Key,
                                  Paid = grp.Count()
                              }
                          )
                          on b.PFLoanID equals c.PFLoanID
                     select new VM_PFLoan
                     {
                         EmpID = b.EmpID,
                         PFLoanID = b.PFLoanID,
                         Interest = b.Interest,
                         LoanAmount = b.LoanAmount,
                         TermMonth = b.TermMonth,
                         StartDate = b.StartDate,
                         Installment = c.Paid,
                         IsApproved = b.IsApproved,
                         ApprovedById = b.ApprovedBy,
                         ApprovedByName = d.LoginName,
                         IdentificationNumber = e.IdentificationNumber
                     }).ToList();
                //return preList;
            }
            else
            {
                v = (from b in context.tbl_PFLoan
                     join e in context.tbl_Employees on b.EmpID equals e.EmpID
                     join d in context.tbl_User on b.ApprovedBy equals d.UserID
                     join c in
                         (
                              from a in context.tbl_PFL_Amortization
                              group a by a.PFLoanID into grp
                              where grp.Count(c => c.Processed == 0) < 1
                              select new
                              {
                                  PFLoanID = grp.Key,
                              }
                          )
                          on b.PFLoanID equals c.PFLoanID
                     where e.IdentificationNumber == identificationNo && b.PFLoanID == loanNo
                     select new VM_PFLoan
                     {
                         EmpID = b.EmpID,
                         PFLoanID = b.PFLoanID,
                         Interest = b.Interest,
                         LoanAmount = b.LoanAmount,
                         TermMonth = b.TermMonth,
                         StartDate = b.StartDate,
                         Installment = b.Installment,
                         IsApproved = b.IsApproved,
                         ApprovedById = b.ApprovedBy,
                         ApprovedByName = d.LoginName,
                         IdentificationNumber = e.IdentificationNumber
                     }).ToList();
                //return list;
            }
            return v;
        }

        public List<VM_PFLoanPayment> LoanPaymentFromSalary(string conYear, string conMonth, int oCode)
        {
            //get all employee loan list BUT NOT GETTIN CURRENT SAVINGS FOR EACH EMPLOYEE
            var v = (from a in context.tbl_PFL_Amortization.Where(w => w.ConMonth == conMonth && w.ConYear == conYear && w.OCode == oCode)
                     join b in context.tbl_PFLoan on new { a.EmpID, a.PFLoanID } equals new { b.EmpID, b.PFLoanID }
                     select new VM_PFLoanPayment
                     {
                         EmployeeID = b.EmpID,
                         LoanID = b.PFLoanID,
                         InstallmentAmount = b.Installment,
                         InstallmentNumber = a.InstallmentNumber,
                         PaymentStatus = a.Processed != 0,
                         PaymentDate = a.PaymentDate ?? DateTime.MinValue,
                         PaymentAmount = a.PaymentAmount ?? 0
                     }).ToList();
            //get employee list from v
            foreach (var item in v)
            {
                item.CurrentSavings = CurrentSavingsAmount(item.EmployeeID);
            }
            return v.ToList();
        }

        //to get current savings amount i only need the emp id, NO NEED ANY LOAN ID.
        /// <summary>
        /// Currents the savings amount.
        /// </summary>
        /// <param name="empId">The emp identifier.</param>
        /// <returns></returns>
        /// <ModifiedBy>Avishek</ModifiedBy>
        /// <DateofModification>Date:Feb-11-2016</DateofModification>
        public decimal CurrentSavingsAmount(int empId)
        {
            decimal savingsAmount = 0;
            decimal loanPaidAmount = 0;
            var contributionInfo = (from a in context.tbl_Contribution.Where(w => w.EmpID == empId)
                                    group a by a.EmpID into grp1
                                    select new
                                    {
                                        id = grp1.Key,
                                        self_amout = grp1.Sum(s => s.SelfContribution),
                                        emp_amount = grp1.Sum(s => s.EmpContribution)
                                    }).FirstOrDefault();
            if (contributionInfo != null)
            {
                savingsAmount = contributionInfo.self_amout + contributionInfo.emp_amount;
            }

            var loanPaymentInfo = from a in context.tbl_PFLoan.Where(w => w.EmpID == empId)
                                  join b in context.tbl_PFL_Amortization.Where(x => x.Processed == 1 && x.EmpID == empId)
                                  on new { a.EmpID, a.PFLoanID } equals new { b.EmpID, b.PFLoanID }
                                  select new
                                  {
                                      id = a.EmpID,
                                      amount = a.Installment
                                  };

            loanPaidAmount = loanPaymentInfo.GroupBy(g => g.id).Select(s => s.Sum(m => m.amount)).FirstOrDefault();
            return savingsAmount - loanPaidAmount;
        }

        /// <summary>
        /// Unpaids the loan.
        /// </summary>
        /// <param name="conYear">The con year.</param>
        /// <param name="conMonth">The con month.</param>
        /// <param name="oCode">The o code.</param>
        /// <returns></returns>
        /// <ModifiedBy>Avishek</ModifiedBy>
        /// <DateofModification>Date:Feb-11-2016</DateofModification>
        public List<VM_PFLoanPayment> UnpaidLoan(int conYear, int conMonth, int oCode)
        {
            //get all employee loan list BUT NOT GETTIN CURRENT SAVINGS FOR EACH EMPLOYEE
            var v = (from a in context.tbl_PFL_Amortization
                     where a.Processed == 0//Convert.ToInt32(a.ConMonth) <= conMonth && Convert.ToInt32(a.ConYear) <= conYear
                     join b in context.tbl_PFLoan on new { a.EmpID, a.PFLoanID } equals new { b.EmpID, b.PFLoanID }
                     join c in context.tbl_Employees on b.EmpID equals c.EmpID
                     where a.OCode == oCode
                     select new VM_PFLoanPayment
                     {
                         EmpID = b.EmpID,
                         EmpName = c.EmpName,
                         EmpDesignation = c.Designation,
                         EmpDepartment = c.Department,
                         PFLoanID = b.PFLoanID,
                         Amount = b.Installment,
                         InstallmentNumber = a.InstallmentNumber,
                         PaymentStatus = a.Processed != 0,
                         PaymentDate = a.PaymentDate ?? DateTime.MinValue,
                         PaymentAmount = a.PaymentAmount ?? 0,
                         ConMonth = a.ConMonth,
                         ConYear = a.ConYear,
                         IdentificationNumber = c.IdentificationNumber,
                     }).ToList();

            v = v.Where(w => Convert.ToInt32(w.ConMonth) <= conMonth && Convert.ToInt32(w.ConYear) <= conYear).ToList();
            return v;
        }

        /// <summary>
        /// Unpaids the loan by date.
        /// </summary>
        /// <param name="frmDate">The FRM date.</param>
        /// <param name="toDate">To date.</param>
        /// <param name="oCode">The o code.</param>
        /// <returns>list</returns>
        /// <CreaatedBy>Abdul Alim</CreaatedBy>
        /// <CreatedADate>01-09-2015</CreatedADate>
        public List<VM_PFLoanPayment> UnpaidLoanByDate(int oCode)
        {
            //get all employee loan list BUT NOT GETTIN CURRENT SAVINGS FOR EACH EMPLOYEE
            List<VM_PFLoanPayment> v = (from a in context.tbl_PFL_Amortization
                                        where a.Processed == 0
                                        join b in context.tbl_PFLoan on new { a.EmpID, a.PFLoanID } equals new { b.EmpID, b.PFLoanID }
                                        join c in context.tbl_Employees on b.EmpID equals c.EmpID
                                        where a.OCode == oCode
                                        select new VM_PFLoanPayment
                                        {
                                            EmpID = b.EmpID,
                                            EmpName = c.EmpName,
                                            EmpDesignation = c.Designation,
                                            EmpDepartment = c.Department,
                                            PFLoanID = b.PFLoanID,
                                            Amount = b.Installment,
                                            InstallmentNumber = a.InstallmentNumber,
                                            PaymentStatus = a.Processed != 0,
                                            PaymentDate = a.PaymentDate ?? DateTime.MinValue,
                                            PaymentAmount = a.PaymentAmount ?? 0,
                                            ConMonth = a.ConMonth,
                                            ConYear = a.ConYear,
                                            IdentificationNumber = c.IdentificationNumber
                                        }).ToList();
            return v;
        }

        public List<VM_Amortization> GetAmortizationDetail(string pfLoanID)
        {
            var v = from a in context.tbl_PFL_Amortization.Where(w => w.PFLoanID == pfLoanID)
                    join b in context.tbl_Employees on a.EmpID equals b.EmpID

                    select new VM_Amortization
                    {
                        Amount = (double)a.Amount,
                        Balance = (double)a.Balance,
                        EmpID = a.EmpID,
                        EmpName = b.EmpName,
                        EmpDesignation = b.Designation,
                        EmpDepartment = b.Department,
                        InstallmentNumber = a.InstallmentNumber,
                        Interest = (double)a.Interest,
                        PaymentDate = a.PaymentDate,
                        PFLoanID = a.PFLoanID,
                        Principal = (double)a.Principal,
                        Processed = a.Processed,
                        ProcessNumber = a.ProcessNumber,
                        TrackingNumber = a.TrackingNumber,
                        ConMonth = a.ConMonth,
                        ConYear = a.ConYear,
                        IdentificationNumber = b.IdentificationNumber
                    };
            return v.ToList();
        }

        public List<VM_PFLoan> EmpLoanInfoList(List<int> empIdList, int oCode)
        {
            var v = from a in context.tbl_Employees.Where(w => empIdList.Contains(w.EmpID))
                    join b in context.tbl_PFLoan on a.EmpID equals b.EmpID
                    select new VM_PFLoan
                    {
                        EmpID = a.EmpID,
                        IdentificationNumber = a.IdentificationNumber,
                        LoanAmount = b.LoanAmount,
                        PFLoanID = b.PFLoanID,
                        Installment = b.Installment,
                        IsApproved = b.IsApproved,
                        CashLedgerID = (Guid)b.CashLedgerID
                    };
            return v.ToList();
        }

        public List<VM_Amortization> AmortizationList(List<int> empIDList, int oCode) // Amortization is actually Loan Detail
        {
            var v = from a in context.tbl_Employees.Where(w => empIDList.Contains(w.EmpID))
                    join b in context.tbl_PFL_Amortization on a.EmpID equals b.EmpID
                    select new VM_Amortization
                    {
                        EmpID = a.EmpID,
                        IdentificationNumber = a.IdentificationNumber,
                        PFLoanID = b.PFLoanID,
                        Processed = b.Processed,
                        InstallmentNumber = b.InstallmentNumber

                    };
            return v.ToList();
        }

        /// <summary>
        /// Amortizations the list with o code.
        /// </summary>
        /// <param name="oCode">The o code.</param>
        /// <returns></returns>
        /// /// <CreatedBy>Fahim</CreatedBy>
        /// <CreatedDate>14/11/2015</CreatedDate>
        public List<VM_Amortization> AmortizationListWithOCode(int oCode) // Amortization is actually Loan Detail
        {
            var v = from b in context.tbl_PFL_Amortization
                    join e in context.tbl_Employees on b.EmpID equals e.EmpID
                    select new VM_Amortization
                    {
                        EmpID = b.EmpID,
                        EmpName = e.EmpName,
                        EmpDepartment = e.Department,
                        EmpDesignation = e.Designation,
                        PFLoanID = b.PFLoanID,
                        Amount = (double)b.Amount,
                        InstallmentNumber = b.InstallmentNumber,
                        PaymentDate = b.PaymentDate

                    };
            //var a = v.ToList();
            return v.OrderBy(s => s.PFLoanID).ToList();
        }
        //End Fahim

        /// <summary>
        /// Amortizations the list.
        /// </summary>
        /// <param name="empID">The emp identifier.</param>
        /// <returns>list</returns>
        /// <CreatedBy>Avishek</CreatedBy>
        /// <CreatedDate>Sep-30-2015</CreatedDate>
        public List<VM_Amortization> AmortizationList(int empID)
        {
            var v = from a in context.tbl_Employees
                    join b in context.tbl_PFL_Amortization on a.EmpID equals b.EmpID
                    where a.EmpID == empID
                    select new VM_Amortization
                    {
                        EmpID = a.EmpID,
                        IdentificationNumber = a.IdentificationNumber,
                        Amount = (double)b.Principal + (double)b.Interest,
                        PFLoanID = b.PFLoanID,
                        Processed = b.Processed,
                        InstallmentNumber = b.InstallmentNumber,
                        Principal = (double)b.Principal,
                        ConYear = b.ConYear,
                        ConMonth = b.ConMonth
                    };
            return v.ToList();
        }

        public List<VM_EmployeeWebUser> GetEmployeeWebUser(string identificationNumber, string name)
        {
            var v = from a in context.tbl_Employees
                    join b in context.tbl_User on a.EmpID equals b.EmpID into leftJoinEmpUser
                    from c in leftJoinEmpUser.DefaultIfEmpty()
                    select new VM_EmployeeWebUser
                    {
                        EmpID = a.EmpID,
                        FullName = a.EmpName,
                        IsActive = c != null && c.IsActive != 0,
                        UserId = c == null ? Guid.Empty : c.UserID,
                        IdentificationNumber = a.IdentificationNumber
                    };
            if (!string.IsNullOrEmpty(identificationNumber))
            {
                v = v.Where(w => w.IdentificationNumber.Contains(identificationNumber));
            }
            if (!string.IsNullOrEmpty(name))
            {
                v = v.Where(w => w.FullName.Contains(name));
            }
            return v.ToList();
        }

        //Modified by Avishek Date:Jul-1-2015
        //Region that when employee id has it's search after get list
        //Start 
        public List<tbl_Contribution> ValidContributionDetail(int EmpID = 0)
        {
            List<tbl_Contribution> result;
            if (EmpID > 0)
            {
                result = (from a in context.tbl_ContributionMonthRecord.Where(f => f.PassVoucher == true)
                          join b in context.tbl_Contribution
                          on new { a.ConYear, a.ConMonth } equals new { b.ConYear, b.ConMonth }
                          where b.EmpID == EmpID
                          select b).ToList();
            }
            else
            {
                result = (from a in context.tbl_ContributionMonthRecord.Where(f => f.PassVoucher == true)
                          join b in context.tbl_Contribution
                          on new { a.ConYear, a.ConMonth } equals new { b.ConYear, b.ConMonth }
                          select b).ToList();
            }
            return result.ToList();
        }
        //End

        //Added by Asif Date:Jul-26-2016
        //Start 
        public List<tbl_Contribution> ValidContributionDetailWithoutVoucher(int EmpID = 0)
        {
            List<tbl_Contribution> result;
            if (EmpID > 0)
            {
                result = (from a in context.tbl_Contribution
                          where a.EmpID == EmpID
                          select a).ToList();
            }
            else
            {
                result = (from a in context.tbl_Contribution
                          select a).ToList();
            }
            return result.ToList();
        }
        //End

        //Added by  Masud 19 September, 2017
        public decimal GetForfeitureAmount(Guid ledgerId, int voucherId)
        {
            SqlParameter ledger = new SqlParameter("@ledgerId", ledgerId);
            SqlParameter voucher = new SqlParameter("@voucherId", voucherId);
            //string cs = "data source=192.168.0.17;initial catalog=EcoTexFinal;user id=sa;password=ssl@1234";


            string cs = "";

            if (DLL.Utility.ApplicationSetting.DbBackUpConnection != null)
            {
                cs = ApplicationSetting.DbBackUpConnection;
            }




            decimal amount = 0.0M;
            using (SqlConnection con = new SqlConnection(cs))
            {
                SqlCommand com = new SqlCommand();
                com.Connection = con;
                com.CommandText = "spGetForfeitureAmount";
                com.CommandType = CommandType.StoredProcedure;
                com.Parameters.Add(voucher);
                com.Parameters.Add(ledger);
                SqlDataReader reader = null;
                con.Open();
                try
                {
                    reader = com.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            amount = Convert.ToDecimal(reader[0]);
                        }
                    }
                }
                finally
                {
                    reader.Close();
                    con.Close();
                }

            }

            return amount;
        }

        public List<VM_forfeiture> GetForfeitureAmount()
        {

            string cs = "data source=192.168.0.17;initial catalog=EcoTexFinal;user id=sa;password=ssl@1234";
            List<VM_forfeiture> forfeitures = new List<VM_forfeiture>();
            using (SqlConnection con = new SqlConnection(cs))
            {
                SqlCommand com = new SqlCommand();
                com.Connection = con;
                com.CommandText = "spGetForfeitureAmountAll";
                com.CommandType = CommandType.StoredProcedure;
                SqlDataReader reader = null;

                try
                {
                    con.Open();
                    reader = com.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            VM_forfeiture forfeiture = new VM_forfeiture();
                            forfeiture.EmpId = Convert.ToInt32(reader["EmpId"]);
                            forfeiture.Amount = Convert.ToDecimal(reader["Amount"]);
                            forfeiture.VoucherId = Convert.ToInt32(reader["VoucherId"]);
                            forfeiture.TransactionDate = Convert.ToDateTime(reader["TransactionDate"]);
                            forfeitures.Add(forfeiture);
                        }
                    }
                }
                finally
                {
                    reader.Close();
                    con.Close();
                }

            }

            return forfeitures;
        }
        //End Masud


        /// <summary>
        /// Valids the contribution detail with transaction.
        /// </summary>
        /// <param name="EmpID">The emp identifier.</param>
        /// <returns></returns>
        public List<tbl_Contribution> ValidContributionDetailWithTransaction(int EmpID = 0)
        {
            var result = from a in context.tbl_ContributionMonthRecord.Where(f => f.PassVoucher == true)
                         join b in context.tbl_Contribution
                         on new { a.ConYear, a.ConMonth } equals new { b.ConYear, b.ConMonth }
                         select b;
            if (EmpID > 0)
            {
                result = result.Where(f => f.EmpID == EmpID);
            }
            return result.ToList();
        }

        //=====================END==========================//
        public void Save()
        {
            context.SaveChanges();
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    context.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public double GetAllDistributableAmount()
        {
            double result = (double)context.tbl_ProfitDistributionSummary.Sum(x => x.DistributableAmount);

            return result;
        }

        public double GetAllDistributedAmount()
        {
            tbl_ProfitDistributionSummary _tbl_ProfitDistributionSummary = context.tbl_ProfitDistributionSummary.FirstOrDefault();
            double result = 0;
            if (_tbl_ProfitDistributionSummary != null)
            {
                result = (double)context.tbl_ProfitDistributionSummary.Sum(x => x.DistributedAmount);
            }
            else
            {
                result = 0;
            }

            return result;
        }

        public List<tbl_Contribution> ContributionDetail(int OCode, DateTime? f, DateTime? t)
        {
            var result = context.tbl_Contribution.Where(x => x.OCode == OCode && x.ContributionDate > f && x.ContributionDate < t).ToList();

            return result.ToList();
        }

        public List<tbl_Employees> GetEmployeeById(int empId)
        {
            List<tbl_Employees> v = context.tbl_Employees.Where(x => x.EmpID == empId).ToList();

            return v;
        }
        public List<LU_tbl_Branch> GetBranchById()
        {
            tbl_Employees em = new tbl_Employees();
            List<LU_tbl_Branch> v = context.LU_tbl_Branch.Where(x => x.BranchID == em.Branch).ToList();

            return v;
        }
        public List<tbl_Employees> GetEmployeeByIdentificationNo(string identificatioNo)
        {
            List<tbl_Employees> v = context.tbl_Employees.Where(x => x.IdentificationNumber == identificatioNo).ToList();

            return v;
        }

        /// <summary>
        /// Gets the user.
        /// </summary>
        /// <param name="empId">The emp identifier.</param>
        /// <param name="name">The name.</param>
        /// <returns>list</returns>
        /// <CreatedBy>Avishek</CreatedBy>
        /// <CreatedDate>Mar-4-2015</CreatedDate>
        public List<VM_EmployeeWebUser> GetUser(int empId, string name)
        {
            try
            {
                var v = from a in context.tbl_Employees
                        join b in context.tbl_User on a.EmpID equals b.EmpID into leftJoinEmpUser
                        from c in leftJoinEmpUser.DefaultIfEmpty()
                        where c.EmpID == empId && c.LoginName == name
                        select new VM_EmployeeWebUser
                        {
                            EmpID = a.EmpID,
                            FullName = a.EmpName,
                            IsActive = c == null ? false : c.IsActive == 0 ? false : true,
                            UserId = c == null ? Guid.Empty : c.UserID,
                            IdentificationNumber = a.IdentificationNumber
                        };
                return v.ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Gets the paymentof employee.
        /// </summary>
        /// <param name="empId">The emp identifier.</param>
        /// <param name="loanId">The loan identifier.</param>
        /// <returns>list</returns>
        /// <CreatedBy>Avishek</CreatedBy>
        /// <CreatedDate>Mar-4-2015</CreatedDate>
        public List<tbl_PFL_Amortization> GetPaymentofEmployee(int empId, string loanId)
        {
            try
            {
                List<tbl_PFL_Amortization> _tbl_Employees;
                if (loanId == "")
                {
                    _tbl_Employees = context.tbl_PFL_Amortization.Where(x => x.EmpID == empId).ToList();
                }
                else
                {
                    _tbl_Employees = context.tbl_PFL_Amortization.Where(x => x.EmpID == empId && x.PFLoanID == loanId).ToList();
                }
                return _tbl_Employees;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        /// <summary>
        /// Gets the name of the ledger by.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>list</returns>
        /// <CreatedBy>Avishek</CreatedBy>
        /// <CreatedDate>Mar-4-2015</CreatedDate>
        public List<acc_Ledger> GetLedgerByName(string name)
        {
            try
            {
                List<acc_Ledger> _acc_Ledger = context.acc_Ledger.Where(x => x.LedgerName == name).ToList();
                return _acc_Ledger;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Profits the distribution.
        /// </summary>
        /// <param name="empId">The emp identifier.</param>
        /// <returns>List</returns>
        /// <CreatedBy>Avishek</CreatedBy>
        /// <CreatedDate>Mar-07-2015</CreatedBy>
        public List<tbl_ProfitDistributionDetail> GetProfitDistributionByEmpId(int empId)
        {
            try
            {
                List<tbl_ProfitDistributionDetail> _tbl_ProfitDistributionDetail = context.tbl_ProfitDistributionDetail.Where(x => x.EmpID == empId).ToList();
                return _tbl_ProfitDistributionDetail;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Gets the employee by identification number.
        /// </summary>
        /// <param name="identificationNumber">The identification number.</param>
        /// <returns>liat</returns>
        /// <createdby>Avishek</createdby>
        /// <createdDate>Apr-8-2015</createdDate>
        public List<tbl_Employees> GetEmployeeByIdentificationNumber(string identificationNumber)
        {
            try
            {
                List<tbl_Employees> v = context.tbl_Employees.Where(x => x.IdentificationNumber == identificationNumber).ToList();
                return v;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Gets the current contributions.
        /// </summary>
        /// <param name="empId">The emp identifier.</param>
        /// <returns>liat</returns>
        /// <createdby>Avishek</createdby>
        /// <createdDate>Apr-8-2015</createdDate>
        public List<tbl_Contribution> GetCurrentContributions(int empId)
        {
            try
            {
                List<tbl_Contribution> v = context.tbl_Contribution.Where(x => x.EmpID == empId).ToList();
                return v;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Employees the with loan.
        /// </summary>
        /// <param name="oCode">The o code.</param>
        /// <param name="identificationNo">The identification no.</param>
        /// <returns>object</returns>  
        /// <createdby>Avishek</createdby>
        /// <createdDate>May-11-2015</createdDate>
        public List<VM_PFLoanPayment> EmployeeWithLoan(int oCode, string identificationNo)
        {
            try
            {
                List<VM_PFLoanPayment> v = (from emp in context.tbl_Employees
                                            join amo in context.tbl_PFL_Amortization
                                            on emp.EmpID equals amo.EmpID
                                            where emp.IdentificationNumber.Contains(identificationNo)
                                            select new VM_PFLoanPayment
                                            {
                                                LoanID = amo.PFLoanID,
                                                IdentificationNumber = emp.IdentificationNumber,
                                                EmpID = emp.EmpID,
                                                EmpName = emp.EmpName //Added by Fahim 22/11/2015
                                            }).ToList();
                return v;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Employees the with loan.
        /// </summary>
        /// <param name="oCode">The o code.</param>
        /// <param name="identificationNo">The identification no.</param>
        /// <returns>object</returns>  
        /// <createdby>Avishek</createdby>
        /// <createdDate>May-13-2015</createdDate>
        public List<VM_PFLoanPayment> EmployeeWithLoanByLoanId(int oCode, string loanid)
        {
            try
            {
                List<VM_PFLoanPayment> v = (from emp in context.tbl_Employees
                                            join amo in context.tbl_PFL_Amortization
                                            on emp.EmpID equals amo.EmpID
                                            where amo.PFLoanID.Contains(loanid) && amo.Processed == 0
                                            select new VM_PFLoanPayment
                                            {
                                                LoanID = amo.PFLoanID,
                                                IdentificationNumber = emp.IdentificationNumber
                                            }).ToList();
                return v;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Employees the with loan.
        /// </summary>
        /// <param name="oCode">The o code.</param>
        /// <param name="identificationNo">The identification no.</param>
        /// <returns>object</returns>  
        /// <createdby>Avishek</createdby>
        /// <createdDate>May-13-2015</createdDate>
        public List<tbl_PFL_Amortization> GetPaymentof(string empId, string loanId)
        {
            try
            {
                List<tbl_PFL_Amortization> _tbl_PFL_Amortization;
                int emp = context.tbl_Employees.Where(x => x.IdentificationNumber == empId).Select(x => x.EmpID).FirstOrDefault();
                _tbl_PFL_Amortization = loanId == "" ? context.tbl_PFL_Amortization.Where(x => x.EmpID == emp).ToList() : context.tbl_PFL_Amortization.Where(x => x.EmpID == emp && x.PFLoanID == loanId).ToList();
                return _tbl_PFL_Amortization;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Emp Loan Info.
        /// </summary>
        /// <param name="loanID" name="employeeID">The loanID.</param>
        /// <param name="identificationNo">The identification no.</param>
        /// <returns>object</returns>  
        /// <createdby>Avishek</createdby>
        /// <createdDate>May-17-2015</createdDate>
        public List<tbl_PFLoan> EmpLoanInfo(int employeeID, string loanID)
        {
            try
            {
                List<tbl_PFLoan> v = context.tbl_PFLoan.Where(x => x.EmpID == employeeID && x.PFLoanID == loanID).ToList();
                return v;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Emps the with loan automatic complete.
        /// </summary>
        /// <param name="identificationNo">The identification no.</param>
        /// <param name="loanNo">The loan no.</param>
        /// <returns>list</returns>
        /// <CreatedBy>Avishek</CreatedBy>
        /// <CreatedDate>Jun-30-2015</CreatedDate>
        public List<VM_PFLoan> EmpWithLoanAutoComplete(string identificationNo = "", string loanNo = "")
        {
            List<VM_PFLoan> v = new List<VM_PFLoan>();
            if (loanNo != "")
            {
                v = (from a in context.tbl_PFLoan.Where(x => x.PFLoanID.ToLower().Trim().Contains(loanNo.ToLower().Trim()))
                     join b in context.tbl_Employees on a.EmpID equals b.EmpID
                     where a.IsApproved == 1
                     group a by new
                     {
                         a.PFLoanID,
                         b.IdentificationNumber,
                         b.EmpName,
                         b.EmpID
                     } into gcs
                     select new VM_PFLoan()
                     {
                         IdentificationNumber = gcs.Key.IdentificationNumber,
                         PFLoanID = gcs.Key.PFLoanID,
                         EmpID = gcs.Key.EmpID,
                         EmpName = gcs.Key.EmpName
                     }).ToList();
            }
            else
            {
                v = (from a in context.tbl_PFLoan
                     join b in context.tbl_Employees.Where(x => x.IdentificationNumber.ToLower().Trim().Contains(identificationNo.ToLower().Trim())) on a.EmpID equals b.EmpID
                     where a.IsApproved == 1
                     group a by new
                     {
                         a.PFLoanID,
                         b.IdentificationNumber,
                         b.EmpName,
                         b.EmpID
                     } into gcs
                     select new VM_PFLoan()
                     {
                         IdentificationNumber = gcs.Key.IdentificationNumber,
                         PFLoanID = gcs.Key.PFLoanID,
                         EmpID = gcs.Key.EmpID,
                         EmpName = gcs.Key.EmpName
                     }).ToList();
            }
            return v;
        }

        /// <summary>
        /// Gets the closed loan for report.
        /// </summary>
        /// <param name="OCode">The o code.</param>
        /// <returns>List</returns>
        /// <CreatedBy>Avishek</CreatedBy>
        /// <CreatedDate>Jun-30-2015</CreatedDate>
        public IQueryable<VM_PFLoan> GetClosedLoanForReport(int OCode)
        {
            var v = from b in context.tbl_PFLoan
                    join e in context.tbl_Employees on b.EmpID equals e.EmpID
                    join d in context.tbl_User on b.ApprovedBy equals d.UserID
                    join c in
                        (
                             from a in context.tbl_PFL_Amortization
                             group a by a.PFLoanID into grp
                             where grp.Count(c => c.Processed == 0) < 1
                             select new
                             {
                                 PFLoanID = grp.Key,
                             }
                         )
                         on b.PFLoanID equals c.PFLoanID
                    select new VM_PFLoan
                    {
                        EmpID = b.EmpID,
                        EmpName = e.EmpName,
                        EmpDepartment = e.Department,
                        EmpDesignation = e.Designation,
                        PFLoanID = b.PFLoanID,
                        Interest = b.Interest,
                        LoanAmount = b.LoanAmount,
                        TermMonth = b.TermMonth,
                        StartDate = b.StartDate,
                        Installment = b.Installment,
                        IsApproved = b.IsApproved,
                        ApprovedById = b.ApprovedBy,
                        ApprovedByName = d.LoginName,
                        IdentificationNumber = e.IdentificationNumber
                    };
            return v;
        }

        /// <summary>
        /// Gets the loan by emp identifier.
        /// </summary>
        /// <param name="empId">The emp identifier.</param>
        /// <returns>List</returns>
        /// <CreatedBy>Avishek</CreatedBy>
        /// <CreaytedDate>Jul-1-2015</CreaytedDate>
        public List<tbl_PFL_Amortization> GetUnpaidLoanByEmpId(int empId)
        {
            List<tbl_PFL_Amortization> tblPflAmortizationList = context.tbl_PFL_Amortization.Where(x => x.EmpID == empId).ToList();
            return tblPflAmortizationList;
        }

        /// <summary>
        /// Gets the pf loan history by emp identifier.
        /// </summary>
        /// <param name="ocode">The ocode.</param>
        /// <param name="fromDate">From date.</param>
        /// <param name="toDate">To date.</param>
        /// <returns>list</returns>
        /// <CreatedBy>Avishek</CreatedBy>
        /// <CreatedDate>Aug-1-2015</CreatedDate>
        public IQueryable<VM_PFLoan> GetPFLoanHistoryByEmpIDandDate(int ocode, DateTime fromDate, DateTime toDate)
        {
            var v = from a in context.tbl_PFLoan
                    where a.OCode == ocode
                    join c in context.tbl_Employees on a.EmpID equals c.EmpID
                    join b in context.tbl_User on a.ApprovedBy equals b.UserID into leftJoinApprover
                    from bb in leftJoinApprover.DefaultIfEmpty()
                    select new VM_PFLoan
                    {
                        ApprovedById = bb == null ? Guid.Empty : bb.UserID,
                        ApprovedByName = bb == null ? "" : bb.LoginName,
                        IsApproved = a.IsApproved,
                        EmpID = a.EmpID,
                        Installment = a.Installment,
                        Interest = a.Interest,
                        LoanAmount = a.LoanAmount,
                        PFLoanID = a.PFLoanID,
                        StartDate = a.StartDate,
                        TermMonth = a.TermMonth,
                        IdentificationNumber = c.IdentificationNumber,
                        PayableAmount = a.PayableAmount ?? 0,
                        RuleUsed = a.RuleUsed ?? 0
                    };
            return v;
        }

        /// <summary>
        /// Gets the active employee.
        /// </summary>
        /// <param name="OCode">The o code.</param>
        /// <param name="toDate">To date.</param>
        /// <returns>List</returns>
        /// <createdBy>Avishek</createdBy>
        /// <CreatedDate>Aug-3-2015</CreatedDate>
        public IQueryable<VM_Employee> GetActiveEmployee(int OCode, DateTime toDate)
        {
            var result = from a in context.tbl_Employees.Where(o => o.OCode == OCode)
                         join d in context.LU_tbl_Designation on a.Designation equals d.DesignationID into LeftJoinDsg
                         from dd in LeftJoinDsg.DefaultIfEmpty()
                         //where a.PFStatus == 1 && a.PFActivationDate <= toDate && a.PFActivationDate >= DateTime.MinValue
                         where a.PFStatus == 1 && a.JoiningDate <= toDate && a.JoiningDate >= DateTime.MinValue


                         select new VM_Employee
                         {
                             BirthDate = a.BirthDate,
                             ContactNumber = a.ContactNumber,
                             DepartmentID = a.Department,
                             DesignationName = dd.DesignationName,
                             BranchID = a.Branch ?? 0,
                             DesignationID = a.Designation,
                             EditDate = a.EditDate,
                             EditUser = a.EditUser,
                             EditUserName = a.EditUserName,
                             Email = a.Email,
                             EmpID = a.EmpID,
                             IdentificationNumber = a.IdentificationNumber,
                             EmpName = a.EmpName,
                             Gender = a.Gender == null ? "" : (a.Gender == "0" ? "Female" : "Male"),
                             JoiningDate = a.JoiningDate,
                             PresentAddress = a.PresentAddress,
                             NID = a.NID,
                             PFStatusID = a.PFStatus ?? 0,
                             EmpImg = a.EmpImg,
                             PFActivationDate = a.PFActivationDate,
                             PFDeactivationDate = a.PFDeactivationDate,
                             opOwnContribution = a.opOwnContribution ?? 0,
                             opEmpContribution = a.opEmpContribution ?? 0,
                             opLoan = a.opLoan ?? 0,
                             opProfit = a.opProfit ?? 0,
                             opDepartmentName = a.opDepartmentName,
                             opDesignationName = a.opDesignationName,
                             Comment = a.Comment,
                             PassVoucher = a.PassVoucher,
                             PassVoucherMessage = a.PassVoucherMessage,
                             PFDeactivationVoucherID = a.PFDeactivationVoucherID ?? 0,
                             OCode = a.OCode ?? 0
                         };

            return result;
        }

        /// <summary>
        /// Emps the pf monthly status.
        /// </summary>
        /// <param name="oCode">The o code.</param>
        /// <param name="empId">The emp identifier.</param>
        /// <returns>list</returns>
        /// <CreatedBy>Avishek</CreatedBy>
        /// <CreatedDate>Aug-17-2015</CreatedDate>
        public IQueryable<VM_PFMonthlyStatus> EmpPFMonthlyStatusForReport(int oCode, int empId, DateTime fromDate, DateTime toDate)
        {
            var v = from a in context.tbl_Contribution.Where(x => x.OCode == oCode && x.ContributionDate >= fromDate && x.ContributionDate <= toDate)

                    join b in context.tbl_Employees.Where(x => x.EmpID == empId) on a.EmpID equals b.EmpID
                    //Avishek Date:Feb-18-2015
                    
                    select new VM_PFMonthlyStatus
                    {
                        EmpID = a.EmpID,
                        IdentificationNumber = b.IdentificationNumber,//Modified by Avishek Date:Feb-18-2015 Reason that show IdentificationNumber in report IdentificationNumber
                        ProcessRunDate = a.ProcessDate,
                        Branch="",
                        ContrebutionDate = (DateTime)a.ContributionDate,
                        EmpContribution = (decimal?)a.EmpContribution ?? 0,
                        Month = a.ConMonth, //DateTime.ParseExact("13/" + a.ConMonth + "/" + a.ConYear, "dd/MM/yyyy", CultureInfo.InvariantCulture)+"",
                        Year = a.ConYear,
                        SelfContribution = (decimal?)a.SelfContribution ?? 0,
                        SCInterest = a.SCInterest ?? 0,
                        ECInterest = a.ECInterest ?? 0
                    };
            return v;
        }
        public IQueryable<VM_PFMonthlyStatus> EmpPFMonthlyStatusForReportOpening(int oCode, int empId, DateTime fromDate, DateTime toDate)
        {
            var v = from a in context.tbl_Contribution.Where(x => x.OCode == oCode && x.ContributionDate <= fromDate)

                    join b in context.tbl_Employees.Where(x => x.EmpID == empId) on a.EmpID equals b.EmpID
                    //Avishek Date:Feb-18-2015

                    select new VM_PFMonthlyStatus
                    {
                        EmpID = a.EmpID,
                        IdentificationNumber = b.IdentificationNumber,//Modified by Avishek Date:Feb-18-2015 Reason that show IdentificationNumber in report IdentificationNumber
                        ProcessRunDate = a.ProcessDate,
                        Branch = "",
                        ContrebutionDate = (DateTime)a.ContributionDate,
                        EmpContribution = (decimal?)a.EmpContribution ?? 0,
                        Month = a.ConMonth, //DateTime.ParseExact("13/" + a.ConMonth + "/" + a.ConYear, "dd/MM/yyyy", CultureInfo.InvariantCulture)+"",
                        Year = a.ConYear,
                        SelfContribution = (decimal?)a.SelfContribution ?? 0,
                        SCInterest = a.SCInterest ?? 0,
                        ECInterest = a.ECInterest ?? 0
                    };
            return v;
        }
        public IQueryable<VM_PFMonthlyStatus> EmpPFMonthlyStatusForReportWithBranch(int oCode, int empId, DateTime fromDate, DateTime toDate)
        {
            var v = from a in context.tbl_Contribution.Where(x => x.OCode == oCode && x.ContributionDate >= fromDate && x.ContributionDate <= toDate)

                    join b in context.tbl_Employees.Where(x => x.EmpID == empId) on a.EmpID equals b.EmpID
                    join br in context.LU_tbl_Branch.Where(x => x.OCode == oCode) on b.Branch equals br.BranchID //Added By Avishek Date:Feb-18-2015

                    select new VM_PFMonthlyStatus
                    {
                        EmpID = a.EmpID,
                        IdentificationNumber = b.IdentificationNumber,//Modified by Avishek Date:Feb-18-2015 Reason that show IdentificationNumber in report IdentificationNumber
                        ProcessRunDate = a.ProcessDate,
                        Branch = br.BranchName,
                        ContrebutionDate = (DateTime)a.ContributionDate,
                        EmpContribution = (decimal?)a.EmpContribution ?? 0,
                        Month = a.ConMonth, //DateTime.ParseExact("13/" + a.ConMonth + "/" + a.ConYear, "dd/MM/yyyy", CultureInfo.InvariantCulture)+"",
                        Year = a.ConYear,
                        SelfContribution = (decimal?)a.SelfContribution ?? 0,
                        SCInterest = a.SCInterest ?? 0,
                        ECInterest = a.ECInterest ?? 0
                    };
            return v;
        }
        /// <summary>
        /// Emps the loan history.
        /// </summary>
        /// <param name="empId">The emp identifier.</param>
        /// <returns>list</returns>
        /// <CreatedBy>Avishek</CreatedBy>
        /// <CreatedDate>Sep-30-2015</CreatedDate>
        public List<VM_PFLoan> EmpLoanHistory(int empId)
        {
            return (from a in context.tbl_PFLoan.Where(x => x.IsApproved == 1) select new VM_PFLoan { EmpID = a.EmpID, PFLoanID = a.PFLoanID, LoanAmount = a.LoanAmount, PayableAmount = a.PayableAmount ?? 0 }).ToList();
        }

        /// <summary>
        /// Gets the loan by identifier.
        /// </summary>
        /// <param name="loanID">The loan identifier.</param>
        /// <returns>object</returns>
        /// <CreatedBy>Avishek</CreatedBy>
        /// <CreatedDate>Nov-11-2015</CreatedDate>
        public List<VM_PFLoan> GetLoanByID(string loanID)
        {
            return (from a in context.tbl_PFLoan.Where(s => s.PFLoanID == loanID)
                    join b in context.tbl_Employees
                         on a.EmpID equals b.EmpID
                    select new VM_PFLoan { EmpID = b.EmpID, EmpName = b.EmpName }).ToList();
        }

        /// <summary>
        /// Gets the name of the employee by.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>list</returns>
        /// <CreatedBy>Avishek</CreatedBy>
        /// <CreatedDate>Nov-11-2015</CreatedDate>
        public List<VM_Employee> GetEmployeeByName(string name)
        {
            return (from a in context.tbl_PFLoan
                    join b in context.tbl_Employees.Where(s => s.EmpName.Trim().Contains(name.Trim()))
                         on a.EmpID equals b.EmpID
                    select new VM_Employee { EmpID = b.EmpID, EmpName = b.EmpName, LoanId = a.PFLoanID, IdentificationNumber = b.IdentificationNumber }).ToList();
        }

        //Added by Fahim // Created by Avishek 23/11/2015
        public List<VM_Amortization> GetLoanAmortizationDetail(int ocode)
        {
            var v = from lon in context.tbl_PFLoan.Where(x => x.OCode == ocode)
                    join a in context.tbl_PFL_Amortization
                    on lon.PFLoanID equals a.PFLoanID
                    join b in context.tbl_Employees
                    on a.EmpID equals b.EmpID

                    select new VM_Amortization
                    {
                        Amount = (double)a.Amount,
                        Balance = (double)a.Balance,
                        EmpID = a.EmpID,
                        EmpName = b.EmpName,
                        InstallmentNumber = a.InstallmentNumber,
                        Interest = (double)a.Interest,
                        PaymentDate = a.PaymentDate,
                        PFLoanID = a.PFLoanID,
                        Principal = (double)a.Principal,
                        Processed = a.Processed,
                        ProcessNumber = a.ProcessNumber,
                        TrackingNumber = a.TrackingNumber,
                        ConMonth = a.ConMonth,
                        ConYear = a.ConYear,
                        IdentificationNumber = b.IdentificationNumber
                    };
            return v.ToList();
        }

        /// <summary>
        /// Gets the name of the employee by.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>list</returns>
        /// <CreatedBy>Avishek</CreatedBy>
        /// <CreatedDate>Dec-8-2015</CreatedDate>
        public List<tbl_PFL_Amortization> GetAmortizationDetailById(int empID)
        {
            return context.tbl_PFL_Amortization.Where(x => x.EmpID == empID).ToList();
        }

        /// <summary>
        /// Gets the loan by identifier no.
        /// </summary>
        /// <param name="identificationNo">The identification no.</param>
        /// <param name="loanNo">The loan no.</param>
        /// <returns>list</returns>
        /// <CreatedBy>Avishek</CreatedBy>
        /// <CreatedDate>Nov-11-2015</CreatedDate>
        public List<VM_PFLoan> GetLoanByIdNo(string identificationNo = "", string loanNo = "")
        {
            List<VM_PFLoan> v = new List<VM_PFLoan>();
            if (identificationNo == "" || loanNo == "")
            {
                v = (from b in context.tbl_PFLoan
                     join e in context.tbl_Employees on b.EmpID equals e.EmpID
                     join d in context.tbl_User on b.ApprovedBy equals d.UserID
                     join c in
                         (
                              from a in context.tbl_PFL_Amortization
                              group a by a.PFLoanID into grp
                              where grp.Count(c => c.Processed == 0) < 1
                              select new
                              {
                                  PFLoanID = grp.Key,
                              }
                          )
                          on b.PFLoanID equals c.PFLoanID
                     select new VM_PFLoan
                     {
                         EmpID = b.EmpID,
                         PFLoanID = b.PFLoanID,
                         EmpName = e.EmpName,
                         IdentificationNumber = e.IdentificationNumber
                     }).ToList();
            }
            else
            {
                v = (from b in context.tbl_PFLoan
                     join e in context.tbl_Employees on b.EmpID equals e.EmpID
                     join d in context.tbl_User on b.ApprovedBy equals d.UserID
                     join c in
                         (
                              from a in context.tbl_PFL_Amortization
                              group a by a.PFLoanID into grp
                              where grp.Count(c => c.Processed == 0) < 1
                              select new
                              {
                                  PFLoanID = grp.Key,
                              }
                          )
                          on b.PFLoanID equals c.PFLoanID
                     where e.IdentificationNumber == identificationNo && b.PFLoanID == loanNo
                     select new VM_PFLoan
                     {
                         EmpID = b.EmpID,
                         PFLoanID = b.PFLoanID,
                         EmpName = e.EmpName,
                         IdentificationNumber = e.IdentificationNumber
                     }).ToList();
            }
            return v;
        }

        //Added by Fahim 20/12/2015
        public List<VM_MIS> GetAllMIS()
        {
            List<VM_MIS> v = (from a in context.acc_Chart_of_Account_Maping
                              join b in context.acc_Ledger on a.Ledger_Id equals b.LedgerID
                              join c in context.acc_tbl_MIS on a.MIS_Id equals c.id
                              select new VM_MIS
                              {
                                  id = a.MIS_Id,
                                  MISName = c.MIS_Name,
                                  LedgerName = b.LedgerName
                              }).OrderBy(o => o.MISName).ToList();
            return v;
        }
        //End Fahim

        ////Added by Fahim 20/12/2015
        public List<VM_MIS> GetMISList()
        {
            List<VM_MIS> v = (from a in context.acc_tbl_MIS
                              select new VM_MIS
                              {
                                  id = a.id,
                                  MISName = a.MIS_Name
                              }).OrderBy(o => o.id).ToList();

            return v;
        }

        ////End Fahim
        ////Added by Fahim 20/12/2015
        public List<VM_acc_Chart_Of_Account_Mapping> GetChartAccountMappingList()
        {
            List<VM_acc_Chart_Of_Account_Mapping> v = (from a in context.acc_Chart_of_Account_Maping
                                                       join b in context.acc_Ledger on a.Ledger_Id equals b.LedgerID
                                                       join c in context.acc_tbl_MIS on a.MIS_Id equals c.id
                                                       select new VM_acc_Chart_Of_Account_Mapping
                                                       {
                                                           id = a.id,
                                                           MIS_Id = a.MIS_Id,
                                                           Ledger_Id = a.Ledger_Id,
                                                           DateOfEntry = a.DateOfEntry,
                                                           EntryBy = a.EntryBy,
                                                           OCode = a.OCode,
                                                           LedgerName = b.LedgerName,
                                                           MISName = c.MIS_Name
                                                       }).OrderBy(o => o.id).ToList();
            return v;
        }
        ////End Fahim

        //Added by Fahim 20/12/2015
        /// <summary>
        /// Gets the ledger list.
        /// </summary>
        /// <returns></returns>
        public List<VM_acc_ledger> GetLedgerList()
        {
            List<VM_acc_ledger> v = (from a in context.acc_Ledger
                                     select new VM_acc_ledger
                                     {
                                         LedgerID = a.LedgerID,
                                         LedgerName = a.LedgerName
                                     }).OrderBy(o => o.LedgerName).ToList();
            return v;
        }
        //End Fahim

        ////Added by Fahim 20/12/2015
        //public List<VM_acc_Chart_Of_Account_Mapping> GetChartAccountMappingList(int id)
        public VM_acc_Chart_Of_Account_Mapping GetChartAccountMappingList(int id)
        {
            VM_acc_Chart_Of_Account_Mapping v = (from a in context.acc_Chart_of_Account_Maping
                                                 join b in context.acc_tbl_MIS on a.MIS_Id equals b.id
                                                 join c in context.acc_Ledger on a.Ledger_Id equals c.LedgerID
                                                 where a.id == id
                                                 select new VM_acc_Chart_Of_Account_Mapping
                                                 {
                                                     id = a.id,
                                                     Ledger_Id = a.Ledger_Id,
                                                     MIS_Id = a.MIS_Id,
                                                     LedgerName = c.LedgerName,
                                                     MISName = b.MIS_Name,
                                                     DateOfEntry = a.DateOfEntry,
                                                     EntryBy = a.EntryBy,
                                                     OCode = a.OCode
                                                 }).FirstOrDefault();
            return v;
        }
        ////End Fahim

        //Added by Fahim 21/12/2015
        public acc_Chart_of_Account_Maping Get_acc_Chart_of_Account_Mapping(VM_acc_Chart_Of_Account_Mapping v)
        {
            acc_Chart_of_Account_Maping accChartOfAccountMapingObject = new acc_Chart_of_Account_Maping();
            accChartOfAccountMapingObject.id = v.id;
            accChartOfAccountMapingObject.Ledger_Id = v.Ledger_Id;
            accChartOfAccountMapingObject.MIS_Id = v.MIS_Id;
            accChartOfAccountMapingObject.OCode = v.OCode;
            accChartOfAccountMapingObject.MIS_Id = v.MIS_Id;
            accChartOfAccountMapingObject.DateOfEntry = v.DateOfEntry;
            accChartOfAccountMapingObject.EntryBy = v.EntryBy;
            return accChartOfAccountMapingObject;
        }
        //End Fahim
        #endregion
        /// All Loan list
        /// </summary>
        /// <param name="oCode"></param>
        /// <AddedBy>Shohid</AddedBy>
        /// <date>Aug 23 2016</date>
        /// <returns></returns>
        public List<VM_PFLoanPayment> AllLoan(int oCode)
        {
            //get all employee loan list BUT NOT GETTIN CURRENT SAVINGS FOR EACH EMPLOYEE
            var v = (from a in context.tbl_PFL_Amortization
                     //where a.Processed == 0//Convert.ToInt32(a.ConMonth) <= conMonth && Convert.ToInt32(a.ConYear) <= conYear
                     join b in context.tbl_PFLoan on new { a.EmpID, a.PFLoanID } equals new { b.EmpID, b.PFLoanID }
                     join c in context.tbl_Employees on b.EmpID equals c.EmpID
                     where a.OCode == oCode
                     select new VM_PFLoanPayment
                     {
                         EmpID = b.EmpID,
                         EmpName = c.EmpName,
                         EmpDesignation = c.Designation,
                         EmpDepartment = c.Department,
                         PFLoanID = b.PFLoanID,
                         Amount = b.Installment,
                         InstallmentNumber = a.InstallmentNumber,
                         PaymentStatus = a.Processed != 0 ? true : false,
                         PaymentDate = a.PaymentDate ?? DateTime.MinValue,
                         PaymentAmount = a.PaymentAmount ?? 0,
                         ConMonth = a.ConMonth,
                         ConYear = a.ConYear,
                         IdentificationNumber = c.IdentificationNumber,
                     }).ToList();

            //  v = v.Where(w => Convert.ToInt32(w.ConMonth) <= conMonth && Convert.ToInt32(w.ConYear) <= conYear).ToList();

            return v;
        }
        #region PF Payment Receive account
        //Added By Kamrul Hasa 2019-02-10
        public List<VMPaymentReceiveStatement> spPrepareReceiveStatement(DateTime? fromDate, DateTime? toDate, ref decimal openingBalance)
        {
            PFTMEntities context = new PFTMEntities();
            SqlParameter fDate = new SqlParameter("@fromDate", fromDate);
            SqlParameter tDate = new SqlParameter("@toDate", toDate);
            SqlParameter inititalBalance = new SqlParameter
            {
                ParameterName = "inititalBalance",
                Value = 0,
                Direction = ParameterDirection.Output
            };
            var data = context.Database.SqlQuery<VMPaymentReceiveStatement>("spPrepareReceiveStatement @fromDate, @toDate, @inititalBalance out", fDate, tDate, inititalBalance).ToList();
            openingBalance = Convert.ToDecimal(inititalBalance.Value);
            return data;
        }

        public List<VMPaymentReceiveStatement> spPreparePaymentStatement(DateTime? fromDate, DateTime? toDate)
        {
            PFTMEntities context = new PFTMEntities();
            SqlParameter fDate = new SqlParameter("@fromDate", fromDate);
            SqlParameter tDate = new SqlParameter("@toDate", toDate);
            return context.Database.SqlQuery<VMPaymentReceiveStatement>("spPreparePaymentStatement @fromDate, @toDate", fDate, tDate).ToList();
        }
        #endregion
        /// Added By Kamrul Hasan for Yearly Loan Report 2019/06/25
       
        public List<VM_YearlyLoan> sp_EmployeeYearlyLoanDetails(DateTime? fromDate, DateTime? toDate)
        {
            PFTMEntities context = new PFTMEntities();
            SqlParameter fDate = new SqlParameter("@fromDate", fromDate);
            SqlParameter tDate = new SqlParameter("@toDate", toDate);
            return context.Database.SqlQuery<VM_YearlyLoan>("sp_EmployeeYearlyLoanDetails @fromDate, @toDate", fDate, tDate).ToList();
        }
        
        public IEnumerable<VM_PFLoan> sp_GetLoanDetailList(DateTime fromDate, DateTime toDate)
        {
            SqlParameter fDate = new SqlParameter("@fromDate", fromDate);
            SqlParameter tDate = new SqlParameter("@toDate", toDate);
            var result = context.Database.SqlQuery<VM_PFLoan>("sp_GetLoanDetailList @fromDate, @toDate", fDate, tDate).ToList();

            return result;
        }

        public void DeleteVDetailsVEntryLedgInterest(string narration, int instrumentId)
        {

            string cs = "";

            if (DLL.Utility.ApplicationSetting.DbBackUpConnection != null)
            {
                cs = ApplicationSetting.DbBackUpConnection;
            }

            using (SqlConnection con = new SqlConnection(cs))

            using (SqlCommand cmd = new SqlCommand("sp_DeleteVDetailsVEntryLedgInterest", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@narration", SqlDbType.VarChar).Value = narration;
                cmd.Parameters.Add("@instrumentId", SqlDbType.Int).Value = instrumentId;



                con.Open();
                cmd.CommandTimeout = 36000;
                cmd.ExecuteNonQuery();
                con.Close();
            }

        }
        /// 
    }
}
