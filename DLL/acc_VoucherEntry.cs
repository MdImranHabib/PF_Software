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
    
    public partial class acc_VoucherEntry
    {
        public acc_VoucherEntry()
        {
            this.acc_VoucherDetail = new HashSet<acc_VoucherDetail>();
        }
    
        public int VoucherID { get; set; }
        public string VoucherName { get; set; }
        public Nullable<System.DateTime> TransactionDate { get; set; }
        public Nullable<System.Guid> EditUser { get; set; }
        public Nullable<System.DateTime> EditDate { get; set; }
        public string Narration { get; set; }
        public Nullable<int> VTypeID { get; set; }
        public string VNumber { get; set; }
        public string EditUserName { get; set; }
        public Nullable<bool> RestrictDelete { get; set; }
        public string PFMonth { get; set; }
        public string PFYear { get; set; }
        public Nullable<int> OCode { get; set; }
        public Nullable<int> ProfitDistributionProcessID { get; set; }
        public Nullable<int> EmpID { get; set; }
        public string ActionName { get; set; }
        public string EntryForGF { get; set; }
        public string EntryForPF { get; set; }
        public string EntryForWPPF { get; set; }
    
        public virtual ICollection<acc_VoucherDetail> acc_VoucherDetail { get; set; }
        public virtual acc_VoucherType acc_VoucherType { get; set; }
    }
}
