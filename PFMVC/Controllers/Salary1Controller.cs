using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using DLL;
using DLL.Repository;
using DLL.ViewModel;
using PFMVC.common;
using System.Globalization;
using DLL.Utility;

namespace PFMVC.Controllers
{
    public class Salary1Controller : Controller
    {

        int PageID = 4;
        private UnitOfWork unitOfWork = new UnitOfWork();
        //DP_Employee dp_Employee = new DP_Employee();
        //DP_Salary dp_Salary = new DP_Salary();
        DataSet dataset = new DataSet();
        OleDbConnection _excelConnection;
        List<VM_Contribution> _lstVmContribution;
        VM_Contribution _vmContribution;
        IEnumerable<VM_Employee> _listOfEmployee;
        //IEnumerable<LU_tbl_PFRules> list_of_rules;

        public struct PFRule
        {
            public decimal EmployeeCon;
            public decimal EmployerCon;
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

        /// <summary>
        /// Imports this instance.
        /// </summary>
        /// <returns>view</returns>
        /// <ReviewBy>Avishek</ReviewBy>
        /// <ReviewDate>24-Feb-2016 vc de</ReviewDate>
        [Authorize]
        public ActionResult Import()
        {
            //Added By Avishek Date:Dec-20-2015
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End
            ViewBag.Message = TempData["Message"] as string;
            ViewBag.ErrorMessage = TempData["ErrorMessage"] as string;
            ViewBag.Month = TempData["Month"] as string;
            return View();
        }

        public ActionResult PendingCountribution()
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End
            //var v = unitOfWork.ContributionRepository.Get().Where(w => w.OCode == oCode).ToList();
            //var v = unitOfWork.CustomRepository.GetPendingContributionList().ToList();
            var listOfPendingContribution = unitOfWork.CustomRepository.GetPendingContributionList().ToList();
            var v = listOfPendingContribution.GroupBy(x => x.EmpID,
                         (key, xs) => xs.OrderByDescending(x => x.ProcessDate).First()).ToList();
            //ViewBag.Message = "Following " + v.Count + " Employees' accounting entry pending!";
            if (v.Count > 0)
            {
                ViewBag.TotalCount = v.Count + "";
            }
            return View("PendingCountribution", v);

            //ViewBag.Message = TempData["Message"] as string;
            //ViewBag.ErrorMessage = TempData["ErrorMessage"] as string;
            //ViewBag.Month = TempData["Month"] as string;
            //return View();
        }

        /// <summary>
        /// Imports the excel.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns>List</returns>
        /// <ModifiedBy>Avishek</ModifiedBy>
        /// <ModificationDate>Apr-20-2015</ModificationDate>
        [Authorize]
        public ActionResult ImportExcel(string date)
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });//Added By Avishek Date:Dec-20-2015
                //return Json(new { Success = false, ErrorMessage = "You must be under a compnay!" }, JsonRequestBehavior.DenyGet);
            }

            bool isAllowedToEdit = PagePermission.HasPermission(User.Identity.Name, PageID, 3);
            if (!isAllowedToEdit)
            {
                ViewBag.Message = "You are not allowed to EXECUTE information! contact system admin!";
                return View("Import");
            }

            //Eddited by Izab Ahmed on 05-12-2018
            DateTime lastDate = unitOfWork.AuditLogRepository.Get().Select(x => x.LastAuditDate.Value).Max();

            if (Convert.ToDateTime(date) <= lastDate)
            {
                TempData["ErrorMessage"] = "You cann't save this voucher because " + Convert.ToDateTime(date).ToString("dd-MMM-yyyy") + " Audit is complete.";
                return RedirectToAction("Import");
                //ViewBag.Message = "You cann't save this contribution because " + Convert.ToDateTime(date).ToString("dd-MMM-yyyy") + " Audit is complete.";
                //return Json(new { Success = false, ErrorMessage = "You cann't save this voucher because " + Convert.ToDateTime(date).ToString("dd-MMM-yyyy") + " Audit is complete." }, JsonRequestBehavior.DenyGet);
            }
            else
            {
                int noOfExcelMember = 0;
                int noOfSystemPfMember = 0;
                int noOfInvalidMember = 0;
                int noOfExcelPfMember = 0;
                string extension = "";

                DateTime datetime;
                try
                {
                    ViewBag.ContributionDate = datetime = DateTime.ParseExact(date, "dd/MMM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                }
                catch (Exception x)//if (!DateTime.TryParse(date, out datetime))
                {
                    TempData["ErrorMessage"] = "Please correct the Contrebution month..." + x.Message;
                    return RedirectToAction("Import");
                }
                ViewBag.Month = datetime.ToString("dd/MMM/yyyy");
                //check if Account transaction for this month already processed
                LU_tbl_Branch branch_detail = new LU_tbl_Branch();
                string branch = branch_detail.BranchLocation;
                string year = datetime.Year + "";
                string month = datetime.Month.ToString().PadLeft(2, '0');
                var isContributionMonthRecorded = unitOfWork.ContributionMonthRecordRepository.Get(w => w.ConYear == year && w.ConMonth == month).SingleOrDefault();

                if (isContributionMonthRecorded != null)
                {
                    if (isContributionMonthRecorded.PassVoucher == false && DLL.Utility.ApplicationSetting.Branch == false)
                    {
                        TempData["ErrorMessage"] = "Contribution for month " + datetime.Month + "/" + datetime.Year + " account transaction complete! Process cannot continue...";
                        return RedirectToAction("Import");
                    }

                    //if (isContributionMonthRecorded.OCode != 1)
                    //{
                    //    TempData["ErrorMessage"] = "Contribution for month " + datetime.Month + "/" + datetime.Year + " account transaction complete! Process cannot continue...";
                    //    return RedirectToAction("Import");
                    //}
                }
                
                //-
                try
                {
                    extension = Path.GetExtension(Request.Files["FileUpload1"].FileName);
                }
                catch
                {
                    TempData["ErrorMessage"] = "Error occured at previous step!";
                    return RedirectToAction("Import");
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
                            _excelConnection = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + path1 + ";Extended Properties=\"Excel 8.0;HDR=Yes;IMEX=2\"");
                            _excelConnection.Open();
                        }
                        else if (Path.GetExtension(path1) == ".xlsx")
                        {
                            _excelConnection = new OleDbConnection(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + path1 + ";Extended Properties='Excel 12.0;HDR=YES;IMEX=1;';");
                            _excelConnection.Open();
                        }

                        OleDbCommand cmd = new OleDbCommand();
                        OleDbDataAdapter oleda = new OleDbDataAdapter();

                        cmd.Connection = _excelConnection;
                        cmd.CommandType = CommandType.Text;
                        //cmd.CommandText = "SELECT * FROM [" + txtSheetName.Text + "$" + txtHDRStrtIndex.Text + ":QQ65536]";
                        try
                        {
                            cmd.CommandText = "Select [EMPLOYEE ID],[EMPLOYEE NAME], [OWN CONT], [EMPLOYER CONT] from [Sheet1$]";
                            oleda = new OleDbDataAdapter(cmd);
                            oleda.Fill(dataset, "SalaryData");
                        }
                        catch (Exception x)
                        {
                            TempData["Month"] = datetime.ToString("dd/MMM/yyyy");
                            TempData["ErrorMessage"] = "Error Encountered - " + x.Message;
                            return RedirectToAction("Import");
                        }
                        finally
                        {
                            OleDbConnection.ReleaseObjectPool();
                            _excelConnection.Close();
                            _excelConnection.Dispose();
                            if (System.IO.File.Exists(path1))
                                System.IO.File.Delete(path1);
                        }
                        try
                        {
                            DataTable dt = dataset.Tables["SalaryData"];
                            var query = dt.AsEnumerable().Select(s => new
                            {
                                EmployeeID = s.Field<string>("EMPLOYEE ID"),
                                EmpName = s.Field<object>("EMPLOYEE NAME"),
                                OwnCont = s.Field<object>("OWN CONT"),
                                EmployerCont = s.Field<object>("EMPLOYER CONT")
                            }).ToList();

                            noOfExcelMember = dt.Rows.Count;
                            _lstVmContribution = new List<VM_Contribution>();

                            foreach (var item in query)
                            {
                                //if no EmployeeID found we should ignore that record, isn't it?
                                if (string.IsNullOrEmpty(item.EmployeeID))
                                {
                                    continue;
                                }

                                PFRule pfRule = GetEmployeeInfo(item.EmployeeID, datetime);
                                _vmContribution = new VM_Contribution();
                                _vmContribution.IdentificationNumber = item.EmployeeID;
                                _vmContribution.EmpID = pfRule.EmpID;
                                _vmContribution.EmpName = pfRule.EmpName;
                                _vmContribution.JoiningDate = pfRule.JoiningDate;
                                _vmContribution.PFActivationDate = pfRule.PFActivationDate;
                                _vmContribution.ConMonth = datetime.Month + "";
                                _vmContribution.ConYear = datetime.Year + "";
                                _vmContribution.ProcessDate = datetime;
                                try
                                {
                                    _vmContribution.EmpName = item.EmpName + "";
                                }
                                catch
                                {
                                    _vmContribution.Message += "Contribution -" + item.EmpName + "- not in correct format.";
                                }
                                /// Added By Kamrul 2019-01-06
                                //try
                                //{
                                //     item.EmployeeID.Contains(branch);
                                //}
                                //catch
                                //{
                                //    _vmContribution.Message += "Employee ID -" + item.EmployeeID + "- not in correct format.";
                                //}
                                /// End Kamrul
                                try
                                {
                                    _vmContribution.SelfContribution = Convert.ToDecimal(item.OwnCont);
                                }
                                catch
                                {
                                    _vmContribution.Message += "Own Cont. -" + item.OwnCont + "- not in correct format, this value required!";
                                }
                                try
                                {
                                    _vmContribution.EmpContribution = Convert.ToDecimal(item.EmployerCont);
                                }
                                catch
                                {
                                    _vmContribution.Message += "Employer cont. -" + item.EmployerCont + "- not in correct format, this value required!";
                                }
                                //--------------------------
                                //try
                                //{
                                //    _vmContribution.IdentificationNumber = item.EmployeeID.Contains('Dh');
                                //}
                                //catch
                                //{
                                //    _vmContribution.Message += "Employer ID. -" + item.EmployeeID + "- not in correct format, this value required!";
                                //}
                                if (!pfRule.canProcess)
                                {
                                    noOfInvalidMember++;
                                    _vmContribution.Message = pfRule.Message;
                                }
                                else
                                {
                                    noOfExcelPfMember++;
                                }
                                _vmContribution.PFRuleID = pfRule.pfRuleID; // get rule id from regarding method
                                _lstVmContribution.Add(_vmContribution); // adding object to list.
                            }
                        }
                        catch (Exception x)
                        {
                            ViewBag.Month = "";
                            ViewBag.ErrorMessage = "Error Encountered - " + x.Message;
                            return RedirectToAction("Import");
                        }
                    }
                    else
                    {
                        TempData["Month"] = datetime.ToString("dd/MMM/yyyy");
                        TempData["ErrorMessage"] = "Please upload Contrebution file in excel format...";
                        return RedirectToAction("Import");
                    }
                }
                else
                {
                    TempData["Month"] = datetime.ToString("dd/MMM/yyyy");
                    TempData["ErrorMessage"] = "Please upload only valid excel file...";
                    return RedirectToAction("Import");
                }
                //hopefully no error occured...

                //for top bar styling - important for visualization

                List<tbl_Employees> pfMember = new List<tbl_Employees>();
                ///---- Added By Kamrul 2019-03-01
                if (ApplicationSetting.JoiningDate == true)
                {
                    pfMember = unitOfWork.EmployeesRepository.Get().Where(p => p.JoiningDate <= datetime).ToList();
                }
                else
                {
                    pfMember = unitOfWork.EmployeesRepository.Get().Where(p => p.PFActivationDate <= datetime).ToList();
                }

                ///--- End Kamrul
                noOfSystemPfMember = pfMember.Count();
                int totalDomain = (noOfExcelMember + (noOfSystemPfMember - noOfExcelPfMember));
                if (noOfSystemPfMember > noOfExcelPfMember)
                {
                    ViewBag.Message1 = (noOfSystemPfMember - noOfExcelPfMember) + " PF member's record not found in excel!";
                    ViewBag.Message1_P = ((((double)noOfSystemPfMember - noOfExcelPfMember) / totalDomain) * 100) + "%";
                }
                ViewBag.Message2 = noOfInvalidMember + " invalid member found who are not in PF member list.";
                double percentageOfInvalidMember = (double)noOfInvalidMember / totalDomain;
                ViewBag.Message2_P = (percentageOfInvalidMember * 100) + "%";
                ViewBag.Message3 = noOfExcelPfMember + " member's record will be processed.";
                ViewBag.Message3_P = ((noOfExcelPfMember / (double)totalDomain) * 100) + "%";
                ViewBag.Month = datetime.ToString("dd/MMM/yyyy");// DateTime.ParseExact(datetime + "", "dd/MMM/yyyy", System.Globalization.CultureInfo.InvariantCulture);;
                ViewBag.Message = "Contribution contribution for month " + datetime.ToString("MMMM, yyyy") + " not processed! following valid data will be process only...";
                return View("ExcelMonthlyContributionNotProcessed", _lstVmContribution);
            }
            //End
            //return RedirectToAction("Import");

        }

        //need to review this method
        public PFRule GetEmployeeInfo(string identificationNumber, DateTime salaryMonth)
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            pfRule = new PFRule();
            DateTime pfActivationDate;
            DateTime joiningDate;
            int membershipDuration = 0;

            if (_listOfEmployee == null)
            {
                _listOfEmployee = unitOfWork.CustomRepository.GetAllEmployee(oCode).ToList();
            }
            try
            {
                //if employee exist in database. search with Identification number
                var v = _listOfEmployee.FirstOrDefault(w => w.IdentificationNumber.Trim() == identificationNumber.Trim());
                joiningDate = v.JoiningDate ?? DateTime.MaxValue;
                pfActivationDate = v.PFActivationDate ?? DateTime.MaxValue;

                if (!v.IsPFMember)
                {
                    pfRule.EmpName = v.EmpName;
                    pfRule.EmpID = v.EmpID;
                    pfRule.EmployeeCon = 0;
                    pfRule.canProcess = false;
                    pfRule.Message = "This employee is not PF Member!";
                    pfRule.JoiningDate = joiningDate;
                    pfRule.PFActivationDate = pfActivationDate;
                    return pfRule;
                }
                pfRule.EmpName = v.EmpName;
                pfRule.EmpID = v.EmpID;
                pfRule.canProcess = true;
                pfRule.Message = "";
                pfRule.JoiningDate = joiningDate;
                pfRule.PFActivationDate = pfActivationDate; //datetime.now should not be activated!
                return pfRule;
            }
            catch
            {
                //will catch if record not exist in DB
                pfRule.EmployeeCon = 0;
                pfRule.EmployerCon = 0;
                pfRule.canProcess = false;
                pfRule.Message = "This employee not in database!";
                pfRule.JoiningDate = DateTime.MaxValue;
                return pfRule;
            }
        }

        [Authorize]
        public ActionResult MonthlyContribution()
        {
            //Added By Avishek Date:Dec-20-2015
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End
            return View("MonthlyContribution");
        }

        /// <summary>
        /// Monthlies the contribution submit.
        /// </summary>
        /// <param name="_dtMonth">The _DT month.</param>
        /// <param name="v">The v.</param>
        /// <returns>bool</returns>
        /// <ReviewBy>Avishek</ReviewBy>
        /// <ReviewDate>Dec-20-2015</ReviewDate>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MonthlyContributionSubmit(string _dtMonth, List<VM_Contribution> v)
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });//Added By Avishek Date:Dec-20-2015
                //return Json(new { Success = false, ErrorMessage = "You must be under a compnay!" }, JsonRequestBehavior.DenyGet);
            }

            tbl_Contribution tbl_contribution;
            DateTime editDate = DateTime.Now;
            string editUserName = User.Identity.Name;
            Guid editUserId = unitOfWork.CustomRepository.GetUserID(User.Identity.Name);
            DateTime datetime;
            try
            {
                datetime = Convert.ToDateTime(_dtMonth);
            }
            catch (Exception x)//if (!DateTime.TryParse(date, out datetime))
            {
                TempData["ErrorMessage"] = "Please correct the Contrebution month..." + x.Message;
                return RedirectToAction("Import");
            }

            string year = datetime.Year + "";
            string month = datetime.Month.ToString().PadLeft(2, '0');
            var isContributionMonthRecorded = unitOfWork.ContributionMonthRecordRepository.Get(w => w.ConMonth == month && w.ConYear == year && w.OCode == oCode).SingleOrDefault();
            if (isContributionMonthRecorded != null)
            {
                //if (isContributionMonthRecorded.OCode != 1)
                //{
                //    TempData["ErrorMessage"] = "Contribution for month " + datetime.ToString("MMMM, yyyy") + " account transaction complete! Process cannot continue...";
                //    return RedirectToAction("Import");
                //}
                if (isContributionMonthRecorded.PassVoucher && DLL.Utility.ApplicationSetting.Branch == false)
                {
                    TempData["ErrorMessage"] = "Contribution for month " + datetime.ToString("MMMM, yyyy") + " account transaction complete! Process cannot continue...";
                    return RedirectToAction("Import");
                }
            }
            if (isContributionMonthRecorded == null)
            {
                //new entry
                tbl_ContributionMonthRecord model = new tbl_ContributionMonthRecord();
                model.ConYear = year;
                model.ConMonth = month;
                model.PassVoucher = false;
                model.PassVoucherMessage = "";
                model.EditDate = editDate;
                model.EditUserName = editUserName;
                model.EditUser = editUserId;
                model.ID = (unitOfWork.ContributionMonthRecordRepository.Get().Max(m => (int?)m.ID) ?? 0) + 1;
                model.OCode = oCode;
                model.ContributionDate = datetime;
                unitOfWork.ContributionMonthRecordRepository.Insert(model);
            }
            else
            {
                //update existing]
                isContributionMonthRecorded.EditDate = editDate;
                isContributionMonthRecorded.EditUserName = editUserName;
                isContributionMonthRecorded.EditUser = editUserId;
                unitOfWork.ContributionMonthRecordRepository.Update(isContributionMonthRecorded);
            }
            //Now let's delete if contribution previously exist.
            bool isRecordExist = unitOfWork.ContributionRepository.IsExist(f => f.ConYear == year && f.ConMonth == month && f.OCode == oCode);
            if (isRecordExist)
            {
                //return Json(new { Success = false, ErrorMessage = "Salary contribution for this month already processed" }, JsonRequestBehavior.AllowGet);
                List<int> listEmpId = v.Select(s => s.EmpID).ToList();
                var existingEmpContribution = unitOfWork.ContributionRepository.Get(w => listEmpId.Contains(w.EmpID) && w.ConYear == year && w.ConMonth == month).ToList();
                existingEmpContribution.ForEach(f => unitOfWork.ContributionRepository.Delete(f));
            }

            foreach (var item in v.Where(item => string.IsNullOrEmpty(item.Message) && item.EmpID > 0))
            {
                tbl_contribution = new tbl_Contribution();
                tbl_contribution.ConMonth = month.PadLeft(2, '0');
                tbl_contribution.ConYear = year;
                tbl_contribution.EmpID = item.EmpID;
                tbl_contribution.EmpContribution = item.EmpContribution;
                //tbl_contribution.ECPercentage = item.ECPercentage;
                tbl_contribution.SelfContribution = item.SelfContribution;
                //tbl_contribution.SCPercentage = item.SCPercentage;
                tbl_contribution.ProcessDate = datetime;
                tbl_contribution.EditDate = DateTime.Now;
                tbl_contribution.EditUser = editUserId;
                tbl_contribution.PFRulesID = -1; //Rule ID is -1 because no formula is applying here...
                //tbl_contribution.Salary = item.Salary; //logic change
                tbl_contribution.ProcessID = -1; // exporting from excel file
                tbl_contribution.OCode = oCode;
                tbl_contribution.ContributionDate = datetime;
                unitOfWork.ContributionRepository.Insert(tbl_contribution);
            }
            try
            {
                unitOfWork.Save();
                //now update salaryMonth record : later added
                var conDetail = unitOfWork.ContributionRepository.Get().Where(w => w.ConMonth == month && w.ConYear == year && w.OCode == oCode).GroupBy(g => new { g.ConYear, g.ConMonth }).Select(s => new { selfCont = s.Sum(f => f.SelfContribution), empCont = s.Sum(f => f.EmpContribution) }).FirstOrDefault();
                var conMonRec = unitOfWork.ContributionMonthRecordRepository.Get(w => w.ConMonth == month && w.ConYear == year && w.OCode == oCode).Single();
                conMonRec.TotalEmpCont = conDetail == null ? 0 : conDetail.empCont;
                conMonRec.TotalSelfCont = conDetail == null ? 0 : conDetail.selfCont;
                conMonRec.OCode = oCode;
                conMonRec.ContributionDate = datetime;
                unitOfWork.ContributionMonthRecordRepository.Update(conMonRec);
                try
                {
                    unitOfWork.Save();
                }
                catch (Exception x)
                {
                    TempData["ErrorMessage"] = "Contribution for month " + datetime.ToString("MMMM, yyyy") + " successfully processed BUT ERROR OCCURED WHEN UPDATING CONTRIBUTION MONTHLY RECORD ENTITY!!!";
                    return RedirectToAction("Import");
                }
                //
                TempData["Message"] = "Contribution for month " + datetime.ToString("MMMM, yyyy") + " successfully processed!";
                return RedirectToAction("Import");
            }
            catch (Exception x)
            {
                TempData["ErrorMessage"] = "Operation failed while updating Contrebution information!!! Check the error: " + x.Message;
                return RedirectToAction("Import");
            }
        }

        /// <summary>
        /// Contributions the previously exist.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns>View</returns>
        /// <ModifiedMy>Avishek</ModifiedMy>
        /// <ModificationDate>Dec-20-2015</ModificationDate>
        public ActionResult ContributionPreviouslyExist(string date)
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            //Added By Avishek Date:Dec-20-2015
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End
            DateTime datetime;
            if (!DateTime.TryParse(date, out datetime))
            {
                ViewBag.ErrorMessage = "Please correct the Contrebution month...";
                return Content("Date received in incorrect format!");
            }
            ViewBag.Month = datetime;
            //check if salary for this month already processed

            string year = datetime.Year + "";
            string month = datetime.Month.ToString().PadLeft(2, '0');
            bool isRecordExist = unitOfWork.ContributionRepository.IsExist(f => f.ConYear == year && f.ConMonth == month && f.OCode == oCode);
            if (isRecordExist)
            {
                var v = unitOfWork.CustomRepository.GetContributionDetail().Where(w => w.ConYear == year && w.ConMonth == month && w.OCode == oCode).ToList();
                ViewBag.Month = datetime.ToString("yyyy/MMM/dd");
                ViewBag.Message = "Contribution for " + datetime.ToString("MMMM") + ", " + datetime.ToString("yyyy") + " already processed.";
                return PartialView("ExcelMonthlyContributionProcessed", v);
            }
            return Content("<br /><br /><div class=\"alert alert-info\">Previously not processed!</div>");
        }

        public ActionResult SalaryMonthRecord()
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            new CultureInfo("en-IN");
            var v = unitOfWork.ContributionMonthRecordRepository.Get(o => o.OCode == oCode).OrderByDescending(o => o.ConYear).ThenByDescending(o => o.ConMonth).ToList();
            return PartialView("SalaryMonthRecord", v);
        }

        #region AccountTransaction Contribution
        /// <summary>
        /// Passes the voucher confirm.
        /// </summary>
        /// <param name="month">The month.</param>
        /// <param name="year">The year.</param>
        /// <returns>bool</returns>
        /// <ModifiedBy>Avishek</ModifiedBy>
        /// <ModificationDate>Dec-20-2015</ModificationDate>
        [HttpPost]
        public ActionResult PassVoucherConfirm(string month, string year)
        {
            int voucherId = 0;
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });//Added By Avishek Date:Dec-20-2015
            }

            string curUserName = User.Identity.Name;
            Guid curUserId = unitOfWork.CustomRepository.GetUserID(User.Identity.Name);
            string refMessage = "";
            month = month.PadLeft(2, '0');

            var isAlreadyTransacted = unitOfWork.ContributionMonthRecordRepository.Get(w => w.ConMonth == month && w.ConYear == year && w.OCode == oCode).SingleOrDefault();
            if (isAlreadyTransacted != null)
            {
                if (isAlreadyTransacted.PassVoucher == true)
                {
                    return Json(new { Success = false, ErrorMessage = "Transaction completed previously at " + isAlreadyTransacted.EditDate.ToString("dd MMM, yyyy") + " by " + isAlreadyTransacted.EditUserName }, JsonRequestBehavior.DenyGet);
                }
            }
            
            var v = unitOfWork.CustomRepository.GetContributionDetail().Where(w => w.ConYear == year && w.ConMonth == month && w.OCode == oCode).ToList();
            
            //Added for Using with Branch Added By Kamrul.
            if (DLL.Utility.ApplicationSetting.Branch == true)
            {
                v = unitOfWork.CustomRepository.GetContributionDetailWithBranch().Where(w => w.ConYear == year && w.ConMonth == month && w.OCode == oCode).ToList();
            }
            if (v.Count == 0)
            {
                return Json(new { Success = false, ErrorMessage = "No record found for account transaction..." }, JsonRequestBehavior.DenyGet);
            }

            List<Guid> ledgerIdList = new List<Guid>();
            List<decimal> credit = new List<decimal>();
            List<decimal> debit = new List<decimal>();
            List<string> chqNumber = new List<string>();
            List<string> pfLoanId = new List<string>();
            List<string> pfMemberId = new List<string>();

            decimal totalSelfContribution = 0;
            decimal totalEmpContribution = 0;
            foreach (var item in v)
            {
                totalSelfContribution += item.SelfContribution;
                totalEmpContribution += item.EmpContribution;
            }

            //remember diff btwn empID and identification number
            //Account should be exist with Each Identification Number 
            //LedgerNameList.Add("Members Fund"); //this is convention
            //members fund should be credited!
            List<acc_Chart_of_Account_Maping> accChartOfAccountMaping = unitOfWork.ChartofAccountMapingRepository.Get().ToList();
            Guid ledgerIdforOwnConLiability = accChartOfAccountMaping.Where(x => x.MIS_Id == 3).Select(x => x.Ledger_Id).FirstOrDefault();
            ledgerIdList.Add(ledgerIdforOwnConLiability);
            credit.Add(totalSelfContribution);
            debit.Add(0);
            chqNumber.Add("");
            pfMemberId.Add("");
            pfLoanId.Add("");

            Guid ledgerIdforOwnConAsset = accChartOfAccountMaping.Where(x => x.MIS_Id == 1).Select(x => x.Ledger_Id).FirstOrDefault();
            ledgerIdList.Add(ledgerIdforOwnConAsset);
            credit.Add(0);
            debit.Add(totalSelfContribution);
            chqNumber.Add("");
            pfMemberId.Add("");
            pfLoanId.Add("");
            //Edited by Suman

           // Guid ledgerIdforEmpConLiability = accChartOfAccountMaping.Where(x => x.MIS_Id == 16).Select(x => x.Ledger_Id).FirstOrDefault();
            Guid ledgerIdforEmpConLiability = accChartOfAccountMaping.Where(x => x.MIS_Id == 18).Select(x => x.Ledger_Id).FirstOrDefault();

            ledgerIdList.Add(ledgerIdforEmpConLiability);
            credit.Add(totalEmpContribution);
            debit.Add(0);
            chqNumber.Add("");
            pfMemberId.Add("");
            pfLoanId.Add("");

            Guid ledgerIdforEmpConAsset = accChartOfAccountMaping.Where(x => x.MIS_Id == 2).Select(x => x.Ledger_Id).FirstOrDefault();
            ledgerIdList.Add(ledgerIdforEmpConAsset);
            credit.Add(0);
            debit.Add(totalEmpContribution);
            chqNumber.Add("");
            pfMemberId.Add("");
            pfLoanId.Add("");

            bool isOperationSuccess = unitOfWork.AccountingRepository.DualEntryVoucherById(0, 5, isAlreadyTransacted.ContributionDate ?? DateTime.Now, ref voucherId, "Contribution and subscription for the month " + month + "of the" + year, ledgerIdList, debit, credit, chqNumber, ref refMessage, curUserName, curUserId, pfMemberId, "", month, year, null, pfLoanId, oCode, "Contribution");

            if (isOperationSuccess)
            {
                isAlreadyTransacted.PassVoucher = true;
                isAlreadyTransacted.PassVoucherMessage = "Tansaction completed successfull at " + DateTime.Now;
                unitOfWork.ContributionMonthRecordRepository.Update(isAlreadyTransacted);
                try
                {
                    unitOfWork.Save();
                    return Json(new { Success = true, Message = "Transction Sucessfull and status updated!" }, JsonRequestBehavior.DenyGet);
                }
                catch (Exception x)
                {
                    return Json(new { Success = false, ErrorMessage = "Transction Sucessfull BUT STATUS UPDATE FAILED WITH FOLLOWING ERROR: " + x.Message + " PLEASE CONTACT SYS ADMIN!" }, JsonRequestBehavior.DenyGet);
                }
            }
            return Json(new { Success = false, ErrorMessage = "Transaction Failded with following error : " + refMessage }, JsonRequestBehavior.DenyGet);
        }
        #endregion

        /// <summary>
        /// Contributions this instance.
        /// </summary>
        /// <returns>view</returns>
        /// <CreatedBy>Avishek</CreatedBy>
        /// <CreatedDate>Apr-27-2015</CreatedDate>
        public ActionResult Contribution()
        {
            //Added By Avishek Date:Dec-20-2015
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End
            return View();
        }

        /// <summary>
        /// Autocompletes the suggestions.
        /// </summary>
        /// <param name="term">The term.</param>
        /// <returns>list</returns>
        /// <CreatedBy>Avishek</CreatedBy>
        /// <CreatedDate>Apr-27-2015</CreatedDate>
        public JsonResult AutocompleteSuggestions(string term)
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            var suggestions = unitOfWork.EmployeesRepository.Get().Where(w => w.IdentificationNumber.Contains(term) && w.OCode == oCode && w.PFStatus != 2).Select(s => new { value = s.EmpID, label = s.IdentificationNumber }).ToList();
            return Json(suggestions, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Gets the employee.
        /// </summary>
        /// <param name="empId">The emp identifier.</param>
        /// <returns>List</returns>
        /// <CreatedBy>Avishek</CreatedBy>
        /// <CreatedDate>Apr-27-2015</CreatedDate>
        [HttpPost]
        public JsonResult GetEmployee(int empId)
        {
            tbl_Employees employee = unitOfWork.EmployeesRepository.GetByID(empId);
            tbl_Employees _employee = new tbl_Employees();
            _employee.EmpName = employee.EmpName;
            _employee.Designation = employee.Designation;
            _employee.Department = employee.Department;
            return Json(_employee);
        }

        /// <summary>
        /// Monthlies the contribution save.
        /// </summary>
        /// <param name="empIdentification">The emp identification.</param>
        /// <param name="wonCon">The won con.</param>
        /// <param name="empCon">The emp con.</param>
        /// <param name="date">The date.</param>
        /// <returns>bool</returns>
        /// <CreatedBy>Avishek</CreatedBy>
        /// <CreatedDate>Apr-27-2015</CreatedDate>
        [HttpPost]
        public ActionResult MonthlyContributionSave(string empIdentification, decimal wonCon, decimal empCon, DateTime date)
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            string message;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });//Added By Avishek Date:Dec-20-2015
                //return Json(new { Success = false, ErrorMessage = "You must be under a compnay!" }, JsonRequestBehavior.DenyGet);
            }
            tbl_Contribution tbl_contribution;

            DateTime editDate = DateTime.Now;
            string editUserName = User.Identity.Name;
            Guid editUserId = unitOfWork.CustomRepository.GetUserID(User.Identity.Name);
            DateTime datetime;
            try
            {
                datetime = date;
            }
            catch (Exception x)
            {
                message = "Please correct the Contrebution month..." + x.Message;
                return Json(message);
            }

            string year = datetime.Year + "";
            string month = datetime.Month.ToString().PadLeft(2, '0');
            int empid = unitOfWork.CustomRepository.GetEmployeeByIdentificationNumber(empIdentification).Select(x => x.EmpID).FirstOrDefault();
            tbl_ContributionMonthRecord isContributionMonthRecorded = new tbl_ContributionMonthRecord();
            DateTime contributionDate = DateTime.Now;
            isContributionMonthRecorded = unitOfWork.ContributionMonthRecordRepository.Get(w => w.ConMonth == month && w.ConYear == year && w.OCode == oCode).SingleOrDefault();
            if (isContributionMonthRecorded != null)
            {
                if (isContributionMonthRecorded.PassVoucher == false && DLL.Utility.ApplicationSetting.Branch == false)
                {
                    message = "Contribution for month " + datetime.ToString("MMMM, yyyy") + " account transaction complete or Voucher Passed ! Process cannot continue...";
                    return Json(message);
                }
                //if (isContributionMonthRecorded.OCode != 1)
                //{
                //    message = "Contribution for month " + datetime.ToString("MMMM, yyyy") + " account transaction complete or Voucher Passed ! Process cannot continue...";
                //    return Json(message);
                //}
            }
            else
            {
                var contributionDateExists = unitOfWork.ContributionRepository.Get(w => w.ConMonth == month && w.ConYear == year && w.OCode == oCode).Select(x => x.ContributionDate).FirstOrDefault();
                var isContributioned = unitOfWork.ContributionRepository.Get(w => w.ConMonth == month && w.ConYear == year && w.OCode == oCode && w.EmpID == empid).SingleOrDefault();
                if (isContributioned != null)
                {
                    message = "Contribution for month " + datetime.ToString("MMMM, yyyy") + " Contribution allready compleated! Process cannot continue...";
                    return Json(message);
                }
                else if (contributionDateExists == null)
                {
                    contributionDate = date;
                }
                else
                {
                    contributionDate = (DateTime)contributionDateExists;
                }
            }

            if (isContributionMonthRecorded == null)
            {
                tbl_ContributionMonthRecord model = new tbl_ContributionMonthRecord();
                model.ConYear = year;
                model.ConMonth = month;
                model.PassVoucher = false;
                model.PassVoucherMessage = "";
                model.EditDate = editDate;
                model.EditUserName = editUserName;
                model.EditUser = editUserId;
                model.ID = (unitOfWork.ContributionMonthRecordRepository.Get().Max(m => (int?)m.ID) ?? 0) + 1;
                model.OCode = oCode;
                model.ContributionDate = datetime;
                unitOfWork.ContributionMonthRecordRepository.Insert(model);
            }

            var existingEmpContribution = unitOfWork.ContributionRepository.Get().Where(w => w.EmpID == empid && w.ConYear == year && w.ConMonth == month).ToList();
            if (existingEmpContribution.Count > 0)
            {
                existingEmpContribution.ForEach(f => unitOfWork.ContributionRepository.Delete(f));
            }

            tbl_contribution = new tbl_Contribution();
            tbl_contribution.ConMonth = month.PadLeft(2, '0');
            tbl_contribution.ConYear = year;
            tbl_contribution.EmpID = empid;
            tbl_contribution.EmpContribution = empCon;
            tbl_contribution.SelfContribution = wonCon;
            tbl_contribution.ProcessDate = datetime;
            tbl_contribution.EditDate = DateTime.Now;
            tbl_contribution.EditUser = editUserId;
            tbl_contribution.PFRulesID = -1;
            tbl_contribution.ProcessID = -1;
            tbl_contribution.OCode = oCode;
            tbl_contribution.ContributionDate = contributionDate == DateTime.Now ? datetime : contributionDate;
            unitOfWork.ContributionRepository.Insert(tbl_contribution);
            try
            {
                unitOfWork.Save();
                var conDetail = unitOfWork.ContributionRepository.Get().Where(w => w.ConMonth == month && w.ConYear == year && w.OCode == oCode).GroupBy(g => new { g.ConYear, g.ConMonth }).Select(s => new { selfCont = s.Sum(f => f.SelfContribution), empCont = s.Sum(f => f.EmpContribution) }).FirstOrDefault();
                var conMonRec = unitOfWork.ContributionMonthRecordRepository.Get(w => w.ConMonth == month && w.ConYear == year && w.OCode == oCode).Single();
                conMonRec.TotalEmpCont = conDetail == null ? 0 : conDetail.empCont;
                conMonRec.TotalSelfCont = conDetail == null ? 0 : conDetail.selfCont;
                conMonRec.OCode = oCode;
                conMonRec.ContributionDate = datetime;
                unitOfWork.ContributionMonthRecordRepository.Update(conMonRec);
                try
                {
                    unitOfWork.Save();
                }
                catch (Exception x)
                {
                    message = "Contribution for month " + datetime.ToString("MMMM, yyyy") + " successfully processed BUT ERROR OCCURED WHEN UPDATING CONTRIBUTION MONTHLY RECORD ENTITY!!!";
                    return RedirectToAction(message);
                }
                //TempData["Message"] = "Contribution for month " + datetime.ToString("MMMM, yyyy") + " successfully processed!";
                message = "Contribution for month " + datetime.ToString("MMMM, yyyy") + " successfully processed!";
                return Json(message);
            }
            catch (Exception x)
            {
                message = "Operation failed while updating Contrebution information!!! Check the error: " + x.Message;
                return Json(message);
            }
        }

    }
}
