using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLL.ViewModel
{
    public class VM_tbl_Loan_Receivable
    {
        public string IdentificationNumber { get; set; }
        public string Loan_id { get; set; }
        public string EmpName { get; set; }
        public decimal Principal { get; set; }
        public decimal interest { get; set; }
        public string PF_Month { get; set; }
        public string PF_Year { get; set; }
        public int Identification_No_Id { get; set; }
        public System.DateTime Edit_date { get; set; }
        public System.Guid Edit_user { get; set; }
        public int OCode { get; set; }
        public bool Is_HR_Receive { get; set; }
    }
}
