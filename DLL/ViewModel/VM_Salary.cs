using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLL.ViewModel
{
    public class VM_Salary
    {
        public int RowID { get; set; }
        public int EmpID { get; set; }
        public string EmpName { get; set; }
        public string Branch { get; set; }
        public string Designation { get; set; }
        [Required]
        [Display(Name="Basic Salary")]
        public decimal Basic { get; set; }
        [Required]
        [Display(Name="Effective Date")]
        [DataType(DataType.DateTime)]        
        public System.DateTime? EffectiveDate { get; set; }
        public System.DateTime JoiningDate { get; set; }

        public virtual tbl_Employees tbl_Employees { get; set; }
        public virtual tbl_User tbl_User { get; set; }
        [Required]
        public string Comment { get; set; }
    }
}
