using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using DLL;
using DLL.Repository;
using DLL.ViewModel;
using Microsoft.Reporting.WebForms;
using PFMVC.common;
using System.Globalization;
using DLL.Utility;

namespace PFMVC.Areas.Report.Controllers
{
    public class ReportPFController : Controller
    {
        MvcApplication _MvcApplication;
        int PageID = 6;
        ReportDataSource rd;
        ReportDataSource rd2;
        //private DLL.Utility.Cheque_BankInfo_BLL qwe = new DLL.Utility.Cheque_BankInfo_BLL();
        //Cheque_BankInfo_BLL aCheque_BankInfo_BLL = new Cheque_BankInfo_BLL();
        [Authorize]
        public ActionResult Index()
        {
            int OCode = ((int?)Session["OCode"]) ?? 0;
            if (OCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            ViewBag.PageName = "Report section";

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
                UnitOfWork unitOfWork = new UnitOfWork();
                int OCode = ((int?)Session["OCode"]) ?? 0;
                var suggestions = unitOfWork.EmployeesRepository.Get().Where(x => x.IdentificationNumber.ToLower().Contains(term.ToLower())).Select(s => new
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
        /// Autocompletes the suggestions.
        /// </summary>
        /// <param name="term">The term.</param>
        /// <returns>list</returns>
        /// <CreatedBy>Avishek</CreatedBy>
        /// <createdDate>Aug-1-2015</createdDate>
        public JsonResult AutocompleteSuggestionsName(string term)
        {
            try
            {
                UnitOfWork unitOfWork = new UnitOfWork();
                int OCode = ((int?)Session["OCode"]) ?? 0;
                var suggestions = unitOfWork.EmployeesRepository.Get().Where(x => x.EmpName.ToLower().Contains(term.ToLower())).Select(s => new
                {
                    value = s.IdentificationNumber,
                    label = s.EmpName
                }).ToList();
                return Json(suggestions, JsonRequestBehavior.AllowGet);
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
        public JsonResult GetEmpId(string identificationNo)
        {
            try
            {
                UnitOfWork unitOfWork = new UnitOfWork();
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

        public ActionResult Report(string id, string reportOptions, int? empID, DateTime? fromDate, DateTime? toDate)
        {
            int OCode = ((int?)Session["OCode"]) ?? 0;
            if (OCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //Added By Avishek Date:Feb-18-2015
            //Start
            CultureInfo CInfo = new CultureInfo("en-IN");
            DateTime fdate = fromDate.GetValueOrDefault();
            DateTime tdate = toDate.GetValueOrDefault();
            string userName = Session["userName"].ToString();
            _MvcApplication = new MvcApplication();
            //Commentout by Avishek Date: Jun-20-2016
            //Start
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                var user = unitOfWork.UserProfileRepository.Get().Where(w => w.LoginName.ToLower() == User.Identity.Name.ToLower() && w.OCode == OCode).SingleOrDefault();
                if (user.EmpID > 0)
                {
                    empID = (int?)user.EmpID;
                }
            }
            //End
            LocalReport lr = new LocalReport();
            ////////////////////////////////////////This Report ia generate for kds (temorary) //////////////////////////////////////////////////////

            

            #region PFMemberContribution
            if (reportOptions == "PFMemberContribution")
            {
                string path = Path.Combine(Server.MapPath("~/Reporting/PF"), "PFMemberContribution.rdlc");
                if (System.IO.File.Exists(path))
                {
                    lr.ReportPath = path;
                }
                else
                {
                    return View("Report/ReportPF/Index");
                }
                List<VM_PFMonthlyStatus> v = new List<VM_PFMonthlyStatus>();
                using (UnitOfWork unitOfWork = new UnitOfWork())
                {
                    //Added By Avishek Date:Feb-18-2015
                    ////Start
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

                    var temp = unitOfWork.CustomRepository.EmpPFMonthlyStatusForReport(OCode, empID ?? 0, fdate, tdate).OrderBy(x => x.ContrebutionDate).ToList();
                    List<tbl_ProfitDistributionDetail> _tbl_ProfitDistributionDetail = unitOfWork.CustomRepository.GetProfitDistributionByEmpId(Convert.ToInt16(empID)).Where(x => x.TransactionDate >= fdate && x.TransactionDate <= tdate).ToList();
                    if (_tbl_ProfitDistributionDetail.Count==0)
                    {
                        foreach (VM_PFMonthlyStatus item in temp)
                    {
                        VM_PFMonthlyStatus atbl_ProfitDistributionDetail = new VM_PFMonthlyStatus();
                        atbl_ProfitDistributionDetail.EmpID = item.EmpID;
                        atbl_ProfitDistributionDetail.IdentificationNumber = item.IdentificationNumber;
                        atbl_ProfitDistributionDetail.ProcessRunDate = item.ProcessRunDate;
                        atbl_ProfitDistributionDetail.ContrebutionDate = item.ContrebutionDate;
                        atbl_ProfitDistributionDetail.EmpContribution = _MvcApplication.GetNumber(item.EmpContribution);
                        atbl_ProfitDistributionDetail.SelfContribution = _MvcApplication.GetNumber(item.SelfContribution);
                        atbl_ProfitDistributionDetail.Year = item.Year;
                        atbl_ProfitDistributionDetail.Month = item.Month;
                        atbl_ProfitDistributionDetail.SelfProfit = 0;
                        atbl_ProfitDistributionDetail.EmpProfit = 0;
                        v.Add(atbl_ProfitDistributionDetail);
                    }
                    }
                    else { 
                    foreach (VM_PFMonthlyStatus item in temp)
                    {
                        VM_PFMonthlyStatus atbl_ProfitDistributionDetail = new VM_PFMonthlyStatus();
                        atbl_ProfitDistributionDetail.EmpID = item.EmpID;
                        atbl_ProfitDistributionDetail.IdentificationNumber = item.IdentificationNumber;
                        atbl_ProfitDistributionDetail.ProcessRunDate = item.ProcessRunDate;
                        atbl_ProfitDistributionDetail.ContrebutionDate = item.ContrebutionDate;
                        atbl_ProfitDistributionDetail.EmpContribution = _MvcApplication.GetNumber(item.EmpContribution);
                        atbl_ProfitDistributionDetail.SelfContribution = _MvcApplication.GetNumber(item.SelfContribution);
                        atbl_ProfitDistributionDetail.Year = item.Year;
                        atbl_ProfitDistributionDetail.Month = item.Month;
                        atbl_ProfitDistributionDetail.SelfProfit = _MvcApplication.GetNumber(Convert.ToDecimal(_tbl_ProfitDistributionDetail.Where(x => x.TransactionDate.Value.Month == Convert.ToInt32(item.Month) && x.TransactionDate.Value.Year == Convert.ToInt32(item.Year)).Select(x => x.SelfProfit).FirstOrDefault()));
                        atbl_ProfitDistributionDetail.EmpProfit = _MvcApplication.GetNumber(Convert.ToDecimal(_tbl_ProfitDistributionDetail.Where(x => x.TransactionDate.Value.Month == Convert.ToInt32(item.Month) && x.TransactionDate.Value.Year == Convert.ToInt32(item.Year)).Select(x => x.EmpProfit).FirstOrDefault()));
                        v.Add(atbl_ProfitDistributionDetail);
                    }
                    }
                    //v = temp.Where(x => x.ProcessRunDate >= fdate && x.ProcessRunDate <= tdate).ToList();
                    List<tbl_Employees> list = unitOfWork.CustomRepository.GetEmployeeById(v.Where(x => x.EmpID == empID.Value).Select(x => x.EmpID).FirstOrDefault());
                    //var opening = unitOfWork.CustomRepository.EmpPFMonthlyStatusForReportOpening(OCode, empID ?? 0, DateTime.MinValue, fdate).OrderBy(x => x.ProcessRunDate).ToList();
                    var opening = unitOfWork.CustomRepository.EmpPFMonthlyStatusForReportOpening(OCode, empID ?? 0, fdate, fdate).OrderBy(x => x.ProcessRunDate).ToList();

                    List<tbl_ProfitDistributionDetail> _tbl_ProfitDistributionDetailforOpening = unitOfWork.CustomRepository.GetProfitDistributionByEmpId(Convert.ToInt16(empID)).Where(x => x.TransactionDate <= fdate).ToList();
                    decimal se = _MvcApplication.GetNumber((decimal)(opening.Where(x => x.ContrebutionDate <= fdate) == null ? 0 : opening.Where(x => x.ContrebutionDate <= fdate).Sum(x => x.SelfContribution)));
                    decimal em = _MvcApplication.GetNumber((decimal)(opening.Where(x => x.ContrebutionDate <= fdate) == null ? 0 : opening.Where(x => x.ContrebutionDate <= fdate).Sum(x => x.EmpContribution)));
                    decimal ope = _MvcApplication.GetNumber((decimal)list.Sum(x => x.opProfit));
                    decimal oem = _MvcApplication.GetNumber((decimal)list.Sum(x => x.opEmpContribution));
                    decimal ose = _MvcApplication.GetNumber((decimal)list.Sum(x => x.opOwnContribution));
                    decimal olo = _MvcApplication.GetNumber(((decimal)(list.Sum(x => x.opLoan) ?? 0)));
                    decimal pro = _MvcApplication.GetNumber(((decimal)(_tbl_ProfitDistributionDetailforOpening.Where(x => x.TransactionDate < fdate).Sum(x => x.DistributedAmount) ?? 0)));
                    //decimal openingBalance = se + ope + oem + ose - olo + pro;
                    decimal openingBalance = se + ope + oem + ose + pro + em;



                    //Added by Fahim 25/10/2015
                    decimal openingSelfContribution = ose + se; //Added by Fahim 25/10/2015
                    decimal openingEmployeeContribution = oem + em; //Added by Fahim 25/10/2015

                    decimal openingSelfProfit = 0;
                    decimal openingEmployeeProfit = 0;
                    decimal selfProfit = 0;
                    decimal employeeProfit = 0;
                    if (openingSelfContribution != 0)
                    {
                        openingSelfProfit = ((ope + pro) * openingSelfContribution) / (openingSelfContribution);
                        //openingEmployeeProfit = ((ope + pro) * openingEmployeeContribution) / (openingEmployeeContribution + openingSelfContribution);
                        selfProfit = pro / 2;
                        employeeProfit = pro / 2;

                        //selfProfit = (ope * ose) / (openingSelfContribution + openingEmployeeContribution);
                        //employeeProfit = (ope * em) / (openingSelfContribution + openingEmployeeContribution);
                    }
                    else
                    {
                        openingSelfProfit = 0;
                        openingEmployeeProfit = 0;
                        selfProfit = 0;
                        employeeProfit = 0;
                    }
                    //End Fahim

                    //Edited by Fahim 01/11/2015                 
                    var joiningDate = Convert.ToDateTime(list.Select(x => x.JoiningDate).FirstOrDefault()).ToShortDateString();
                    var activationDate = Convert.ToDateTime(list.Select(x => x.PFActivationDate).FirstOrDefault()).ToShortDateString();
                    var deactivationDate = Convert.ToDateTime(list.Select(x => x.PFDeactivationDate).FirstOrDefault()).ToShortDateString();
                    if (deactivationDate == "01-01-0001")
                    {
                        deactivationDate = " ";
                    }
                    //Fahim End

                    //if (fromDate != null && toDate != null)
                    //{
                    //    // string fromYear = fromDate.Value.Year + "";
                    //    //string fromMonth = fromDate.Value.Month.ToString().PadLeft(2, '0');
                    //    //v = temp.Where(t => Convert.ToDateTime(t.Month) >= fromDate && Convert.ToDateTime(t.Month) <= toDate).ToList();
                    //    v = temp.Where(t => Convert.ToInt32(t.Month) >= fromDate.Value.Month && Convert.ToInt32(t.Month) <= toDate.Value.Month).ToList();
                    //}
                    //else if (fromDate == null && toDate != null)
                    //{
                    //    //v = temp.Where(t => Convert.ToDateTime(t.Month) <= toDate).ToList();
                    //    v = temp.Where(t => Convert.ToInt32(t.Month) <= toDate.Value.Month).ToList();
                    //}
                    //else if (fromDate != null && toDate == null)
                    //{
                    //    //v = temp.Where(t => Convert.ToDateTime(t.Month) >= fromDate).ToList();
                    //    v = temp.Where(t => Convert.ToInt32(t.Month) >= fromDate.Value.Month).ToList();
                    //}
                    //else
                    //{
                    //    v = temp;
                    //}
                    if (empID > 0)
                    {
                        v = v.Where(w => w.EmpID == empID).ToList();
                    }
                    var getCompany = unitOfWork.CompanyInformationRepository.GetByID(OCode);
                    ReportParameterCollection reportParameters = new ReportParameterCollection();
                    reportParameters.Add(new ReportParameter("rpCompanyName", getCompany.CompanyName + ""));
                    reportParameters.Add(new ReportParameter("rpCompanyAddress", getCompany.CompanyAddress + ""));
                    reportParameters.Add(new ReportParameter("rpMemberName", list.Select(x => x.EmpName).FirstOrDefault() + ""));
                    reportParameters.Add(new ReportParameter("rpDesignation", list.Select(x => x.opDesignationName).FirstOrDefault() + ""));
                    reportParameters.Add(new ReportParameter("rpID", list.Select(x => x.IdentificationNumber).FirstOrDefault() + ""));
                    reportParameters.Add(new ReportParameter("rpOpeningBalance", _MvcApplication.GetNumber(openingBalance).ToString("N", CInfo) + ""));
                    reportParameters.Add(new ReportParameter("rpFromDate", fromDate + ""));
                    reportParameters.Add(new ReportParameter("rpToDate", toDate + ""));
                    reportParameters.Add(new ReportParameter("rpuserName", userName + ""));
                    //Edited by Fahim 25/10/2015
                    reportParameters.Add(new ReportParameter("rpOpeningSelfContribution", _MvcApplication.GetNumber(openingSelfContribution) + ""));
                    reportParameters.Add(new ReportParameter("rpOpeningEmployeeContribution", _MvcApplication.GetNumber(openingEmployeeContribution) + ""));
                    reportParameters.Add(new ReportParameter("rpOpeningSeflProfit", _MvcApplication.GetNumber(openingSelfProfit) + ""));
                    reportParameters.Add(new ReportParameter("rpSelfProfit", _MvcApplication.GetNumber(selfProfit) + ""));
                    reportParameters.Add(new ReportParameter("rpEmployeeProfit", _MvcApplication.GetNumber(employeeProfit) + ""));
                    reportParameters.Add(new ReportParameter("rpOpeningEmployeeProfit", _MvcApplication.GetNumber(openingEmployeeProfit) + ""));
                    //reportParameters.Add(new ReportParameter("rpLoanableAmount", _MvcApplication.GetNumber(openingEmployeeProfit) + ""));
                    //reportParameters.Add(new ReportParameter("rpPayableToEmployee", _MvcApplication.GetNumber(openingEmployeeProfit) + ""));
                    //reportParameters.Add(new ReportParameter("rpseviceDuration", "8 years" + ""));

                    
                    //New Editer By Fahim 01/11/2015

                    reportParameters.Add(new ReportParameter("rpJoiningDate", joiningDate));
                    reportParameters.Add(new ReportParameter("rpActivationDate", activationDate));
                    reportParameters.Add(new ReportParameter("rpDeactivationDate", deactivationDate));

                    //End Fahim
                    lr.SetParameters(reportParameters);
                    //End
                }
                rd = new ReportDataSource("DataSet1", v);

            }
            #endregion

            #region PFMonthlyContribution
            else if (reportOptions == "PFMonthlyContribution")
            {
                string path = Path.Combine(Server.MapPath("~/Reporting/PF"), "PFMonthlyContribution.rdlc");
                if (System.IO.File.Exists(path))
                {
                    lr.ReportPath = path;
                }
                else
                {
                    return View("Report/ReportPF/Index");
                }

                List<VM_PFMonthlyStatus> v = new List<VM_PFMonthlyStatus>();
                using (UnitOfWork unitOfWork = new UnitOfWork())
                {
                    var temp = unitOfWork.CustomRepository.PFMonthlyStatus(OCode).ToList();
                    //Added By Avishek Date:Feb-18-2015
                    //Start
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

                    v = temp.Where(x => x.ProcessRunDate >= fdate && x.ProcessRunDate <= tdate).OrderBy(x => x.ProcessRunDate).ToList();
                    foreach (var item in v)
                    {
                        item.SelfContribution = _MvcApplication.GetNumber(item.SelfContribution);
                        item.EmpContribution = _MvcApplication.GetNumber(item.EmpContribution);
                    }
                    //if (fromDate != null && toDate != null)
                    //{
                    //    // string fromYear = fromDate.Value.Year + "";
                    //    //string fromMonth = fromDate.Value.Month.ToString().PadLeft(2, '0');
                    //    //v = temp.Where(t => Convert.ToDateTime(t.Month) >= fromDate && Convert.ToDateTime(t.Month) <= toDate).ToList();
                    //    v = temp.Where(t => Convert.ToInt32(t.Month) >= fromDate.Value.Month && Convert.ToInt32(t.Month) <= toDate.Value.Month).ToList();
                    //}
                    //else if (fromDate == null && toDate != null)
                    //{
                    //    //v = temp.Where(t => Convert.ToDateTime(t.Month) <= toDate).ToList();
                    //    v = temp.Where(t => Convert.ToInt32(t.Month) <= toDate.Value.Month).ToList();
                    //}
                    //else if (fromDate != null && toDate == null)
                    //{
                    //    //v = temp.Where(t => Convert.ToDateTime(t.Month) >= fromDate).ToList();
                    //    v = temp.Where(t => Convert.ToInt32(t.Month) >= fromDate.Value.Month).ToList();
                    //}
                    //else
                    //{
                    //    v = temp;
                    //}
                    var getCompany = unitOfWork.CompanyInformationRepository.GetByID(OCode);
                    ReportParameterCollection reportParameters = new ReportParameterCollection();
                    reportParameters.Add(new ReportParameter("rpCompanyName", getCompany.CompanyName + ""));
                    reportParameters.Add(new ReportParameter("rpCompanyAddress", getCompany.CompanyAddress + ""));
                    reportParameters.Add(new ReportParameter("rpFromDate", fromDate + ""));
                    reportParameters.Add(new ReportParameter("rpToDate", toDate + ""));
                    reportParameters.Add(new ReportParameter("rpuserName", userName + ""));
                    lr.SetParameters(reportParameters);
                    //End
                }

                //if (w != null)
                //{
                //    ReportParameter[] param = new ReportParameter[3];
                //    param[0] = new ReportParameter("ItemName", w.Name, false);
                //    param[1] = new ReportParameter("ItemDescription", w.Description, false);
                //    param[2] = new ReportParameter("AvailQuantity", w.Qty + "", false);
                //    lr.SetParameters(param);
                //}
                rd = new ReportDataSource("DataSet1", v);

            }
            #endregion

            #region Yearly Statement
            else if (reportOptions == "YearlyStatement")
            {
                if (empID == null)
                {
                    return Content("Please select a member ID");
                }

                string path = Path.Combine(Server.MapPath("~/Reporting/PF"), "YearlyContribution.rdlc");
                decimal openingBalance = 0;
                VM_Employee vm_employee = new VM_Employee();
                using (UnitOfWork unitOfWork = new UnitOfWork())
                {
                    vm_employee = unitOfWork.CustomRepository.GetAllEmployee(OCode).Where(e => e.EmpID == empID).SingleOrDefault();
                }
                UnitOfWork getCompanyInfo = new UnitOfWork();
                var getCompany = getCompanyInfo.CompanyInformationRepository.GetByID(OCode);
                if (System.IO.File.Exists(path))
                {
                    lr.ReportPath = path;
                }
                else
                {
                    return View("Report/ReportPF/Index");
                }
                List<VM_PFMonthlyStatus> v = new List<VM_PFMonthlyStatus>();
                using (UnitOfWork unitOfWork = new UnitOfWork())
                {
                    //var temp = unitOfWork.CustomRepository.EmpPFMonthlyStatus(empID ?? 0).ToList();//Date:Sep-30
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

                    var temp = unitOfWork.CustomRepository.EmpPFMonthlyStatusForReport(OCode, empID ?? 0, fdate, tdate).OrderBy(x => x.ContrebutionDate).ToList();

                    List<tbl_ProfitDistributionDetail> _tbl_ProfitDistributionDetail = unitOfWork.CustomRepository.GetProfitDistributionByEmpId(Convert.ToInt16(empID)).Where(x => x.TransactionDate >= fdate && x.TransactionDate <= tdate).ToList();
                    List<VM_Amortization> _VM_Amortization = unitOfWork.CustomRepository.AmortizationList(Convert.ToInt16(empID)).Where(x => x.Processed != 1).ToList();
                    foreach (VM_PFMonthlyStatus item in temp)
                    {
                        VM_PFMonthlyStatus atbl_ProfitDistributionDetail = new VM_PFMonthlyStatus();
                        atbl_ProfitDistributionDetail.EmpID = item.EmpID;
                        atbl_ProfitDistributionDetail.IdentificationNumber = item.IdentificationNumber;
                        atbl_ProfitDistributionDetail.ProcessRunDate = item.ProcessRunDate;
                        atbl_ProfitDistributionDetail.ContrebutionDate = item.ContrebutionDate;
                        atbl_ProfitDistributionDetail.EmpContribution = _MvcApplication.GetNumber(item.EmpContribution);
                        atbl_ProfitDistributionDetail.SelfContribution = _MvcApplication.GetNumber(item.SelfContribution);
                        atbl_ProfitDistributionDetail.Year = item.Year;
                        atbl_ProfitDistributionDetail.Month = item.Month;
                        atbl_ProfitDistributionDetail.SCInterest = _MvcApplication.GetNumber(Convert.ToDecimal(_tbl_ProfitDistributionDetail.Where(x => x.TransactionDate.Value.Month == Convert.ToInt32(item.Month) && x.TransactionDate.Value.Year == Convert.ToInt32(item.Year)).Select(x => x.DistributedAmount).FirstOrDefault()));//Meningfull property name are not used for avoiding currently sisutation 
                        atbl_ProfitDistributionDetail.PaidAmount = _MvcApplication.GetNumber(Convert.ToDecimal(_VM_Amortization.Where(x => x.ConMonth == item.Month && x.ConYear == item.Year).Select(X => X.Amount).FirstOrDefault()));
                        atbl_ProfitDistributionDetail.Principle = _MvcApplication.GetNumber(Convert.ToDecimal(_VM_Amortization.Where(x => x.ConMonth == item.Month && x.ConYear == item.Year).Select(X => X.Principal).FirstOrDefault()));
                        v.Add(atbl_ProfitDistributionDetail);
                    }
                    if (vm_employee.JoiningDate < fdate)
                    {
                        var sum = unitOfWork.CustomRepository.EmpPFMonthlyStatusForReport(OCode, empID ?? 0, DateTime.MinValue, fdate).ToList();
                        var profit = unitOfWork.CustomRepository.GetProfitDistributionByEmpId(Convert.ToInt16(empID)).Where(x => x.TransactionDate >= DateTime.MinValue && x.TransactionDate <= fdate).ToList();
                        double loan = unitOfWork.CustomRepository.AmortizationList(Convert.ToInt16(empID)).Sum(x => x.Principal);
                        double loanPaid = unitOfWork.CustomRepository.AmortizationList(Convert.ToInt16(empID)).Where(x => x.Processed != 1 && x.PaymentDate <= fdate).Sum(x => x.Principal);
                        double totalLoan = loan - loanPaid;
                        if ((double)(sum.Sum(x => x.EmpContribution) + sum.Sum(x => x.SelfContribution)) > loan)
                        {
                            openingBalance = _MvcApplication.GetNumber(sum.Sum(x => x.EmpContribution) +
                         sum.Sum(x => x.SelfContribution) +
                         (profit.Sum(x => x.DistributedAmount) ?? 0) -
                         (decimal)(totalLoan == 0 ? 0 : totalLoan));
                        }
                        else
                        {
                            openingBalance = _MvcApplication.GetNumber(sum.Sum(x => x.EmpContribution) +
                            sum.Sum(x => x.SelfContribution) +
                            (profit.Sum(x => x.DistributedAmount) ?? 0));
                        }
                    }
                    else
                    {
                        openingBalance = 0;
                    }
                }
                rd = new ReportDataSource("DataSet1", v);

                ReportParameterCollection reportParameters = new ReportParameterCollection();
                reportParameters.Add(new ReportParameter("EmpID", empID + ""));
                reportParameters.Add(new ReportParameter("rpOpeningBalance", openingBalance + ""));
                reportParameters.Add(new ReportParameter("EmpName", vm_employee.EmpName));
                reportParameters.Add(new ReportParameter("JoiningDate", vm_employee.JoiningDate + ""));
                reportParameters.Add(new ReportParameter("Designation", vm_employee.DesignationName));
                reportParameters.Add(new ReportParameter("Branch", vm_employee.BranchName));
                reportParameters.Add(new ReportParameter("rpCompanyName", getCompany.CompanyName + ""));
                reportParameters.Add(new ReportParameter("rpCompanyAddress", getCompany.CompanyAddress + ""));
                reportParameters.Add(new ReportParameter("rpFromDate", fdate.ToShortDateString() + ""));
                reportParameters.Add(new ReportParameter("rpToDate", tdate.ToShortDateString() + ""));
                lr.SetParameters(reportParameters);
                //this.reportViewer.LocalReport.SetParameters(reportParameters);
            }


            #endregion

            #region PFClosedMemberlist
            else if (reportOptions == "PFClosedMemberlist")
            {
                string path = Path.Combine(Server.MapPath("~/Reporting/PF"), "PFClosedMemberlist.rdlc");
                if (System.IO.File.Exists(path))
                {
                    lr.ReportPath = path;
                }
                else
                {
                    return View("Report/ReportPF/Index");
                }
                List<VM_Employee> v = new List<VM_Employee>();
                using (UnitOfWork unitOfWork = new UnitOfWork())
                {

                    v = unitOfWork.CustomRepository.GetDeactivatedPFMemberReport(fromDate, toDate).ToList();
                    var getCompany = unitOfWork.CompanyInformationRepository.GetByID(OCode);
                    ReportParameterCollection reportParameters = new ReportParameterCollection();
                    reportParameters.Add(new ReportParameter("rpCompanyName", getCompany.CompanyName + ""));
                    reportParameters.Add(new ReportParameter("rpCompanyAddress", getCompany.CompanyAddress + ""));
                    lr.SetParameters(reportParameters);
                }
                rd = new ReportDataSource("DataSet1", v);
            }
            #endregion

            #region PFMonthlyDetail
            else if (reportOptions == "PFMonthlyDetail")
            {
                string path = Path.Combine(Server.MapPath("~/Reporting/PF"), "PFMonthlyDetail.rdlc");
                if (System.IO.File.Exists(path))
                {
                    lr.ReportPath = path;
                }
                else
                {
                    return Content("Report path not exist!");
                }

                using (UnitOfWork unitOfWork = new UnitOfWork())
                {
                    if (fdate != null)
                    {
                        string year = fdate.Year + "";
                        string month = fdate.Month.ToString().PadLeft(2, '0');
                        bool isRecordExist = unitOfWork.ContributionRepository.IsExist(f => f.ConYear == year && f.ConMonth == month);
                        if (isRecordExist)
                        {
                            var v = unitOfWork.CustomRepository.GetContributionDetail().Where(w => w.ConYear == year && w.ConMonth == month).OrderBy(x => x.EmpName).ToList();
                            rd = new ReportDataSource("DataSet1", v);
                        }
                        else
                        {
                            return Content("<br /><br /><div class=\"alert alert-info\">Previously not processed!</div>");
                        }
                    }
                    var getCompany = unitOfWork.CompanyInformationRepository.GetByID(OCode);
                    ReportParameterCollection reportParameters = new ReportParameterCollection();
                    reportParameters.Add(new ReportParameter("rpCompanyName", getCompany.CompanyName + ""));
                    reportParameters.Add(new ReportParameter("rpCompanyAddress", getCompany.CompanyAddress + ""));
                    lr.SetParameters(reportParameters);
                }
            }
            #endregion
            
             #region PF Payment Receive account
            //Added By Kamrul Hasan February 10, 2019
            //Cash book start
            
            else if (reportOptions == "PaymentReceiveStatement")
            {
                string path = Path.Combine(Server.MapPath("~/Reporting/PF"), "PaymentReceiveReport.rdlc");
                if (System.IO.File.Exists(path))
                {
                    lr.ReportPath = path;
                }
                else
                {
                    return View("Report/ReportLoan/Index");
                }
                List<VMPaymentReceiveStatement> received = new List<VMPaymentReceiveStatement>();
                List<VMPaymentReceiveStatement> payment = new List<VMPaymentReceiveStatement>();
                using (UnitOfWork unitOfWork = new UnitOfWork())
                //using (PFTMEntities context = new PFTMEntities())
                {
                    decimal openingBalance = 0;
                    received = unitOfWork.CustomRepository.spPrepareReceiveStatement(fromDate, toDate, ref openingBalance);
                    payment = unitOfWork.CustomRepository.spPreparePaymentStatement(fromDate, toDate);
                    var getCompany = unitOfWork.CompanyInformationRepository.GetByID(OCode);
                    ReportParameterCollection reportParameters = new ReportParameterCollection();
                    reportParameters.Add(new ReportParameter("rpToDate", String.Format("{0:MMM/d/yyyy}", toDate)));
                    reportParameters.Add(new ReportParameter("rpFromDate", String.Format("{0:MMM/d/yyyy}", fromDate)));
                    reportParameters.Add(new ReportParameter("rpOpeningBalance", openingBalance + ""));
                    reportParameters.Add(new ReportParameter("rpCompanyName", getCompany.CompanyName + ""));
                    reportParameters.Add(new ReportParameter("rpCompanyAddress", getCompany.CompanyAddress + ""));
                    lr.SetParameters(reportParameters);
                }
                rd = new ReportDataSource("DataSet1", received);
                rd2 = new ReportDataSource("DataSet2", payment);
                lr.DataSources.Add(rd2);
            }
            //End Kamrul Cash book report
            #endregion

            #region Check Print

                ///Added By Kamrul Hasan 18/06/2019 for Settled Member Check Print
            else if (reportOptions == "CheckPrint")
            {
                string path = Path.Combine(Server.MapPath("~/Reporting/PF"), "ChequePrint.rdlc");
                if (System.IO.File.Exists(path))
                {
                    lr.ReportPath = path;
                }
                else
                {
                    return View("Report/ReportPF/Index");
                }
                int OCODE = ((int?)Session["OCode"]) ?? 0;
                if (OCODE == 0)
                {
                    RedirectToAction("Login", "Account");
                }

                if (OCODE == 0)
                {
                    return Json(new { Success = false, ErrorMessage = "You must be under a compnay to execute this process!" }, JsonRequestBehavior.DenyGet);
                }
                //string chequeno = txtChequeNo.Text;
                //string OCODE = ((SessionUser)Session["SessionUser"]).OCode;
                //Cheque_BankInfo_BLL qwe = new Cheque_BankInfo_BLL();
                List<ChequeR> chequePrint = new List<ChequeR>();
                using (UnitOfWork unitOfWork = new UnitOfWork())
                {
                chequePrint = unitOfWork.CustomRepository.GetChequePrint(empID, OCODE);
                //ReportViewerAc.LocalReport.DataSources.Clear();
                //ReportDataSource reportDataset = new ReportDataSource("DS_ChequePrint", chequePrint);
                //ReportViewerAc.LocalReport.DataSources.Add(reportDataset);
                //ReportViewerAc.LocalReport.ReportPath = Server.MapPath("/ERP_Accounting/Reports/ChequePrint.rdlc");
                //ReportViewerAc.LocalReport.Refresh();
                
                    //var getCompany = unitOfWork.CompanyInformationRepository.GetByID(OCode);
                    //ReportParameterCollection reportParameters = new ReportParameterCollection();
                    //reportParameters.Add(new ReportParameter("rpCompanyName", getCompany.CompanyName + ""));
                    //reportParameters.Add(new ReportParameter("rpCompanyAddress", getCompany.CompanyAddress + ""));
                    //lr.SetParameters(reportParameters);
                }
                rd = new ReportDataSource("DS_ChequePrint", chequePrint);
                //if (chequePrint.Count > 0)
                //{
                //    Session["rptDs"] = "DS_ChequePrint";
                //    Session["rptDt"] = chequePrint;
                //    Session["rptFile"] = "/ERP_Accounting/Reports/ChequePrint.rdlc";
                //    Session["rptTitle"] = "Employee Wise Leave Info";
                //    Response.Redirect("ReportViewer.aspx");
                //}

            }
            #endregion
            lr.DataSources.Add(rd);

            string reportType = id;
            string mimeType;
            string encoding;
            string fileNameExtension;
            string deviceInfo =

            "<DeviceInfo>" +
            "  <OutputFormat>" + id + "</OutputFormat>" +
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
