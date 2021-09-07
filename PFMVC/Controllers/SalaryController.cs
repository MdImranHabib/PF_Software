using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using CustomJsonResponse;
using DLL;
using DLL.DataPrepare;
using DLL.Repository;
using DLL.ViewModel;
using Telerik.Web.Mvc;
using System.Threading;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;
using PFMVC.common;

namespace PFMVC.Controllers
{
    public class SalaryController : Controller
    {
        int PageID = 4;
        private UnitOfWork unitOfWork = new UnitOfWork();
        DP_Employee dp_Employee = new DP_Employee();
        //DP_Salary dp_Salary = new DP_Salary();
        DataSet dataset = new DataSet();
        OleDbConnection excelConnection;
        List<VM_Contribution> lst_vm_contribution;
        VM_Contribution vm_Contribution = new VM_Contribution();
        IEnumerable<VM_Employee> list_of_employee;
        IEnumerable<LU_tbl_PFRules> list_of_rules;
        string MessageToCarry = "";


        public struct PFRule
        {
            public decimal SelfContribution;
            public decimal EmpContribution;
            public string EmpName;
            public int EmpID;
            public string MembershipDuration;
            public DateTime PFActivationDate;
            public bool canProcess;
            public string Message;
            public int pfRuleID;
            public DateTime JoiningDate;
        };

        public PFRule pfRule;


        [Authorize]
        public ActionResult Index()
        {
            ViewBag.PageName = "Employee Information";
            //IsUserValidToAccessModule isUserValidToAccessModule = new IsUserValidToAccessModule();
            string ModuleName = "Salary";
            bool b = true;// isUserValidToAccessModule.CheckValidation(User.Identity.Name, ModuleName);
            if (b)
            {
                var result = unitOfWork.EmployeesRepository.Get().Select(s => new { s.EmpID });
                ViewBag.Employee = new SelectList( result, "EmpID", "EmpID");
                return View();
            }
            else
            {
                ViewBag.PageName = "Salary";
                return View("Unauthorized");
            }
        }

    
        public ActionResult SalaryForm(int empID, int rowID = 0)
        {
            //var v = unitOfWork.CustomRepository.GetSalary().Where(e => e.EmpID == empID && e.RowID == rowID).FirstOrDefault();
            var v = new VM_Salary();
            CurrentEmployeeInfoViewBag(empID);
            return PartialView("SalaryForm", v);
        }

        public void CurrentEmployeeInfoViewBag(int empID)
        {
            var g = unitOfWork.CustomRepository.GetAllEmployee().Where(e => e.EmpID == empID).FirstOrDefault();
            ViewBag.EmpName = g.EmpName;
            ViewBag.EmpID = g.IdentificationNumber;
        }

        public ActionResult GetEmployeeSalary(int empID)
        {
            var v = unitOfWork.BasicSalaryRepository.Get().Where(w => w.EmpID == empID).LastOrDefault();
            VM_Salary vm_salary;
            if (v != null)
            {
                 vm_salary= dp_Salary.VM_Salary(v);
            }
            else
            {
                vm_salary = new VM_Salary();
            }
            return PartialView("_SalaryInformation",  vm_salary);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SalaryForm(VM_Salary v)
        {
            if (ModelState.IsValid)
            {
                string _message = "";
                tbl_Basic_Salary s;
                if (v.RowID == 0)
                {

                    s = dp_Salary.tbl_Basic_Salary(v);
                    s.EditDate = System.DateTime.Now;
                    s.EditUser = unitOfWork.CustomRepository.GetUserID(User.Identity.Name);
                    unitOfWork.BasicSalaryRepository.Insert(s);
                    _message = "Salary information stored!";
                }
                else
                {
                    return Json(new { Success = false, ErrorMessage = "Error! RowID not zero!" }, JsonRequestBehavior.DenyGet);
                }
                //else
                //{
                //    s = dp_Salary.tbl_Basic_Salary(v);
                //    s.EditDate = System.DateTime.Now;
                //    s.EditUser = unitOfWork.CustomRepository.GetUserID(User.Identity.Name);
                //    unitOfWork.BasicSalaryRepository.Update(s);
                //    _message = "Salary information updated";
                //}
                try
                {
                    unitOfWork.Save();
                    return Json(new { Success = true, Message = _message }, JsonRequestBehavior.AllowGet);
                }
                catch (DataException x)
                {
                    return Json(new { Success = false, ErrorMessage = x.Message + " \n=> " + x.InnerException }, JsonRequestBehavior.AllowGet);
                }
              
            }
            var errorList = (from item in ModelState.Values
                             from error in item.Errors
                             select error.ErrorMessage).ToList();

            return Json(JsonResponseFactory.ErrorResponse(errorList), JsonRequestBehavior.DenyGet);
        }

        //public ActionResult GetPFStatus(int empID)
        //{
        //    var v = unitOfWork.CustomRepository.GetEmpPFStatus().Where(e => e.EmpID == empID).FirstOrDefault();
        //    return PartialView("_CurrentPFStatus", v);
        //}

        public ActionResult GetSalaryHistory(int empID)
        {
            var v = unitOfWork.BasicSalaryRepository.Get().Where(w => w.EmpID == empID).OrderByDescending(o => o.EffectiveDate).ToList();
            CurrentEmployeeInfoViewBag(empID);
            return PartialView("_SalaryHistory", v);
        }

        public string GetMaxID()
        {
            var query = "SELECT isnull(MAX(convert(int, EmpID)),0) FROM [dbo].[tbl_Employees]";
            var data = unitOfWork.CountryRepository.GetRowCount(query);
            string s = (Convert.ToInt32(data) + 1) + "";
            return s.Trim().PadLeft(6, '0');
        }


        [Authorize]
        public ActionResult Import()
        {
            ViewBag.Message = TempData["MessageToCarry"] as string;
            return View();
        }

        
        public ActionResult ImportExcel(string date)
        {
            bool IsAllowedToEdit = PagePermission.HasPermission(User.Identity.Name, PageID, 3);
            if (!IsAllowedToEdit)
            {
                ViewBag.Message = "You are not allowed to EXECUTE information! contact system admin!";
                return View("Import");
            }

            int no_of_excel_member = 0;
            int no_of_system_pf_member = 0;
            int no_of_invalid_member = 0;
            int no_of_excel_pf_member = 0;
            string extension = "";

            DateTime datetime;
            if(!DateTime.TryParse(date, out datetime))
            {   
                ViewBag.Message = "Please correct the salary month...";
                return View("Import");
            }
            ViewBag.Month = datetime;
            //check if salary for this month already processed


            string year = datetime.Year + "";
            string month = datetime.Month.ToString().PadLeft(2, '0');

           
            /*
            bool isRecordExist = unitOfWork.ContributionRepository.IsExist(f => f.ConYear == year && f.ConMonth == month);
            if (isRecordExist)
            {
                var v = unitOfWork.CustomRepository.GetContributionDetail().Where(w => w.ConYear == year && w.ConMonth == month).ToList();

                ViewBag.Month = datetime.ToString("yyyy/MMM/dd");
                ViewBag.Message = "Salary contribution for " + datetime.ToString("MMMM") + ", " + datetime.ToString("yyyy") + " already processed.";
                double rate = InteresetRate(datetime);
                decimal rate_monthly = (decimal)(rate/100)/12;
                var PFMonthlyInterest = unitOfWork.CustomRepository.GetMonthlyPFInterest(month, year).ToList();

                decimal _CumulativeEmpContribution = 0;
                decimal _CumulativeSelfContribution = 0;
                foreach (var item in v)
                {
                    _CumulativeEmpContribution = PFMonthlyInterest.Where(w => w.EmpID == item.EmpID).Select(s => s.EmpContributionTillNow).SingleOrDefault();
                    _CumulativeSelfContribution = PFMonthlyInterest.Where(w => w.EmpID == item.EmpID).Select(s => s.SelfContributionTillNow).SingleOrDefault();
                    item.CumulativeEmpContribution =  _CumulativeEmpContribution + item.EmployerContribution;
                    item.CumulativeSelfContribution = _CumulativeSelfContribution + item.SelfContribution;
                    //item.SCInterest = _CumulativeSelfContribution * rate_monthly;
                    //item.ECInterest = _CumulativeEmpContribution * rate_monthly;
                }
                
                ViewBag.CurrentInterestRate = "Interest rate for "+datetime.ToString("MMMM, yyyy")+" is "+rate.ToString("0.00");
                if (v[0].InterestRate != null)
                {
                    ViewBag.AppliedInterestRate = "Applied interest rate : " + v[0].InterestRate.Value.ToString("0.00");
                }
                else
                {
                    ViewBag.AppliedInterestRate = "Applied interest rate : NULL (Manually Imported)";
                }
                return PartialView("ExcelMonthlyContributionProcessed", v);
            }

            */


            try
            {
                extension = System.IO.Path.GetExtension(Request.Files["FileUpload1"].FileName);
            }
            catch
            {
                MessageToCarry = "Error occured at previous step!";
                RedirectToAction("Import");
            }
            
            if (extension.Contains(".xls") || extension.Contains(".xlsx"))
            {
                if (Request.Files["FileUpload1"].ContentLength > 0)
                {

                    string path1 = string.Format("{0}/{1}", Server.MapPath("~/ImportedExcel/Salary"), datetime.Month + "-" + datetime.Year + extension);
                    if (System.IO.File.Exists(path1))
                        System.IO.File.Delete(path1);

                    Request.Files["FileUpload1"].SaveAs(path1);



                    if (Path.GetExtension(path1) == ".xls")
                    {
                        excelConnection = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + path1 + ";Extended Properties=\"Excel 8.0;HDR=Yes;IMEX=2\"");
                        excelConnection.Open();
                    }
                    else if (Path.GetExtension(path1) == ".xlsx")
                    {
                        excelConnection = new OleDbConnection(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + path1 + ";Extended Properties='Excel 12.0;HDR=YES;IMEX=1;';");
                        excelConnection.Open();
                    }

                    OleDbCommand cmd = new OleDbCommand();
                    OleDbDataAdapter oleda = new OleDbDataAdapter();



                    cmd.Connection = excelConnection;
                    cmd.CommandType = CommandType.Text;
                    //cmd.CommandText = "SELECT * FROM [" + txtSheetName.Text + "$" + txtHDRStrtIndex.Text + ":QQ65536]";

                    cmd.CommandText = "Select [Employee ID],[Basic Salary] from [Sheet1$]";

                    oleda = new OleDbDataAdapter(cmd);
                    oleda.Fill(dataset, "SalaryData");

                    OleDbConnection.ReleaseObjectPool();
                    excelConnection.Close();
                    excelConnection.Dispose();

                    
                    DataTable dt = dataset.Tables["SalaryData"];
                    try
                    {
                        var query = dt.AsEnumerable().Select(s => new
                            {
                                EmployeeID = s.Field<string>("Employee ID"),
                                Salary = s.Field<string>("Basic Salary")
                            }).ToList();

                        no_of_excel_member = dt.Rows.Count;

                        lst_vm_contribution = new List<VM_Contribution>();

                        decimal Salary = 0;
                        foreach (var item in query)
                        {
                            
                            vm_Contribution = new VM_Contribution();

                            //if (item.Salary == null)
                            //{
                            //    continue;
                            //}
                            Salary = 0;
                            try
                            {
                                Salary = Convert.ToDecimal(item.Salary);
                            }
                            catch(Exception x)
                            {
                                vm_Contribution.Message += "Salary format error! "+x.Message;
                            }

                            PFRule pfRule = GetEmployeeInfo(item.EmployeeID, datetime);
                            vm_Contribution.IdentificationNumber = item.EmployeeID; // 
                            vm_Contribution.EmpID = pfRule.EmpID;
                            vm_Contribution.EmpName = pfRule.EmpName;
                            vm_Contribution.JoiningDate = pfRule.JoiningDate;
                            vm_Contribution.ConMonth = datetime.Month + "";
                            vm_Contribution.ConYear = datetime.Year + "";
                            vm_Contribution.ECPercentage = pfRule.EmpContribution;

                            vm_Contribution.WorkingDuration = pfRule.MembershipDuration;
                            vm_Contribution.ProcessDate = datetime;
                            vm_Contribution.Salary = Salary;
                            vm_Contribution.SCPercentage = pfRule.SelfContribution;
                            vm_Contribution.SelfContribution = Salary * (pfRule.SelfContribution / 100);
                            vm_Contribution.EmpContribution = Salary * (pfRule.EmpContribution / 100);
                            if (!pfRule.canProcess)
                            {
                                no_of_invalid_member++;
                                vm_Contribution.Message = pfRule.Message;
                            }
                            else
                            {
                                no_of_excel_pf_member++;
                            }
                            vm_Contribution.PFRuleID = pfRule.pfRuleID;
                            lst_vm_contribution.Add(vm_Contribution);
                        }
                    }
                    catch
                    {
                        ViewBag.Message = "Please check salary file column datatype correctly...";
                        return View("Import");
                    }
                }
                else
                {
                    ViewBag.Message = "Please upload salary file in excel format...";
                    return View("Import");
                }
            }
            else
            {
                ViewBag.Message = "Please upload only valid excel file...";
                return View("Import");
            }


            //hopefully no error occured...

            var pf_member = unitOfWork.EmployeesRepository.Get().Where(p => p.PFActivationDate <= datetime);
            no_of_system_pf_member = pf_member.Count();
            int total_domain = (no_of_excel_member + (no_of_system_pf_member - no_of_excel_pf_member));
            if (no_of_system_pf_member > no_of_excel_pf_member)
            {
                ViewBag.Message1 = (no_of_system_pf_member - no_of_excel_pf_member) + " PF member's record not found in excel!";
                ViewBag.Message1_P = ((((double)no_of_system_pf_member - no_of_excel_pf_member) / (double)total_domain) * 100)+"%";
            }
            ViewBag.Message2 = no_of_invalid_member + " invalid member found who are not in PF member list.";
            double percentage_of_invalid_member = (double)no_of_invalid_member / (double)total_domain;
            ViewBag.Message2_P = (percentage_of_invalid_member * 100) + "%";
            ViewBag.Message3 = no_of_excel_pf_member + " member's record will be processed.";
            ViewBag.Message3_P = (((double)no_of_excel_pf_member / (double)total_domain) * 100)+"%";
            


            ViewBag.Month = datetime.ToString("dd/MM/yy");
            ViewBag.Message = "Salary contribution for month "+datetime.ToString("MMMM, yyyy")+" not processed! following valid data will be process only...";
            return View("ExcelMonthlyContributionNotProcessed", lst_vm_contribution);
        }

        
        //need to review this method
        public PFRule GetEmployeeInfo(string identificationNumber, DateTime salaryMonth)
        {
            pfRule = new PFRule();
            DateTime pfActivationDate;
            string EmpName;
            DateTime joiningDate;
            int membershipDuration = 0;

            if (list_of_employee == null)
            {
                list_of_employee = unitOfWork.CustomRepository.GetAllEmployee().ToList();
            }

            try
            {
                //if employee exist in database. search with Identification number
                var v = list_of_employee.Where(w => w.IdentificationNumber == identificationNumber).Single();
                EmpName = v.EmpName;
                joiningDate = v.JoiningDate ?? System.DateTime.MaxValue;
                if (!v.IsPFMember)
                {
                    pfRule.EmpName = v.EmpName;
                    pfRule.EmpID = v.EmpID;
                    pfRule.SelfContribution = 0;
                    pfRule.EmpContribution = 0;
                    pfRule.canProcess = false;
                    pfRule.Message = "This employee is not PF Member!";
                    pfRule.JoiningDate = joiningDate;

                    return pfRule;
                }   
                else
                {
                    //if not exist...
                    pfActivationDate = v.PFActivationDate ?? DateTime.MaxValue;
                    pfRule.EmpID = v.EmpID;

                }
            }
            catch
            {
                //will catch if record not exist in DB
                pfRule.SelfContribution = 0;
                pfRule.EmpContribution = 0;
                pfRule.canProcess = false;
                pfRule.Message = "This employee not in database!";
                pfRule.JoiningDate = System.DateTime.MaxValue;
                return pfRule;
            }

            try
            {
                membershipDuration = ((salaryMonth.Year - pfActivationDate.Year) * 12) + salaryMonth.Month - pfActivationDate.Month;
            }
            catch
            {
                membershipDuration = 0;
            }

            if (list_of_rules == null)
            {
                list_of_rules = unitOfWork.PFRulesRepository.Get().OrderByDescending(o => o.WorkingDurationInMonth).Where( w => w.IsActive == 1 && w.EffectiveFrom <= salaryMonth).ToList();
            }


            foreach (var item in list_of_rules)
            {
                if (item.WorkingDurationInMonth <= membershipDuration)
                {
                    pfRule.pfRuleID = item.ROWID;
                    pfRule.SelfContribution = item.EmployeeContribution;
                    pfRule.EmpContribution = item.EmployerContribution;
                    pfRule.EmpName = EmpName;
                    pfRule.JoiningDate = joiningDate;
                    pfRule.MembershipDuration = (membershipDuration / 12) + " year " + (membershipDuration % 12) + " month";
                    pfRule.canProcess = true;
                    
                    return pfRule;
                }
            }

            //following are those entities those not fall under any pfRule
            var x = list_of_employee.Where(w => w.IdentificationNumber == identificationNumber).SingleOrDefault();
            pfRule.EmpName = x.EmpName;
            pfRule.EmpID = x.EmpID;
            pfRule.JoiningDate = joiningDate;
            pfRule.SelfContribution = 0;
            pfRule.EmpContribution = 0;
            pfRule.Message = "No rule defined for this member";
            pfRule.MembershipDuration = (membershipDuration / 12) + " year " + (membershipDuration % 12) + " month";
            pfRule.canProcess = true;
            return pfRule;
        }

        [Authorize]
        public ActionResult MonthlyContribution()
        {
            return View("MonthlyContribution");
        }

        //public ActionResult MonthlyContributionInformation(string dtMonth)
        //{
        //    DateTime datetime;
        //    if (!DateTime.TryParse(dtMonth, out datetime))
        //    {
        //        ViewBag.ErrorMessage = "Please correct the salary month...";
        //        return PartialView("_Error");
        //    }

        //    string year = datetime.Year+"";
        //    string month = datetime.Month.ToString().PadLeft(2, '0');

        //    bool isRecordExist = unitOfWork.ContributionRepository.IsExist(f => f.ConYear == year && f.ConMonth == month);
        //    if (isRecordExist)
        //    {
        //        var v = unitOfWork.CustomRepository.GetContributionDetail().Where(w => w.ConYear == year && w.ConMonth == month).ToList();


        //        ViewBag.Message = "Salary contribution for " + datetime.ToString("MMMM") + ", " + datetime.ToString("yyyy") + " already processed.";
        //        return PartialView("MonthlyContributionProcessed", v);
        //    }
        //    else
        //    {
        //        ViewBag.Message = "Salary contribution for "+datetime.ToString("MMMM")+", "+datetime.ToString("yyyy")+" not processed!";


        //        if (lst_of_salary == null)
        //        {
        //            lst_of_salary = unitOfWork.CustomRepository.GetSalary().ToList();
        //        }
        //        if (list_of_rules == null)
        //        {
        //            list_of_rules = unitOfWork.PFRulesRepository.Get().OrderByDescending(o => o.WorkingDurationInMonth).ToList();
        //        }
        //        lst_vm_contribution = new List<VM_Contribution>();
        //        foreach (var item in lst_of_salary)
        //        {   
        //            vm_Contribution = new VM_Contribution();
        //            PFRule pfRule = GetEmployeeInfo(item.EmpID, datetime, );
        //            vm_Contribution.ConMonth = datetime.Month + "";
        //            vm_Contribution.ConYear = datetime.Year + "";
        //            vm_Contribution.ECInterest = pfRule.EmployerCon;
        //            vm_Contribution.EmpID = item.EmpID;
        //            vm_Contribution.EmpName = item.EmpName;
        //            vm_Contribution.ProcessDate = datetime;
        //            vm_Contribution.Salary = item.Basic;
        //            vm_Contribution.SCInterest = pfRule.EmployeeCon;
        //            vm_Contribution.SelfContribution = item.Basic * (pfRule.EmployeeCon / 100);
        //            vm_Contribution.EmployerContribution = item.Basic * (pfRule.EmployerCon / 100);
        //            //vm_Contribution.Total = vm_Contribution.SelfContribution + vm_Contribution.EmployerContribution;
        //            vm_Contribution.WorkingDuration = pfRule.JobDuration;
        //            vm_Contribution.JoiningDate = item.JoiningDate;
        //            lst_vm_contribution.Add(vm_Contribution);
        //        }
        //        ViewBag.Month = datetime.ToString("dd/MM/yy");
        //        return PartialView("MonthlyContributionNotProcessed", lst_vm_contribution);
        //    }
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MonthlyContributionSubmit(string _dtMonth, List<VM_Contribution> v)
        {
            tbl_Contribution tbl_contribution;
            DateTime datetime;
            if (!DateTime.TryParse(_dtMonth, out datetime))
            {
                ViewBag.ErrorMessage = "Please correct the salary month...";
                return PartialView("_Error");
            }

            string year = datetime.Year + "";
            string month = datetime.Month.ToString().PadLeft(2, '0');

            //Now let's delete if contribution previously exist.
            bool isRecordExist = unitOfWork.ContributionRepository.IsExist(f => f.ConYear == year && f.ConMonth == month);
            if (isRecordExist)
            {
                //return Json(new { Success = false, ErrorMessage = "Salary contribution for this month already processed" }, JsonRequestBehavior.AllowGet);
                List<int> ListEmpID = v.Select(s => s.EmpID).ToList();
                var ExistingEmpContribution = unitOfWork.ContributionRepository.Get().Where(w => ListEmpID.Contains(w.EmpID) && w.ConYear == year && w.ConMonth == month).ToList();
                ExistingEmpContribution.ForEach(f => unitOfWork.ContributionRepository.Delete(f));
            }

            foreach (var item in v)
            {
                if (string.IsNullOrEmpty(item.Message) && item.EmpID > 0)
                {
                    tbl_contribution = new tbl_Contribution();
                    tbl_contribution.ConMonth = month.PadLeft(2, '0');
                    tbl_contribution.ConYear = year;
                    tbl_contribution.EmpID = item.EmpID;
                    tbl_contribution.EmpContribution = item.EmpContribution;
                    tbl_contribution.ECPercentage = item.ECPercentage;
                    tbl_contribution.SelfContribution = item.SelfContribution;
                    tbl_contribution.SCPercentage = item.SCPercentage;
                    tbl_contribution.ProcessDate = System.DateTime.Now;
                    tbl_contribution.EditDate = System.DateTime.Now;
                    tbl_contribution.EditUser = unitOfWork.CustomRepository.GetUserID(User.Identity.Name);
                    tbl_contribution.PFRulesID = item.PFRuleID;
                    tbl_contribution.Salary = item.Salary;

                    unitOfWork.ContributionRepository.Insert(tbl_contribution);
                }
            }

            try
            {
                unitOfWork.Save();
                TempData["MessageToCarry"] = "Salary for month " + datetime.ToString("MMMM, yyyy") + " successfully processed!";
                return RedirectToAction("Import");
            }
            catch(Exception x)
            {
                //return Json(new { Success = false, ErrorMessage = "Check Error:" + "\n" + x.Message }, JsonRequestBehavior.AllowGet);
                MessageToCarry = "Operation failed while updating salary information!!! Check the error: " + x.Message;
                return RedirectToAction("Import");
            }
            /*
            //============test============//
            //get all data from contribution table for that month
            var list_vm_contribution = unitOfWork.ContributionRepository.Get().Where(w => w.ConYear == year && w.ConMonth == month).ToList();

            //get currently applicable interest rate
            double rate = InteresetRate(datetime);
            //monthly factor
            decimal rate_monthly = (decimal)(rate / 100) / 12;

            //contribution summation group by empID -- this is need to calculate SCInterest ann ECInterest
            var PFMonthlyInterest = unitOfWork.CustomRepository.GetMonthlyPFInterest(month, year).ToList();

            decimal _CumulativeEmpContribution = 0;
            decimal _CumulativeSelfContribution = 0;
            foreach (var item in list_vm_contribution)
            {
                _CumulativeEmpContribution = PFMonthlyInterest.Where(w => w.EmpID == item.EmpID).Select(s => s.EmpContributionTillNow).SingleOrDefault();
                _CumulativeSelfContribution = PFMonthlyInterest.Where(w => w.EmpID == item.EmpID).Select(s => s.SelfContributionTillNow).SingleOrDefault();
                item.SCInterest = _CumulativeSelfContribution * rate_monthly;
                item.ECInterest = _CumulativeEmpContribution * rate_monthly;
                item.InterestRate = (decimal)rate;
                unitOfWork.ContributionRepository.Update(item);
            }

            try
            {
                unitOfWork.Save();
                //return Json(new { Success = true, Message = "Salary contribution processed for the month "+datetime.ToString("MMMM") }, JsonRequestBehavior.AllowGet);
                string s = datetime.ToString("dd/MM/yy");
                TempData["MessageToCarry"] = "Salary for month " + datetime.ToString("MMMM, yyyy") + " successfully processed!";
                return RedirectToAction("ImportExcel", "Salary", new { date = s });
            }
            catch (Exception x)
            {
                //return Json(new { Success = false, ErrorMessage = "Check Error:" + "\n" + x.Message }, JsonRequestBehavior.AllowGet);
                MessageToCarry = "Operation failed while calculating monthly interest. Salary information successfully inserted!!! Check the error: " + x.Message;
                return RedirectToAction("Import");
            }

            //============End of test================//
            */
        }


        [HttpPost]
        public ActionResult DeleteAndReupload(string date)
        {

            tbl_Contribution tbl_contribution;
            DateTime datetime;
            if (!DateTime.TryParse(date, out datetime))
            {
                ViewBag.ErrorMessage = "Please correct the salary month...";
                return PartialView("_Error");
            }
            string year = datetime.Year + "";
            string month = datetime.Month.ToString().PadLeft(2, '0');

            bool isRecordExist = unitOfWork.ContributionRepository.IsExist(f => f.ConYear == year && f.ConMonth == month);
            if (isRecordExist)
            {
                var v = unitOfWork.ContributionRepository.Get().Where(w => w.ConMonth == month && w.ConYear == year).ToList();
                foreach (var item in v)
                {
                    unitOfWork.ContributionRepository.Delete(item);
                }
                try
                {
                    unitOfWork.Save();
                    return Json(new { Success = true, Message = "Salary contribution for month " + datetime.ToString("MMMM, yyyy") + " deleted successfully...you may re-upload this file again" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception x)
                {   
                    return Json(new { Success = false, ErrorMessage = "Operation not successfull... check the error : " + x.Message }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {   
                return Json(new { Success = false, ErrorMessage =  "Salary contribution for month " + datetime.ToString("MMMM, yyyy") + " not found..." }, JsonRequestBehavior.AllowGet);
            }
        }

        public double InteresetRate(DateTime datetime)
        {
            int month = datetime.Month;
            int year = datetime.Year;

            var v = unitOfWork.InterestRateRepository.Get().OrderByDescending(o => o.ConYear).ThenBy( o => o.ConMonth).ToList();

            foreach (var item in v)
            {

                if (Convert.ToInt32(item.ConYear) <= year && Convert.ToInt32(item.ConMonth) <= month)
                {
                    return item.InterestRate;
                }
                else if (Convert.ToInt32(item.ConYear) < year)// && Convert.ToInt32(item.ConMonth) > month)
                {
                    return item.InterestRate;
                }
            }
            return 0;
        }


        public ActionResult PFRulesForMonth()
        {
            var v = unitOfWork.PFRulesRepository.Get().OrderByDescending(o => o.WorkingDurationInMonth).ToList();
            return PartialView("_PFRuleList", v);
            
        }

        public ActionResult ContributionPreviouslyExist(string date)
        {
            DateTime datetime;
            if (!DateTime.TryParse(date, out datetime))
            {
                ViewBag.Message = "Please correct the salary month...";
                return Content("Date received in incorrect format!");
            }
            ViewBag.Month = datetime;
            //check if salary for this month already processed


            string year = datetime.Year + "";
            string month = datetime.Month.ToString().PadLeft(2, '0');
            bool isRecordExist = unitOfWork.ContributionRepository.IsExist(f => f.ConYear == year && f.ConMonth == month);
            if (isRecordExist)
            {
                var v = unitOfWork.CustomRepository.GetContributionDetail().Where(w => w.ConYear == year && w.ConMonth == month).ToList();

                ViewBag.Month = datetime.ToString("yyyy/MMM/dd");

                ViewBag.Message = "Salary contribution for " + datetime.ToString("MMMM") + ", " + datetime.ToString("yyyy") + " already processed.";
                //double rate = InteresetRate(datetime);
                //decimal rate_monthly = (decimal)(rate / 100) / 12;
                //var PFMonthlyInterest = unitOfWork.CustomRepository.GetMonthlyPFInterest(month, year).ToList();

                //decimal _CumulativeEmpContribution = 0;
                //decimal _CumulativeSelfContribution = 0;
                foreach (var item in v)
                {
                    //_CumulativeEmpContribution = PFMonthlyInterest.Where(w => w.EmpID == item.EmpID).Select(s => s.EmpContributionTillNow).SingleOrDefault();
                    //_CumulativeSelfContribution = PFMonthlyInterest.Where(w => w.EmpID == item.EmpID).Select(s => s.SelfContributionTillNow).SingleOrDefault();
                    //item.CumulativeEmpContribution = _CumulativeEmpContribution + item.EmployerContribution;
                    //item.CumulativeSelfContribution = _CumulativeSelfContribution + item.SelfContribution;
                    //item.SCInterest = _CumulativeSelfContribution * rate_monthly;
                    //item.ECInterest = _CumulativeEmpContribution * rate_monthly;
                }
                //ViewBag.CurrentInterestRate = "Interest rate for " + datetime.ToString("MMMM, yyyy") + " is " + rate.ToString("0.00");
                //ViewBag.AppliedInterestRate = "Applied interest rate : " + v[0].InterestRate.Value.ToString("0.00");

                return PartialView("ExcelMonthlyContributionProcessed", v);
            }
            //return PartialView("ExcelMonthlyContributionProcessed", Enumerable.Empty < VM_Contribution>());       
            return Content("<br /><br /><div class=\"alert alert-info\">Previously not processed!</div>");
        }
    }
}
