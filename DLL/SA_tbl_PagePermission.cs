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
    
    public partial class SA_tbl_PagePermission
    {
        public int RoleID { get; set; }
        public int PageID { get; set; }
        public bool CanVisit { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanExecute { get; set; }
        public System.Guid EditUser { get; set; }
        public System.DateTime EditDate { get; set; }
        public Nullable<int> OCode { get; set; }
        public string EditUserName { get; set; }
    }
}
