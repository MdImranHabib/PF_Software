using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLL.ViewModel
{
    public class VM_PFRules
    {
        public int ROWID { get; set; }
        [Required]
        [Display(Name="Working duration in month")]
        public int WorkingDurationInMonth { get; set; }
        [Required]
        [Display(Name = "Employee Contribution (%)")]
        public decimal EmployeeContribution { get; set; }
        [Required]
        [Display(Name = "Employer Contribution (%)")]
        public decimal EmployerContribution { get; set; }
        [Required]
        public bool IsActive { get; set; }
        [Required]
        public Nullable<System.DateTime> EffectiveFrom { get; set; }
        public string status { get { if (IsActive) return "Activated"; else return "Deactivated"; } }
        public string RuleName { get; set; }
    }
}
