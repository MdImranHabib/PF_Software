using System.Linq;
using DLL;

namespace PFMVC.common
{
    public class GetRoles
    {
        public static string GetRoleForUser(string userName)
        {
            using (PFTMEntities db = new PFTMEntities())
            {
                var currentRoles = (from a in db.tbl_User.Where(w => w.LoginName == userName)
                                      join b in db.tbl_UserRole on a.RoleID equals b.RoleID
                                      select new
                                      {
                                          RoleName = b.RoleName
                                      }).FirstOrDefault();

                if (currentRoles != null)
                {
                    return currentRoles.RoleName + "";
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public static string[] GetAllRoles()
        {
            using (PFTMEntities db = new PFTMEntities())
            {
                string[] currentRoles = (from a in db.tbl_UserRole
                                         select a
                                        ).Select(x => x.RoleName).ToArray();
                return currentRoles;
            }
        }
    }
}