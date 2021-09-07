using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLL.ViewModel
{
   public class VM_acc_Subsidiary
    {
        public int Subsidiary_Id { get; set; }
        [Required(ErrorMessage = "Subsidiary Name is required")]    
        public string Subsidiary_Name { get; set; }
        public Guid EditUser { get; set; }
        public System.DateTime EditDate { get; set; }
        public int OCode { get; set; }
    }
}
