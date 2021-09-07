using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLL.ViewModel
{
    public class VM_acc_Chart_Of_Account_Mapping
    {
        public int id { get; set; }
        public int MIS_Id { get; set; }
        public System.Guid Ledger_Id { get; set; }
        public System.DateTime DateOfEntry { get; set; }
        public System.Guid EntryBy { get; set; }
        public int OCode { get; set; }
        public string LedgerName { get; set; }
        public string MISName { get; set; }
    }
}
