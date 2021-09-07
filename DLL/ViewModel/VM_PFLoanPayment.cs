using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLL.ViewModel
{
    public class VM_PFLoanPayment
    {
        public int EmployeeID { get; set; }
        public string EmpName { get; set; } // Added by Fahim 22/11/2015 
        public string IdentificationNumber { get; set; }
        public string LoanID { get; set; }
        public decimal PaymentAmount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PreImportMessage { get; set; }
        public decimal InstallmentAmount { get; set; }
        public int InstallmentNumber { get; set; }
        public string TrackingNumber { get; set; }
        public decimal CurrentSavings { get; set; }
        public bool PaymentStatus { get; set; }
        public string ConYear { get; set; }
        public string ConMonth { get; set; }
        //created by abdul alim
        public string MonthandYear {get; set; }
        public DateTime convertedDatetime { get; set; }

        public string MonthYear
        {
            get
            {
                if (!string.IsNullOrEmpty(ConMonth) && !string.IsNullOrEmpty(ConYear))
                {
                    return Convert.ToDateTime(ConYear + "/" + ConMonth + "/01").ToString("MMMM, yyyy");
                }
                else
                {
                    return "";
                }
            }
        }
        public decimal PrincipalAmount { get; set; }
        public decimal Interest { get; set; }
        public int EmpID { get; set; }
        public string PFLoanID { get; set; }
        public decimal Amount { get; set; }

        public string EmpDesignation { get; set; }

        public string EmpDepartment { get; set; }
    }
}
