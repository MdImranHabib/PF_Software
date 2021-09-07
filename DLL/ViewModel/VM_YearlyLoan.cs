using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLL.ViewModel
{
    public class VM_YearlyLoan
    {
        public string IdentificationNumber { get; set; }
        public string EmpName { get; set; }
        public int EmpID { get; set; }
        public Nullable<decimal> oppeningLoan { get; set; }
        public Nullable<decimal> additionDuring { get; set; }
        public Nullable<decimal> Installment { get; set; }
        public Nullable<decimal> Interest { get; set; }
    }
}
