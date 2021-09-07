using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLL.ViewModel
{
    public class VM_Designation
    {
        public string DesignationID { get; set; }
        [Required(ErrorMessage = "Required")]
        [StringLength(20, ErrorMessage = "Name cannot be longer than 20 characters.")]
        public string DesignationName { get; set; }
        public System.Guid EditUser { get; set; }
        public System.DateTime EditDate { get; set; }
    }
}
