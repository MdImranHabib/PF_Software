using DLL.DataPrepare;
using DLL.Repository;
using DLL.ViewModel;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;

// added by Fahim 19/12/2015
namespace PFMVC.Areas.Report.Controllers
{
    public class ReportForfeitureController : Controller
    {
        MvcApplication _MvcApplication;
        ReportDataSource rd;

        //
        // GET: /Report/ReportForfeiture/

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Report(string fileType, DateTime? fromDate, DateTime? toDate)
        {
            int OCode = ((int?)Session["OCode"]) ?? 0;
            string userName = Session["userName"].ToString();
            decimal _total = 0;
            DateTime fdate = fromDate.GetValueOrDefault();
            DateTime tdate = toDate.GetValueOrDefault();
            _MvcApplication = new MvcApplication();
            LocalReport lr = new LocalReport();
            decimal initialBalance = 0;
            int initialBalanceType;
            decimal creditBalanceBeforeDate;
            decimal debitBalanceBeforeDate;
            string groupName;


            string path = Path.Combine(Server.MapPath("~/Reporting/Forfeiture"), "ForfeitureDetailsReport.rdlc");
            if (System.IO.File.Exists(path) && OCode != 0)
            {
                lr.ReportPath = path;
            }
            else
            {
                return View("Report/ReportForfeiture/Index");
            }

            List<VM_acc_VoucherDetail> _VM_acc_VoucherDetail = new List<VM_acc_VoucherDetail>();
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                if (fromDate == null)
                {
                    fromDate = DateTime.MinValue;
                }
                else
                {
                    fdate = fromDate.GetValueOrDefault();
                    fdate = fdate.AddSeconds(-1);
                }
                if (toDate == null)
                {
                    toDate = DateTime.MaxValue;
                }
                else
                {
                    tdate = toDate.GetValueOrDefault();
                    tdate = tdate.AddDays(1).AddSeconds(-1);
                }

                Guid ledgerId = unitOfWork.ChartofAccountMapingRepository.Get(x => x.MIS_Id == 6).Select(x => x.Ledger_Id).FirstOrDefault();

                //Guid _ledgerId = unitOfWork.ACC_LedgerRepository.Get().Where(w => w.LedgerName == "Forfeiture").Select(s => s.LedgerID).FirstOrDefault();

                IEnumerable<int> _voucherIdList = unitOfWork.CustomRepository.GetVoucherDetailsByLedgerId(ledgerId, OCode, fdate, tdate).Select(s => s.VoucherID);

                foreach (var item in _voucherIdList)
                {
                    var _voucherdetails = unitOfWork.CustomRepository.GetVoucherDetailsByVoucherId(item, OCode);
                    foreach (var items in _voucherdetails)
                    {
                        _VM_acc_VoucherDetail.Add(items);
                    }
                }
                unitOfWork.AccountingRepository.sp_GetTransactionBalanceBeforeDate(ledgerId, fromDate ?? DateTime.MinValue, out initialBalance, out initialBalanceType, out creditBalanceBeforeDate, out debitBalanceBeforeDate, out groupName, OCode);
                if (initialBalanceType == 1)
                {
                    _total = creditBalanceBeforeDate - debitBalanceBeforeDate;
                }
                else if (initialBalanceType == 2)
                {
                    _total = debitBalanceBeforeDate - creditBalanceBeforeDate;
                }
                var getCompany = unitOfWork.CompanyInformationRepository.GetByID(OCode);
                ReportParameterCollection reportParameters = new ReportParameterCollection();
                reportParameters.Add(new ReportParameter("rpCompanyName", getCompany.CompanyName + ""));
                reportParameters.Add(new ReportParameter("rpUserName", (userName) + ""));
                reportParameters.Add(new ReportParameter("rpBeforeDateBalance", _total + ""));
                reportParameters.Add(new ReportParameter("rpCompanyAddress", getCompany.CompanyAddress + ""));
                reportParameters.Add(new ReportParameter("rpFromDate", fromDate.HasValue ? fromDate.Value.ToString("MM/dd/yyyy") : string.Empty + ""));
                reportParameters.Add(new ReportParameter("rpToDate", toDate.HasValue ? toDate.Value.ToString("MM/dd/yyyy") : string.Empty + ""));
                lr.SetParameters(reportParameters);
            }
            _VM_acc_VoucherDetail = _VM_acc_VoucherDetail.OrderBy(x => x.TransactionDate).ToList();
            rd = new ReportDataSource("DataSet1", _VM_acc_VoucherDetail);
            lr.DataSources.Add(rd);

            string reportType = fileType;
            string mimeType;
            string encoding;
            string fileNameExtension;
            string deviceInfo =

            "<DeviceInfo>" +
            "  <OutputFormat>" + fileType + "</OutputFormat>" +
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

        //public ActionResult Report(string fileType, DateTime? fromDate, DateTime? toDate)
        //{
        //    int OCode = ((int?)Session["OCode"]) ?? 0;
        //    string userName = Session["userName"].ToString();
        //    decimal _total = 0;
        //    DateTime fdate = fromDate.GetValueOrDefault();
        //    DateTime tdate = toDate.GetValueOrDefault();
        //    _MvcApplication = new MvcApplication();
        //    LocalReport lr = new LocalReport();
        //    decimal initialBalance = 0;
        //    int initialBalanceType;
        //    decimal creditBalanceBeforeDate;
        //    decimal debitBalanceBeforeDate;
        //    string groupName;


        //    string path = Path.Combine(Server.MapPath("~/Reporting/Forfeiture"), "ForfeitureDetailsReport.rdlc");
        //    if (System.IO.File.Exists(path) && OCode != 0)
        //    {
        //        lr.ReportPath = path;
        //    }
        //    else
        //    {
        //        return View("Report/ReportForfeiture/Index");
        //    }

        //    List<VM_acc_VoucherDetail> _VM_acc_VoucherDetail = new List<VM_acc_VoucherDetail>();
        //    using (UnitOfWork unitOfWork = new UnitOfWork())
        //    {
        //        if (fromDate == null)
        //        {
        //            fromDate = DateTime.MinValue;
        //        }
        //        else
        //        {
        //            fdate = fromDate.GetValueOrDefault();
        //            fdate = fdate.AddSeconds(-1);
        //        }
        //        if (toDate == null)
        //        {
        //            toDate = DateTime.MaxValue;
        //        }
        //        else
        //        {
        //            tdate = toDate.GetValueOrDefault();
        //            tdate = tdate.AddDays(1).AddSeconds(-1);
        //        }

        //        Guid ledgerId = unitOfWork.ChartofAccountMapingRepository.Get(x => x.MIS_Id == 6).Select(x => x.Ledger_Id).FirstOrDefault();

        //        //Guid _ledgerId = unitOfWork.ACC_LedgerRepository.Get().Where(w => w.LedgerName == "Forfeiture").Select(s => s.LedgerID).FirstOrDefault();

        //        IEnumerable<int> _voucherIdList = unitOfWork.CustomRepository.GetVoucherDetailsByLedgerId(ledgerId, OCode, fdate, tdate).Select(s => s.VoucherID);

        //        foreach (var item in _voucherIdList)
        //        {
        //            var _voucherdetails = unitOfWork.CustomRepository.GetVoucherDetailsByVoucherId(item, OCode);
        //            foreach (var items in _voucherdetails)
        //            {
        //                _VM_acc_VoucherDetail.Add(items);
        //            }
        //        }
        //        unitOfWork.AccountingRepository.sp_GetTransactionBalanceBeforeDate(ledgerId, fromDate ?? DateTime.MinValue, out initialBalance, out initialBalanceType, out creditBalanceBeforeDate, out debitBalanceBeforeDate, out groupName, OCode);
        //        if (initialBalanceType == 1)
        //        {
        //            _total = creditBalanceBeforeDate - debitBalanceBeforeDate;
        //        }
        //        else if (initialBalanceType == 2)
        //        {
        //            _total = debitBalanceBeforeDate - creditBalanceBeforeDate;
        //        }
        //        var getCompany = unitOfWork.CompanyInformationRepository.GetByID(OCode);
        //        ReportParameterCollection reportParameters = new ReportParameterCollection();
        //        reportParameters.Add(new ReportParameter("rpCompanyName", getCompany.CompanyName + ""));
        //        reportParameters.Add(new ReportParameter("rpUserName", (userName) + ""));
        //        reportParameters.Add(new ReportParameter("rpBeforeDateBalance", _total + ""));
        //        reportParameters.Add(new ReportParameter("rpCompanyAddress", getCompany.CompanyAddress + ""));
        //        reportParameters.Add(new ReportParameter("rpFromDate", fromDate.HasValue ? fromDate.Value.ToString("MM/dd/yyyy") : string.Empty + ""));
        //        reportParameters.Add(new ReportParameter("rpToDate", toDate.HasValue ? toDate.Value.ToString("MM/dd/yyyy") : string.Empty + ""));
        //        lr.SetParameters(reportParameters);
        //    }
        //    _VM_acc_VoucherDetail = _VM_acc_VoucherDetail.OrderBy(x => x.TransactionDate).ToList();
        //    rd = new ReportDataSource("DataSet1", _VM_acc_VoucherDetail);
        //    lr.DataSources.Add(rd);

        //    string reportType = fileType;
        //    string mimeType;
        //    string encoding;
        //    string fileNameExtension;
        //    string deviceInfo =

        //    "<DeviceInfo>" +
        //    "  <OutputFormat>" + fileType + "</OutputFormat>" +
        //    "</DeviceInfo>";

        //    Warning[] warnings;
        //    string[] streams;
        //    byte[] renderedBytes;

        //    renderedBytes = lr.Render(
        //        reportType,
        //        deviceInfo,
        //        out mimeType,
        //        out encoding,
        //        out fileNameExtension,
        //        out streams,
        //        out warnings);

        //    return File(renderedBytes, mimeType);
        //}

    }
}
