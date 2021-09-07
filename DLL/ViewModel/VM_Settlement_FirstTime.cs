using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLL.ViewModel
{
    public class VM_Settlement_FirstTime
    {
        public string IdentificationNo { get; set; }
        public decimal Balance { get; set; }
        public decimal SettlementAmount { get; set; }
        public string Message { get; set; }
    }
}
