using System.Web.Mvc;

namespace PFMVC.Areas.Loan
{
    public class LoanAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Loan";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Loan_default",
                "Loan/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
