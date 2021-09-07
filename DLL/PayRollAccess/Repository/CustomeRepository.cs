using DLL.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using DLL.PayRollAccess.ViewModel;

namespace DLL.PayRollAccess.Repository
{
    public class CustomeRepository : IDisposable
    {
        private PREntities pRContext = new PREntities();
        private bool disposed = false;

        public CustomeRepository(PREntities context)
        {
            this.pRContext = context;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    pRContext.Dispose();
                }
            }
            disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Gets all employee.
        /// </summary>
        /// <param name="oCode">The o code.</param>
        /// <returns>list</returns>
        /// <ModifiedBy>Fahim</ModifiedBy>
        /// <DateofModification>Hun-01-2016</DateofModification>
        public IQueryable<VM_Employee> GetAllEmployeeFromPayroll()
        {
            var result = from a in pRContext.HRM_PersonalInformations
                         join b in pRContext.HRM_DEPARTMENTS on a.DepartmentId equals b.DPT_ID
                         join c in pRContext.HRM_Emp_PF_Contribution on a.EID equals c.EID
                         join d in pRContext.HRM_DESIGNATIONS on a.DesginationId equals d.DEG_ID
                         join e in pRContext.HRM_SECTIONS on a.SectionId equals e.SEC_ID

                         select new VM_Employee
                         {
                             IdentificationNumber = a.EID,
                             FirstName = a.FirstName,
                             Lastname = a.LastName,
                             BirthDate = a.DateOfBrith,
                             DesignationName = d.DEG_NAME,
                             DepartmentName = b.DPT_NAME,
                             SectionName = e.SEC_NAME,
                             Gender = a.Gender,
                             PresentAddress = a.PresentAddress,
                             ContactNumber = a.ContactNumber,
                             Email = a.Email,
                             NID = a.NationalID,
                             JoiningDate = a.JoiningDate,
                             Basic = d.BASIC ?? 0
                         };

            return result;
        }

        /// <summary>
        /// Monthlies the contribution save.
        /// </summary>
        /// <param name="conMonth">The con month.</param>
        /// <param name="conYear">The con year.</param>
        /// <returns>list</returns>
        /// <CreatedBy>Avishek</CreatedBy>
        /// <DateofCreation>01-Jun-2016</DateofCreation>
        public List<ViewModel.VM_Salary> MonthlyContribution(string conMonth, string conYear)
        {
            int? conMonthInt = Convert.ToInt32(conMonth);
            int? conYearInt = Convert.ToInt32(conYear);

            try
            {
                List<ViewModel.VM_Salary> contributionSalary = (from con in pRContext.HRM_Emp_PF_Contribution
                                                                where con.PF_Month == conMonthInt
                                                                && con.PF_Year == conYearInt
                                                                && (con.Employee_PF_Contribution != null && con.Employee_PF_Contribution != 0)
                                                                && (con.Employer_PF_Contribution != null && con.Employer_PF_Contribution != 0)
                                                                select new ViewModel.VM_Salary
                           {
                               IdentificationNo = con.EID,
                               ProcessDate = con.EditDate,
                               OwnContribution = con.Employee_PF_Contribution.Value,
                               EmpContribution = con.Employer_PF_Contribution.Value,
                               LoanPrincipal = con.PF_Loan_Amount.Value,
                               LoanInterest = con.PF_Loan_Interest.Value
                           }).ToList();
                return contributionSalary;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<VM_Contribution> MonthlyContributionsFromPayroll()
        {
            List<VM_Contribution> contributionSalary = (from con in pRContext.HRM_Emp_PF_Contribution                                                           
                                                        select new VM_Contribution
                                                            {
                                                                ConYearInt = con.PF_Year ?? 0,
                                                                ConMonthInt = con.PF_Month ?? 0
                                                            }).Distinct().ToList();
            return contributionSalary;
            
        }
    }
}
