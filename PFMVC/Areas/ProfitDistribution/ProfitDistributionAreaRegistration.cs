using System.Web.Mvc;

namespace PFMVC.Areas.ProfitDistribution
{
    public class ProfitDistributionAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "ProfitDistribution";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "ProfitDistribution_default",
                "ProfitDistribution/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
