using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using DLL.ViewModel;
using Microsoft.Reporting.WebForms;
using System.IO;

namespace PFMVC.Areas.Accounting.Controllers
{
    public class RPController : BaseController
    {
        //Receipt & Payment Controller
      

        public ActionResult RPIndex()
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

        public ActionResult RPFilter()
        {
            return PartialView("_RPFilter");
        }

        public ActionResult GetRP(DateTime? fromDate, DateTime? toDate) 
        {
            //Added By Avishek Date:Jan-19-2016
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End
            DateTime f = fromDate ?? Convert.ToDateTime("01/01/2000");
            DateTime t = toDate ?? Convert.ToDateTime("01/01/2040");
            //that important
            t = t.AddHours(23).AddMinutes(59).AddSeconds(59);

            List<VM_acc_VoucherDetail> cashGridBefore = unitOfWork.AccountingRepository.GenerateCashBalance(DateTime.MinValue, f.AddDays(-1), oCode); // -1 is added before I want result before that day but in repository query is <=
            List<VM_acc_VoucherDetail> receiptGrid = unitOfWork.AccountingRepository.GenerateRP(f, t, 2, oCode).Where(d => d.Credit > 0).ToList(); //need only credit part here...
            List<VM_acc_VoucherDetail> paymentGrid = unitOfWork.AccountingRepository.GenerateRP(f, t, 1, oCode).Where(d => d.Debit>0).ToList(); //need only debit part here...;
            List<VM_acc_VoucherDetail> cashGridAfter = unitOfWork.AccountingRepository.GenerateCashBalance(f, t, oCode) ;
            
            ViewBag.FromDate = f.ToString("dd/MMM/yyyy");
            ViewBag.ToDate = t.ToString("dd/MMM/yyyy");
            return PartialView("_RPModel", new Tuple<List<VM_acc_VoucherDetail>, List<VM_acc_VoucherDetail>,List<VM_acc_VoucherDetail>,List<VM_acc_VoucherDetail>>(cashGridBefore,  receiptGrid, paymentGrid,cashGridAfter));
        }

        public ActionResult GetRPReport(DateTime? fromDate, DateTime? toDate)
        {
            LocalReport lr = new LocalReport();
            //Added By Avishek Date:Jan-19-2016
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End
            DateTime f = fromDate ?? DateTime.MinValue;
            DateTime t = toDate ?? DateTime.MaxValue;
            //that important
            t = t.AddHours(23).AddMinutes(59).AddSeconds(59);

            //check report physical path
            string path = Path.Combine(Server.MapPath("~/Areas/Accounting/AccountingReport"), "RP.rdlc");
            if (System.IO.File.Exists(path))
            {
                lr.ReportPath = path;
            }
            else
            {
                return View("Report/ReportPF/Index");
            }
            //if exist 
            List<VM_acc_VoucherDetail> cashBefore = unitOfWork.AccountingRepository.GenerateCashBalance(DateTime.MinValue, f.AddDays(-1), oCode); // -1 is added before I want result before that day but in repository query is <=
            List<VM_acc_VoucherDetail> receipt = unitOfWork.AccountingRepository.GenerateRP(f, t, 2, oCode).Where(d => d.Credit > 0).ToList(); //need only credit part here...
            List<VM_acc_VoucherDetail> payment = unitOfWork.AccountingRepository.GenerateRP(f, t, 1, oCode).Where(d => d.Debit > 0).ToList(); //need only debit part here...;
            List<VM_acc_VoucherDetail> cashAfter = unitOfWork.AccountingRepository.GenerateCashBalance(f, t, oCode);


            var companyInformation = unitOfWork.CompanyInformationRepository.Get(o => o.CompanyID == oCode).Single();
            if (companyInformation != null)
            {
                ReportParameterCollection reportParameters = new ReportParameterCollection();
                reportParameters.Add(new ReportParameter("rpCompanyName", companyInformation.CompanyName));
                reportParameters.Add(new ReportParameter("rpCompanyAddress", companyInformation.CompanyAddress));
                reportParameters.Add(new ReportParameter("rpFromDate", f.ToString("dd/MMM/yyyy")));
                reportParameters.Add(new ReportParameter("rpToDate", t.ToString("dd/MMM/yyyy")));
                //reportParameters.Add(new ReportParameter("rpTotal", (total_asset < total_liabilities ? total_liabilities : total_asset) + ""));
                lr.SetParameters(reportParameters);
            }

            lr.DataSources.Clear();
            ReportDataSource rd1 = new ReportDataSource("DS_CashBefore", cashBefore);
            ReportDataSource rd2 = new ReportDataSource("DS_Receipt", receipt);
            ReportDataSource rd3 = new ReportDataSource("DS_Payment", payment);
            ReportDataSource rd4 = new ReportDataSource("DS_CashAfter", cashAfter);

            lr.DataSources.Add(rd1);
            lr.DataSources.Add(rd2);
            lr.DataSources.Add(rd3);
            lr.DataSources.Add(rd4);

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
