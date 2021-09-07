using System.Web.Mvc;

namespace PFMVC.Areas.Contribution
{
    public class ContributionAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Contribution";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Contribution_default",
                "Contribution/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
