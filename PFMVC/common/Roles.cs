using System;
using System.Linq;
using DLL;

namespace PFMVC.common
{
    public class Roles
    {
        public static bool AddUserToRole(string userName, string role)
        {
            using (PFTMEntities db = new PFTMEntities())
            {
                var v = db.tbl_User.Where(w => w.LoginName == userName).SingleOrDefault();
                int roleID = db.tbl_UserRole.Where(w => w.RoleName == role).Select(s => s.RoleID).SingleOrDefault();

                try
                {
                    v.RoleID = roleID;
                    db.SaveChanges();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public static bool RemoveUserFromRole(string userName)//, string role)
        {

            using (PFTMEntities db = new PFTMEntities())
            {
                var v = db.tbl_User.Where(w => w.LoginName == userName).SingleOrDefault();
                //string roleID = db.tbl_UserRole.Where(w => w.RoleName == role).Select(s => s.RoleID).SingleOrDefault();
                try
                {
                    v.RoleID = null;
                    db.SaveChanges();
                    return true;
                }
                catch (Exception x)
                {
                    return false;
                }
            }
        }
        public static int GetRoleID(string roleName)
        {
            using (PFTMEntities db = new PFTMEntities())
            {   
                int roleID = db.tbl_UserRole.Where(w => w.RoleName == roleName).Select(s => s.RoleID).SingleOrDefault();
                return roleID;
            }
            return 0;
        }
    }
}