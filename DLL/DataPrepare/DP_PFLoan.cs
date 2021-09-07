using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DLL.ViewModel;

namespace DLL.DataPrepare
{
    public class DP_PFLoan
    {
        public tbl_PFLoan tbl_PFLoan(VM_PFLoan x)
        {
            tbl_PFLoan y = new tbl_PFLoan();
            y.EmpID = x.EmpID;
            y.StartDate = x.StartDate ?? System.DateTime.Now;
            y.PFLoanID = x.PFLoanID;
            y.TermMonth = x.TermMonth;
            y.Interest = x.Interest;
            y.Installment = x.Installment;
            y.LoanAmount = x.LoanAmount;
            
            return y;
        }

        public VM_PFLoan VM_PFLoan(tbl_PFLoan x)
        {
            VM_PFLoan y = new VM_PFLoan();
            y.EmpID = x.EmpID;
            y.StartDate = x.StartDate;
            y.PFLoanID = x.PFLoanID;
            y.TermMonth = x.TermMonth;
            y.Interest = x.Interest;
            y.Installment = x.Installment;
            y.LoanAmount = x.LoanAmount;
            return y;
        }
    }
}
