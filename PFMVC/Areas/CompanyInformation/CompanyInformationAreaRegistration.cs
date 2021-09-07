using System.Web.Mvc;

namespace PFMVC.Areas.CompanyInformation
{
    public class CompanyInformationAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "CompanyInformation";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "CompanyInformation_default",
                "CompanyInformation/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }

}
