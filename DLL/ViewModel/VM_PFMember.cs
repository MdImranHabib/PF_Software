using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLL.ViewModel
{
    public class VM_PFMember
    {
        public int EmpID { get; set; }
        public int SelfContribution { get; set; }
        public int OrgContribution { get; set; }
        public System.DateTime RetirementDate { get; set; }
        public string PFStatusID { get; set; }
        public string PFStatus { get;set;}
    }
}
