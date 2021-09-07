using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DLL.ViewModel;
using DLL;

namespace DLL.DataPrepare
{
    public class DP_Country
    {
        public LU_tbl_Country tbl_Country(VM_Country x)
        {
            LU_tbl_Country y = new LU_tbl_Country();
            y.CountryID = x.CountryID;
            y.CountryName = x.Country;
            return y;
        }

        public VM_Country vm_Country(LU_tbl_Country x)
        {
            VM_Country y = new VM_Country();
            y.CountryID = x.CountryID;
            y.Country = x.CountryName;
            return y;
        }
    }
}
