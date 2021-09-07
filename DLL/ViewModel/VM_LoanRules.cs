using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLL.ViewModel
{
    public class VM_LoanRules
    {
        public int ROWID { get; set; }
        [Required]
        public int WorkingDurationInMonth { get; set; }
        [Required]
        
        [Display(Name = "Own Part Payable")]
        public decimal OwnPartPayable { get; set; }
        [Required]
        
        [Display(Name = "Employer Part payable")]
        public decimal EmpPartPayable { get; set; }
        public decimal OwnProfitPartPayable { get; set; }
        public decimal EmpProfitPartPayable { get; set; }
        public System.Guid EditUser { get; set; }
        public System.DateTime EditDate { get; set; }
        public bool IsActive { get; set; }
        [Required]
        public System.DateTime EffectiveFrom { get; set; }
        public string RuleName { get; set; }
        public string status { get { if (IsActive) return "Activated"; else return "Deactivated"; } }
        public int OCode { get; set; }
        public decimal IntarestRate { get; set; }
        public decimal Installment { get; set; }
    }
}
