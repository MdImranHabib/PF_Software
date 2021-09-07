using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLL.ViewModel
{
    public class VM_Department
    {
        public string DepartmentID { get; set; }
        [Required(ErrorMessage = "Required")]
        [StringLength(40, ErrorMessage = "Name cannot be longer than 20 characters.")]
        public string DepartmentName { get; set; }
        public System.Guid EditUser { get; set; }
        public System.DateTime EditDate { get; set; }
    }
}
