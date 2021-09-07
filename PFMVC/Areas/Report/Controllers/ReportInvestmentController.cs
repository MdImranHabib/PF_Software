using DLL;
using DLL.Repository;
using DLL.ViewModel;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace PFMVC.Areas.Report.Controllers
{
    public class ReportInvestmentController : Controller
    {
        ReportDataSource _rd;

        public ActionResult Index()
        {
            return View();
        }

        public JsonResult AutocompleteSuggestionsForInstrument(string term)
        {
            try
            {
                UnitOfWork unitOfWork = new UnitOfWork();
                int oCode = ((int?)Session["OCode"]) ?? 0;
                var suggestions = unitOfWork.InstrumentRepository.Get().Where(w => w.InstrumentNumber.ToLower().Trim().Contains(term.ToLower().Trim()) && w.OCode == oCode).Select(s => new
                {
                    label = s.InstrumentNumber,
                    value = s.InstrumentID
                }).ToList();
                return Json(suggestions, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public JsonResult AutocompleteSuggestionsForInstitution(string term)
        {
            try
            {
                UnitOfWork unitOfWork = new UnitOfWork();
                int oCode = ((int?)Session["OCode"]) ?? 0;
                var suggestions = unitOfWork.InstrumentRepository.Get().Where(w => w.Institution.ToLower().Trim().Contains(term.ToLower().Trim()) && w.OCode == oCode).Select(s => new
                {
                    value = s.Institution.Replace("&", "%26").Replace("#", "%23"),
                    label = s.Institution//Regex.IsMatch(s.Institution, @"^[ A-Za-z0-9]$&,^")
                }).ToList().Distinct();
                return Json(suggestions, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public JsonResult AutocompleteSuggestionsForBranch(string term)
        {
            var sessionId = Session["InstrumentID"].ToString();
            string sessionInstitution = Session["Institution"].ToString();
            if (string.IsNullOrEmpty(sessionInstitution))
            {
                try
                {
                    UnitOfWork unitOfWork = new UnitOfWork();
                    int oCode = ((int?)Session["OCode"]) ?? 0;
                    var suggestions = unitOfWork.InstrumentRepository.Get().Where(w => w.InstrumentID.ToString() == sessionId && w.OCode == oCode).Select(s => new
                    {
                        value = s.Branch,
                        label = s.Branch
                    }).ToList();
                    return Json(suggestions, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            else
            {
                try
                {
                    UnitOfWork unitOfWork = new UnitOfWork();
                    int oCode = ((int?)Session["OCode"]) ?? 0;
                    var suggestions = unitOfWork.InstrumentRepository.Get().Where(w => w.Institution.ToLower() == sessionInstitution.ToLower() && w.OCode == oCode).Select(s => new
                    {
                        value = s.Branch,
                        label = s.Branch
                    }).ToList();
                    return Json(suggestions, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public JsonResult GetInstrumentInformation(string identificationNumber)
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            try
            {
                UnitOfWork unitOfWork = new UnitOfWork();
                var suggestion = unitOfWork.InstrumentRepository.Get().Where(w => (w.InstrumentID.ToString() == identificationNumber.ToString()) && (w.OCode == oCode)).Select(s => new
                {
                    value = s.Institution,
                    label = s.Branch
                }).FirstOrDefault();
                Session["InstrumentID"] = identificationNumber;
                return Json(suggestion);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void SetBranchSessionId(string institution, string instrumentId)
        {
            Session["Institution"] = institution;
            Session["InstrumentID"] = instrumentId;
        }

        public ActionResult Report(string fileType, string reportOptions, string Institute, DateTime? fromDate = null, DateTime? toDate = null)
        {
            Institute = Institute.Replace("%26", "&").Replace("%23", "#");
            int oCode = ((int?)Session["OCode"]) ?? 0;
            string userName = Session["userName"].ToString();
            DateTime fdate = fromDate.GetValueOrDefault();
            DateTime tdate = toDate.GetValueOrDefault();
            LocalReport lr = new LocalReport();


            if (reportOptions == "InvestmentWithDateRange")
            {
                string path = Path.Combine(Server.MapPath("~/Reporting/Investment"), "InvestmentWithDateRange.rdlc");
                if (System.IO.File.Exists(path) && oCode != 0)
                {
                    lr.ReportPath = path;
                }
                else
                {
                    return View("Report/ReportInvestment/Index");
                }

                // List<VM_Instrument> instrumentList = new List<VM_Instrument>();
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

                    //var v = unitOfWork.InstrumentRepository.Get().Where(x => x.DepositDate >= fromDate && x.DepositDate <= toDate).Select
                    var v = unitOfWork.InstrumentRepository.Get().Where(x => x.DepositDate <= toDate).Select
                        (a => new
                        {
                            a.InstrumentID,
                            a.InstrumentNumber,
                            a.InstrumentType,
                            a.Institution,
                            a.Branch,
                            a.DepositDate,
                            a.MaturityDate,
                            a.MaturityPeriod,
                            a.InterestRate,
                            OpeningPrincipal = 0,
                            Addition = a.Amount,
                            Encashment = 0,
                            OpeningInterest = 0,
                            Accrued = a.AcruadIntarestAmount,
                            InterestReceived = 0
                        }).ToList();

                    #region
                    //List<tbl_Instrument> instrumentDetails = new List<tbl_Instrument>();
                    //decimal initialBalance = 0;
                    //decimal total = 0;
                    //if (!string.IsNullOrEmpty(instrumentId))
                    //{
                    //    instrumentDetails = unitOfWork.CustomRepository.GetInstrumentById(instrumentId, institution, branch);
                    //}
                    //else if ((instrumentId == null && (institution != null && branch != null)) || (instrumentId == "" && (institution != "" && branch != "")))
                    //{
                    //    instrumentDetails = unitOfWork.CustomRepository.GetInstrumentById(instrumentId, institution, branch);
                    //}

                    //string ledgerName;

                    //foreach (tbl_Instrument item in instrumentDetails)
                    //{
                    //    ledgerName = item.InstrumentType + "-" + item.InstrumentNumber;
                    //    var ledgerId = unitOfWork.ACC_LedgerRepository.Get().Where(w => w.LedgerName == ledgerName).Select(s => s.LedgerID).FirstOrDefault();
                    //    int initialBalanceType;
                    //    decimal creditBalanceBeforeDate;
                    //    decimal debitBalanceBeforeDate;
                    //    string groupName;
                    //    unitOfWork.AccountingRepository.sp_GetTransactionBalanceBeforeDate(ledgerId, fromDate ?? DateTime.MinValue, out initialBalance, out initialBalanceType, out creditBalanceBeforeDate, out debitBalanceBeforeDate, out groupName, oCode);
                    //    if (initialBalanceType == 1)
                    //    {
                    //        total = total + debitBalanceBeforeDate;
                    //    }
                    //    else if (initialBalanceType == 2)
                    //    {
                    //        total = total + creditBalanceBeforeDate;
                    //    }
                    //    List<VM_acc_VoucherDetail> voucherIdList = unitOfWork.AccountingRepository.GetVoucherDetailWithLedgerIDId(ledgerId).ToList();
                    //    foreach (var items in voucherIdList)
                    //    {
                    //        List<VM_acc_VoucherDetail> voucherDetail = unitOfWork.AccountingRepository.GetVoucherDetail(items.VoucherID).ToList();
                    //        foreach (var item1 in voucherDetail)
                    //        {
                    //            VM_Instrument instrument = new VM_Instrument();
                    //            instrument.InstrumentNumber = item.InstrumentNumber;
                    //            instrument.LedgerName = item1.LedgerName;
                    //            instrument.ChequeNumber = item1.ChequeNumber;
                    //            instrument.VoucherNumber = item1.VNumber;
                    //            instrument.Debit = item1.Debit.ToString(CultureInfo.InvariantCulture);
                    //            instrument.Credit = item1.Credit.ToString(CultureInfo.InvariantCulture);
                    //            instrument.TransactionDate = item1.TransactionDate ?? DateTime.MinValue;
                    //            instrumentList.Add(instrument);
                    //        }
                    //    }
                    //}
                    #endregion

                    var getCompany = unitOfWork.CompanyInformationRepository.GetByID(oCode);
                    ReportParameterCollection reportParameters = new ReportParameterCollection
                    {
                        new ReportParameter("rpCompanyName", getCompany.CompanyName + ""),
                        new ReportParameter("rpUserName", userName + ""),
                        new ReportParameter("rpCompanyAddress", getCompany.CompanyAddress + ""),
                        //new ReportParameter("rpInstitution", institution + ""),
                        //new ReportParameter("rpBranch", branch + ""),
                        //new ReportParameter("rpInitialBalance", initialBalance + ""),
                        //new ReportParameter("rpOpeningBalance", total + ""),
                        new ReportParameter("rpFromDate",
                            fromDate.Value.ToString("MM/dd/yyyy")),
                        new ReportParameter("rpToDate",
                            toDate.Value.ToString("MM/dd/yyyy"))
                    };
                    lr.SetParameters(reportParameters);
                    _rd = new ReportDataSource("DataSet1", v);
                    lr.DataSources.Add(_rd);
                }
                //instrumentList = instrumentList.OrderBy(x => x.TransactionDate).ToList();

            }
            else if (reportOptions == "InvestmentWithInstrumentWise")
            {
                string path = Path.Combine(Server.MapPath("~/Reporting/Investment"), "InvestmentWithInstrumentWise.rdlc");
                if (System.IO.File.Exists(path) && oCode != 0)
                {
                    lr.ReportPath = path;
                }
                else
                {
                    return View("Report/ReportInvestment/Index");
                }

                // List<VM_Instrument> instrumentList = new List<VM_Instrument>();
                using (UnitOfWork unitOfWork = new UnitOfWork())
                {


                    var v = unitOfWork.InstrumentRepository.Get().Where(x => x.Institution == Institute).Select
                        (a => new
                        {
                            a.InstrumentID,
                            a.InstrumentNumber,
                            a.InstrumentType,
                            a.Institution,
                            a.Branch,
                            a.DepositDate,
                            a.MaturityDate,
                            a.MaturityPeriod,
                            a.InterestRate,
                            OpeningPrincipal = 0,
                            Addition = a.Amount,
                            Encashment = 0,
                            OpeningInterest = 0,
                            Accrued = a.AcruadIntarestAmount,
                            InterestReceived = 0
                        }).Where(i=>i.Institution==Institute).ToList();


                    var getCompany = unitOfWork.CompanyInformationRepository.GetByID(oCode);
                    ReportParameterCollection reportParameters = new ReportParameterCollection
                    {
                        new ReportParameter("rpCompanyName", getCompany.CompanyName + ""),
                        new ReportParameter("rpUserName", userName + ""),
                        new ReportParameter("rpCompanyAddress", getCompany.CompanyAddress + ""),                        
                     
                    };
                    lr.SetParameters(reportParameters);
                    _rd = new ReportDataSource("DataSet1", v);
                    lr.DataSources.Add(_rd);
                }
            }


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


    }
}
