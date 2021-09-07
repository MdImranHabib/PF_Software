using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLL.ViewModel
{
    public class VM_InterestRateViewModel
    {
        public int InterestRateCount { get; set; }

        public decimal InterestRate { get; set; } 

    }

    public class Interest
    {
        public int ID { get; set; }
        public int InstrumentID { get; set; }
        public decimal InterestRate { get; set; }
        public System.DateTime StartDate { get; set; }
        public System.DateTime EndDate { get; set; }
        public System.Guid EditUser { get; set; }
        public System.DateTime EditDate { get; set; }
        public int InterestYear { get; set; }
    }
}
