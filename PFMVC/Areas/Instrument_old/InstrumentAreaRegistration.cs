using System.Web.Mvc;

namespace PFMVC.Areas.Instrument
{
    public class InstrumentAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Instrument";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Instrument_default",
                "Instrument/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
