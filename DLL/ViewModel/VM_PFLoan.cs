using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLL.ViewModel
{
    public class VM_PFLoan
    {
        [Required]
        public int EmpID { get; set; }
        [Required]
        public string IdentificationNumber { get; set; }
        [Required]
        public string PFLoanID { get; set; }
        [Required]
        [Display(Name="Loan Amount")]
        public decimal LoanAmount { get; set; }
     
        [Required]
        [Display(Name = "Term Month")]
        public int TermMonth { get; set; }
        [Required]
        public decimal Interest { get; set; }
        [Required]
        [Display(Name="Monthly Pay")]
        public decimal Installment { get; set; }
        [Required]
        public System.DateTime? StartDate { get; set; }

        public decimal? Outstanding { get; set; }
        public byte IsApproved { get; set; }
        public Guid? ApprovedById { get; set; }
        public string ApprovedByName { get; set; }

        public string EmpName { get; set; }
        public decimal PayableAmount { get; set; }
        public int RuleUsed { get; set; }
        public Guid CashLedgerID { get; set; }
        public string CashLedgerName { get; set; }
        //Added by Fahim 08/12/2015
        public string EmpDesignation { get; set; }
        public string EmpDepartment { get; set; }
        public decimal Balance { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal InterestAmount { get; set; }
        public int InstallmentPaid { get; set; }
        //End Fahim
    }
}
