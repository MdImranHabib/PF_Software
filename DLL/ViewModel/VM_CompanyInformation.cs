using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLL.ViewModel
{
    public class VM_CompanyInformation
    {
        public int CompanyID { get; set; }
        [Required]
        public string CompanyName { get; set; }
        [Required]
        public string CompanyAddress { get; set; }
        [Required]
        public System.DateTime? SystemImplementationDate { get; set; }
        [Required]
        public System.DateTime? AccountingYearBeginningFrom { get; set; }
        public System.Guid EditUser { get; set; }
        public string EditUserName { get; set; }
        public Nullable<System.DateTime> EditDate { get; set; }
    }
}
