using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLL.ViewModel
{
    public class VM_acc_VoucherEntry
    {
        public int VoucherID { get; set; }
        public string VoucherName { get; set; }
        public DateTime TransactionDate { get; set; }
        public int VTypeID { get; set; }
        public string VNumber { get; set; }
        public List<VM_acc_VoucherDetail> lst_vm_vDetail { get; set; }
    }
}
