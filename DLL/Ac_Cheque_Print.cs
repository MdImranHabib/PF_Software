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
    
    public partial class Ac_Cheque_Print
    {
        public int ChequePrint_id { get; set; }
        public string ChequeNo { get; set; }
        public string ChequeType { get; set; }
        public string AccountNo { get; set; }
        public Nullable<int> ClientInfo_id { get; set; }
        public Nullable<decimal> Amount { get; set; }
        public Nullable<System.DateTime> ChequeDate { get; set; }
        public Nullable<System.Guid> EditUser { get; set; }
        public System.DateTime EditDate { get; set; }
        public Nullable<int> BankInfo_Id { get; set; }
        public System.Guid LedgerId { get; set; }
        public Nullable<int> OCode { get; set; }
    }
}