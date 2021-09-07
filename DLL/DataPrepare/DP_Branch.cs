using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DLL.ViewModel;
using DLL;

namespace DLL.DataPrepare
{
    public class DP_Branch
    {
        public LU_tbl_Branch tbl_Branch(VM_Branch x)
        {
            LU_tbl_Branch y = new LU_tbl_Branch();
            y.BranchName = x.BranchName;
            y.BranchLocation = x.BranchLocation;
            y.BranchID = x.BranchID;
            return y;
        }

        public VM_Branch vm_Branch(LU_tbl_Branch x)
        {
            VM_Branch y = new VM_Branch();
            y.BranchName = x.BranchName;
            y.BranchLocation = x.BranchLocation;
            y.BranchID = x.BranchID;
            return y;
        }
    }
}
