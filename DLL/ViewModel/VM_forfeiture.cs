using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLL.ViewModel
{
    public class VM_forfeiture
    {
        public int EmpId { set; get; }
        public decimal Amount { set; get; }
        public int VoucherId { set; get; }
        public DateTime TransactionDate { set; get; }
    }
}
