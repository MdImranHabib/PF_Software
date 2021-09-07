using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLL.ViewModel
{
    public class VM_acc_Cash_Flow_Group
    {
        public int CashFlowGroup_Id { get; set; }
        [Required(ErrorMessage = "Cash Flow Group name is required")]
        public string CashFlow_Group { get; set; }
        public Guid EditUser { get; set; }
        public System.DateTime EditDate { get; set; }
        public int OCode { get; set; }
    }
}
