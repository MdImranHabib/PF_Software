//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DLL
{
    using System;
    using System.Collections.Generic;
    
    public partial class acc_Ledger
    {
        public acc_Ledger()
        {
            this.acc_VoucherDetail = new HashSet<acc_VoucherDetail>();
        }
    
        public System.Guid LedgerID { get; set; }
        public string LedgerName { get; set; }
        public Nullable<System.Guid> EditUser { get; set; }
        public Nullable<System.DateTime> EditDate { get; set; }
        public int GroupID { get; set; }
        public Nullable<decimal> InitialBalance { get; set; }
        public Nullable<int> BalanceType { get; set; }
        public Nullable<bool> RestrictDelete { get; set; }
        public Nullable<System.Guid> ParentLedgerID { get; set; }
        public string Comment { get; set; }
        public string EditUserName { get; set; }
        public string PFMemberID { get; set; }
        public string PFAdditionalInformation { get; set; }
        public Nullable<bool> IsSystemDefault { get; set; }
        public Nullable<int> OCode { get; set; }
        public string LedgerCode { get; set; }
        public string UsedProject { get; set; }
    
        public virtual acc_Group acc_Group { get; set; }
        public virtual ICollection<acc_VoucherDetail> acc_VoucherDetail { get; set; }
    }
}
