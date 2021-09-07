using System.Web.Mvc;

namespace PFMVC.Areas.PFContribution
{
    public class PFContributionAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "PFContribution";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "PFContribution_default",
                "PFContribution/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
