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
    
    public partial class LU_tbl_LoanRules
    {
        public int ROWID { get; set; }
        public int WorkingDurationInMonth { get; set; }
        public decimal OwnPartPayable { get; set; }
        public decimal EmpPartPayable { get; set; }
        public decimal OwnProfitPartPayable { get; set; }
        public decimal EmpProfitPartPayable { get; set; }
        public System.Guid EditUser { get; set; }
        public System.DateTime EditDate { get; set; }
        public byte IsActive { get; set; }
        public Nullable<System.DateTime> EffectiveFrom { get; set; }
        public string RuleName { get; set; }
        public Nullable<int> OCode { get; set; }
        public Nullable<decimal> IntarestRate { get; set; }
        public Nullable<decimal> InstallmentNoumber { get; set; }
    }
}