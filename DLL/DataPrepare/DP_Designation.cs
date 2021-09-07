using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DLL.ViewModel;
using DLL;

namespace DLL.DataPrepare
{
    public class DP_Designation
    {
        public LU_tbl_Designation tbl_Designation(VM_Designation x)
        {
            LU_tbl_Designation y = new LU_tbl_Designation();
            y.DesignationID = x.DesignationID;
            y.DesignationName = x.DesignationName;
            return y;
        }

        public VM_Designation vm_Designation(LU_tbl_Designation x)
        {
            VM_Designation y = new VM_Designation();
            y.DesignationID = x.DesignationID;
            y.DesignationName = x.DesignationName;
            return y;
        }
    }
}