//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DLL.PayRollAccess
{
    using System;
    using System.Collections.Generic;
    
    public partial class HRM_DESIGNATIONS
    {
        public int DEG_ID { get; set; }
        public string DEG_NAME { get; set; }
        public string GRADE { get; set; }
        public Nullable<decimal> GROSS_SAL { get; set; }
        public Nullable<decimal> HOUSE_RENT { get; set; }
        public Nullable<decimal> BASIC { get; set; }
        public Nullable<decimal> MEDICAL { get; set; }
        public Nullable<decimal> CONVEYANCE { get; set; }
        public Nullable<decimal> FOOD_ALLOW { get; set; }
        public Nullable<decimal> FixedAllowance { get; set; }
        public Nullable<decimal> OTHERS { get; set; }
        public Nullable<decimal> AttendanceBonus { get; set; }
        public Nullable<decimal> Holiday_Allowance { get; set; }
        public Nullable<System.Guid> EDIT_USER { get; set; }
        public Nullable<System.DateTime> EDIT_DATE { get; set; }
        public string OCODE { get; set; }
    }
}