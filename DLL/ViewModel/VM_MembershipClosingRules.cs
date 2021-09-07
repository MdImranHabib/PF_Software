using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLL.ViewModel
{
    public class VM_MembershipClosingRules
    {
        public int ROWID { get; set; }
        [Required]
        public int PFDurationInMonth { get; set; }
        [Required]
        //[Range(0, 101, ErrorMessage = "Self percentage range should be withing 100")]
        //[Display(Name="Own Part Payable")]
        //public bool IsOwnPartPayable { get; set; }
        //[Required]
        //[Range(0, 101, ErrorMessage = "Employer percentage range should be withing 100")]
        //[Display(Name = "Employer Part payable")]
        //public bool IsEmployerPartPayable { get; set; }
        //public bool IsProfitPartPayable { get; set; }
        public System.Guid EditUser { get; set; }
        public System.DateTime EditDate { get; set; }
        public bool IsActive { get; set; }
        [Required]
        public System.DateTime EffectiveFrom { get; set; }
        public string RuleName { get; set; }
        public string status { get { if (IsActive) return "Activated"; else return "Deactivated"; } }
        public decimal EmpProfitPercent { get; set; }
        public decimal OwnProfitPercent { get; set; }
        [Display(Name = "Own Part Percent")]
        public decimal EmployerPartPercent { get; set; }
        [Display(Name = "Employer Part Percent")]
        public decimal OwnPartPercent { get; set; }
    }
}
