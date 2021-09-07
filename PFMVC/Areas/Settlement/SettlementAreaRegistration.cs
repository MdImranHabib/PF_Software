using System.Web.Mvc;

namespace PFMVC.Areas.Settlement
{
    public class SettlementAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Settlement";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Settlement_default",
                "Settlement/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
