using System.Web.Mvc;

namespace PFMVC.Controllers
{
    public class DownloadController : Controller
    {
        //
        // GET: /Download/

        public ActionResult SampleSalaryImportSheet()
        {
            return File("~/TestFiles/monthlySalaryContribution.xlsx", "application/vnd.ms-excel", "monthlySalaryContribution.xlsx");
        }

        public ActionResult SampleEmployeeImportSheet()
        {
            return File("~/TestFiles/EmployeeImport.xlsx", "application/vnd.ms-excel", "EmployeeImport.xlsx");
        }

        public ActionResult SampleLoanPaymentSheet()
        {
            return File("~/TestFiles/LoanPayment.xlsx", "application/vnd.ms-excel", "LoanPayment.xlsx");
        }
    }
}
