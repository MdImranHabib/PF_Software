using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DLL.ViewModel;
using DLL;

namespace DLL.DataPrepare
{
    public class DP_Department
    {
        public LU_tbl_Department tbl_Department(VM_Department x)
        {
            LU_tbl_Department y = new LU_tbl_Department();
            y.DepartmentID = x.DepartmentID;
            y.DepartmentName = x.DepartmentName;
            return y;
        }

        public VM_Department vm_Department(LU_tbl_Department x)
        {
            VM_Department y = new VM_Department();
            y.DepartmentID = x.DepartmentID;
            y.DepartmentName = x.DepartmentName;
            return y;
        }
    }
}
