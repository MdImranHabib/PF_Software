using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLL.ViewModel
{
    public class VM_PFStatus
    {
        [Display(Name="PF Status")]
        public int PFStatusID { get; set; }
        public int EmpID { get; set; }
        public string PFStatus { get; set; }
        [Required]
        public decimal MinSalary { get; set; }
        [Required]
        public decimal MinWorkingDuration { get; set; }

        [Display(Name="Self Contribution")]
        [Required]
        public int SelfContribution { get; set; }
        [Display(Name = "Org Contribution")]
        [Required]
        public int OrgContribution { get; set; }
        [Display(Name = "Retirement Date")]
        [Required]
        public System.DateTime? RetirementDate { get; set; }
    }
}
