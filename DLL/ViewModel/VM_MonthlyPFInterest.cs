using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLL.ViewModel
{
    public class VM_MonthlyPFInterest
    {
        public int EmpID { get; set; }
        public decimal SelfContributionTillNow { get; set; }
        public decimal EmpContributionTillNow { get; set; }
    }
}
