using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLL.ViewModel
{
    public class VM_acc_VoucherDetail
    {
        public Guid? VoucherDetailID { get; set; }
        public string LedgerName { get; set; }
        [RegularExpression(@"^\d+.\d{0,2}$")]
        [Range(0, 9999999999999999.99)]
        [DisplayFormat(DataFormatString = "{0:0}",ApplyFormatInEditMode = true)]
        public decimal Debit { get; set; }
        [RegularExpression(@"^\d+.\d{0,2}$")]
        [Range(0, 9999999999999999.99)]
        [DisplayFormat(DataFormatString = "{0:0}",ApplyFormatInEditMode = true)]
        public decimal Credit { get; set; }
        public string ChequeNumber { get; set; }
        public Guid LedgerID { get;set; }
        public int VoucherID {get;set;}
        public string VNumber { get; set; }
        public DateTime? TransactionDate { get; set; }
        [DisplayFormat(DataFormatString = "{0:0}",ApplyFormatInEditMode = true)]
        public decimal? InitialBalance { get; set; }
        public string Narration { get; set; }
        public string GroupName { get; set; }
        public int GroupID { get; set; }
        public string NatureName { get; set; }
        public int NatureID { get; set; }
        public string Particulars { get; set; }

        public Guid EditUser { get; set; }
        public DateTime EditDate { get; set; }
        [DisplayFormat(DataFormatString = "{0:0}",ApplyFormatInEditMode = true)]
        public string Balance 
        {
            get 
            {
                if (InitialBalance == null)
                {
                    InitialBalance = 0;
                }
                var r = InitialBalance+Credit-Debit;
                if (r > 0)
                {
                    return r + " Cr.";
                }
                else
                {
                    return (-1)*r + " Dr.";
                }
            }
        }

        public Guid? ParentLedgerID { get; set; }
        [DisplayFormat(DataFormatString = "{0:0}",ApplyFormatInEditMode = true)]
        public string strBalance { get; set; }
        [DisplayFormat(DataFormatString = "{0:0}",ApplyFormatInEditMode = true)]
        public decimal CBalance { get; set; } // C for cumulative balance
        [DisplayFormat(DataFormatString = "{0:0}",ApplyFormatInEditMode = true)]
        public string strCBalance { get; set; } // C for cumulative balance
        public string aCredit { get; set; }
        public string aDebit { get; set; }
    }
}
