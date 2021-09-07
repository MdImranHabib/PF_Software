using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLL.ViewModel
{
    public class VM_GroupDetail
    {
        public string GroupName { get; set; }
        public int GroupID { get; set; }
        public decimal InitialBalance { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public Guid LedgerID { get; set; }
        public string LedgerName { get; set; }
        public decimal Balance { get; set; }
        public string strBalance { get; set; }
    }
}
