using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLL.ViewModel
{
    public class VM_acc_ledger
    {
        public System.Guid LedgerID { get; set; }
        [Required(ErrorMessage="Ledger name is required")]
        public string LedgerName { get; set; }
        [Required(ErrorMessage="Ledger under which group - information is required")]
        public int GroupID { get; set; }
        public string GroupName { get; set; }
        public int ParentGroupID { get; set; }
        public string ParentGroup { get; set; }
        [DisplayFormat(DataFormatString = "{0:0}",ApplyFormatInEditMode = true)]
        public Nullable<decimal> InitialBalance { get; set; }
        public Nullable<int> BalanceType { get; set; }
        public string BalanceTypeName { get; set; }
        public Guid EditUser { get; set; }
        public DateTime EditDate { get; set; }
        public string EditUserName { get; set; }
        public int OCode { get; set; }
        public string LedgerCode { get; set; }
        public int NatureID { get; set; }
        [DisplayFormat(DataFormatString = "{0:0}",ApplyFormatInEditMode = true)]
        public decimal Debit { get; set; }
        [DisplayFormat(DataFormatString = "{0:0}",ApplyFormatInEditMode = true)]
        public decimal Credit { get; set; }
        [DisplayFormat(DataFormatString = "{0:0}",ApplyFormatInEditMode = true)]
        public decimal Balance { get; set; }
        [DisplayFormat(DataFormatString = "{0:0}",ApplyFormatInEditMode = true)]
        public string strBalance { get; set; }
        [DisplayFormat(DataFormatString = "{0:0}",ApplyFormatInEditMode = true)]
        public decimal CBalance { get; set; }
        [DisplayFormat(DataFormatString = "{0:0}",ApplyFormatInEditMode = true)]
        public string strCBalance { get; set; }
        [DisplayFormat(DataFormatString = "{0:0}",ApplyFormatInEditMode = true)]
        public string strDebit { get; set; }
        [DisplayFormat(DataFormatString = "{0:0}",ApplyFormatInEditMode = true)]
        public string strCredit { get; set; }
        public decimal decDebit { get; set; }
        public decimal decCredit { get; set; }
        public string LoginName { get; set; }
    }
}
