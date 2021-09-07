using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLL.ViewModel
{
    public class VM_EmployeeWebUser
    {
        public Guid UserId { get; set; }
        public int EmpID { get; set; }
        public string IdentificationNumber { get; set; }
        public bool IsActive { get; set; }
        public string FullName { get; set; }
    }
}
