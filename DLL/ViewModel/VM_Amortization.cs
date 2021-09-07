using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLL.ViewModel
{
     public class VM_Amortization
    {
         public int EmpID { get; set; }
         public string IdentificationNumber { get; set; }
         public string PFLoanID { get; set; }
         public int ReScheduleID { get; set; }
        public int InstallmentNumber { get; set; }
        public string MonthName { get; set; }
        public double Amount { get; set; }
        public double Interest { get; set; }
        public double Principal { get; set; }
        public double Balance { get; set; }
        public double MonthlyPaid { get; set; }
        public short Processed { get; set; }
        
        public Nullable<System.DateTime> PaymentDate { get; set; }
        public string TrackingNumber { get; set; }
        public int? ProcessNumber { get; set; }
        public string PaymentStatus { get { if (Processed == 1) return "Paid"; else return ""; } }
        public string ConYear { get; set; }
        public string ConMonth { get; set; }
        //Edited by Fahim 22/11/2015
        public DateTime MonthYear
        {
            get
            {
                if (!string.IsNullOrEmpty(ConMonth) && !string.IsNullOrEmpty(ConYear))
                {
                    return Convert.ToDateTime(ConYear + "/" + ConMonth + "/01");
                }
                else
                {
                    return DateTime.MinValue;
                }
            }
        }
        //End Fahim
        public decimal LoanAmount { get; set; }
        public decimal TotalInterest { get; set; }
        public string EmpName { get; set; }

        public string EmpDesignation { get; set; }

        public string EmpDepartment { get; set; }
    }

}
