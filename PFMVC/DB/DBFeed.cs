using DLL;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PFMVC.DB
{
    public static class DBFeed
    {
        public static bool InitialFeed(ref string message)
        {
            using (PFTMEntities entities = new PFTMEntities())
            {
                Guid x = Guid.NewGuid();
                tbl_User user = new tbl_User();
                user.UserID = x;
                user.LoginName = "admin";
                user.UserFullName = "Admin";
                user.EditDate = System.DateTime.Now;
                user.EditUser = x;
                user.IsActive = 1;
                user.RoleID = 1; // needs to check if works
                entities.tbl_User.Add(user);

                tbl_UserPassword user_pass = new tbl_UserPassword();
                user_pass.UserID = x;
                user_pass.Password = PFMVC.common.GetMD5HashPassword.GetMd5Hash("admin");
                user_pass.EditDate = DateTime.Now;
                user_pass.EditUser = x;
                entities.tbl_UserPassword.Add(user_pass);

                //create module information
                if (entities.SA_tbl_Module.Count() == 0)
                {
                    var module = new List<SA_tbl_Module>
                                {
                                    new SA_tbl_Module { id = 1, name = "Basic"},
                                    new SA_tbl_Module { id = 2, name = "PF"},
                                    new SA_tbl_Module { id= 3, name= "Loan"},
                                    new SA_tbl_Module { id= 4, name = "Investment"}
                                };
                    module.ForEach(s => entities.SA_tbl_Module.Add(s));
                    entities.SaveChanges();
                }
                //if page information
                if (entities.SA_tbl_Page.Count() == 0)
                {
                    var pages = new List<SA_tbl_Page>
                                {
                                    new SA_tbl_Page{ PageID =1 , ModuleID= 1, PageName = "User Management",  FileName = null},
                                    new SA_tbl_Page{ PageID = 2, ModuleID= 1, PageName = "Basic Settings",  FileName = null},
                                    new SA_tbl_Page{ PageID = 3, ModuleID= 1, PageName = "Employee Setup",  FileName = null},
                                    new SA_tbl_Page{ PageID = 4, ModuleID= 2, PageName = "Salary Setup",  FileName = null},
                                    new SA_tbl_Page{ PageID = 5, ModuleID= 2, PageName = "PF Core",  FileName = null},
                                    new SA_tbl_Page{ PageID = 6, ModuleID= 2, PageName = "PF Report",  FileName = null},
                                    new SA_tbl_Page{ PageID = 7, ModuleID= 3, PageName = "Loan Management",  FileName = null},
                                    new SA_tbl_Page{ PageID = 8, ModuleID= 3, PageName = "Loan Approval",  FileName = null},
                                    new SA_tbl_Page{ PageID = 9, ModuleID= 3, PageName = "Loan Report",  FileName = null}
                                };
                    pages.ForEach(s => entities.SA_tbl_Page.Add(s));
                }

                if (entities.tbl_UserRole.Count() == 0)
                {
                    var role = new List<tbl_UserRole>
                                {
                                    new tbl_UserRole { RoleID = 1, RoleName= "Admin", EditDate = DateTime.Now, EditUser = x },
                                    new tbl_UserRole { RoleID = 2, RoleName= "Demo User", EditDate = DateTime.Now, EditUser = x },
                 
                                };
                    role.ForEach(s => entities.tbl_UserRole.Add(s));

                }


                if (entities.SA_tbl_PagePermission.Count() == 0)
                {
                    var pagePermission = new List<SA_tbl_PagePermission>
                                {
                                    new SA_tbl_PagePermission { RoleID  = 1, PageID= 1, CanVisit = true, CanEdit = true, CanDelete = true, CanExecute = true, EditDate = DateTime.Now, EditUser = x},
                                    new SA_tbl_PagePermission { RoleID  = 1, PageID= 2, CanVisit = true, CanEdit = true, CanDelete = true, CanExecute = true, EditDate = DateTime.Now, EditUser = x},
                                    new SA_tbl_PagePermission { RoleID  = 1, PageID= 3, CanVisit = true, CanEdit = true, CanDelete = true, CanExecute = true, EditDate = DateTime.Now, EditUser = x},
                                    new SA_tbl_PagePermission { RoleID  = 1, PageID= 4, CanVisit = true, CanEdit = true, CanDelete = true, CanExecute = true, EditDate = DateTime.Now, EditUser = x},
                                    new SA_tbl_PagePermission { RoleID  = 1, PageID= 5, CanVisit = true, CanEdit = true, CanDelete = true, CanExecute = true, EditDate = DateTime.Now, EditUser = x},
                                    new SA_tbl_PagePermission { RoleID  = 1, PageID= 6, CanVisit = true, CanEdit = true, CanDelete = true, CanExecute = true, EditDate = DateTime.Now, EditUser = x},
                                    new SA_tbl_PagePermission { RoleID  = 1, PageID= 7, CanVisit = true, CanEdit = true, CanDelete = true, CanExecute = true, EditDate = DateTime.Now, EditUser = x},
                                    new SA_tbl_PagePermission { RoleID  = 1, PageID= 8, CanVisit = true, CanEdit = true, CanDelete = true, CanExecute = true, EditDate = DateTime.Now, EditUser = x},
                                    new SA_tbl_PagePermission { RoleID  = 1, PageID= 9, CanVisit = true, CanEdit = true, CanDelete = true, CanExecute = true, EditDate = DateTime.Now, EditUser = x},
                                };
                    pagePermission.ForEach(f => entities.SA_tbl_PagePermission.Add(f));
                }

                if (entities.acc_Nature.Count() == 0)
                {
                    var nature = new List<acc_Nature>
                                {
                                    new acc_Nature { NatureID = 1, NatureName = "Asset", NatureType = 1, EditDate = DateTime.Now, EditUser = x },
                                    new acc_Nature { NatureID = 2, NatureName = "Liabilities", NatureType = 2, EditDate = DateTime.Now, EditUser = x },
                                    new acc_Nature { NatureID = 3, NatureName = "Expenses", NatureType = 1, EditDate = DateTime.Now, EditUser = x },
                                    new acc_Nature { NatureID = 4, NatureName = "Revenue", NatureType = 2, EditDate = DateTime.Now, EditUser = x },
                                };
                    nature.ForEach(s => entities.acc_Nature.Add(s));
                }

                if (entities.acc_VoucherType.Count() == 0)
                {
                    var nature = new List<acc_VoucherType>
                                {
                                    new acc_VoucherType { VTypeID = 1, VTypeName  = "Payment Voucher", EditDate = DateTime.Now, EditUser = x },
                                    new acc_VoucherType { VTypeID = 2, VTypeName  = "Receive Voucher", EditDate = DateTime.Now, EditUser = x },
                                    new acc_VoucherType { VTypeID = 3, VTypeName  = "Journal Voucher", EditDate = DateTime.Now, EditUser = x },
                                    new acc_VoucherType { VTypeID = 4, VTypeName  = "Contra Voucher", EditDate = DateTime.Now, EditUser = x },
                                    new acc_VoucherType { VTypeID = 5, VTypeName  = "Salary Voucher", EditDate = DateTime.Now, EditUser = x },
                                    new acc_VoucherType { VTypeID = 6, VTypeName  = "Encashment Voucher", EditDate = DateTime.Now, EditUser = x },
                                };
                    nature.ForEach(s => entities.acc_VoucherType.Add(s));

                }

                if (entities.acc_Group.Count() == 0)
                {
                    var group = new List<acc_Group>
                                {
                                    new acc_Group { GroupID = 1, GroupName = "Cash & cash equivalent", ParentGroupID = null, NatureID = 1, EditUser = x, EditDate = DateTime.Now, RestrictDelete = true, IsSystemDefault = true },
                                    new acc_Group { GroupID = 2, GroupName = "Current asset", ParentGroupID = null, NatureID = 1, EditUser = x, EditDate = DateTime.Now, RestrictDelete = true , IsSystemDefault = true},
                                    new acc_Group { GroupID = 3, GroupName = "Non-current asset", ParentGroupID = null, NatureID = 1, EditUser = x, EditDate = DateTime.Now, RestrictDelete = true , IsSystemDefault = true},
                                    new acc_Group { GroupID = 4, GroupName = "Current liabilities", ParentGroupID = null, NatureID = 2, EditUser = x, EditDate = DateTime.Now , RestrictDelete = true, IsSystemDefault = true},
                                    new acc_Group { GroupID = 5, GroupName = "Non-current liabilities", ParentGroupID = null, NatureID = 2, EditUser = x, EditDate = DateTime.Now , RestrictDelete = true, IsSystemDefault = true},
                                    new acc_Group { GroupID = 6, GroupName = "Direct income", ParentGroupID = 12, NatureID = 4, EditUser = x, EditDate = DateTime.Now , RestrictDelete = true, IsSystemDefault = true},
                                    new acc_Group { GroupID = 7, GroupName = "Indirect income", ParentGroupID = null, NatureID = 4, EditUser = x, EditDate = DateTime.Now , RestrictDelete = true, IsSystemDefault = true},
                                    new acc_Group { GroupID = 8, GroupName = "Direct expenses", ParentGroupID = null, NatureID = 3, EditUser = x, EditDate = DateTime.Now , RestrictDelete = true, IsSystemDefault = true},
                                    new acc_Group { GroupID = 9, GroupName = "Indirect expenses", ParentGroupID = null, NatureID = 3, EditUser = x, EditDate = DateTime.Now , RestrictDelete = true, IsSystemDefault = true},
                                    new acc_Group { GroupID = 10, GroupName = "Fixed asset", ParentGroupID = null, NatureID = 1, EditUser = x, EditDate = DateTime.Now , RestrictDelete = true, IsSystemDefault = true},
                                    new acc_Group { GroupID = 11, GroupName = "Retained Earning", ParentGroupID = null, NatureID = 2, EditUser = x, EditDate = DateTime.Now , RestrictDelete = true, IsSystemDefault = true},
                                    new acc_Group { GroupID = 12, GroupName = "Capital", ParentGroupID = null, NatureID = 2, EditUser = x, EditDate = DateTime.Now , RestrictDelete = true, IsSystemDefault = true},
                                    new acc_Group { GroupID = 13, GroupName = "Forfeiture Account", ParentGroupID = null, NatureID = 2, EditUser = x, EditDate = DateTime.Now , RestrictDelete = true, IsSystemDefault = true},
                                    new acc_Group { GroupID = 14, GroupName = "Company Current Account", ParentGroupID = null, NatureID = 1, EditUser = x, EditDate = DateTime.Now , RestrictDelete = true, IsSystemDefault = true},
                                    //new acc_Group { GroupID = 15, GroupName = "Members Fund", ParentGroupID = 12, NatureID = 2, EditUser = x, EditDate = DateTime.Now , RestrictDelete = true, IsSystemDefault = true},
                                    new acc_Group { GroupID = 16, GroupName = "Loan", ParentGroupID = 2, NatureID = 1, EditUser = x, EditDate = DateTime.Now , RestrictDelete = true, IsSystemDefault = true},
                                    new acc_Group { GroupID = 17, GroupName = "Investment", ParentGroupID = 2, NatureID = 1, EditUser = x, EditDate = DateTime.Now , RestrictDelete = true, IsSystemDefault = true},
                                };
                    group.ForEach(f => entities.acc_Group.Add(f));
                }

                if (entities.acc_Ledger.Count() == 0)
                {
                    var ledger = new List<acc_Ledger>
                                {
                                    
                                    new acc_Ledger{ LedgerID = Guid.NewGuid(), InitialBalance = 0, LedgerName = "Forfeiture", GroupID = 13,  EditUser = x, EditDate = DateTime.Now , RestrictDelete = true, IsSystemDefault = true}, // under Liability
                                    new acc_Ledger{ LedgerID = Guid.NewGuid(), InitialBalance = 0, LedgerName = "Company Current Account", GroupID = 14,  EditUser = x, EditDate = DateTime.Now , RestrictDelete = true, IsSystemDefault = true}, // under asset
                                    new acc_Ledger{ LedgerID = Guid.NewGuid(), InitialBalance = 0, LedgerName = "Loan", GroupID = 16,  EditUser = x, EditDate = DateTime.Now , RestrictDelete = true, IsSystemDefault = true}, // under asset
                                    new acc_Ledger{ LedgerID = Guid.NewGuid(), InitialBalance = 0, LedgerName = "Member Fund", GroupID = 12,  EditUser = x, EditDate = DateTime.Now , RestrictDelete = true, IsSystemDefault = true} // under asset
                                };
                    ledger.ForEach(f => entities.acc_Ledger.Add(f));
                }
                try
                {
                    entities.SaveChanges();
                    return true;
                }
                catch (Exception xx)
                {
                    message = xx.Message;
                    return false;
                }
            }
        }
    }
}