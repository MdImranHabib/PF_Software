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
    
    public partial class tbl_NomineeInformation
    {
        public int EmpID { get; set; }
        public int NomineeID { get; set; }
        public string NomineeName { get; set; }
        public string NomineeAddress { get; set; }
        public string Relation { get; set; }
        public Nullable<System.DateTime> RegistrationDate { get; set; }
        public Nullable<System.DateTime> EditDate { get; set; }
        public string NomineeImageFileName { get; set; }
        public string NomineeNationalID { get; set; }
        public Nullable<decimal> Nomineepercentage { get; set; }
        public Nullable<System.Guid> EditUser { get; set; }
        public string NomineeSignFileName { get; set; }
        public Nullable<int> OCode { get; set; }
    }
}
