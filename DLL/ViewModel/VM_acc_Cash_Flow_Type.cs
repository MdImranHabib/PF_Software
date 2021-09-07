using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLL.ViewModel
{
    public class VM_acc_Cash_Flow_Type
    {
        public int CashFlowType_Id { get; set; }
        [Required(ErrorMessage = "Cash Flow Group is required")]
        public int CashFlowGroup_Id { get; set; }
        [Required(ErrorMessage = "Cash Flow Type name is required")]
        public string CashFlow_Type { get; set; }
        public Guid EditUser { get; set; }
        public System.DateTime EditDate { get; set; }
        public int OCode { get; set; }
    }
}
