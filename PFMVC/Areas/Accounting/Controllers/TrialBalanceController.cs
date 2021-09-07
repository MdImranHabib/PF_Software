using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using DLL.Repository;
using Microsoft.Reporting.WebForms;

namespace PFMVC.Areas.Accounting.Controllers
{
    public class TrialBalanceController : BaseController
    {


        int PageID = 19;
        ReportDataSource rd;
        UnitOfWork unitOfWork = new UnitOfWork();

        public ActionResult TrialBalanceIndex()
        {
            //Added By Avishek Date:Jan-19-2016
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End
            return View();
        }

        public ActionResult TrialBalanceFilter()
        {
            ViewData["Nature"] = new SelectList(unitOfWork.ACC_NatureRepository.Get(), "NatureID", "NatureName");
            ViewData["Group"] = new SelectList(unitOfWork.ACC_GroupRepository.Get(), "GroupID", "GroupName");
            ViewData["Ledger"] = new SelectList(unitOfWork.ACC_LedgerRepository.Get(), "LedgerID", "LedgerName");
            return PartialView("_TrialBalanceFilter");
        }


        public ActionResult GetTrialBalance(Guid? ledgerID, int? groupID, int? natureID, DateTime? toDate)
        {
            int OCode = ((int?)Session["OCode"]) ?? 0;
            //DateTime f = fromDate ?? DateTime.MinValue;
            //DateTime t = toDate ?? DateTime.MaxValue;
            //var v = unitOfWork.AccountingRepository.sp_GenerateTrialBalance(ledgerID, natureID, groupID);
            DateTime f = DateTime.MinValue;
            DateTime t = toDate ?? Convert.ToDateTime("01/01/3099");

            var v = unitOfWork.AccountingRepository.GenerateBalanceBook2(ledgerID, natureID, groupID,f, t.AddHours(23).AddMinutes(59).AddSeconds(59), OCode);
            return PartialView("_TrialBalanceModel", v);
        }

        public ActionResult Report(DateTime? toDate)
        {
            LocalReport lr = new LocalReport();
            //Added By Avishek Date:Jan-19-2016
            int OCode = ((int?)Session["OCode"]) ?? 0;
            if (OCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End
            //get company information
            var company = unitOfWork.CompanyInformationRepository.Get().Where(g => g.CompanyID == OCode).Single();

       
            string path = Path.Combine(Server.MapPath("~/Areas/Accounting/AccountingReport"), "TrialBalance.rdlc");
            if (System.IO.File.Exists(path))
            {
                lr.ReportPath = path;
            }
            else
            {
                return View("Report/ReportPF/Index");
            }

            DateTime f = DateTime.MinValue;
            DateTime t = toDate ?? DateTime.Now;
            var v = unitOfWork.AccountingRepository.GenerateBalanceBook2(null, null, null,f, t.AddHours(23).AddMinutes(59).AddSeconds(59), OCode);

            ReportParameterCollection reportParameters = new ReportParameterCollection();
            reportParameters.Add(new ReportParameter("rpDateTimeASON", t+""));
            reportParameters.Add(new ReportParameter("rpCompanyName", company.CompanyName));
            reportParameters.Add(new ReportParameter("rpCompanyAddress", company.CompanyAddress));
            //reportParameters.Add(new ReportParameter("EmpName", vm_employee.EmpName));
            //reportParameters.Add(new ReportParameter("JoiningDate", vm_employee.JoiningDate + ""));
            //reportParameters.Add(new ReportParameter("Designation", vm_employee.DesignationName));
            //reportParameters.Add(new ReportParameter("Branch", vm_employee.BranchName));
            lr.SetParameters(reportParameters);
            

            rd = new ReportDataSource("DataSet1", v);
            lr.DataSources.Add(rd);

            string reportType = "PDF";
            string mimeType;
            string encoding;
            string fileNameExtension;



            string deviceInfo =

            "<DeviceInfo>" +
            "  <OutputFormat>" + "PDF" + "</OutputFormat>" +
                //"  <PageWidth></PageWidth>" +
                //"  <PageHeight></PageHeight>" +
                //"  <MarginTop>0.0in</MarginTop>" +
                //"  <MarginLeft>0.0in</MarginLeft>" +
                //"  <MarginRight>0.0in</MarginRight>" +
                //"  <MarginBottom>0.0in</MarginBottom>" +
            "</DeviceInfo>";

            Warning[] warnings;
            string[] streams;
            byte[] renderedBytes;

            renderedBytes = lr.Render(
                reportType,
                deviceInfo,
                out mimeType,
                out encoding,
                out fileNameExtension,
                out streams,
                out warnings);


            return File(renderedBytes, mimeType);
        }

    }
}
