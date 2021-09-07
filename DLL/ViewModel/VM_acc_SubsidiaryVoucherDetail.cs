using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLL.ViewModel
{
    public class VM_acc_SubsidiaryVoucherDetail
    {
        
        public int Subsidiary_Voucher_Detail_Id { get; set; }
        [Required(ErrorMessage = "Voucher Detail ID is required")]
        public Guid VoucherDetailID { get; set; }
        [Required(ErrorMessage = "Subsidiary Id is required")]
        public decimal Amount { get; set; }
        public int Subsidiary_Id { get; set; }
        public int VoucherID { get; set; }
        public Guid EditUser { get; set; }
        public System.DateTime EditDate { get; set; }
        public int OCode { get; set; }
    }
}
