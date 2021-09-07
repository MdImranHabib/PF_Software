//Project KDS Edited by Fahim 23/11/2015
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using DLL.Repository;
using DLL.ViewModel;
using Microsoft.Reporting.WebForms;
using PFMVC.common;
using System.Globalization;
using DLL;

namespace PFMVC.Areas.Report.Controllers
{
    public class ReportLoanController : Controller
    {
        MvcApplication _MvcApplication;
        int PageID = 18;
        ReportDataSource rd;

        private UnitOfWork unitOfWork = new UnitOfWork();
        [Authorize]
        public ActionResult Index()
        {
            //Added By Avishek Date:Jan-19-2016
            int OCode = ((int?)Session["OCode"]) ?? 0;
            if (OCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End

            Session["EmpId"] = "";
            ViewBag.PageName = "Rport section";

            bool b = PagePermission.HasPermission(User.Identity.Name, PageID, 0);
            if (b)
            {
                return View();
            }
            else
            {
                ViewBag.PageName = "PF Report";
                return View("Unauthorized");
            }
        }

        //Edited by Fahim 22/11/2015 Added if else Condition 
        /// <summary>
        /// Autocompletes the suggestions loan.
        /// </summary>
        /// <param name="term">The term.</param>
        /// <returns>list</returns>
        /// <modifiedBy>Avishek</modifiedBy>
        /// <ModifiedDate>Aug-1-2015</ModifiedDate>
        public JsonResult AutocompleteSuggestionsLoan(string term)
        {
            try
            {
                int OCode = ((int?)Session["OCode"]) ?? 0;
                string _sessionEmpId = Session["EmpId"].ToString();
                if (_sessionEmpId.Length == 1) //Added by Fahim 22/11/2015
                {
                    _sessionEmpId = "0" + _sessionEmpId;
                }
                if (_sessionEmpId != null && _sessionEmpId != "")
                {
                    var suggestions = unitOfWork.CustomRepository.EmployeeWithLoanByLoanId(OCode, term).Select(s => new
                    {
                        label = s.LoanID,
                        value = s.IdentificationNumber
                    }).Where(x => x.value == _sessionEmpId).GroupBy(x => x.value).Select(g => g.FirstOrDefault()).ToList();
                    return Json(suggestions, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var suggestions = unitOfWork.CustomRepository.EmployeeWithLoanByLoanId(OCode, term).Select(s => new
                    {
                        label = s.LoanID,
                        value = s.IdentificationNumber
                    }).GroupBy(x => x.value).Select(g => g.FirstOrDefault()).ToList();
                    return Json(suggestions, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Autocompletes the suggestions.
        /// </summary>
        /// <param name="term">The term.</param>
        /// <returns>list</returns>
        /// <CreatedBy>Avishek</CreatedBy>
        /// <createdDate>Aug-1-2015</createdDate>
        public JsonResult AutocompleteSuggestionsForEmpId(string term)
        {
            try
            {
                int OCode = ((int?)Session["OCode"]) ?? 0;
                var suggestions = unitOfWork.CustomRepository.EmployeeWithLoan(OCode, term).Select(s => new
                {
                    value = s.EmpName,
                    label = s.IdentificationNumber
                }).GroupBy(x => x.label).Select(g => g.FirstOrDefault()).ToList();
                return Json(suggestions, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        /// <summary>
        /// Autocompletes the name of the suggestions.
        /// </summary>
        /// <param name="term">The term.</param>
        /// <returns>list</returns>
        /// <CreatedBy>Fahim</CreatedBy>
        /// <CreatedDate>04/11/2015</CreatedDate>
        public JsonResult AutocompleteSuggestionsName(string term)
        {
            int OCode = ((int?)Session["OCode"]) ?? 0;
            var suggestions = unitOfWork.CustomRepository.GetEmployeeByName(term).Where(x => x.LoanId != null).Select(s => new { value = s.IdentificationNumber, label = s.EmpName }).ToList();
            return Json(suggestions, JsonRequestBehavior.AllowGet);
        }

        //End Fahim

        //Added by Fahim 22/11/2015
        public JsonResult GetEmpName(int empId)
        {
            try
            {
                int OCode = ((int?)Session["OCode"]) ?? 0;
                string suggestions = unitOfWork.CustomRepository.GetEmployeeById(empId).Select(x => x.EmpName).FirstOrDefault();
                return Json(suggestions, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        //End Fahim

        //Edited by Fahim 22/11/2015
        public JsonResult GetEmpId(string identificationNo)
        {
            //Added by Fahim 22/11/2015
            if (identificationNo == "")
            {
                Session["EmpId"] = "";
                return Json("");
            }
            //End Fahim
            try
            {
                int OCode = ((int?)Session["OCode"]) ?? 0;
                int suggestions = unitOfWork.CustomRepository.GetEmployeeByIdentificationNumber(identificationNo.Trim()).Select(s => s.EmpID).FirstOrDefault();
                Session["EmpId"] = suggestions.ToString();
                return Json(suggestions);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public ActionResult Report(string id = "", string reportOptions = "", int empID = 0, DateTime? fromDate = null, DateTime? toDate = null, string loanID = "")
        {
            //Added By Avishek Date:Jan-19-2016
            int OCode = ((int?)Session["OCode"]) ?? 0;
            if (OCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End
            string userName = Session["userName"].ToString();
            _MvcApplication = new MvcApplication();
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                var user = unitOfWork.UserProfileRepository.Get().Where(w => w.LoginName == User.Identity.Name && w.OCode == OCode).SingleOrDefault();
                if (user != null)
                {
                    if (user.EmpID > 0)
                    {
                        empID = user.EmpID ?? 0;
                    }
                }
            }

            LocalReport lr = new LocalReport();

            #region LoanList
            if (reportOptions == "LoanList")
            {
                string path = Path.Combine(Server.MapPath("~/Reporting/Loan"), "LoanList.rdlc");
                if (System.IO.File.Exists(path))
                {
                    lr.ReportPath = path;
                }
                else
                {
                    return View("Report/ReportLoan/Index");
                }
                List<VM_PFLoan> v = new List<VM_PFLoan>();
                using (UnitOfWork unitOfWork = new UnitOfWork())
                {
                    var temp = unitOfWork.CustomRepository.GetPFLoanHistoryByEmpID(OCode);

                    if (fromDate != null && toDate != null)
                    {
                        // string fromYear = fromDate.Value.Year + "";
                        //string fromMonth = fromDate.Value.Month.ToString().PadLeft(2, '0');
                        //v = temp.Where(t => Convert.ToDateTime(t.StartDate) >= fromDate && Convert.ToDateTime(t.StartDate) <= toDate).ToList();
                        v = temp.Where(t => t.StartDate >= fromDate && t.StartDate <= toDate).ToList();
                        foreach (var item in v)
                        {
                            item.Installment = _MvcApplication.GetNumber(item.Installment);
                            item.Interest = _MvcApplication.GetNumber(item.Interest);
                            item.LoanAmount = _MvcApplication.GetNumber(item.LoanAmount);
                            item.PayableAmount = _MvcApplication.GetNumber(item.PayableAmount);
                        }
                    }
                    //else if (fromDate == null && toDate != null)
                    //{
                    //    v = temp.Where(t => Convert.ToDateTime(t.StartDate) <= toDate).ToList();
                    //}
                    //else if (fromDate != null && toDate == null)
                    //{
                    //    v = temp.Where(t => Convert.ToDateTime(t.StartDate) >= fromDate).ToList();
                    //}
                    else
                    {
                        v = temp.ToList();
                    }
                    if (empID > 0)
                    {
                        v = v.Where(w => w.EmpID == empID).ToList();
                    }
                    var getCompany = unitOfWork.CompanyInformationRepository.GetByID(OCode);
                    ReportParameterCollection reportParameters = new ReportParameterCollection();
                    reportParameters.Add(new ReportParameter("rpCompanyName", getCompany.CompanyName + ""));
                    reportParameters.Add(new ReportParameter("rpCompanyAddress", getCompany.CompanyAddress + ""));
                    reportParameters.Add(new ReportParameter("rpFromDate", ((DateTime)fromDate).ToShortDateString() + ""));
                    reportParameters.Add(new ReportParameter("rpuserName", userName + ""));
                    reportParameters.Add(new ReportParameter("rpToDate", ((DateTime)toDate).ToShortDateString() + ""));
                    lr.SetParameters(reportParameters);
                }
                rd = new ReportDataSource("DataSet1", v);

            }
            #endregion

            # region LoanListWithEmpId
            else if (reportOptions == "LoanListWithEmpId")
            {
                string path = Path.Combine(Server.MapPath("~/Reporting/Loan"), "LoanList.rdlc");
                if (System.IO.File.Exists(path))
                {
                    lr.ReportPath = path;
                }
                else
                {
                    return View("Report/ReportLoan/Index");
                }
                List<VM_PFLoan> v = new List<VM_PFLoan>();
                using (UnitOfWork unitOfWork = new UnitOfWork())
                {
                    var temp = unitOfWork.CustomRepository.GetPFLoanHistoryByEmpID(OCode);

                    if (fromDate != null && toDate != null)
                    {
                        v = temp.Where(t => t.StartDate >= fromDate && t.StartDate <= toDate && t.EmpID == empID).ToList();
                        foreach (var item in v)
                        {
                            item.Installment = _MvcApplication.GetNumber(item.Installment);
                            item.Interest = _MvcApplication.GetNumber(item.Interest);
                            item.LoanAmount = _MvcApplication.GetNumber(item.LoanAmount);
                            item.PayableAmount = _MvcApplication.GetNumber(item.PayableAmount);
                        }
                    }

                    var getCompany = unitOfWork.CompanyInformationRepository.GetByID(OCode);
                    ReportParameterCollection reportParameters = new ReportParameterCollection();
                    reportParameters.Add(new ReportParameter("rpCompanyName", getCompany.CompanyName + ""));
                    reportParameters.Add(new ReportParameter("rpCompanyAddress", getCompany.CompanyAddress + ""));
                    reportParameters.Add(new ReportParameter("rpFromDate", fromDate + ""));
                    reportParameters.Add(new ReportParameter("rpToDate", toDate + ""));
                    reportParameters.Add(new ReportParameter("rpuserName", userName + ""));

                    lr.SetParameters(reportParameters);
                }
                rd = new ReportDataSource("DataSet1", v);
            }
            #endregion

            #region LoanClosed
            else if (reportOptions == "LoanClosed")
            {
                string path = Path.Combine(Server.MapPath("~/Reporting/Loan"), "LoanClosed.rdlc");
                if (System.IO.File.Exists(path))
                {
                    lr.ReportPath = path;
                }
                else
                {
                    return View("Report/ReportLoan/Index");
                }

                List<VM_PFLoan> v = new List<VM_PFLoan>();
                using (UnitOfWork unitOfWork = new UnitOfWork())
                {
                    var temp = unitOfWork.CustomRepository.GetClosedLoanForReport(OCode);
                    if (fromDate != null && toDate != null)
                    {
                        // string fromYear = fromDate.Value.Year + "";
                        //string fromMonth = fromDate.Value.Month.ToString().PadLeft(2, '0');
                        //v = temp.Where(t => Convert.ToDateTime(t.StartDate) >= fromDate && Convert.ToDateTime(t.StartDate) <= toDate).ToList();
                        v = temp.Where(t => t.StartDate >= fromDate && t.StartDate <= toDate).ToList();
                    }
                    else if (fromDate == null && toDate != null)
                    {
                        v = temp.Where(t => Convert.ToDateTime(t.StartDate) <= toDate).ToList();
                    }
                    else if (fromDate != null && toDate == null)
                    {
                        v = temp.Where(t => Convert.ToDateTime(t.StartDate) >= fromDate).ToList();
                    }
                    else
                    {
                        v = temp.ToList();
                    }
                    if (empID > 0)
                    {
                        v = v.Where(w => w.EmpID == empID).ToList();
                    }
                    foreach (var item in v)
                    {
                        item.Installment = _MvcApplication.GetNumber(item.Installment);
                        item.Interest = _MvcApplication.GetNumber(item.Interest);
                        item.LoanAmount = _MvcApplication.GetNumber(item.LoanAmount);
                        item.PayableAmount = _MvcApplication.GetNumber(item.PayableAmount);
                    }

                    var getCompany = unitOfWork.CompanyInformationRepository.GetByID(OCode);
                    ReportParameterCollection reportParameters = new ReportParameterCollection();
                    reportParameters.Add(new ReportParameter("rpCompanyName", getCompany.CompanyName + ""));
                    reportParameters.Add(new ReportParameter("rpCompanyAddress", getCompany.CompanyAddress + ""));
                    reportParameters.Add(new ReportParameter("rpFromDate", ((DateTime)fromDate).ToShortDateString() + ""));
                    reportParameters.Add(new ReportParameter("rpToDate", ((DateTime)toDate).ToShortDateString() + ""));
                    reportParameters.Add(new ReportParameter("rpuserName", userName + ""));
                    lr.SetParameters(reportParameters);
                }
                rd = new ReportDataSource("DataSet1", v);
            }

            else if (reportOptions == "LoanClosedWithEmpId")
            {
                string path = Path.Combine(Server.MapPath("~/Reporting/Loan"), "LoanClosed.rdlc");
                if (System.IO.File.Exists(path))
                {
                    lr.ReportPath = path;
                }
                else
                {
                    return View("Report/ReportLoan/Index");
                }

                List<VM_PFLoan> v = new List<VM_PFLoan>();
                using (UnitOfWork unitOfWork = new UnitOfWork())
                {
                    var temp = unitOfWork.CustomRepository.GetClosedLoanForReport(OCode);
                    if (fromDate != null && toDate != null)
                    {
                        v = temp.Where(t => t.StartDate >= fromDate && t.StartDate <= toDate && t.EmpID == empID).ToList();
                    }
                    else
                    {
                        v = temp.ToList();
                    }
                    foreach (var item in v)
                    {
                        item.Installment = _MvcApplication.GetNumber(item.Installment);
                        item.Interest = _MvcApplication.GetNumber(item.Interest);
                        item.LoanAmount = _MvcApplication.GetNumber(item.LoanAmount);
                        item.PayableAmount = _MvcApplication.GetNumber(item.PayableAmount);
                    }
                    var getCompany = unitOfWork.CompanyInformationRepository.GetByID(OCode);
                    ReportParameterCollection reportParameters = new ReportParameterCollection();
                    reportParameters.Add(new ReportParameter("rpCompanyName", getCompany.CompanyName + ""));
                    reportParameters.Add(new ReportParameter("rpCompanyAddress", getCompany.CompanyAddress + ""));
                    reportParameters.Add(new ReportParameter("rpFromDate", fromDate + ""));
                    reportParameters.Add(new ReportParameter("rpToDate", toDate + ""));
                    reportParameters.Add(new ReportParameter("rpuserName", userName + ""));
                    lr.SetParameters(reportParameters);
                }
                rd = new ReportDataSource("DataSet1", v);
            }
            #endregion

            #region UnpaidLoan
            else if (reportOptions == "UnpaidLoan" || reportOptions == "UnpaidLoanWithDate")
            {
                string path = Path.Combine(Server.MapPath("~/Reporting/Loan"), "UnpaidLoan.rdlc");
                if (System.IO.File.Exists(path))
                {
                    lr.ReportPath = path;
                }
                else
                {
                    return View("Report/ReportLoan/Index");
                }
                List<VM_PFLoanPayment> v = new List<VM_PFLoanPayment>();
                using (UnitOfWork unitOfWork = new UnitOfWork())
                {
                    int year = DateTime.Now.Year;
                    int month = DateTime.Now.Month;
                    if (reportOptions == "UnpaidLoanWithDate")
                    {
                        List<VM_PFLoanPayment> list = unitOfWork.CustomRepository.UnpaidLoanByDate(OCode);
                        foreach (var item in list)
                        {
                            VM_PFLoanPayment aVM_PFLoanPayment = new VM_PFLoanPayment();
                            string date = "";
                            if (item.ConMonth.Trim().Length == 1)
                            {
                                date = "01/" + "0" + item.ConMonth.Trim() + "/" + item.ConYear;
                            }
                            else
                            {
                                date = "01/" + item.ConMonth + "/" + item.ConYear;
                            }
                            item.convertedDatetime = DateTime.ParseExact(date, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                            v.Add(item);
                        }
                        v = v.Where(c => c.convertedDatetime >= fromDate && c.convertedDatetime <= toDate).ToList();
                    }
                    else
                    {
                        v = unitOfWork.CustomRepository.UnpaidLoan(year, month, OCode)
                                .Where(x => x.PaymentDate <= DateTime.Now).ToList();
                    }
                    foreach (var item in v)
                    {
                        item.Amount = _MvcApplication.GetNumber(item.Amount);
                        item.Interest = _MvcApplication.GetNumber(item.Interest);
                        item.InstallmentAmount = _MvcApplication.GetNumber(item.InstallmentAmount);
                        item.PaymentAmount = _MvcApplication.GetNumber(item.PaymentAmount);
                        item.PrincipalAmount = _MvcApplication.GetNumber(item.PrincipalAmount);
                    }
                    if (empID > 0)
                    {
                        v = v.Where(x => x.EmpID == empID).ToList();
                    }
                    var getCompany = unitOfWork.CompanyInformationRepository.GetByID(OCode);
                    ReportParameterCollection reportParameters = new ReportParameterCollection();
                    reportParameters.Add(new ReportParameter("rpCompanyName", getCompany.CompanyName + ""));
                    reportParameters.Add(new ReportParameter("rpCompanyAddress", getCompany.CompanyAddress + ""));
                    reportParameters.Add(new ReportParameter("rpuserName", userName + ""));
                    reportParameters.Add(new ReportParameter("rpFromDate", fromDate + ""));
                    reportParameters.Add(new ReportParameter("rpToDate", toDate + ""));
                    lr.SetParameters(reportParameters);
                }
                rd = new ReportDataSource("DataSet1", v);

                //VM_Employee vm_employee = new VM_Employee();
                //using (UnitOfWork unitOfWork = new UnitOfWork())
                //{
                //    vm_employee = unitOfWork.CustomRepository.GetAllEmployee().Where(e => e.EmpID == empID).Single();
                //}

                //ReportParameterCollection reportParameters = new ReportParameterCollection();
                //reportParameters.Add(new ReportParameter("EmpID", empID));
                //reportParameters.Add(new ReportParameter("EmpName", vm_employee.EmpName));
                //reportParameters.Add(new ReportParameter("JoiningDate", vm_employee.JoiningDate + ""));
                //reportParameters.Add(new ReportParameter("Designation", vm_employee.DesignationName));
                //reportParameters.Add(new ReportParameter("Branch", vm_employee.BranchName));
                //lr.SetParameters(reportParameters);
                //this.reportViewer.LocalReport.SetParameters(reportParameters);
            }


            else if (reportOptions == "UnpaidLoanWithId")
            {
                string path = Path.Combine(Server.MapPath("~/Reporting/Loan"), "UnpaidLoan.rdlc");
                if (System.IO.File.Exists(path))
                {
                    lr.ReportPath = path;
                }
                else
                {
                    return View("Report/ReportLoan/Index");
                }
                List<VM_PFLoanPayment> v = new List<VM_PFLoanPayment>();
                using (UnitOfWork unitOfWork = new UnitOfWork())
                {
                    int year = DateTime.Now.Year;
                    int month = DateTime.Now.Month;

                    v = unitOfWork.CustomRepository.UnpaidLoan(year, month, OCode).Where(x => x.EmployeeID == empID).ToList();
                    foreach (var item in v)
                    {
                        item.Amount = _MvcApplication.GetNumber(item.Amount);
                        item.Interest = _MvcApplication.GetNumber(item.Interest);
                        item.InstallmentAmount = _MvcApplication.GetNumber(item.InstallmentAmount);
                        item.PaymentAmount = _MvcApplication.GetNumber(item.PaymentAmount);
                        item.PrincipalAmount = _MvcApplication.GetNumber(item.PrincipalAmount);
                    }
                    var getCompany = unitOfWork.CompanyInformationRepository.GetByID(OCode);
                    ReportParameterCollection reportParameters = new ReportParameterCollection();
                    reportParameters.Add(new ReportParameter("rpCompanyName", getCompany.CompanyName + ""));
                    reportParameters.Add(new ReportParameter("rpCompanyAddress", getCompany.CompanyAddress + ""));
                    reportParameters.Add(new ReportParameter("rpuserName", userName + ""));
                    lr.SetParameters(reportParameters);
                }
                rd = new ReportDataSource("DataSet1", v);

                //VM_Employee vm_employee = new VM_Employee();
                //using (UnitOfWork unitOfWork = new UnitOfWork())
                //{
                //    vm_employee = unitOfWork.CustomRepository.GetAllEmployee().Where(e => e.EmpID == empID).Single();
                //}

                //ReportParameterCollection reportParameters = new ReportParameterCollection();
                //reportParameters.Add(new ReportParameter("EmpID", empID));
                //reportParameters.Add(new ReportParameter("EmpName", vm_employee.EmpName));
                //reportParameters.Add(new ReportParameter("JoiningDate", vm_employee.JoiningDate + ""));
                //reportParameters.Add(new ReportParameter("Designation", vm_employee.DesignationName));
                //reportParameters.Add(new ReportParameter("Branch", vm_employee.BranchName));
                //lr.SetParameters(reportParameters);
                //this.reportViewer.LocalReport.SetParameters(reportParameters);
            }
            #endregion

            #region PaymentDetail
            else if (reportOptions == "PaymentDetail")
            {
                string path = Path.Combine(Server.MapPath("~/Reporting/Loan"), "PaymentDetail.rdlc");
                if (System.IO.File.Exists(path))
                {
                    lr.ReportPath = path;
                }
                else
                {
                    return View("Report/ReportLoan/Index");
                }
                List<VM_Amortization> v = new List<VM_Amortization>();
                List<VM_PFLoan> vm_pfLoan = new List<VM_PFLoan>();
                ReportDataSource rd2;
                using (UnitOfWork unitOfWork = new UnitOfWork())
                {
                    double loanAmount = 0;
                    double paidAmount = 0;
                    double dueAmount = 0;
                    _MvcApplication = new MvcApplication();
                    if (fromDate != null || toDate != null)
                    {
                        v = unitOfWork.CustomRepository.GetAmortizationDetail(loanID).Where(x => x.MonthYear >= fromDate && x.MonthYear <= toDate).ToList();
                    }
                    else
                    {
                        v = unitOfWork.CustomRepository.GetAmortizationDetail(loanID).ToList();
                    }
                    foreach (var item in v)
                    {
                        item.Amount = (double)_MvcApplication.GetNumber((decimal)item.Amount);
                        item.Interest = (double)_MvcApplication.GetNumber((decimal)item.Interest);
                        item.Balance = (double)_MvcApplication.GetNumber((decimal)item.Balance);
                        item.MonthlyPaid = (double)_MvcApplication.GetNumber((decimal)item.MonthlyPaid);
                        item.Principal = (double)_MvcApplication.GetNumber((decimal)item.Principal);
                        if (item.PaymentStatus == "Paid")
                        {
                            paidAmount += (item.Principal);
                        }
                    }
                    vm_pfLoan = unitOfWork.CustomRepository.GetPFLoanHistoryByEmpID().Where(w => w.PFLoanID == loanID).ToList();
                    loanAmount = v.Select(x => x.Amount).FirstOrDefault();
                    dueAmount = (loanAmount - paidAmount);

                    foreach (var item in vm_pfLoan)
                    {
                        item.Installment = _MvcApplication.GetNumber((decimal)item.Installment);
                        item.Interest = _MvcApplication.GetNumber((decimal)item.Interest);
                        item.EmpName = v.Where(x => x.EmpID == item.EmpID).Select(x => x.EmpName).FirstOrDefault(); //Added by Fahim 08/12/2015
                    }
                    var getCompany = unitOfWork.CompanyInformationRepository.GetByID(OCode);
                    ReportParameterCollection reportParameters = new ReportParameterCollection();
                    reportParameters.Add(new ReportParameter("rpCompanyName", getCompany.CompanyName + ""));
                    reportParameters.Add(new ReportParameter("rpCompanyAddress", getCompany.CompanyAddress + ""));
                    reportParameters.Add(new ReportParameter("rpuserName", userName + ""));
                    reportParameters.Add(new ReportParameter("rpLoanAmount", _MvcApplication.GetNumber((decimal)loanAmount) + ""));
                    reportParameters.Add(new ReportParameter("rpPaidAmount", _MvcApplication.GetNumber((decimal)paidAmount) + ""));
                    reportParameters.Add(new ReportParameter("rpDueAmount", _MvcApplication.GetNumber((decimal)dueAmount) + ""));
                    reportParameters.Add(new ReportParameter("rpFromDate", String.Format("{0:MMM/d/yyyy}", fromDate)));
                    reportParameters.Add(new ReportParameter("rpToDate", String.Format("{0:MMM/d/yyyy}", toDate)));
                    lr.SetParameters(reportParameters);
                }
                rd = new ReportDataSource("DataSet1", v);
                rd2 = new ReportDataSource("DataSet2", vm_pfLoan); 
                lr.DataSources.Add(rd2);
            }
            #endregion
            //Added by Avishek Date:Nov-16-2015
           #region CompanyPaymentDetail
            else if (reportOptions == "CompanyPaymentDetail")
            {

                fromDate = fromDate ?? DateTime.MinValue;

                string path = Path.Combine(Server.MapPath("~/Reporting/Loan"), "CompanyPaymentDetail.rdlc");
                if (System.IO.File.Exists(path))
                {
                    lr.ReportPath = path;
                }
                else
                {
                    return View("Report/ReportLoan/Index");
                }
                List<VM_Amortization> v = new List<VM_Amortization>();
                List<VM_PFLoan> vm_pfLoan = new List<VM_PFLoan>();
                ReportDataSource rd2;
                using (UnitOfWork unitOfWork = new UnitOfWork())
                {
                    double loanAmount = 0;
                    double paidAmount = 0;
                    double dueAmount = 0;
                    _MvcApplication = new MvcApplication();
                    if (fromDate != null || toDate != null)
                    {
                        v = unitOfWork.CustomRepository.GetLoanAmortizationDetail(OCode).Where(x => x.MonthYear >= fromDate && x.MonthYear <= toDate).ToList();
                    }
                    foreach (var item in v)
                    {
                        item.Amount = (double)_MvcApplication.GetNumber((decimal)item.Amount);
                        item.Interest = (double)_MvcApplication.GetNumber((decimal)item.Interest);
                        item.Balance = (double)_MvcApplication.GetNumber((decimal)item.Balance);
                        item.MonthlyPaid = (double)_MvcApplication.GetNumber((decimal)item.MonthlyPaid);
                        item.Principal = (double)_MvcApplication.GetNumber((decimal)item.Principal);
                        if (item.PaymentStatus == "Paid")
                        {
                            paidAmount += (item.Principal);
                        }

                        loanAmount += item.Principal;
                    }
                    vm_pfLoan = unitOfWork.CustomRepository.GetPFLoanHistoryByEmpID().Where(w => w.PFLoanID == loanID).ToList();
                    dueAmount = (loanAmount - paidAmount);

                    foreach (var item in vm_pfLoan)
                    {
                        item.Installment = _MvcApplication.GetNumber((decimal)item.Installment);
                        item.Interest = _MvcApplication.GetNumber((decimal)item.Interest);
                    }
                    var getCompany = unitOfWork.CompanyInformationRepository.GetByID(OCode);
                    ReportParameterCollection reportParameters = new ReportParameterCollection();
                    reportParameters.Add(new ReportParameter("rpCompanyName", getCompany.CompanyName + ""));
                    reportParameters.Add(new ReportParameter("rpCompanyAddress", getCompany.CompanyAddress + ""));
                    reportParameters.Add(new ReportParameter("rpuserName", userName + ""));
                    reportParameters.Add(new ReportParameter("rpLoanAmount", _MvcApplication.GetNumber((decimal)loanAmount) + ""));
                    reportParameters.Add(new ReportParameter("rpPaidAmount", _MvcApplication.GetNumber((decimal)paidAmount) + ""));
                    reportParameters.Add(new ReportParameter("rpDueAmount", _MvcApplication.GetNumber((decimal)dueAmount) + ""));
                    reportParameters.Add(new ReportParameter("rpFromDate", String.Format("{0:MMM/d/yyyy}", fromDate)));
                    reportParameters.Add(new ReportParameter("rpToDate", String.Format("{0:MMM/d/yyyy}", toDate)));
                    lr.SetParameters(reportParameters);
                }
                rd = new ReportDataSource("DataSet1", v);
                rd2 = new ReportDataSource("DataSet2", vm_pfLoan);
                lr.DataSources.Add(rd2);
            }
            #endregion 
            //End



            //Addedd by Fahim 11/11/2015
            #region PaidInstallmentDetail
            else if (reportOptions == "PaidInstallmentDetail")
            {
                string path = Path.Combine(Server.MapPath("~/Reporting/Loan"), "PaidInstallmentDetails.rdlc");
                if (System.IO.File.Exists(path))
                {
                    lr.ReportPath = path;
                }
                else
                {
                    return View("Report/ReportLoan/Index");
                }
                List<VM_Amortization> v = new List<VM_Amortization>();
                using (UnitOfWork unitOfWork = new UnitOfWork())
                {
                    _MvcApplication = new MvcApplication();
                    v = unitOfWork.CustomRepository.AmortizationListWithOCode(OCode).Where(x => x.PaymentDate >= fromDate && x.PaymentDate <= toDate).OrderBy(x => x.PaymentDate).ToList();
                    foreach (var item in v)
                    {
                        item.Amount = (double)_MvcApplication.GetNumber((decimal)item.Amount);
                    }
                    var getCompany = unitOfWork.CompanyInformationRepository.GetByID(OCode);
                    ReportParameterCollection reportParameters = new ReportParameterCollection();
                    reportParameters.Add(new ReportParameter("rpCompanyName", getCompany.CompanyName + ""));
                    reportParameters.Add(new ReportParameter("rpCompanyAddress", getCompany.CompanyAddress + ""));
                    reportParameters.Add(new ReportParameter("rpuserName", userName + ""));
                    reportParameters.Add(new ReportParameter("rpFromDate", String.Format("{0:MMM/d/yyyy}", fromDate)));
                    reportParameters.Add(new ReportParameter("rpToDate", String.Format("{0:MMM/d/yyyy}", toDate)));
                    lr.SetParameters(reportParameters);
                }
                rd = new ReportDataSource("DataSet1", v);
                ReportDataSource rd2 = new ReportDataSource("DataSet2", v); //Added by Fahim 08/12/2015 //BanglaVision
                lr.DataSources.Add(rd2);
            }
            #endregion

            #region AmortizationSchedule
            else if (reportOptions == "AmortizationSchedule")
            {
                string path = Path.Combine(Server.MapPath("~/Reporting/Loan"), "AmortizationSchedule.rdlc");
                if (System.IO.File.Exists(path))
                {
                    lr.ReportPath = path;
                }
                else
                {
                    return View("Report/ReportLoan/Index");
                }
                List<VM_Amortization> v = new List<VM_Amortization>();
                List<VM_PFLoan> vm_pfLoan = new List<VM_PFLoan>();
                ReportDataSource rd2;
                using (UnitOfWork unitOfWork = new UnitOfWork())
                {
                    double loanAmount = 0;
                    double paidAmount = 0;
                    double dueAmount = 0;
                    _MvcApplication = new MvcApplication();
                    if (fromDate != null || toDate != null)
                    {
                        v = unitOfWork.CustomRepository.GetAmortizationDetail(loanID).Where(x => x.MonthYear >= fromDate && x.MonthYear <= toDate).ToList();
                    }
                    else
                    {
                        v = unitOfWork.CustomRepository.GetAmortizationDetail(loanID).ToList();
                    }
                    double interestPaid = 0;
                    double paidPrincipleAmount = 0;
                    double payableAmount = 0;
                    for (int i = 0; i < v.Count; i++)
                    {
                        if (v[i].Interest != 0)
                        {
                            interestPaid += v[i].Interest;
                            payableAmount = v[i].Interest;
                        }
                        else
                        {
                            paidPrincipleAmount += v[i].Principal;
                            payableAmount = v[i].Principal;
                        }

                        v[i].LoanAmount = Convert.ToDecimal(v.Select(x => x.Principal).Sum() - paidPrincipleAmount);
                        if (i == 0)
                        {
                            v[i].Amount = v[i].Amount - payableAmount;
                        }
                        else
                        {
                            v[i].Amount = v[i - 1].Amount - payableAmount;
                        }
                        v[i].Interest = (double)_MvcApplication.GetNumber((decimal)v[i].Interest);
                        v[i].Balance = (double)_MvcApplication.GetNumber((decimal)v[i].Balance);
                        v[i].MonthlyPaid = (double)_MvcApplication.GetNumber((decimal)v[i].MonthlyPaid);
                        v[i].Principal = (double)_MvcApplication.GetNumber((decimal)v[i].Principal);
                        v[i].TotalInterest = Convert.ToDecimal(v.Select(x => x.Interest).Sum() - interestPaid);
                        if (v[i].PaymentStatus == "Paid")
                        {
                            paidAmount += (v[i].Principal);
                        }

                    }

                    //foreach (var item in v)
                    //{
                    //    item.Amount = (double)_MvcApplication.GetNumber((decimal)item.Amount);
                    //    item.Interest = (double)_MvcApplication.GetNumber((decimal)item.Interest);
                    //    item.Balance = (double)_MvcApplication.GetNumber((decimal)item.Balance);
                    //    item.MonthlyPaid = (double)_MvcApplication.GetNumber((decimal)item.MonthlyPaid);
                    //    item.Principal = (double)_MvcApplication.GetNumber((decimal)item.Principal);
                    //    item.TotalInterest = Convert.ToDecimal(v.Select(x => x.Interest).Sum());
                    //    if (item.PaymentStatus == "Paid")
                    //    {
                    //        paidAmount += (item.Principal);
                    //    }
                    //}



                    vm_pfLoan = unitOfWork.CustomRepository.GetPFLoanHistoryByEmpID().Where(w => w.PFLoanID == loanID).ToList();
                    loanAmount = v.Select(x => x.Amount).FirstOrDefault();
                    dueAmount = (loanAmount - paidAmount);

                    foreach (var item in vm_pfLoan)
                    {
                        item.Installment = _MvcApplication.GetNumber((decimal)item.Installment);
                        item.Interest = _MvcApplication.GetNumber((decimal)item.Interest);
                        item.EmpName = v.Where(x => x.EmpID == item.EmpID).Select(x => x.EmpName).FirstOrDefault(); //Added by Fahim 08/12/2015
                    }
                    var getCompany = unitOfWork.CompanyInformationRepository.GetByID(OCode);
                    ReportParameterCollection reportParameters = new ReportParameterCollection();
                    reportParameters.Add(new ReportParameter("rpCompanyName", getCompany.CompanyName + ""));
                    reportParameters.Add(new ReportParameter("rpCompanyAddress", getCompany.CompanyAddress + ""));
                    reportParameters.Add(new ReportParameter("rpuserName", userName + ""));
                    reportParameters.Add(new ReportParameter("rpLoanAmount", _MvcApplication.GetNumber((decimal)loanAmount) + ""));
                    reportParameters.Add(new ReportParameter("rpPaidAmount", _MvcApplication.GetNumber((decimal)paidAmount) + ""));
                    reportParameters.Add(new ReportParameter("rpDueAmount", _MvcApplication.GetNumber((decimal)dueAmount) + ""));
                    reportParameters.Add(new ReportParameter("rpFromDate", String.Format("{0:MMM/d/yyyy}", fromDate)));
                    reportParameters.Add(new ReportParameter("rpToDate", String.Format("{0:MMM/d/yyyy}", toDate)));
                    lr.SetParameters(reportParameters);
                }
                rd = new ReportDataSource("DataSet1", v);
                rd2 = new ReportDataSource("DataSet2", vm_pfLoan);
                lr.DataSources.Add(rd2);
            }
            #endregion

            #region DueAndPaidLoanList
            else if (reportOptions == "DueAndPaidLoanList")
            {
                string path = Path.Combine(Server.MapPath("~/Reporting/Loan"), "DueAndPaidLoanList.rdlc");
                if (System.IO.File.Exists(path))
                {
                    lr.ReportPath = path;
                }
                else
                {
                    return View("Report/ReportLoan/Index");
                }
                List<VM_PFLoanPayment> v = new List<VM_PFLoanPayment>();
                using (UnitOfWork unitOfWork = new UnitOfWork())
                {
                    int year = DateTime.Now.Year;
                    int month = DateTime.Now.Month;
                    
                    v = unitOfWork.CustomRepository.AllLoan(OCode)
                                .Where(x => x.PaymentDate <= DateTime.Now).ToList();
                    //}
                    foreach (var item in v)
                    {
                        item.Amount = _MvcApplication.GetNumber(item.Amount);
                        item.Interest = _MvcApplication.GetNumber(item.Interest);
                        item.InstallmentAmount = _MvcApplication.GetNumber(item.InstallmentAmount);
                        item.PaymentAmount = _MvcApplication.GetNumber(item.PaymentAmount);
                        item.PrincipalAmount = _MvcApplication.GetNumber(item.PrincipalAmount);
                    }
                    //if (empID > 0)
                    //{
                    //    v = v.Where(x => x.EmpID == empID).ToList();
                    //}
                    var getCompany = unitOfWork.CompanyInformationRepository.GetByID(OCode);
                    ReportParameterCollection reportParameters = new ReportParameterCollection();
                    reportParameters.Add(new ReportParameter("rpCompanyName", getCompany.CompanyName + ""));
                    reportParameters.Add(new ReportParameter("rpCompanyAddress", getCompany.CompanyAddress + ""));
                    reportParameters.Add(new ReportParameter("rpuserName", userName + ""));
                    reportParameters.Add(new ReportParameter("rpFromDate", fromDate + ""));
                    reportParameters.Add(new ReportParameter("rpToDate", toDate + ""));
                    lr.SetParameters(reportParameters);
                }
                rd = new ReportDataSource("DataSet1", v);
            }

            #endregion

            #region Loan Detail Report
            else if (reportOptions == "LoanSummary")
            {
                string path = Path.Combine(Server.MapPath("~/Reporting/Loan"), "LoanSummary.rdlc");
                if (System.IO.File.Exists(path))
                {
                    lr.ReportPath = path;
                }
                else
                {
                    return View("Report/ReportLoan/Index");
                }
                List<VM_Amortization> v = new List<VM_Amortization>();
                // var data = (rsp_GetLoanSummary_Result) null;
                using (PFTMEntities context = new PFTMEntities())
                {
                    _MvcApplication = new MvcApplication();

                    if (fromDate != null && toDate != null)
                    {

                        DateTime fromdt = (Convert.ToDateTime(fromDate.ToString()));
                        DateTime dodt = (Convert.ToDateTime(toDate.ToString()));

                        int fd = fromdt.Day; //Int32.Parse(datevalue.Day.ToString());
                        int fm = fromdt.Month;
                        int fy = fromdt.Year;

                        int td = dodt.Day; //Int32.Parse(datevalue.Day.ToString());
                        int tm = dodt.Month;
                        int ty = dodt.Year;

                        //var data = context.rsp_GetLoanSummary(fm, fy, tm, ty).ToList();
                        var data = unitOfWork.CustomRepository.sp_GenerateLoanBalance(fm, fy, tm, ty).OrderBy(s => s.PFLoanID).ToList();


                        var getCompany = unitOfWork.CompanyInformationRepository.GetByID(OCode);
                        ReportParameterCollection reportParameters = new ReportParameterCollection();
                        reportParameters.Add(new ReportParameter("rpCompanyName", getCompany.CompanyName + ""));
                        reportParameters.Add(new ReportParameter("rpCompanyAddress", getCompany.CompanyAddress + ""));
                        reportParameters.Add(new ReportParameter("rpuserName", userName + ""));
                        reportParameters.Add(new ReportParameter("rpFromDate", String.Format("{0:MMM/d/yyyy}", fromDate)));
                        reportParameters.Add(new ReportParameter("rpToDate", String.Format("{0:MMM/d/yyyy}", toDate)));
                        lr.SetParameters(reportParameters);

                        rd = new ReportDataSource("DataSet1", data);
                        ReportDataSource rd2 = new ReportDataSource("DataSet2", data);
                        lr.DataSources.Add(rd2);
                    }
                    else
                    {
                        return Content("Please insert from month and to month! ");
                    }
                }
                //rd = new ReportDataSource("DataSet1", v);
                //ReportDataSource rd2 = new ReportDataSource("DataSet2", v);
                //lr.DataSources.Add(rd2);
            }

            #endregion

            #region LoanListWithOutstanding
             else if (reportOptions == "LoanDetailList")
            {
                string path = Path.Combine(Server.MapPath("~/Reporting/Loan"), "LoanDetailList.rdlc");
                if (System.IO.File.Exists(path))
                {
                    lr.ReportPath = path;
                }
                else
                {
                    return View("Report/ReportLoan/Index");
                }
                List<VM_PFLoan> v = new List<VM_PFLoan>();
                double loanAmount = 0;
                double dueAmount = 0;
                DateTime fdate = fromDate.GetValueOrDefault();
                DateTime tdate = toDate.GetValueOrDefault();
                List<VM_PFLoan> vm_pfLoan = new List<VM_PFLoan>();
                double paidAmount = 0;
                using (UnitOfWork unitOfWork = new UnitOfWork())
                {
                    //var temp = unitOfWork.CustomRepository.GetPFLoanHistoryByEmpID(OCode);
                    var temp = unitOfWork.CustomRepository.sp_GetLoanDetailList(fdate, tdate).ToList();
                    // var loanAmount= unitOfWork.CustomRepository.AmortizationList
                    if (fromDate != null && toDate != null)
                    {
                        // string fromYear = fromDate.Value.Year + "";
                        //string fromMonth = fromDate.Value.Month.ToString().PadLeft(2, '0');
                        //v = temp.Where(t => Convert.ToDateTime(t.StartDate) >= fromDate && Convert.ToDateTime(t.StartDate) <= toDate).ToList();
                        v = temp.Where(t => t.StartDate >= fromDate && t.StartDate <= toDate).ToList();
                        foreach (var item in v)
                        {
                            item.Installment = _MvcApplication.GetNumber(item.Installment);
                            item.Interest = _MvcApplication.GetNumber(item.Interest);
                            item.LoanAmount = _MvcApplication.GetNumber(item.LoanAmount);
                            item.PayableAmount = _MvcApplication.GetNumber(item.PayableAmount);
                        }
                    }


                    //else if (fromDate == null && toDate != null)
                    //{
                    //    v = temp.Where(t => Convert.ToDateTime(t.StartDate) <= toDate).ToList();
                    //}
                    //else if (fromDate != null && toDate == null)
                    //{
                    //    v = temp.Where(t => Convert.ToDateTime(t.StartDate) >= fromDate).ToList();
                    //}
                    else
                    {
                        v = temp.ToList();
                    }
                    if (empID > 0)
                    {
                        v = v.Where(w => w.EmpID == empID).ToList();
                    }
                    var va = unitOfWork.CustomRepository.GetAmortizationDetail(loanID).ToList();

                    foreach (var item in va)
                    {
                        item.Amount = (double)_MvcApplication.GetNumber((decimal)item.Amount);
                        item.Interest = (double)_MvcApplication.GetNumber((decimal)item.Interest);
                        item.Balance = (double)_MvcApplication.GetNumber((decimal)item.Balance);
                        item.MonthlyPaid = (double)_MvcApplication.GetNumber((decimal)item.MonthlyPaid);
                        item.Principal = (double)_MvcApplication.GetNumber((decimal)item.Principal);
                        if (item.PaymentStatus == "Paid")
                        {
                            paidAmount += (item.Principal);
                        }
                    }
                    //vm_pfLoan = unitOfWork.CustomRepository.GetPFLoanHistoryByEmpID().Where(w => w.EmpID == empID).ToList();
                    vm_pfLoan = unitOfWork.CustomRepository.sp_GetLoanDetailList(fdate, tdate).Where(w => w.EmpID == empID).ToList();

                    loanAmount = va.Select(x => x.Amount).FirstOrDefault();
                    dueAmount = (loanAmount - paidAmount);
                    var getCompany = unitOfWork.CompanyInformationRepository.GetByID(OCode);
                    ReportParameterCollection reportParameters = new ReportParameterCollection();
                    reportParameters.Add(new ReportParameter("rpCompanyName", getCompany.CompanyName + ""));
                    reportParameters.Add(new ReportParameter("rpCompanyAddress", getCompany.CompanyAddress + ""));
                    reportParameters.Add(new ReportParameter("rpFromDate", ((DateTime)fromDate).ToShortDateString() + ""));
                    reportParameters.Add(new ReportParameter("rpuserName", userName + ""));

                    reportParameters.Add(new ReportParameter("rpToDate", ((DateTime)toDate).ToShortDateString() + ""));
                    lr.SetParameters(reportParameters);
                }
                rd = new ReportDataSource("DataSet1", v);

            }
         #endregion

            #region YearlyReportDetail
            else if (reportOptions == "YearlyLoanDetail")
            {
                try
                {

                    string path = Path.Combine(Server.MapPath("~/Reporting/Loan"), "YearlyReportDetail.rdlc");
                    if (System.IO.File.Exists(path))
                    {
                        lr.ReportPath = path;
                    }
                    else
                    {
                        return View("Report/ReportPF/Index");
                    }


                    using (UnitOfWork unitOfWork = new UnitOfWork())
                    {
                        var getCompany = unitOfWork.CompanyInformationRepository.GetByID(OCode);
                        var YearlyLoanDate = unitOfWork.CustomRepository.sp_EmployeeYearlyLoanDetails((DateTime)fromDate, (DateTime)toDate).ToList();
                        ReportParameterCollection reportParameters = new ReportParameterCollection();
                        reportParameters.Add(new ReportParameter("rpCompanyName", getCompany.CompanyName + ""));
                        reportParameters.Add(new ReportParameter("rpCompanyAddress", getCompany.CompanyAddress + ""));
                        reportParameters.Add(new ReportParameter("rpUserName", User.Identity.Name + ""));
                        
                        reportParameters.Add(new ReportParameter("rpFromDate", String.Format("{0:MMM/d/yyyy}", fromDate)));
                        reportParameters.Add(new ReportParameter("rpToDate", String.Format("{0:MMM/d/yyyy}", toDate)));

                        lr.SetParameters(reportParameters);


                        rd = new ReportDataSource("DataSet1", YearlyLoanDate);

                    }
                }
                catch (Exception exception)
                {
                    throw exception;
                }

            }
            #endregion
            else
            {
                return Content("Please select report option!");
            }
            




            lr.DataSources.Add(rd);

            string reportType = id;
            string mimeType;
            string encoding;
            string fileNameExtension;



            string deviceInfo =

            "<DeviceInfo>" +
            "  <OutputFormat>" + id + "</OutputFormat>" +
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
