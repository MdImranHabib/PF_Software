using System.Linq;
using DLL;

namespace PFMVC.common
{
    public class PagePermission
    {
        public static bool HasPermission(string userName, int pageID, int mode)
        {
            using (PFTMEntities context = new PFTMEntities())
            {
                var v = context.tbl_User.Where(w => w.LoginName == userName).Select(s => new { s.UserID, s.RoleID, s.IsActive }).SingleOrDefault();
                if (v == null)
                {
                    return false;
                }
                else if (v.RoleID == 0)
                {
                    return false;
                }
                else if (v.IsActive == 0)
                {
                    return false;
                }
                var permission = context.SA_tbl_PagePermission.Where(w => w.PageID == pageID && w.RoleID == v.RoleID).SingleOrDefault();
                if (permission == null)
                {
                    return false;
                }
                else
                {
                    if (mode == 0)//visit
                    {
                        if (permission.CanVisit == true)
                            return true;
                        else
                            return false;
                    }
                    else if(mode == 1)//edit
                    {
                        if (permission.CanEdit == true)
                            return true;
                        else
                            return false;
                    }
                    else if (mode == 2)//delete
                    {
                        if (permission.CanDelete == true)
                            return true;
                        else
                            return false;
                    }
                    else if (mode == 3)//execute
                    {
                        if (permission.CanExecute == true)
                            return true;
                        else
                            return false;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }

    }
}