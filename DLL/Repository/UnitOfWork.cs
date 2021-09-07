using DLL.ViewModel;
using System;

namespace DLL.Repository
{
    public class UnitOfWork : IDisposable
    {
        private PFTMEntities context = new PFTMEntities();

        private GenericRepository<LU_tbl_Country> countryRepository;
        private GenericRepository<LU_tbl_Branch> branchRepository;
        private GenericRepository<LU_tbl_Designation> designationRepositoty;
        private GenericRepository<LU_tbl_Department> departmentRepository;
        private GenericRepository<tbl_Employees> employeesRepository;
        
        
        
        private GenericRepository<tbl_PFLoan> pfLoanRepository;
        private GenericRepository<tbl_PFL_Amortization> amortizationRepository;
        private GenericRepository<LU_tbl_PFRules> pfRulesRepository;
        private GenericRepository<tbl_Contribution> contributionRepository;
        private GenericRepository<tbl_NomineeInformation> nomineeRepository;
        
        private GenericRepository<tbl_LoginHistory> loginHistoryRepository;
        private GenericRepository<tbl_User> userProfileRepository;
        private GenericRepository<SA_tbl_PagePermission> pagePermissionRepository;
        private GenericRepository<tbl_UserPassword> userPasswordRepository;
        private GenericRepository<LU_tbl_MembershipClosingRules> membershipClosingRulesRepository;
        private GenericRepository<LU_tbl_LoanRules> loanRulesRepository;
        private GenericRepository<tbl_ErrorLog> errorLogRepository;

        private GenericRepository<acc_Group> acc_groupRepository;
        private GenericRepository<acc_Ledger> acc_ledgerRepository;
        private GenericRepository<acc_VoucherDetail> acc_voucherDetailRepository;
        private GenericRepository<acc_Nature> acc_natureRepository;
        private GenericRepository<acc_VoucherType> acc_voucherTypeRepository;
        private GenericRepository<acc_VoucherEntry> acc_voucherEntryRepository;
        private GenericRepository<LU_tbl_CompanyInformation> companyInformationRepository;
        private GenericRepository<tbl_Instrument> instrumentRepository;
        private GenericRepository<tbl_ContributionMonthRecord> contributionMonthRecordRepository;
        //Added by Kamrul For Cash Flow and Subsidiary 2019-04-01
        //private GenericRepository<acc_CashFlowGroup> cashFlowGroup;
        //private GenericRepository<acc_CashFlow_Type> cashFlowType;
        //private GenericRepository<acc_CashFlowMapping> cashFlowMapping;
       // private GenericRepository<acc_Subsidiary> acc_subsidiaryRepository;
        //private GenericRepository<acc_SubsidiaryVoucherDetail> acc_subsidiaryVoucherDetailRepository;
        private GenericRepository<Ac_Cheque_Print> checkRepository;
        //End of Addition

        private GenericRepository<tbl_ProfitDistributionSummary> profitDistributionSummaryRepository;
        private GenericRepository<tbl_ProfitDistributionDetail> profitDistributionDetailRepository;

        //Added by Avishek Date:Dec-20-2015
        private GenericRepository<acc_Chart_of_Account_Maping> chartofAccountMapingRepository;
        private GenericRepository<acc_tbl_MIS> mISRepository;
        //End
        //Added by Izab Ahmed Date: Dec-04-2018
        private GenericRepository<tbl_FinancialAuditLog> financialAuditLog;
        //End

        //======================audit log repository====================//
        public GenericRepository<tbl_FinancialAuditLog> AuditLogRepository
        {
            get
            {
                if (this.financialAuditLog == null)
                {
                    this.financialAuditLog = new GenericRepository<tbl_FinancialAuditLog>(context);
                }
                return financialAuditLog;
            }
        }

        
        //======================custom repository====================//
        private SPRepository spRepository;
        public SPRepository SPRepository
        {
            get
            {

                if (this.spRepository == null)
                {
                    this.spRepository = new SPRepository(context);
                }
                return spRepository;
            }
        }
        //====================end of custom repository==================//

        //======================custom repository====================//
        private AccountingRepository accountingRepository;
        public AccountingRepository AccountingRepository
        {
            get
            {

                if (this.accountingRepository == null)
                {
                    this.accountingRepository = new AccountingRepository(context);
                }
                return accountingRepository;
            }
        }
        //====================end of custom repository==================//

        //======================custom repository====================//
        private CustomRepository customRepository;
        public CustomRepository CustomRepository
        {
            get
            {

                if (this.customRepository == null)
                {
                    this.customRepository = new CustomRepository(context);
                }
                return customRepository;
            }
        }
        public GenericRepository<acc_Chart_of_Account_Maping> ChartofAccountMapingRepository1
        {
            get
            {

                if (this.chartofAccountMapingRepository == null)
                {
                    this.chartofAccountMapingRepository = new GenericRepository<acc_Chart_of_Account_Maping>(context);
                }
                return chartofAccountMapingRepository;
            }
        }

        private GenericRepository<tbl_Instrument_Accured_Interest> instrumentAccuredInterestRepository;
        public GenericRepository<tbl_Instrument_Accured_Interest> InstrumentAccuredInterestRepository
        {
            get
            {

                if (this.instrumentAccuredInterestRepository == null)
                {
                    this.instrumentAccuredInterestRepository = new GenericRepository<tbl_Instrument_Accured_Interest>(context);
                }
                return instrumentAccuredInterestRepository;
            }
        }
        //====================end of custom repository==================//

        public GenericRepository<LU_tbl_LoanRules> LoanRulesRepository
        {
            get
            {
                if (this.loanRulesRepository == null)
                {
                    this.loanRulesRepository = new GenericRepository<LU_tbl_LoanRules>(context);
                }
                return loanRulesRepository;
            }
        }
        //====================Start Cash Flow repository==================//


        private GenericRepository<tbl_Instrument_Interest_Rate> instrumentInterestRateRepository;

        public GenericRepository<tbl_Instrument_Interest_Rate> InstrumentInterestRateRepository
        {
            get
            {

                if (this.instrumentInterestRateRepository == null)
                {
                    this.instrumentInterestRateRepository = new GenericRepository<tbl_Instrument_Interest_Rate>(context);
                }
                return instrumentInterestRateRepository;
            }
        }


        //public GenericRepository<acc_CashFlowGroup> ACC_CashFlowGroup
        //{
        //    get
        //    {

        //        if (this.cashFlowGroup == null)
        //        {
        //            this.cashFlowGroup = new GenericRepository<acc_CashFlowGroup>(context);
        //        }
        //        return cashFlowGroup;
        //    }
        //}
        //public GenericRepository<acc_CashFlow_Type> ACC_CashFlowType
        //{
        //    get
        //    {

        //        if (this.cashFlowType == null)
        //        {
        //            this.cashFlowType = new GenericRepository<acc_CashFlow_Type>(context);
        //        }
        //        return cashFlowType;
        //    }
        //}
        private GenericRepository<acc_Group_Maping> _groupMapingRepository;

        public GenericRepository<acc_Group_Maping> GroupMaping
        {
            get
            {

                if (this._groupMapingRepository == null)
                {
                    this._groupMapingRepository = new GenericRepository<acc_Group_Maping>(context);
                }
                return _groupMapingRepository;
            }
        }

        //public GenericRepository<acc_CashFlowMapping> ACC_CashFlowMapping
        //{
        //    get
        //    {

        //        if (this.cashFlowMapping == null)
        //        {
        //            this.cashFlowMapping = new GenericRepository<acc_CashFlowMapping>(context);
        //        }
        //        return cashFlowMapping;
        //    }
        //}

        //public GenericRepository<acc_Subsidiary> ACC_Subsidiary
        //{
        //    get
        //    {

        //        if (this.acc_subsidiaryRepository == null)
        //        {
        //            this.acc_subsidiaryRepository = new GenericRepository<acc_Subsidiary>(context);
        //        }
        //        return acc_subsidiaryRepository;
        //    }
        //}

        //public GenericRepository<acc_SubsidiaryVoucherDetail> ACC_SubsidiaryVoucherDetail
        //{
        //    get
        //    {

        //        if (this.acc_subsidiaryVoucherDetailRepository == null)
        //        {
        //            this.acc_subsidiaryVoucherDetailRepository = new GenericRepository<acc_SubsidiaryVoucherDetail>(context);
        //        }
        //        return acc_subsidiaryVoucherDetailRepository;
        //    }
        //}
        //====================end  repository==================//


        public GenericRepository<tbl_ProfitDistributionSummary> ProfitDistributionSummaryRepository
        {
            get
            {

                if (this.profitDistributionSummaryRepository == null)
                {
                    this.profitDistributionSummaryRepository = new GenericRepository<tbl_ProfitDistributionSummary>(context);
                }
                return profitDistributionSummaryRepository;
            }
        }

        public GenericRepository<tbl_ProfitDistributionDetail> ProfitDistributionDetailRepository
        {
            get
            {

                if (this.profitDistributionDetailRepository == null)
                {
                    this.profitDistributionDetailRepository = new GenericRepository<tbl_ProfitDistributionDetail>(context);
                }
                return profitDistributionDetailRepository;
            }
        }

        public GenericRepository<tbl_ContributionMonthRecord> ContributionMonthRecordRepository
        {
            get
            {

                if (this.contributionMonthRecordRepository == null)
                {
                    this.contributionMonthRecordRepository = new GenericRepository<tbl_ContributionMonthRecord>(context);
                }
                return contributionMonthRecordRepository;
            }
        }


        public GenericRepository<tbl_Contribution> ContributionMonthListRepository
        {
            get
            {

                if (this.contributionRepository == null)
                {
                    this.contributionRepository = new GenericRepository<tbl_Contribution>(context);
                }
                return contributionRepository;
            }
        }


        public GenericRepository<tbl_Instrument> InstrumentRepository
        {
            get
            {

                if (this.instrumentRepository == null)
                {
                    this.instrumentRepository = new GenericRepository<tbl_Instrument>(context);
                }
                return instrumentRepository;
            }
        }

        public GenericRepository<LU_tbl_CompanyInformation> CompanyInformationRepository
        {
            get
            {

                if (this.companyInformationRepository == null)
                {
                    this.companyInformationRepository = new GenericRepository<LU_tbl_CompanyInformation>(context);
                }
                return companyInformationRepository;
            }
        }


        public GenericRepository<acc_VoucherEntry> ACC_VoucherEntryRepository
        {
            get
            {

                if (this.acc_voucherEntryRepository == null)
                {
                    this.acc_voucherEntryRepository = new GenericRepository<acc_VoucherEntry>(context);
                }
                return acc_voucherEntryRepository;
            }
        }


        public GenericRepository<acc_VoucherType> ACC_VoucherTypeRepository
        {
            get
            {

                if (this.acc_voucherTypeRepository == null)
                {
                    this.acc_voucherTypeRepository = new GenericRepository<acc_VoucherType>(context);
                }
                return acc_voucherTypeRepository;
            }
        }

        public GenericRepository<acc_Nature> ACC_NatureRepository
        {
            get
            {

                if (this.acc_natureRepository == null)
                {
                    this.acc_natureRepository = new GenericRepository<acc_Nature>(context);
                }
                return acc_natureRepository;
            }
        }

        public GenericRepository<acc_VoucherDetail> ACC_VoucherDetailRepository
        {
            get
            {

                if (this.acc_voucherDetailRepository == null)
                {
                    this.acc_voucherDetailRepository = new GenericRepository<acc_VoucherDetail>(context);
                }
                return acc_voucherDetailRepository;
            }
        }


        public GenericRepository<acc_Ledger> ACC_LedgerRepository
        {
            get
            {

                if (this.acc_ledgerRepository == null)
                {
                    this.acc_ledgerRepository = new GenericRepository<acc_Ledger>(context);
                }
                return acc_ledgerRepository;
            }
        }


        public GenericRepository<acc_Group> ACC_GroupRepository
        {
            get
            {

                if (this.acc_groupRepository == null)
                {
                    this.acc_groupRepository = new GenericRepository<acc_Group>(context);
                }
                return acc_groupRepository;
            }
        }


        public GenericRepository<tbl_ErrorLog> ErrorLogRepository
        {
            get
            {

                if (this.errorLogRepository == null)
                {
                    this.errorLogRepository = new GenericRepository<tbl_ErrorLog>(context);
                }
                return errorLogRepository;
            }
        }

        public GenericRepository<LU_tbl_MembershipClosingRules> MembershipClosingRulesRepository
        {
            get
            {

                if (this.membershipClosingRulesRepository == null)
                {
                    this.membershipClosingRulesRepository = new GenericRepository<LU_tbl_MembershipClosingRules>(context);
                }
                return membershipClosingRulesRepository;
            }
        }

        public GenericRepository<tbl_UserPassword> UserPasswordRepository
        {
            get
            {

                if (this.userPasswordRepository == null)
                {
                    this.userPasswordRepository = new GenericRepository<tbl_UserPassword>(context);
                }
                return userPasswordRepository;
            }
        }

         public GenericRepository<SA_tbl_PagePermission> PagePermissionRepository
        {
            get
            {

                if (this.pagePermissionRepository == null)
                {
                    this.pagePermissionRepository = new GenericRepository<SA_tbl_PagePermission>(context);
                }
                return pagePermissionRepository;
            }
        }

        public GenericRepository<tbl_User> UserProfileRepository
        {
            get
            {

                if (this.userProfileRepository == null)
                {
                    this.userProfileRepository = new GenericRepository<tbl_User>(context);
                }
                return userProfileRepository;
            }
        }

        public GenericRepository<tbl_LoginHistory> LoginHistoryRepository
        {
            get
            {
                if (this.loginHistoryRepository == null)
                {
                    this.loginHistoryRepository = new GenericRepository<tbl_LoginHistory>(context);
                }
                return loginHistoryRepository;
            }
        }



        public GenericRepository<tbl_NomineeInformation> NomineeRepository
        {
            get
            {
                if (this.nomineeRepository == null)
                {
                    this.nomineeRepository = new GenericRepository<tbl_NomineeInformation>(context);
                }
                return nomineeRepository;
            }
        }

        public GenericRepository<tbl_Contribution> ContributionRepository
        {
            get
            {
                if (this.contributionRepository == null)
                {
                    this.contributionRepository = new GenericRepository<tbl_Contribution>(context);
                }
                return contributionRepository;
            }
        }

        public GenericRepository<LU_tbl_PFRules> PFRulesRepository
        {
            get
            {
                if (this.pfRulesRepository == null)
                {
                    this.pfRulesRepository = new GenericRepository<LU_tbl_PFRules>(context);
                }
                return pfRulesRepository;
            }
        }

        public GenericRepository<tbl_PFL_Amortization> AmortizationRepository
        {
            get
            {
                if (this.amortizationRepository == null)
                {
                    this.amortizationRepository = new GenericRepository<tbl_PFL_Amortization>(context);
                }
                return amortizationRepository;
            }
        }

        public GenericRepository<tbl_PFLoan> PFLoanRepository
        {
            get
            {
                if (this.pfLoanRepository == null)
                {
                    this.pfLoanRepository = new GenericRepository<tbl_PFLoan>(context);
                }
                return pfLoanRepository;
            }
        }

        //public GenericRepository<tbl_PFLoanRequest> PFLoanRequestRepository
        //{
        //    get
        //    {
        //        if (this.pfLoanRequestRepository == null)
        //        {
        //            this.pfLoanRequestRepository = new GenericRepository<tbl_PFLoanRequest>(context);
        //        }
        //        return pfLoanRequestRepository;
        //    }
        //}

   

     

        public GenericRepository<tbl_Employees> EmployeesRepository
        {
            get
            {
                if (this.employeesRepository == null)
                {
                    this.employeesRepository = new GenericRepository<tbl_Employees>(context);
                }
                return employeesRepository;
            }
        }
        public GenericRepository<Ac_Cheque_Print> ChequeRepository
        {
            get
            {
                if (this.checkRepository == null)
                {
                    this.checkRepository = new GenericRepository<Ac_Cheque_Print>(context);
                }
                return ChequeRepository;
            }
        }
        public GenericRepository<LU_tbl_Department> DepartmentRepository
        {
            get
            {
                if (this.departmentRepository == null)
                {
                    this.departmentRepository = new GenericRepository<LU_tbl_Department>(context);
                }
                return departmentRepository;
            }
        }

        public GenericRepository<LU_tbl_Designation> DesignationRepository
        {
            get
            {

                if (this.designationRepositoty == null)
                {
                    this.designationRepositoty = new GenericRepository<LU_tbl_Designation>(context);
                }
                return designationRepositoty;
            }
        }

        public GenericRepository<LU_tbl_Branch> BranchRepository
        {
            get
            {

                if (this.branchRepository == null)
                {
                    this.branchRepository = new GenericRepository<LU_tbl_Branch>(context);
                }
                return branchRepository;
            }
        }

        public GenericRepository<LU_tbl_Country> CountryRepository
        {
            get
            {

                if (this.countryRepository == null)
                {
                    this.countryRepository = new GenericRepository<LU_tbl_Country>(context);
                }
                return countryRepository;
            }
        }

        //Addec By Avishek Date:Dec-20-2015 
        public GenericRepository<acc_Chart_of_Account_Maping> ChartofAccountMapingRepository
        {
            get
            {

                if (this.chartofAccountMapingRepository == null)
                {
                    this.chartofAccountMapingRepository = new GenericRepository<acc_Chart_of_Account_Maping>(context);
                }
                return chartofAccountMapingRepository;
            }
        }

        public GenericRepository<acc_tbl_MIS> MISRepository
        {
            get
            {

                if (this.mISRepository == null)
                {
                    this.mISRepository = new GenericRepository<acc_tbl_MIS>(context);
                }
                return mISRepository;
            }
        }
        //End

        public void Save()
        {
            try
            {
                context.SaveChanges();
            } catch(Exception ex)
            {
                throw ex;
            }
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
    }
}