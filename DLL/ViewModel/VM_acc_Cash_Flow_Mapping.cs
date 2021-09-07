using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace DLL.ViewModel
{
    public class VM_acc_Cash_Flow_Mapping
    {
        public int CashFlowMapping_Id { get; set; }

        [Required(ErrorMessage = "Ledger name is required")]
        public Guid LedgerID { get; set; }
        [Required(ErrorMessage = "Cash Flow Type name is required")]
        public int CashFlowType_Id { get; set; }
        public string CashFlowType { get; set; }
        public Guid EditUser { get; set; }
        public System.DateTime EditDate { get; set; }
        public int OCode { get; set; }


        public string CashFlow_Group { get; set; }
        public string LedgerName { get; set; }
        //public string CashFlow_Type { get; set; }
    }
}
