using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DLL.ViewModel;


namespace DLL.DataPrepare
{
    public class DP_UserManagement
    {
        public tbl_User userProfile(VM_UserInfo x)
        {
            tbl_User y = new tbl_User();
            y.UserID = x.UserId;
            y.LoginName = x.UserName;
            y.BranchID = x.BranchID;
            y.UserFullName = x.FullName;
            y.IsActive = x.IsActive == false ? (byte)0 : (byte)1;
            y.Phone = x.Phone;
            y.EmailNotificationActive = x.EmailNotificationActive == false ? (byte)0 : (byte)1;
            y.Email = x.Email;
            y.DepartmentID = x.DepartmentID;
            return y;
        }

        public VM_UserInfo vm_userManagement(tbl_User x)
        {
            VM_UserInfo y = new VM_UserInfo();
            y.UserId = x.UserID;
            y.UserName = x.LoginName;
            y.BranchID = x.BranchID ?? 0;
            y.FullName = x.UserFullName;
            y.IsActive = x.IsActive == 0 ? false : true;
            y.Phone = x.Phone;
            y.EmailNotificationActive = x.EmailNotificationActive == 0 ? false : true;
            y.Email = x.Email;
            y.DepartmentID = x.DepartmentID;
            return y;
        }
    }
}
