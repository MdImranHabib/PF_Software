using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLL.ViewModel
{
    public class VM_UserLoginHistory
    {
        public string UserName { get; set; }
        public System.DateTime LoginTime { get; set; }

        public System.DateTime? SignOut { get; set; }
        public int rowid { get; set; }
        public string UserFullName { get; set; }
        public string Host { get; set;}
        public string Terminal { get; set; }
    }
}
