using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLL.ViewModel
{

    public class VM_AuditLog
    {
        public int LogID { get; set; }
        public DateTime LastAuditDate { get; set; }
        public DateTime LogDate { get; set; }
        public int? OCode { get; set; }
        public System.Guid EditUser { get; set; }
        public string EditUserName { get; set; }
    }
}
