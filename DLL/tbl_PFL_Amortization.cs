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
    
    public partial class tbl_PFL_Amortization
    {
        public int EmpID { get; set; }
        public string PFLoanID { get; set; }
        public int InstallmentNumber { get; set; }
        public byte ReScheduleID { get; set; }
        public decimal Amount { get; set; }
        public decimal Interest { get; set; }
        public decimal Principal { get; set; }
        public decimal Balance { get; set; }
        public byte Processed { get; set; }
        public System.Guid EditUser { get; set; }
        public System.DateTime EditDate { get; set; }
        public Nullable<System.DateTime> PaymentDate { get; set; }
        public string TrackingNumber { get; set; }
        public Nullable<int> ProcessNumber { get; set; }
        public Nullable<decimal> PaymentAmount { get; set; }
        public string ConYear { get; set; }
        public string ConMonth { get; set; }
        public Nullable<int> OCode { get; set; }
        public Nullable<decimal> PaidAmount { get; set; }
    
        public virtual tbl_Employees tbl_Employees { get; set; }
        public virtual tbl_User tbl_User { get; set; }
    }
}
