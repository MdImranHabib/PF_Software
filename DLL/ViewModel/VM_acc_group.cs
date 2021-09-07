using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLL.ViewModel
{
    public class VM_acc_group
    {
        public int GroupID { get; set; }
        [Required(AllowEmptyStrings=false, ErrorMessage="Group name is required.")]
        public string GroupName { get; set; }
        public Nullable<int> ParentGroupID { get; set; }

        [Required(ErrorMessage="Nature type is required.")]
        public int NatureID { get; set; }

        public string ParentGroupName { get; set; }
        public string NatureName { get; set; }
        public Guid EditUser { get; set; }
        public DateTime EditDate { get; set; }
        public string EditUserName { get; set; }
        public int? OCode { get; set; }
        public string GroupCode { get; set; }
    }
}
