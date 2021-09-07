using System;
using System.Net;
using DLL;

namespace PFMVC.common
{
    public static class LogApplicationError
    {

        public static void LogApplicationError1(string Message, string InnerException, string UserName)
        {
            using (PFTMEntities context = new PFTMEntities())
            {
                tbl_ErrorLog error_log = new tbl_ErrorLog();
                error_log.Message = Message;
                error_log.InnerException = InnerException + "";
                error_log.UserName = UserName;
                error_log.Time = DateTime.Now;
                error_log.Type = "Application Error";
                try
                {
                    error_log.Terminal = System.Web.HttpContext.Current.Request.UserHostAddress;
                }
                catch
                {

                }
                error_log.HostName = Dns.GetHostName();
                try
                {
                    context.tbl_ErrorLog.Add(error_log);
                    context.SaveChanges();
                }
                catch
                {

                }
            }
        }
    }
}