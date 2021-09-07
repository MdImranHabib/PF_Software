using System.Web.Mvc;

namespace PFMVC.Areas.MemberInOut 
{
    public class MemberInOutAreaRegistration:AreaRegistration
    {
        public override string AreaName {
            get {
                return "MemberInOut";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) {
            context.MapRoute(
                "MemberInOut_default",
                "MemberInOut/{controller}/{action}/{id}",
                new {
                    action = "Index", id = UrlParameter.Optional
                }
            );
        }
    }
}