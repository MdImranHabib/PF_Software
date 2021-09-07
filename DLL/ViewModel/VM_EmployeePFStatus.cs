using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLL.ViewModel
{
    public class VM_EmployeePFStatus
    {
        public int EmpID { get; set; }
        public string EmpName { get; set; }
        public string Month { get; set; }
        public decimal SelfContribution { get; set; }
        public decimal EmployerContribution { get; set; }
        public decimal SCInterest { get; set; }
        public decimal ECInterest { get; set; }
        public string MonthYear { get { return Convert.ToDateTime(Month).ToString("MMMM, yyyy"); } }
        public string Total { get { return (SelfContribution + EmployerContribution) + ""; } }

        public int PFRulesID { get; set; }
        public Nullable<decimal> Salary { get; set; }
        public Nullable<decimal> SCPercentage { get; set; }
        public Nullable<decimal> ECPercentage { get; set; }
        public Nullable<decimal> InterestRate { get; set; }

        public decimal CumulativeSelfContribution { get; set; }
        public decimal CumulativeEmpContribution { get; set; }
    }
}
