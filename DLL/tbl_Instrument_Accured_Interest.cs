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
    
    public partial class tbl_Instrument_Accured_Interest
    {
        public int ID { get; set; }
        public int InstrumentID { get; set; }
        public System.DateTime AccuredInterestDate { get; set; }
        public decimal AccuredInterestAmount { get; set; }
        public System.Guid EditUser { get; set; }
        public System.DateTime EditDate { get; set; }
        public int OCode { get; set; }
        public string UsedProject { get; set; }
    }
}
