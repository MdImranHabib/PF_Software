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
    
    public partial class tbl_LoginHistory
    {
        public string UserName { get; set; }
        public System.DateTime LoginTime { get; set; }
        public Nullable<System.DateTime> SignOut { get; set; }
        public int rowid { get; set; }
        public string Terminal { get; set; }
        public string HostName { get; set; }
        public string OCode { get; set; }
    }
}
