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
    
    public partial class acc_VoucherType
    {
        public acc_VoucherType()
        {
            this.acc_VoucherEntry = new HashSet<acc_VoucherEntry>();
        }
    
        public int VTypeID { get; set; }
        public string VTypeName { get; set; }
        public Nullable<System.Guid> EditUser { get; set; }
        public Nullable<System.DateTime> EditDate { get; set; }
        public string EditUserName { get; set; }
        public Nullable<int> OCode { get; set; }
    
        public virtual ICollection<acc_VoucherEntry> acc_VoucherEntry { get; set; }
    }
}
