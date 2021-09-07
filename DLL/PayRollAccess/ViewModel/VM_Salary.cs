using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLL.PayRollAccess.ViewModel
{
    public class VM_Salary
    {
        public int EmpID { get; set; }
        public Nullable<System.DateTime> ProcessDate { get; set; }
        public decimal OwnContribution { get; set; }
        public decimal EmpContribution { get; set; }
        public decimal LoanPrincipal { get; set; }
        public decimal LoanInterest { get; set; }
        public string IdentificationNo { get; set; }
    }
}
