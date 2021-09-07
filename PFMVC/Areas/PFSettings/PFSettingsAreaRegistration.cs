using System.Web.Mvc;

namespace PFMVC.Areas.PFSettings
{
    public class PFSettingsAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "PFSettings";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "PFSettings_default",
                "PFSettings/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
