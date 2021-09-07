using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using DLL.Repository;
using Microsoft.Reporting.WebForms;

namespace PFMVC.Areas.Accounting.Controllers
{
    public class CacheBookController : BaseController
    {
        int PageID = 22;
        ReportDataSource rd;
        UnitOfWork unitOfWork = new UnitOfWork();

        public ActionResult CacheIndex()
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

        public ActionResult GetCacheBook(DateTime? fromDate, DateTime? toDate)
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            DateTime f = fromDate ?? DateTime.MinValue;
            DateTime t = toDate ?? DateTime.MaxValue;
            var v = unitOfWork.AccountingRepository.GenerateCacheBook(f, t, oCode);
            return PartialView("_CacheBook", v);
        }


        public ActionResult Report()
        {
            LocalReport lr = new LocalReport();
            //Added By Avishek Date:Jan-19-2016
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End

            string path = Path.Combine(Server.MapPath("~/Areas/Accounting/AccountingReport"), "CashBook.rdlc");
            if (System.IO.File.Exists(path))
            {
                lr.ReportPath = path;
            }
            else
            {
                return View("Report/ReportPF/Index");
            }

            DateTime f = DateTime.MinValue;
            DateTime t = DateTime.Now;
            var v = unitOfWork.AccountingRepository.GenerateCacheBook(f, t, oCode);
            var companyInformation = unitOfWork.CompanyInformationRepository.Get(o => o.CompanyID == oCode).Single();

            ReportParameterCollection reportParameters = new ReportParameterCollection();
            reportParameters.Add(new ReportParameter("rpFromDate", f + ""));
            reportParameters.Add(new ReportParameter("rpToDate", t + ""));
            reportParameters.Add(new ReportParameter("rpCompanyName", companyInformation.CompanyName));
            reportParameters.Add(new ReportParameter("rpCompanyAddress", companyInformation.CompanyAddress));
            
            lr.SetParameters(reportParameters);
            rd = new ReportDataSource("DataSet1", v);
            lr.DataSources.Add(rd);

            string reportType = "PDF";
            string mimeType;
            string encoding;
            string fileNameExtension;
            string deviceInfo =

            @"<DeviceInfo>" +
            "  <OutputFormat>PDF</OutputFormat>" +
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
