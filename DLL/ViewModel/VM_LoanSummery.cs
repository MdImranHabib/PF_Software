using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLL.ViewModel
{
    public class VM_LoanSummery
    {
        /// <summary>
       /// Created For Showing Loan Summery New Report 2019-05-05 by Kamrul
       /// </summary>
            public int EmpID { get; set; }
            public string IdentificationNumber { get; set; }
            public string PFLoanID { get; set; }
            public int ReScheduleID { get; set; }
            public int InstallmentNumber { get; set; }
            public string MonthName { get; set; }
            public decimal Amount { get; set; }
            public decimal Interest { get; set; }
            public decimal Principal { get; set; }
            //public string PaidAmount { get; set; }
            public decimal? PaidAmount { get; set; }

            public DateTime JoiningDate { get; set; }
            public DateTime StartDate { get; set; }
            public decimal InterestRate { get; set; }
       
            public decimal? PrincipalPaid { get; set; }
            public decimal? InterestPaid { get; set; }
            public decimal? PrincipalDue { get; set; }
            public decimal? InterestDue { get; set; }

            public decimal Balance { get; set; }
            public decimal loanAmount { get; set; }

            public double MonthlyPaid { get; set; }
            public short Processed { get; set; }

            public Nullable<System.DateTime> PaymentDate { get; set; }
            public string TrackingNumber { get; set; }
            public int? ProcessNumber { get; set; }
            public string PaymentStatus { get { if (Processed == 1) return "Paid"; else return ""; } }
            public string ConYear { get; set; }
            public string ConMonth { get; set; }
            //Edited by Fahim 22/11/2015
            
            //End Fahim

            public string EmpName { get; set; }

            public string EmpDesignation { get; set; }

            public string EmpDepartment { get; set; }
        }
    
}
