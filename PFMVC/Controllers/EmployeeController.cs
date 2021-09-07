using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using CustomJsonResponse;
using DLL;
using DLL.DataPrepare;
using DLL.Repository;
using DLL.ViewModel;
using PFMVC.common;
using Telerik.Web.Mvc;
using System.Collections;
using System.Globalization;
using Microsoft.Reporting.WebForms;
using System.Data.Entity.Validation;
using DLL.Utility;
using System.Web.Hosting;
using PFMVC.Models;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PFMVC.Controllers
{
    public class EmployeeController : Controller
    {
        int PageID = 3;

        private UnitOfWork unitOfWork = new UnitOfWork();
        private PFTMEntities db = new PFTMEntities();
        
        DP_Employee _dpEmployee = new DP_Employee();
        OleDbConnection _excelConnection;
        DataSet _dataset = new DataSet();
        DateTime _pfActivationDate = DateTime.MinValue;
        DateTime _pfJoiningDate = DateTime.MinValue;
        List<string> _listEmpIdofExistingEmp;
        MvcApplication _mvcApplication;

        /// <summary>
        /// Indexes this instance.
        /// </summary>
        /// <returns>View</returns>
        /// <ReViewBy>Avishek</ReViewBy>
        /// <ReViewDate>24-Feb-2016</ReViewDate>
        [Authorize]
        public ActionResult Index()
        {

            ViewBag.branch_id = new SelectList(db.LU_tbl_Branch, "branch_id", "branch_name");
            ViewBag.branchList = db.LU_tbl_Branch;
            ViewBag.PageName = "Employee Information";
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                RedirectToAction("Login", "Account");
            }
            bool b = PagePermission.HasPermission(User.Identity.Name, PageID, 0);
            if (b)
            {
                return View();
            }
            ViewBag.PageName = "Employee Setup";
            return View("Unauthorized");
        }

        /// <summary>
        /// _s the select employees.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="size">The size.</param>
        /// <param name="orderBy">The order by.</param>
        /// <returns>Object</returns>
        /// <ReViewBy>Avishek</ReViewBy>
        /// <ReViewDate>24-Feb-2016</ReViewDate>
        [GridAction]
        public ActionResult _SelectEmployees(int page, int size, string orderBy)
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                RedirectToAction("Login", "Account");
            }
            if (DLL.Utility.ApplicationSetting.Branch == true) {
                return View(new GridModel(GetAllEmployeeByBranch(oCode)));
            }
            return View(new GridModel(GetEmployees(oCode)));
        }

        [Authorize]
        public ActionResult _MailingEmployeeList()
        {
            ViewBag.PageName = "Employee Information";
            int OCode = ((int?)Session["OCode"]) ?? 0;
            if (OCode == 0)
            {
                RedirectToAction("Login", "Account");
            }
            bool b = PagePermission.HasPermission(User.Identity.Name, PageID, 0);
            if (b)
            {
                return View();
            }
            ViewBag.PageName = "Employee Setup";
            return View("Unauthorized");
        }
        /// <summary>
        /// For Email Sending Added By Kamrul
        /// </summary>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <param name="orderBy"></param>
        /// <returns>Email Address</returns>
        [GridAction]
        public ActionResult _MailingEmployees(int page, int size, string orderBy)
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                RedirectToAction("Login", "Account");
            }
            return View(new GridModel(GetEmployees(oCode).Where(a => a.Email != null).ToList()));
        }


        private IEnumerable<VM_Employee> GetEmployees(int OCode)
        {
            var result = unitOfWork.CustomRepository.GetAllEmployee(OCode).OrderBy(c => c.EmpID).ToList();
            return result;
        }
        private IEnumerable<VM_Employee> GetAllEmployeeByBranch(int OCode)
        {
            var result = unitOfWork.CustomRepository.GetAllEmployeeByBranch(OCode).OrderBy(c => c.EmpID).ToList();
            return result;
        }

        public JsonResult AutocompleteByLedgerOptions(string term)
        {

            int OCode = ((int?)Session["OCode"]) ?? 0;

            var suggestions = unitOfWork.ACC_LedgerRepository.Get().Where(w => w.LedgerName.ToLower().Trim().Contains(term.ToLower().Trim())).Select(s => new
            {
                value = s.LedgerID,
                label = s.LedgerName
            }).ToList();
            return Json(suggestions, JsonRequestBehavior.AllowGet);
        }

        //for single employee
        /// <summary>
        /// Emps the identifier validation.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>Json</returns>
        /// <ReViewBy>Avishek</ReViewBy>
        /// <ReViewDate>24-Feb-2016</ReViewDate>
        public ActionResult EmpIdValidation(string id)
        {
            //we are searching identification number not empID
            bool isEmployeeExist = unitOfWork.EmployeesRepository.IsExist(e => e.IdentificationNumber == id);
            if (isEmployeeExist)
            {
                return Json(new { Success = true, Message = "Employee found!" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { Success = false, ErrorMessage = "No employee exist with this ID!" }, JsonRequestBehavior.AllowGet);
        }

        //from list of employee for quick
        public bool IsEmpIDExist(string identificationNumber)
        {
            if (_listEmpIdofExistingEmp == null)
            {
                _listEmpIdofExistingEmp = unitOfWork.EmployeesRepository.Get().Select(s => s.IdentificationNumber).ToList();
            }
            //again check
            if (_listEmpIdofExistingEmp.Count < 1)
            {
                _listEmpIdofExistingEmp = unitOfWork.EmployeesRepository.Get().Select(s => s.IdentificationNumber).ToList();
            }

            if (_listEmpIdofExistingEmp != null)
            {
                return _listEmpIdofExistingEmp.Contains(identificationNumber);
            }
            return false;
        }

        /// <summary>
        /// Gets the employee by identifier.
        /// </summary>
        /// <param name="empID">The emp identifier.</param>
        /// <returns>list</returns>
        /// <CreatedBy>Avishek</CreatedBy>
        /// <CreatedDate>Dec-13-2015</CreatedDate>
        public ActionResult GetEmployeeByID(int empID)
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                RedirectToAction("Login", "Account");
            }
            CultureInfo cInfo = new CultureInfo("en-IN");
            var v = unitOfWork.CustomRepository.GetEmployeeById(empID).FirstOrDefault();
            ViewBag.EmpID = empID;
            ViewBag.EditUserName = v.EditUserName + "-" + v.EditDate + "-" + v.Comment;
            tbl_Employees tblEmployees = unitOfWork.CustomRepository.GetEmployeeById(v.EmpID).FirstOrDefault();
            List<tbl_Contribution> tblContribution = unitOfWork.CustomRepository.GetCurrentContributions(v.EmpID).ToList();
            VM_Employee vmEmployee = new VM_Employee();
            if (tblEmployees != null)
            {
                vmEmployee.EmpName = tblEmployees.EmpName;
                vmEmployee.opDesignationName = tblEmployees.opDesignationName;
                vmEmployee.opDepartmentName = tblEmployees.opDepartmentName;
                vmEmployee.ContactNumber = tblEmployees.ContactNumber;
                vmEmployee.Email = tblEmployees.Email;
                vmEmployee.JoiningDate = tblEmployees.JoiningDate;
                vmEmployee.PFActivationDate = tblEmployees.PFActivationDate;
                vmEmployee.PresentAddress = tblEmployees.PresentAddress;
                vmEmployee.opOwnContribution = tblEmployees.opOwnContribution.GetValueOrDefault();
                vmEmployee.opEmpContribution = tblEmployees.opEmpContribution.GetValueOrDefault();
                vmEmployee.opLoan = tblEmployees.opLoan.GetValueOrDefault();
                vmEmployee.opLoan = vmEmployee.opLoan == 0 ? 0 : _mvcApplication.GetNumber(vmEmployee.opLoan);
                vmEmployee.opProfit = tblEmployees.opProfit.GetValueOrDefault();
                vmEmployee.PFStatusID = Convert.ToInt32(tblEmployees.PFStatus);
            }
            ViewBag.CurrentOwnCont = (tblContribution.Sum(x => x.EmpContribution)).ToString("N", cInfo);
            ViewBag.CurrentEmpCont = (tblContribution.Sum(x => x.EmpContribution)).ToString("N", cInfo);
            return PartialView("_EmployeeDetail", vmEmployee);
        }

        public ActionResult EmployeeForm(int id = 0)
        {


            ViewBag.BranchID = new SelectList(db.LU_tbl_Branch, "BranchID", "BranchName");
            //ViewBag.branch_id = new SelectList(db.tblBranches, "branch_id", "branch_name");
            VM_Employee v;
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                RedirectToAction("Login", "Account");
            }
            if (id == 0)
            {
                v = new VM_Employee();
                return PartialView("_EmployeeForm", v);
            }
            tbl_Employees t = unitOfWork.EmployeesRepository.GetByID(id);
            v = _dpEmployee.VM_Employee(t);
            return PartialView("_EmployeeForm", v);
        }

        public void BranchOptions(string s = "")
        {
            ViewData["BranchOptions"] = new SelectList(unitOfWork.BranchRepository.Get(), "BranchID", "BranchName", s);
        }

        public void DesignationOptions(string s = "")
        {
            ViewData["DesignationOptions"] = new SelectList(unitOfWork.DesignationRepository.Get(), "DesignationID", "DesignationName", s);
        }

        public void DepartmentOptions(string s = "")
        {
            ViewData["DepartmentOptions"] = new SelectList(unitOfWork.DepartmentRepository.Get(), "DepartmentID", "DepartmentName", s);
        }

        [HttpPost]
        public ActionResult EmployeeForm(VM_Employee v)
        {
            ViewBag.BranchID = new SelectList(db.LU_tbl_Branch, "BranchID", "BranchName");
            int oCode = ((int?)Session["OCode"]) ?? 1;
            if (oCode == 0)
            {
                RedirectToAction("Login", "Account");//Added by Avishek Date:Dec-20-2015
                //return Json(new { Success = false, ErrorMessage = "You must be under a compnay!" }, JsonRequestBehavior.DenyGet);
            }

            bool isAllowedToEdit = PagePermission.HasPermission(User.Identity.Name, PageID, 1);
            if (!isAllowedToEdit) return Json(new { Success = false, ErrorMessage = "You are not allowed to edit information! contact system admin!" }, JsonRequestBehavior.AllowGet);

            if (ModelState.IsValid)
            {
                string message = "";
                bool b;
                b = v.EmpID > 0 ? unitOfWork.EmployeesRepository.IsExist(filter: c => (c.NID == v.NID || c.Email == v.Email || c.IdentificationNumber == v.IdentificationNumber) && c.EmpID != v.EmpID) : unitOfWork.EmployeesRepository.IsExist(filter: c => c.NID == v.NID || c.Email == v.Email || c.IdentificationNumber == v.IdentificationNumber);
                if (!b)
                {
                    tbl_Employees s;
                    if (v.EmpID == 0  )
                    {
                        s = _dpEmployee.tbl_Employees(v);
                        s.PFStatus = 1; //New member, so inactive. later logic change and all employee are pf member by default
                        s.EmpID = GetMaxID() + 1;
                        s.EditDate = DateTime.Now;
                        s.EditUser = unitOfWork.CustomRepository.GetUserID(User.Identity.Name);
                        s.EditUserName = User.Identity.Name;
                        if (DLL.Utility.ApplicationSetting.Branch == true)
                        {
                            s.Branch = v.BranchID;
                        }
                        else {
                            s.Branch = 1;
                        }
                        s.OCode = oCode;
                        s.PassVoucher = true;//added by sohana
                        unitOfWork.EmployeesRepository.Insert(s);
                        message = "New employee inserted!";
                    }
                    else
                    {
                        //added by sohana
                        s = new tbl_Employees();

                        s = unitOfWork.EmployeesRepository.Get().First(x => x.EmpID == v.EmpID);
                        // s = _dpEmployee.tbl_Employees(v);
                        //End by sohana
                        s.OCode = oCode;
                        s.EditDate = DateTime.Now;
                        s.EditUser = unitOfWork.CustomRepository.GetUserID(User.Identity.Name);
                        s.EditUserName = User.Identity.Name;
                        s.PassVoucher = false;  //Modified by Avishek Date: 28-May-2016
                        //added by sohana
                        //s.PassVoucher = true;

                        s.BirthDate = v.BirthDate;
                        if (DLL.Utility.ApplicationSetting.Branch == true)
                        {
                            s.Branch = v.BranchID;
                        }
                        else
                        {
                            s.Branch = 1;
                        }
                        s.ContactNumber = v.ContactNumber;
                        s.Department = v.DepartmentID;
                        s.Designation = v.DesignationID;
                        s.Email = v.Email;
                        s.EmpID = v.EmpID;
                        s.EmpImg = v.EmpImg;
                        s.EmpName = v.EmpName;
                        s.Gender = v.Gender;
                        s.JoiningDate = v.JoiningDate;
                        s.NID = v.NID;
                        s.Comment = v.Comment;
                        s.LocationName = v.LocationName;
                        s.PFDeactivationDate = v.PFDeactivationDate;
                        s.PFDeactivatedByName = v.PFDeactivatedByName;
                        s.PassVoucherMessage = v.PassVoucherMessage;
                        s.PassVoucherEntryID = v.PassVoucherEntryID;
                        s.PFDeactivationVoucherID = v.PFDeactivationVoucherID;
                        s.PFRuleUsedForDeactivation = v.PFRuleUsedForDeactivation;


                        s.PFStatus = v.PFStatusID;
                        s.PresentAddress = v.PresentAddress;

                        s.SignatureImg = v.SignatureImg;
                        s.IdentificationNumber = v.IdentificationNumber;

                        s.opOwnContribution = v.opOwnContribution;
                        s.opEmpContribution = v.opEmpContribution;
                        s.opProfit = v.opProfit;
                        s.opLoan = v.opLoan;
                        s.opDepartmentName = v.opDepartmentName;
                        s.opDesignationName = v.opDesignationName;
                        s.PFActivationDate = v.PFActivationDate ?? DateTime.Now;
                        //end by sohana
                        unitOfWork.EmployeesRepository.Update(s);
                        RemoveVoucher(oCode, s);
                        message = "Employee information updated by " + User.Identity.Name;
                    }
                    try
                    {
                        unitOfWork.Save();
                        return Json(new { Success = true, Message = message }, JsonRequestBehavior.AllowGet);

                    }
                    catch (DataException x)
                    {
                        return Json(new { Success = false, ErrorMessage = x.Message + " \n=> " + x.InnerException }, JsonRequestBehavior.AllowGet);
                    }
                }
                ModelState.AddModelError("", "This NationalID/Email/ID already exist for another user!");
            }
            var errorList = (from item in ModelState.Values
                             from error in item.Errors
                             select error.ErrorMessage).ToList();

            return Json(JsonResponseFactory.ErrorResponse(errorList), JsonRequestBehavior.DenyGet);
        }

        private void RemoveVoucher(int OCode, tbl_Employees s)
        {
            acc_VoucherEntry accVoucherEntry = unitOfWork.ACC_VoucherEntryRepository.Get(x => x.EmpID == s.EmpID && x.ActionName == "Member add" && x.OCode == OCode).FirstOrDefault();
            if (accVoucherEntry != null)
            {
                string empId = s.EmpID.ToString();
                List<acc_VoucherDetail> accVoucherDetail = unitOfWork.ACC_VoucherDetailRepository.Get(x => x.PFMemberID == Convert.ToInt32(empId) && x.VoucherID == accVoucherEntry.VoucherID && x.OCode == OCode).ToList();
                foreach (acc_VoucherDetail item in accVoucherDetail)
                {
                    unitOfWork.ACC_VoucherDetailRepository.Delete(item);
                    unitOfWork.Save();
                }
                unitOfWork.ACC_VoucherEntryRepository.Delete(accVoucherEntry);
                unitOfWork.Save();
            }
        }

        public int GetMaxID()
        {
            int data = unitOfWork.EmployeesRepository.Get().Select(s => s.EmpID).DefaultIfEmpty().Max();
            return data;
        }

        public ActionResult DeletePossible(int id)
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                RedirectToAction("Login", "Account");//Added by Avishek Date:Dec-20-2015
            }
            bool b = PagePermission.HasPermission(User.Identity.Name, PageID, 2);
            if (!b)
            {
                return Json(new { Success = false, ErrorMessage = "You are not authorized to delete information!" }, JsonRequestBehavior.AllowGet);
            }
            bool a = unitOfWork.ContributionRepository.IsExist(i => i.EmpID == id);
            if (!a)
            {
                bool hasLoan = unitOfWork.PFLoanRepository.IsExist(w => w.EmpID == id);
                if (!hasLoan)
                {
                    return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { Success = false, ErrorMessage = "This employee has loan from PF. So, cannot be deleted..." }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { Success = false, ErrorMessage = "This employee is a PF member and has contribution to fund. So, cannot be deleted..." }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DeleteConfirm(int id)
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                RedirectToAction("Login", "Account");//Added by Avishek Date:Dec-20-2015
            }
            bool b = PagePermission.HasPermission(User.Identity.Name, PageID, 2);
            if (!b)
            {
                return Json(new { Success = false, ErrorMessage = "You are not authorized to delete information!" }, JsonRequestBehavior.AllowGet);
            }
            int empId = id;
            bool a = unitOfWork.ContributionRepository.IsExist(i => i.EmpID == empId);
            if (!a)
            {
                //var objSalary = unitOfWork.BasicSalaryRepository.Get().Where(e => e.EmpID == EmpID).ToList();
                var objEmp = unitOfWork.EmployeesRepository.Get(e => e.EmpID == empId).SingleOrDefault();
                try
                {
                    unitOfWork.EmployeesRepository.Delete(objEmp);

                    //Now delete from User Table
                    var user = unitOfWork.UserProfileRepository.Get(w => w.EmpID == empId).SingleOrDefault();
                    if (user != null)
                    {
                        unitOfWork.UserProfileRepository.Delete(user);
                        var userPassword = unitOfWork.UserPasswordRepository.Get(w => w.UserID == user.UserID).SingleOrDefault();
                        if (userPassword != null)
                        {
                            unitOfWork.UserPasswordRepository.Delete(userPassword);
                        }
                    }
                    try
                    {
                        unitOfWork.Save();
                        TempData["Message"] = "All related data successfully deleted!";
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    //delete photo
                    var destinationPathSign = Path.Combine(Server.MapPath("~/Picture/Signature/"), empId + ".jpg");
                    var destinationPathPhoto = Path.Combine(Server.MapPath("~/Picture/Photos/"), empId + ".jpg");
                    try
                    {
                        System.IO.File.Delete(destinationPathSign);
                        System.IO.File.Delete(destinationPathPhoto);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    return Json(new { Success = true, Message = TempData["Message"] as string }, JsonRequestBehavior.DenyGet);
                }
                catch (Exception x)
                {
                    return Json(new { Success = false, ErrorMessage = "Problem While deleting record., \nDetails:" + x.Message }, JsonRequestBehavior.DenyGet);
                }
            }
            return Json(new { Success = false, ErrorMessage = "This employee is a PF member and has contribution to fund. So, cannot be deleted..." }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveImage(HttpPostedFileBase EmpImgFile, int employeeID)
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                RedirectToAction("Login", "Account");//Added by Avishek Date:Dec-20-2015
            }
            bool isAllowedToEdit = PagePermission.HasPermission(User.Identity.Name, PageID, 1);
            if (!isAllowedToEdit) return Json(new { Success = false, ErrorMessage = "You are not allowed to edit information! contact system admin!" }, JsonRequestBehavior.AllowGet);

            // Some browsers send file names with full path. We only care about the file name.
            string fileName = employeeID + Path.GetExtension(EmpImgFile.FileName);
            var destinationPath = Path.Combine(Server.MapPath("~/Picture/Photos/"), fileName);

            // TODO: Store description...
            try
            {
                System.IO.File.Delete(destinationPath);
            }
            catch (Exception x)
            {
                return Json(new { Success = false, ErrorMessage = "Previous file unable to delete!\n" + x.Message }, JsonRequestBehavior.DenyGet);
            }
            try
            {
                var v = unitOfWork.EmployeesRepository.GetByID(employeeID);
                v.EmpImg = employeeID + "";
                unitOfWork.Save();
                EmpImgFile.SaveAs(destinationPath);
                return Json(new { Success = true, Message = "Image successfully altered!" }, JsonRequestBehavior.DenyGet);
            }
            catch (Exception x)
            {
                return Json(new { Success = false, ErrorMessage = "Failed to save file! " + x.Message }, JsonRequestBehavior.DenyGet);
            }
        }

        public ActionResult SaveSignature(HttpPostedFileBase EmpSignFile, int employeeID)
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                RedirectToAction("Login", "Account");
            }
            bool isAllowedToEdit = PagePermission.HasPermission(User.Identity.Name, PageID, 1);
            if (!isAllowedToEdit) return Json(new { Success = false, ErrorMessage = "You are not allowed to edit information! contact system admin!" }, JsonRequestBehavior.AllowGet);

            // Some browsers send file names with full path. We only care about the file name.
            string tempfileName = Path.GetFileNameWithoutExtension(EmpSignFile.FileName);
            string fileName = employeeID + Path.GetExtension(EmpSignFile.FileName);
            var destinationPath = Path.Combine(Server.MapPath("~/Picture/Signature/"), fileName);

            // TODO: Store description...
            try
            {
                System.IO.File.Delete(destinationPath);
            }
            catch (Exception x)
            {
                return Json(new { Success = false, ErrorMessage = "Previous file unable to delete!\n" + x.Message }, JsonRequestBehavior.DenyGet);
            }
            try
            {
                var v = unitOfWork.EmployeesRepository.GetByID(employeeID);
                v.EmpImg = employeeID + "";
                unitOfWork.Save();
                EmpSignFile.SaveAs(destinationPath);
                return Json(new { Success = true, Message = "Image successfully altered!" }, JsonRequestBehavior.DenyGet);
            }
            catch (Exception x)
            {
                return Json(new { Success = false, ErrorMessage = "Failed to save file! " + x.Message }, JsonRequestBehavior.DenyGet);
            }
        }

        [Authorize]
        public ActionResult Import()
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                RedirectToAction("Login", "Account");
            }
            ViewBag.Message = TempData["message"] as string;
            return View("Import");
        }

        public ActionResult ImportExcel() //HttpPostedFileBase EmpExcelFile
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                RedirectToAction("Login", "Account");//Added by Avishek Date:Dec-20-2015
            }

            if (oCode == 0)
            {
                return Json(new { Success = false, ErrorMessage = "You must be under a compnay to execute this process!" }, JsonRequestBehavior.DenyGet);
            }
            new CultureInfo("en-IN");

            if (Request.Files.Count == 0)
            {
                TempData["message"] = "Please upload valid excel file...";
                return RedirectToAction("Import");
            }

            if (Request.Files["FileUpload1"].ContentLength > 0)
            {
                string path1 = "";
                try
                {
                    string extension = Path.GetExtension(Request.Files["FileUpload1"].FileName);
                    path1 = string.Format("{0}/{1}", Server.MapPath("~/ImportedExcel/Employee"), extension);
                    if (System.IO.File.Exists(path1))
                        System.IO.File.Delete(path1);
                    Request.Files["FileUpload1"].SaveAs(path1);
                    //EmpExcelFile.SaveAs(path1);


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
                    
                    cmd.CommandText = "Select [Employee ID],[Employee Name],[Branch],[Email], [Joining Date], [PF Activation Date], [Designation], [Department] , [OWN CONT], [EMP CONT], [Profit], [Loan] from [Sheet1$]"; //[Basic Salary],  [Effective Date], <- will be read from salary sheet. from excel we will not process salary.
                    
                    oleda = new OleDbDataAdapter(cmd);
                    oleda.Fill(_dataset, "EmployeeImport");
                }
                catch (Exception x)
                {
                    TempData["message"] = x.Message;
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
                    DataTable dt = _dataset.Tables["EmployeeImport"];
                    //let's check if this datatable contains duplicate employee id
                    Hashtable ht = new Hashtable();
                    foreach (DataRow dr in dt.Rows)
                    {
                        if (dr["Employee ID"] != null)
                        {
                            string s = dr["Employee ID"] + "";
                        }
                    }
                    //
                    var query = dt.AsEnumerable().Select(s => new
                    {
                        EmployeeID = s.Field<string>("Employee ID"),
                        EmployeeName = s.Field<string>("Employee Name"),
                        
                        Branch = s.Field<string>("Branch"),
                        JoiningDate = s.Field<object>("Joining Date"),
                        Email = s.Field<string>("Email"),

                        OwnCont = s.Field<object>("OWN CONT"),
                        EmpCont = s.Field<object>("EMP CONT"),
                        Profit = s.Field<object>("Profit"),
                        Loan = s.Field<object>("Loan"),
                        opDesignationName = s.Field<object>("Designation"),
                        opDepartment = s.Field<object>("Department"),
                        PFActivationDate = s.Field<object>("PF Activation Date"),
                    });

                    var branch_detail = unitOfWork.BranchRepository.Get().Select(x => x.BranchLocation).ToList();
                      
                    //string branch = branch_detail;
                    bool flag = true;
                    List<VM_Employee> lstVmEmployee = new List<VM_Employee>();
                    foreach (var item in query)
                    {
                        if (item.EmployeeID != null)
                        {
                            
                            VM_Employee vmEmployee = new VM_Employee();
                            // this  employee id is actually identification number
                            vmEmployee.IdentificationNumber = item.EmployeeID.Trim();
                            if (string.IsNullOrEmpty(vmEmployee.IdentificationNumber))
                            {
                                continue;
                            }
                            // Added for Branch Integration Added By Kamrul 2019-01-13
                            //int b_id = Convert.ToInt32(item.Branch);
                            if (DLL.Utility.ApplicationSetting.Branch == true)
                            {
                                if (vmEmployee.IdentificationNumber != null)
                                {

                                    string b_name = (item.Branch).Trim();

                                    var B_Name = unitOfWork.BranchRepository.Get().Where(x => x.BranchName == b_name).FirstOrDefault();
                                    
                                    //vmEmployee.BranchID = Convert.ToInt32(item.Branch);
                                    vmEmployee.BranchName = B_Name.BranchName ?? "";
                                    vmEmployee.BranchID = B_Name.BranchID;

                                }
                            }
                            else {
                                vmEmployee.BranchName =  "";
                                vmEmployee.BranchID = 1;
                            }
                            //End Kamrul
                            if (vmEmployee.BranchID == 0)
                            {
                                vmEmployee.PreImportMessage += "Branch is not inserted";
                                flag = false;
                                //continue;
                            }
                            if (IsEmpIDExist(item.EmployeeID))
                            {
                                vmEmployee.PreImportMessage += "This EmployeeID/Identification Number already exist! If you continue all previous record will be removed permanently!";

                            }
                            vmEmployee.EmpName = item.EmployeeName;
                            
                            try
                            {
                                if (item.JoiningDate != null)
                                {
                                    vmEmployee.JoiningDate = Convert.ToDateTime(item.JoiningDate);
                                }
                            }
                            catch (Exception x)
                            {
                                vmEmployee.JoiningDate = null;
                                //vm_employee.PreImportMessage += "joining date"+item.JoiningDate+" not in correct format! "+x.Message;
                                //flag = false;
                            }
                            //Added by Kamrul 2019-01-06
                            //if (DLL.Utility.ApplicationSetting.Branch == true)
                            //{
                            //    try
                            //    {
                            //        vmEmployee.IdentificationNumber = item.EmployeeID.Trim();
                            //        foreach (var bname in branch_detail)
                            //        {
                            //            item.EmployeeID.Contains(bname);
                            //            //if (!vmEmployee.IdentificationNumber.Contains(bname))
                            //            //{
                            //            //    vmEmployee.PreImportMessage += "Employee ID " + item.EmployeeID + " not in correct format";
                            //            //    flag = false;
                            //            //}
                            //        }

                            //    }
                            //    catch
                            //    {
                            //        vmEmployee.PreImportMessage += "Employee ID " + item.EmployeeID + " not in correct format";
                            //        flag = false;
                            //    }
                            //}
                            //End Kamrul
                            try
                            {
                                vmEmployee.opOwnContribution = Convert.ToDecimal(item.OwnCont);
                            }
                            catch
                            {
                                vmEmployee.PreImportMessage += "Own contribution " + item.OwnCont + " not in correct format";
                                flag = false;
                            }
                            try
                            {
                                vmEmployee.opEmpContribution = Convert.ToDecimal(item.EmpCont);
                            }
                            catch
                            {
                                vmEmployee.PreImportMessage += "Employer contribution " + item.EmpCont + " not in correct format";
                                flag = false;
                            }
                            try
                            {
                                vmEmployee.opLoan = Convert.ToDecimal(item.Loan);
                            }
                            catch
                            {
                                vmEmployee.PreImportMessage += "Loan " + item.Loan + " not in correct format";
                                flag = false;
                            }
                            try
                            {
                                vmEmployee.opProfit = Convert.ToDecimal(item.Profit);
                            }
                            catch
                            {
                                vmEmployee.PreImportMessage += "Profit " + item.Profit + " not in correct format";
                                flag = false;
                            }
                            vmEmployee.opDesignationName = item.opDesignationName + "";
                            vmEmployee.opDepartmentName = item.opDepartment + "";
                            try
                            {
                                _pfActivationDate = Convert.ToDateTime(item.PFActivationDate);
                                vmEmployee.PFActivationDate = _pfActivationDate;
                            }
                            catch (Exception x)
                            {
                                vmEmployee.PreImportMessage += "PF Activation date " + item.PFActivationDate + " not in correct format!";
                                flag = false;
                            }
                            vmEmployee.OCode = 1;
                            vmEmployee.Email = item.Email;
                            lstVmEmployee.Add(vmEmployee);
                        }
                    }
                    if (flag)
                    {
                        ViewBag.Message = "Files contain no error and can uploaded safely...";
                        return View("_EmployeeImportNoError", lstVmEmployee);
                    }
                    ViewBag.Message = "You must solve the following error to save these data...";
                    return View("_EmployeeImportErrorFound", lstVmEmployee);
                }
                catch (Exception x)
                {
                    TempData["message"] = x.Message;
                    return RedirectToAction("Import");
                }
            }
            TempData["message"] = "No file uploaded...";
            return RedirectToAction("Import");
        }

        /// <summary>
        /// Imports the excel save.
        /// </summary>
        /// <param name="v">The v.</param>
        /// <returns></returns>
        /// <ModifiedBy>Avishek</ModifiedBy>
        /// <ModificationDate>Apr-20-2015</ModificationDate>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ImportExcelSave(List<VM_Employee> v)
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                RedirectToAction("Login", "Account");//Added by Avishek Date:Dec-20-2015
                //return Json(new { Success = false, ErrorMessage = "You must be under a compnay to execute this process!" }, JsonRequestBehavior.DenyGet);
            }
            int empId = GetMaxID() + 1;
            var editUser = unitOfWork.CustomRepository.GetUserID(User.Identity.Name);
            //Added by Avishek Date:Feb-18-2015
            //Start
            List<tbl_Employees> tblEmployees = unitOfWork.EmployeesRepository.Get().Where(x => x.OCode == oCode).ToList();
            //List<LU_tbl_Branch> branch_detail= unitOfWork.BranchRepository.Get().Where(b=>b.BranchID==2).ToList();
            //var branch_detail = unitOfWork.BranchRepository.Get().Select(x => x.BranchName).ToList();
            //End
            foreach (var item in v)
            {
                tbl_Employees tblEmployee = new tbl_Employees();
                bool hasId = tblEmployees.FirstOrDefault(x => x.IdentificationNumber.Trim() == item.IdentificationNumber.Trim()) == null ? true : false;
                if (hasId)
                {
                    tblEmployee.EmpID = empId;
                    tblEmployee.EmpName = item.EmpName;
                    tblEmployee.Branch = item.BranchID;
                    if (tblEmployee.Branch == 0 || tblEmployee.Branch == null)
                    {
                        tblEmployee.Branch = 1;
                    }
                    
                    tblEmployee.opDesignationName = item.opDesignationName;
                    tblEmployee.opDepartmentName = item.opDepartmentName;
                    //excel employee id will be treated as Identification Number
                    tblEmployee.IdentificationNumber = item.IdentificationNumber.Trim();
                    tblEmployee.JoiningDate = item.JoiningDate ?? System.DateTime.Now;
                    tblEmployee.Email = item.Email;
                    tblEmployee.NID = item.NID;
                    tblEmployee.PFStatus = 1; //initially inactive // logic change initially active
                    tblEmployee.opOwnContribution = item.opOwnContribution;
                    tblEmployee.opEmpContribution = item.opEmpContribution;
                    tblEmployee.opLoan = item.opLoan;
                    tblEmployee.opProfit = item.opProfit;
                    tblEmployee.EditDate = DateTime.Now;
                    tblEmployee.EditUser = editUser;
                    tblEmployee.EditUserName = User.Identity.Name;
                    tblEmployee.PFActivationDate = item.PFActivationDate ?? DateTime.Now;
                    tblEmployee.PFStatus = 1;
                    tblEmployee.Comment = "[Excel Upload]";
                    tblEmployee.PassVoucher = false;
                    tblEmployee.OCode = oCode;
                    unitOfWork.EmployeesRepository.Insert(tblEmployee);
                    if (tblEmployee.opLoan != null && tblEmployee.opLoan != 0)
                    {
                        SaveLoan(tblEmployee);
                    }
                    //now prepare the next id
                    empId = empId + 1;
                }
            }
            try
            {
                unitOfWork.Save();
                return Json(new { Success = true, Message = "Inserted Successfully!" }, JsonRequestBehavior.AllowGet);
            }
            //catch (DbEntityValidationException e)
            //{
            //    foreach (var eve in e.EntityValidationErrors)
            //    {
            //        Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
            //            eve.Entry.Entity.GetType().Name, eve.Entry.State);
            //        foreach (var ve in eve.ValidationErrors)
            //        {
            //            Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
            //                ve.PropertyName, ve.ErrorMessage);
            //        }
            //    }
            //    throw;
            //}
            catch (Exception x)
            {
                ErrorGenerate(x);
                return Json(new { Success = false, ErrorMessage = "Error: " + x.Message, InnerException = "Detail Error: " + x.InnerException }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Saves the loan.
        /// </summary>
        /// <param name="tbl_employee">The tbl_employee.</param>
        /// <CreattedBy>Avishek</ModifiedBy>
        /// <CreattedDate>Apr-20-2015</ModificationDate>
        private void SaveLoan(tbl_Employees tbl_employee)
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                RedirectToAction("Login", "Account");
            }
            string loanid = "";
            var v = unitOfWork.PFLoanRepository.Get().Select(s => new { s.PFLoanID }).OrderBy(o => o.PFLoanID).LastOrDefault();
            if (v != null)
            {
                int i = Convert.ToInt32(v.PFLoanID);
                i = i + 1;
                loanid = i.ToString().PadLeft(6, '0');
            }
            else
            {
                loanid = "000001";
            }
            DateTime _PFActivationDate = tbl_employee.PFActivationDate;
            DateTime _PFJoiningDate = tbl_employee.JoiningDate ?? DateTime.MinValue;

            DateTime _PFDeActivationDate = tbl_employee.PFDeactivationDate ?? DateTime.MinValue;
            var pfDuration = ((DateTime.Now.Year - _PFActivationDate.Year) * 12) + (DateTime.Now.Month - _PFActivationDate.Month);
            
            //var pfDuration = ((DateTime.Now.Year - tbl_employee.PFActivationDate.Year) * 12) + (DateTime.Now.Month - tbl_employee.PFActivationDate.Month);
            //Changed by Kamrul 2018-12-23
            if (ApplicationSetting.JoiningDate == true)
            {
                pfDuration = ((DateTime.Now.Year - _PFJoiningDate.Year) * 12) + (DateTime.Now.Month - _PFJoiningDate.Month);
            }
            

            //End Kamrul
            var loanRule = unitOfWork.LoanRulesRepository.Get().ToList();

            decimal ruleId = 0;
            foreach (var item in loanRule.OrderBy(x => x.WorkingDurationInMonth).Where(item => pfDuration >= item.WorkingDurationInMonth))
            {
                ruleId = item.ROWID;
            }
            tbl_PFLoan tbl_pfLoan = new tbl_PFLoan();
            tbl_pfLoan.EmpID = tbl_employee.EmpID;
            tbl_pfLoan.PFLoanID = loanid;
            tbl_pfLoan.LoanAmount = (decimal)tbl_employee.opLoan;
            tbl_pfLoan.TermMonth = (int)loanRule.Where(x => x.ROWID == ruleId).Select(x => x.InstallmentNoumber).FirstOrDefault();
            tbl_pfLoan.Interest = (int)loanRule.Where(x => x.ROWID == ruleId).Select(x => x.IntarestRate).FirstOrDefault();
            tbl_pfLoan.StartDate = DateTime.Now;
            tbl_pfLoan.EditUser = unitOfWork.CustomRepository.GetUserID(User.Identity.Name);
            tbl_pfLoan.EditDate = System.DateTime.Now;
            tbl_pfLoan.IsApproved = 1;
            tbl_pfLoan.ApprovedBy = unitOfWork.CustomRepository.GetUserID(User.Identity.Name);
            tbl_pfLoan.EditUserName = User.Identity.Name;
            tbl_pfLoan.RuleUsed = (int)ruleId;
            tbl_pfLoan.OCode = oCode;
            unitOfWork.PFLoanRepository.Insert(tbl_pfLoan);

            int voucherId = 0;
            List<Guid> ledgerIdList = new List<Guid>();
            List<decimal> debit = new List<decimal>();

            Guid ledgerId = unitOfWork.ChartofAccountMapingRepository.Get(x => x.id == 5).Select(x => x.Ledger_Id).FirstOrDefault();
            ledgerIdList.Add(ledgerId);
            debit.Add(tbl_pfLoan.LoanAmount);

            //Modified By Avishek Date: Dec-20-2015
            //bool isOperationSuccess = unitOfWork.AccountingRepository.SingleEntryVoucher(tbl_pfLoan.EmpID, 5, DateTime.Now, ref voucherID, "Loan approve ", LedgerNameList, Debit, User.Identity.Name, unitOfWork.CustomRepository.GetUserID(User.Identity.Name), tbl_pfLoan.EmpID.ToString(), "Loan approved", "", "", null, tbl_pfLoan.PFLoanID, OCode, "Approve Loan");
            unitOfWork.AccountingRepository.SingleEntryVoucherById(tbl_pfLoan.EmpID, 5, DateTime.Now, ref voucherId, "Loan approve ", ledgerIdList, debit, User.Identity.Name, unitOfWork.CustomRepository.GetUserID(User.Identity.Name), tbl_pfLoan.EmpID.ToString(), "Loan approved", "", "", null, tbl_pfLoan.PFLoanID, oCode, "Approve Loan");
            //End
            GenerateAmortization(tbl_pfLoan, tbl_pfLoan.EmpID, tbl_pfLoan.PFLoanID, tbl_pfLoan.StartDate);
        }

        /// <summary>
        /// Generates the amortization.
        /// </summary>
        /// <param name="tbl_pfLoan">The TBL_PF loan.</param>
        /// <param name="empID">The emp identifier.</param>
        /// <param name="pfLoanID">The pf loan identifier.</param>
        /// <param name="startDate">The start date.</param>
        /// <CreattedBy>Avishek</ModifiedBy>
        /// <CreattedDate>Apr-20-2015</ModificationDate>
        private void GenerateAmortization(tbl_PFLoan tbl_pfLoan, int empID, string pfLoanID, DateTime startDate)
        {
            int i = 1;
            tbl_PFL_Amortization tblPflAmortization;
            List<tbl_PFL_Amortization> lstAmortization = new List<tbl_PFL_Amortization>();
            int oCode = ((int?)Session["OCode"]) ?? 0;
            double intRate = Convert.ToDouble(tbl_pfLoan.Interest / 100);
            double loanTenor = Convert.ToDouble(tbl_pfLoan.TermMonth);
            double loanAmount = Convert.ToDouble(tbl_pfLoan.LoanAmount);
            double pHlemi;
            double tmpgp;
            double tmpk;
            tmpk = 1 / (1 + intRate * 1 / 12);
            tmpgp = (Math.Pow(tmpk, loanTenor) - 1) / (tmpk - 1) * tmpk;
            pHlemi = loanAmount / tmpgp / 1;
            double pHlemiRound = Math.Round(pHlemi, 4);
            if (loanAmount > 0 && loanTenor > 0)
            {
                decimal balance = Convert.ToDecimal(loanAmount);
                startDate = startDate.AddMonths(-1);
                while (balance > 0)
                {
                    tblPflAmortization = new tbl_PFL_Amortization();
                    tblPflAmortization.InstallmentNumber = i;
                    tblPflAmortization.Amount = Math.Round(balance, 4);
                    tblPflAmortization.Interest = Math.Round(tblPflAmortization.Amount * Convert.ToDecimal(intRate) / 12, 4);
                    tblPflAmortization.Principal = Math.Round(Convert.ToDecimal(pHlemi) - tblPflAmortization.Interest, 4);
                    tblPflAmortization.Balance = Math.Round(tblPflAmortization.Amount - tblPflAmortization.Principal, 4);
                    tblPflAmortization.Processed = 0;
                    startDate = startDate.AddMonths(1);
                    string year = startDate.Year + "";
                    string month = startDate.Month.ToString().PadLeft(2, '0');
                    tblPflAmortization.ConMonth = month;
                    tblPflAmortization.ConYear = year;
                    if (tblPflAmortization.Amount < Convert.ToDecimal(pHlemiRound))
                    {
                        balance = 0;
                        tblPflAmortization.Balance = 0;
                    }
                    else
                    {
                        balance = tblPflAmortization.Balance;
                    }
                    lstAmortization.Add(tblPflAmortization);
                    i++;
                }
                Guid userId = unitOfWork.CustomRepository.GetUserID(User.Identity.Name);
                foreach (tbl_PFL_Amortization item in lstAmortization)
                {
                    item.EditUser = userId;
                    item.EditDate = System.DateTime.Now;
                    item.ReScheduleID = 1;
                    item.EmpID = empID;
                    item.PFLoanID = pfLoanID;
                    item.OCode = oCode;
                    unitOfWork.AmortizationRepository.Insert(item);
                }
                try
                {
                    unitOfWork.Save();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        [Authorize]
        public ActionResult PFMembers()
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                RedirectToAction("Login", "Account");
            }
            var v = unitOfWork.CustomRepository.GetAllEmployee(oCode).ToList();
            var totalEmployee = v.Count;
            var pfMembers = v.Where(x => x.PFStatusID == 1).ToList();
            var pfDeactivated = v.Where(x => x.PFStatusID == 2).ToList();
            var totalPfMembers = pfMembers.Count;
            ViewBag.Message = totalPfMembers + " person(s) are PF member from " + totalEmployee + " employee";
            ViewBag.Deactivated = pfDeactivated.Count + " member deactivated";
            return View("PFMembers", pfMembers);
        }

        #region Start of Nomineee part

        [Authorize]
        public ActionResult GetNomineeInformation(int empID)
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                RedirectToAction("Login", "Account");
            }
            var objEmp = unitOfWork.EmployeesRepository.Get(w => w.EmpID == empID).Single();

            @ViewBag.EmpID = objEmp.EmpID;
            @ViewBag.EmpName = objEmp.EmpName;

            return View("NomineeInformation");
        }

        [GridAction]
        public ActionResult _SelectNominee(int empID)
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                RedirectToAction("Login", "Account");
            }
            return View(new GridModel(GetNominee(empID)));
        }

        private IEnumerable<tbl_NomineeInformation> GetNominee(int empID)
        {
            var v = unitOfWork.NomineeRepository.Get().Where(w => w.EmpID == empID).ToList();
            return v;
        }

        public ActionResult NomineeForm(int empID = 0, int nomineeID = 0)
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                RedirectToAction("Login", "Account");//Added By Avishek Date:Dec-20-2015
            }
            tbl_NomineeInformation tbl_nomineeInformation;
            if (empID == 0)
            {
                return Json(new { Success = false, ErrorMessage = "Employee ID not found!" }, JsonRequestBehavior.AllowGet);
            }
            ViewBag.EmpID = empID;
            if (nomineeID < 1)
            {
                tbl_nomineeInformation = new tbl_NomineeInformation();
                tbl_nomineeInformation.EmpID = empID;
                return PartialView("_NomineeForm", tbl_nomineeInformation);
            }
            tbl_nomineeInformation = unitOfWork.NomineeRepository.Get(w => w.EmpID == empID && w.NomineeID == nomineeID).Single();
            return PartialView("_NomineeForm", tbl_nomineeInformation);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult NomineeForm(VM_NomineeInformation vm_nomineeInformation)
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                RedirectToAction("Login", "Account");
            }
            tbl_NomineeInformation tblNomineeInformation;
            if (ModelState.IsValid)
            {
                //maximum payable percentage to that nominee
                var v = unitOfWork.NomineeRepository.Get(w => w.EmpID == vm_nomineeInformation.EmpID).AsEnumerable().Sum(s => s.Nomineepercentage);

                if (vm_nomineeInformation.NomineeID > 0)
                {
                    var v1 = unitOfWork.NomineeRepository.Get(w => w.EmpID == vm_nomineeInformation.EmpID && w.NomineeID != vm_nomineeInformation.NomineeID).AsEnumerable().Sum(s => s.Nomineepercentage);
                    if (vm_nomineeInformation.Nomineepercentage > (100 - v1))
                    {
                        return Json(new { Success = false, ErrorMessage = "Max payable percentage should be " + (100 - v1) }, JsonRequestBehavior.AllowGet);
                    }
                }
                else if (vm_nomineeInformation.Nomineepercentage > (100 - v))
                {
                    return Json(new { Success = false, ErrorMessage = "Max payable percentage should be " + (100 - v) }, JsonRequestBehavior.AllowGet);
                }
                tblNomineeInformation = vm_nomineeInformation.NomineeID > 0 ? unitOfWork.NomineeRepository.Get(w => w.EmpID == vm_nomineeInformation.EmpID && w.NomineeID == vm_nomineeInformation.NomineeID).Single() : new tbl_NomineeInformation();
                tblNomineeInformation.EmpID = vm_nomineeInformation.EmpID;
                tblNomineeInformation.EditDate = System.DateTime.Now;
                tblNomineeInformation.EditUser = unitOfWork.CustomRepository.GetUserID(User.Identity.Name);
                tblNomineeInformation.NomineeAddress = vm_nomineeInformation.NomineeAddress;
                tblNomineeInformation.NomineeName = vm_nomineeInformation.NomineeName;
                tblNomineeInformation.NomineeNationalID = vm_nomineeInformation.NomineeNationalID;
                tblNomineeInformation.Nomineepercentage = vm_nomineeInformation.Nomineepercentage;
                tblNomineeInformation.RegistrationDate = vm_nomineeInformation.RegistrationDate;
                tblNomineeInformation.Relation = vm_nomineeInformation.Relation;

                if (vm_nomineeInformation.NomineeID == 0)
                {
                    tblNomineeInformation.NomineeID = GetNomineeMaxID() + 1;
                    var nomineeNidExist = unitOfWork.NomineeRepository.Get(n => n.NomineeNationalID == vm_nomineeInformation.NomineeNationalID && n.EmpID == vm_nomineeInformation.EmpID).SingleOrDefault();
                    if (nomineeNidExist != null)
                    {
                        return Json(new { Success = false, ErrorMessage = "This nominee national id previously exist for this employee" }, JsonRequestBehavior.AllowGet);
                    }
                    unitOfWork.NomineeRepository.Insert(tblNomineeInformation);
                }
                else
                {
                    tblNomineeInformation.NomineeID = vm_nomineeInformation.NomineeID;
                    unitOfWork.NomineeRepository.Update(tblNomineeInformation);
                }
                try
                {
                    unitOfWork.Save();
                    return Json(new { Success = true, Message = "Successfully Saved..." }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception x)
                {
                    return Json(new { Success = false, ErrorMessage = "Check the error : " + x.Message }, JsonRequestBehavior.AllowGet);
                }
            }
            var errorList = (from item in ModelState.Values
                             from error in item.Errors
                             select error.ErrorMessage).ToList();

            return Json(JsonResponseFactory.ErrorResponse(errorList), JsonRequestBehavior.DenyGet);
        }

        public int GetNomineeMaxID()
        {
            int data;
            try
            {
                data = unitOfWork.NomineeRepository.Get().Max(m => m.NomineeID);
            }
            catch
            {
                data = 0000;
            }
            return data;
        }

        public ActionResult SaveImageNom(HttpPostedFileBase NomImgFile, int nomID, int employeeID)
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                RedirectToAction("Login", "Account");
            }
            bool isAllowedToEdit = PagePermission.HasPermission(User.Identity.Name, PageID, 1);
            if (!isAllowedToEdit) return Json(new { Success = false, ErrorMessage = "You are not allowed to edit information! contact system admin!" }, JsonRequestBehavior.AllowGet);

            // Some browsers send file names with full path. We only care about the file name.
            string fileName = employeeID + "_" + nomID + Path.GetExtension(NomImgFile.FileName);
            var destinationPath = Path.Combine(Server.MapPath("~/Picture/NomPhotos/"), fileName);

            // TODO: Store description...
            try
            {
                System.IO.File.Delete(destinationPath);
            }
            catch (Exception x)
            {
                return Json(new { Success = false, ErrorMessage = "Previous file unable to delete!\n" + x.Message }, JsonRequestBehavior.DenyGet);
            }
            try
            {
                var v = unitOfWork.NomineeRepository.Get(w => w.EmpID == employeeID && w.NomineeID == nomID).Single();

                v.NomineeImageFileName = employeeID + "_" + nomID;
                unitOfWork.Save();
                NomImgFile.SaveAs(destinationPath);
                return Json(new { Success = true, Message = "Image successfully altered!" }, JsonRequestBehavior.DenyGet);
            }
            catch (Exception x)
            {
                return Json(new { Success = false, ErrorMessage = "Failed to save file! " + x.Message }, JsonRequestBehavior.DenyGet);
            }
        }

        public ActionResult SaveSignatureNom(HttpPostedFileBase NomSignFile, int employeeID, int nomID)
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                RedirectToAction("Login", "Account");
            }
            bool isAllowedToEdit = PagePermission.HasPermission(User.Identity.Name, PageID, 1);
            if (!isAllowedToEdit) return Json(new { Success = false, ErrorMessage = "You are not allowed to edit information! contact system admin!" }, JsonRequestBehavior.AllowGet);

            // Some browsers send file names with full path. We only care about the file name.
            string fileName = employeeID + "_" + nomID + Path.GetExtension(NomSignFile.FileName);
            var destinationPath = Path.Combine(Server.MapPath("~/Picture/NomSignature/"), fileName);

            // TODO: Store description...
            try
            {
                System.IO.File.Delete(destinationPath);
            }
            catch (Exception x)
            {
                return Json(new { Success = false, ErrorMessage = "Previous file unable to delete!\n" + x.Message }, JsonRequestBehavior.DenyGet);
            }
            try
            {
                var v = unitOfWork.NomineeRepository.Get(w => w.EmpID == employeeID && w.NomineeID == nomID).Single();
                v.NomineeSignFileName = employeeID + "_" + nomID;
                unitOfWork.Save();
                NomSignFile.SaveAs(destinationPath);
                return Json(new { Success = true, Message = "Image successfully altered!" }, JsonRequestBehavior.DenyGet);
            }
            catch (Exception x)
            {
                return Json(new { Success = false, ErrorMessage = "Failed to save file! " + x.Message }, JsonRequestBehavior.DenyGet);
            }
        }

        //end of nominee part
        #endregion

        #region Close PF Member
        /// <summary>
        /// Closes the pf member.
        /// </summary>
        /// <param name="empID">The emp identifier.</param>
        /// <param name="closingDate">The closing date.</param>
        /// <returns></returns>
        /// <ModifiedBy>Avishek</ModifiedBy>
        /// <ModificationDate>24-Feb-2016</ModificationDate>
        public ActionResult ClosePFMember(int empID, DateTime? closingDate, decimal? profitRate)
        {
            ViewBag.branchList = db.LU_tbl_Branch;
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                RedirectToAction("Login", "Account");//Added by Avishek Date:Dec-20-2015
            }
            CultureInfo cInfo = new CultureInfo("en-IN");
            _mvcApplication = new MvcApplication();//Added by Avishek Date Sep-21-2015
            //Eddited by Izab Ahmed on 05-12-2018
            
            
            decimal Rateprofit = profitRate ?? 0;
            decimal selfprofitamount = 0;


            if (empID > 0)
            {
                DateTime _closingDate = closingDate ?? DateTime.MinValue;
                if (_closingDate != DateTime.MinValue)
                {
                    ViewBag.ClosingDate = _closingDate.ToString("dd'/'MMM'/'yyyy");
                }

                var empDetail = unitOfWork.CustomRepository.GetAllEmployee(oCode).Single(w => w.EmpID == empID);//unitOfWork.EmployeesRepository.Get().Where(w => w.EmpID == empID).Single();
                var contributionDetail = unitOfWork.CustomRepository.ValidContributionDetail().Where(w => w.EmpID == empID).GroupBy(x => x.EmpID).Select(y => new { OwnCont = y.Sum(z => z.SelfContribution), EmpCont = y.Sum(z => z.EmpContribution), EmpId = y.Key }).ToList();
                var settlementVoucher = unitOfWork.ACC_VoucherEntryRepository.Get(g => g.OCode == oCode && g.ActionName == "Settlement").Select(s => s.VoucherID).ToList();
                //var withdrawalAmount = unitOfWork.ACC_VoucherDetailRepository.Get().Where(x => settlementVoucher.Contains(x.VoucherID) && x.Credit != 0.00M && x.PFMemberID == Convert.ToInt32(empID)).ToList();
                decimal ownProfit = 0;
                decimal empProfit = 0;
                if (ApplicationSetting.JoiningDate == true)
                {
                    ownProfit = unitOfWork.ProfitDistributionDetailRepository.Get().Where(f => f.EmpID == empID && f.TransactionDate >= empDetail.JoiningDate).GroupBy(f => f.EmpID).Select(s => s.Sum(d => d.SelfProfit)).FirstOrDefault();
                    empProfit = unitOfWork.ProfitDistributionDetailRepository.Get().Where(f => f.EmpID == empID && f.TransactionDate >= empDetail.JoiningDate).GroupBy(f => f.EmpID).Select(s => s.Sum(d => d.EmpProfit)).FirstOrDefault();
                }
                else
                {
                    ownProfit = unitOfWork.ProfitDistributionDetailRepository.Get().Where(f => f.EmpID == empID && f.TransactionDate >= empDetail.PFActivationDate).GroupBy(f => f.EmpID).Select(s => s.Sum(d => d.SelfProfit)).FirstOrDefault();
                    empProfit = unitOfWork.ProfitDistributionDetailRepository.Get().Where(f => f.EmpID == empID && f.TransactionDate >= empDetail.PFActivationDate).GroupBy(f => f.EmpID).Select(s => s.Sum(d => d.EmpProfit)).FirstOrDefault();
                }
                decimal totalPayables = _mvcApplication.GetNumber(empDetail.opEmpContribution +
                    empDetail.opOwnContribution + empDetail.opProfit) +
                    _mvcApplication.GetNumber(contributionDetail.Sum(x => x.EmpCont)) +
                    _mvcApplication.GetNumber(contributionDetail.Sum(x => x.OwnCont)) -
                    //_mvcApplication.GetNumber(withdrawalAmount.Sum(x => x.Credit ?? 0)) +
                    _mvcApplication.GetNumber(ownProfit) +
                    _mvcApplication.GetNumber(empProfit);
                //ViewBag.TotalPayables = _mvcApplication.GetNumber(totalPayables).ToString("N", cInfo);
                ViewBag.TotalPayables = _mvcApplication.GetNumber(totalPayables).ToString("N", cInfo);

                if (Rateprofit != 0)
                {
                    int totalprofitDay = 0;
                    tbl_ProfitDistributionSummary tblprofit = unitOfWork.ProfitDistributionSummaryRepository.Get().OrderByDescending(p => p.ProcessID).FirstOrDefault();
                    tbl_FinancialAuditLog auditDate = unitOfWork.AuditLogRepository.Get().OrderByDescending(p => p.LastAuditDate).FirstOrDefault();
                    DateTime profitFromDate = DateTime.Now;
                    if (tblprofit == null)
                    {
                        profitFromDate = auditDate.LastAuditDate.Value.AddDays(1);

                    }
                    else
                    {
                        profitFromDate = tblprofit.ToDate.Value.AddDays(1);
                    }

                    totalprofitDay = Convert.ToInt32((closingDate.Value - profitFromDate).TotalDays + 1); /// For Adding Extra Day.
                    
                    if (profitRate != null)
                    {
                        selfprofitamount = ((totalPayables * Rateprofit / 100) / 365) * totalprofitDay;
                    }

                }

                ViewBag.AdjustedProfit = _mvcApplication.GetNumber(selfprofitamount).ToString("N", cInfo);



                ViewBag.EmpID = empDetail.EmpID;
                ViewBag.EmpName = empDetail.EmpName;

                if (ApplicationSetting.JoiningDate == true)
                {
                    ViewBag.PFActivationDate = empDetail.JoiningDate;
                }
                else
                {

                    ViewBag.PFActivationDate = empDetail.PFActivationDate;
                }
                ViewBag.IdentificationNumber = empDetail.IdentificationNumber;
                ViewBag.InitialBalance = "OC: " + _mvcApplication.GetNumber(empDetail.opOwnContribution).ToString("N", cInfo) + ", EC: " + _mvcApplication.GetNumber(empDetail.opEmpContribution).ToString("N", cInfo) + ", PR: " + _mvcApplication.GetNumber(empDetail.opProfit).ToString("N", cInfo);
                ViewBag.Self = contributionDetail.Sum(x => x.OwnCont) + empDetail.opOwnContribution;
                ViewBag.Profit = ownProfit + (empDetail.opProfit / 2);

                if (empDetail.PFStatusID == 0)
                {
                    ViewBag.MembershipStatus = "Inactive";
                }
                else if (empDetail.PFStatusID == 1)
                {
                    ViewBag.MembershipStatus = "Active";
                }
                else if (empDetail.PFStatusID == 2)
                {
                    ViewBag.MembershipStatus = "Closed";
                    if (_closingDate == DateTime.MinValue)
                    {
                        _closingDate = empDetail.PFDeactivationDate ?? DateTime.MinValue;
                    }
                }
                else
                {
                    ViewBag.MembershipStatus = "Undefined!!!";
                }

                if (_closingDate > DateTime.MinValue)
                {
                    //Eddited by Izab Ahmed on 05-12-2018
                    DateTime lastDate = DateTime.Now;
                        //unitOfWork.AuditLogRepository.Get().Select(x => x.LastAuditDate.Value).Max();

                        
                    DateTime lastAuditDate = unitOfWork.AuditLogRepository.Get().Select(x => x.LastAuditDate.Value).Max();

                    //if (_closingDate >= lastDate)
                    if (_closingDate <= lastAuditDate)
                    {
                        ViewBag.ErrorMessage = "You cann't save this settlement because " + _closingDate.ToString("dd-MMM-yyyy") + " Audit is complete.";
                        //TempData["ErrorMessage"] = "You cann't save this settlement because " + _closingDate.ToString("dd-MMM-yyyy") + " Audit is complete.";
                        //return Json(new { Success = false, ErrorMessage = "You cann't save this settlement because " + _closingDate.ToString("dd-MMM-yyyy") + " Audit is complete." }, JsonRequestBehavior.AllowGet);
                        return View("ClosePFMember");

                    }
                    else
                    {


                        bool isOwnPartPayable = false;
                        bool isEmpPartPayable = false;
                        bool isOwnProfitPayable = false;
                        bool isEmpProfitPayable = false;
                        var pfDuration = ((_closingDate.Year - empDetail.PFActivationDate.Value.Year) * 12) + _closingDate.Month - empDetail.PFActivationDate.Value.Month;
                        
                        if (ApplicationSetting.JoiningDate == true)
                        {
                            pfDuration = ((_closingDate.Year - empDetail.JoiningDate.Value.Year) * 12) + _closingDate.Month - empDetail.JoiningDate.Value.Month;
                        }
                        
                        var getClosingRules = unitOfWork.MembershipClosingRulesRepository.Get().Where(o => o.OCode == oCode && o.PFDurationInMonth <= pfDuration).OrderByDescending(o => o.PFDurationInMonth).ToList();
                        var getClosingRule = getClosingRules.FirstOrDefault();

                        if (getClosingRule != null && getClosingRule.OwnPartPayable > 0)
                        {
                            isOwnPartPayable = true;
                        }
                        if (getClosingRule != null && getClosingRule.EmployerPartPayable > 0)
                        {
                            isEmpPartPayable = true;
                        }
                        if (getClosingRule != null && (getClosingRule.OwnProfitPercent != null && getClosingRule.OwnProfitPercent > 0))
                        {
                            isOwnProfitPayable = true;
                        }
                        if (getClosingRule != null && (getClosingRule.EmpProfitPercent != null && getClosingRule.EmpProfitPercent > 0))
                        {
                            isEmpProfitPayable = true;
                        }

                        //contribution part
                        var contribution = unitOfWork.ContributionRepository
                            .Get()
                            //.Where(w => w.EmpID == empDetail.EmpID && w.ContributionDate >= empDetail.PFActivationDate)
                           .Where(w => w.EmpID == empDetail.EmpID && w.ContributionDate >= empDetail.JoiningDate)

                            .GroupBy(g => g.EmpID)
                            .Select(s => new
                            {
                                total_selfContribution = s.Sum(m => m.SelfContribution),
                                total_employerContribution = s.Sum(n => n.EmpContribution)
                            })
                            .FirstOrDefault();
                        //check profit part
                        //now check laon part
                        //Eddit by Avishek Date: Dec-07-2015 Reason that for work with small amount of data
                        var loan = unitOfWork.CustomRepository.GetAmortizationDetailById(empID).ToList();
                        var unpaidloan = loan.Where(x => x.Processed != 1).OrderBy(x => x.InstallmentNumber).FirstOrDefault();
                        //End
                        //if (contribution != null && empDetail.opOwnContribution != null)
                        //{
                        decimal forfeitureAmount = 0;
                        decimal totalPayable = 0;
                        if (isOwnPartPayable)
                        {
                            ViewBag.Payable_selfContribution = _mvcApplication.GetNumber(((contribution == null ? 0 : contribution.total_selfContribution) * getClosingRule.OwnPartPayable) / 100).ToString("N", cInfo) ?? "";
                            totalPayable += (((contribution == null ? 0 : contribution.total_selfContribution) + empDetail.opOwnContribution) * getClosingRule.OwnPartPayable) / 100;
                            totalPayable = totalPayable - (unpaidloan == null ? 0 : (unpaidloan.Interest + unpaidloan.Amount));
                            decimal forfeitureOwnCon = 100 - getClosingRule.OwnPartPayable;
                            forfeitureAmount += (((contribution == null ? 0 : contribution.total_selfContribution) + empDetail.opOwnContribution) * forfeitureOwnCon) / 100;
                        }
                        else
                        {
                            forfeitureAmount += (contribution == null ? 0 : contribution.total_selfContribution) + empDetail.opOwnContribution;
                        }
                        if (isEmpPartPayable)
                        {
                            ViewBag.Payable_empContribution = _mvcApplication.GetNumber(((contribution == null ? 0 : contribution.total_employerContribution) * getClosingRule.EmployerPartPayable) / 100).ToString("N", cInfo);
                            totalPayable += (((contribution == null ? 0 : contribution.total_employerContribution) + empDetail.opEmpContribution) * getClosingRule.EmployerPartPayable) / 100;
                            decimal forfeitureEmpCon = 100 - getClosingRule.EmployerPartPayable;
                            forfeitureAmount += (((contribution == null ? 0 : contribution.total_employerContribution) + empDetail.opEmpContribution) * forfeitureEmpCon) / 100;
                        }
                        else
                        {
                            forfeitureAmount += (contribution == null ? 0 : contribution.total_employerContribution) + empDetail.opEmpContribution;
                        }
                        if (isOwnProfitPayable)
                        {
                            ViewBag.Payable_OwnProfit = _mvcApplication.GetNumber(((ownProfit + (empDetail.opProfit / 2)) * getClosingRule.OwnProfitPercent ?? 0) / 100).ToString("N", cInfo) ?? "";
                            //totalPayable += ownProfit + (empDetail.opProfit * (contribution == null ? 0 : contribution.total_selfContribution))==0?0:
                            //                ((ownProfit + ((empDetail.opProfit * (contribution == null ? 0 : contribution.total_selfContribution)) / ((contribution == null ? 0 : contribution.total_employerContribution) + (contribution == null ? 0 : contribution.total_selfContribution)))) * getClosingRule.OwnProfitPercent ?? 0) / 100;

                            totalPayable += ((ownProfit + (empDetail.opProfit / 2)) * getClosingRule.OwnProfitPercent ?? 0) / 100;
                            decimal forfeitureSelfProfit = 100 - getClosingRule.OwnProfitPercent ?? 0;
                            //forfeitureAmount += ownProfit + (empDetail.opProfit * (contribution == null ? 0 : contribution.total_selfContribution))==0?0:
                            //                ((ownProfit + ((empDetail.opProfit * (contribution == null ? 0 : contribution.total_selfContribution)) / ((contribution == null ? 0 : contribution.total_employerContribution) + (contribution == null ? 0 : contribution.total_selfContribution)))) * forfeitureSelfProfit) / 100;
                            forfeitureAmount += ((ownProfit + (empDetail.opProfit / 2)) * forfeitureSelfProfit) / 100;
                        }
                        else
                        {
                            forfeitureAmount += ownProfit + ((empDetail.opProfit * (contribution == null ? 0 : contribution.total_selfContribution)) / ((contribution == null ? 0 : contribution.total_employerContribution) + (contribution == null ? 0 : contribution.total_selfContribution)));
                        }
                        if (isEmpProfitPayable)
                        {
                           // ViewBag.Payable_EmpProfit = _mvcApplication.GetNumber(((empProfit) * getClosingRule.EmpProfitPercent ?? 0) / 100).ToString("N", cInfo);
                            ViewBag.Payable_EmpProfit = _mvcApplication.GetNumber(((empProfit + (empDetail.opProfit / 2)) * getClosingRule.OwnProfitPercent ?? 0) / 100).ToString("N", cInfo) ?? "";

                            //totalPayable += (ownProfit + ((empDetail.opProfit * (contribution == null ? 0 : contribution.total_employerContribution))))==0?0:
                            //    ((ownProfit + ((empDetail.opProfit * (contribution == null ? 0 : contribution.total_employerContribution)) / ((contribution == null ? 0 : contribution.total_employerContribution) + (contribution == null ? 0 : contribution.total_selfContribution)))) * getClosingRule.OwnProfitPercent ?? 0) / 100;
                            totalPayable += ((empProfit + (empDetail.opProfit / 2)) * getClosingRule.EmpProfitPercent ?? 0) / 100;
                            decimal forfeitureEmpProfit = 100 - getClosingRule.EmpProfitPercent ?? 0;
                            //forfeitureAmount += ownProfit + (empDetail.opProfit * (contribution == null ? 0 : contribution.total_employerContribution))==0?0:
                            //    ((ownProfit + ((empDetail.opProfit * (contribution == null ? 0 : contribution.total_employerContribution)) / ((contribution == null ? 0 : contribution.total_employerContribution) + (contribution == null ? 0 : contribution.total_selfContribution)))) * forfeitureSelfProfit) / 100;

                            forfeitureAmount += ((empProfit + (empDetail.opProfit / 2)) * forfeitureEmpProfit) / 100;
                        }
                        else
                        {
                            forfeitureAmount += ownProfit + ((empDetail.opProfit * (contribution == null ? 0 : contribution.total_employerContribution)) / ((contribution == null ? 0 : contribution.total_employerContribution) + (contribution == null ? 0 : contribution.total_selfContribution)));
                        }
                        
                        totalPayable = totalPayable + selfprofitamount;  //// For Adding Adjusted Profit
                        ViewBag.UnpaidLoan = _mvcApplication.GetNumber((unpaidloan == null ? 0 : (unpaidloan.Amount))).ToString("N", cInfo);
                        //Added By Kamrul Hasan -- for Showing Loan Interest Amount
                        ViewBag.UnpaidLoanInterest = _mvcApplication.GetNumber((unpaidloan == null ? 0 : (unpaidloan.Interest))).ToString("N", cInfo);
                        //
                        ViewBag.MembershipDuration = pfDuration.ToString("N", cInfo);
                        ViewBag.ForfeitureAmount = _mvcApplication.GetNumber(forfeitureAmount).ToString("N", cInfo);
                        ViewBag.TotalPayable = _mvcApplication.GetNumber(totalPayable).ToString("N", cInfo);
                        ViewBag.Balance = _mvcApplication.GetNumber(totalPayables + selfprofitamount - (((unpaidloan == null ? 0 : (unpaidloan.Interest + unpaidloan.Amount))) + forfeitureAmount)).ToString("N", cInfo);
                        ViewBag.TotalDebit = _mvcApplication.GetNumber(totalPayable).ToString("N", cInfo);
                        decimal totalCredit = (unpaidloan == null ? 0 : (unpaidloan.Interest + unpaidloan.Amount)) + forfeitureAmount + totalPayable;
                        ViewBag.TotalCredit = _mvcApplication.GetNumber(totalCredit).ToString("N", cInfo);
                        //}
                        //get the contribution detail
                        //Added by Avishek Date:Feb-18-2015
                        //var result = unitOfWork.CustomRepository.EmpPFMonthlyStatus(oCode, empID).Where(w => w.ContrebutionDate >= empDetail.PFActivationDate).ToList();
                        var result = unitOfWork.CustomRepository.EmpPFMonthlyStatus(oCode, empID).Where(w => w.ContrebutionDate >= empDetail.JoiningDate).ToList();

                        var unpaidDetail = unitOfWork.CustomRepository.AmortizationList(empID).OrderBy(x => x.PFLoanID).ToList();
                        ViewBag.UnpaidDetail = unpaidDetail;
                        //End
                        ViewBag.ContributionDetail = result;
                        //getLoan detail
                        //will be implemented if need
                    }
                }
                LedgerOptions();
                return View("ClosePFMember");
            }
            ViewBag.Message = "EmpID not valid!";
            return RedirectToAction("PFMembers");
        }
        #endregion

        /// <summary>
        /// Closes the pf member.
        /// </summary>
        /// <param name="empID">The emp identifier.</param>
        /// <param name="closingDate">The closing date.</param>
        /// <returns></returns>
        /// <ModifiedBy>Avishek</ModifiedBy>
        /// <ModificationDate>24-Feb-2016</ModificationDate>
        [HttpPost]
        public ActionResult ClosePFMember(int empID, DateTime closingDate)
        {
            ViewBag.branchList = db.LU_tbl_Branch;
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                RedirectToAction("Login", "Account");
            }
            if (empID > 0)
            {
                if (ClosingMembershipPossible(empID))
                {
                    var v = unitOfWork.EmployeesRepository.Get(w => w.EmpID == empID).Single();
                    v.PFStatus = 2; //02 for closed state.
                    var p = unitOfWork.EmployeesRepository.Get(w => w.EmpID == empID).Single();
                    p.PFDeactivationDate = closingDate;
                    p.PFDeactivatedBy = unitOfWork.CustomRepository.GetUserID(User.Identity.Name);
                    p.PFDeactivatedByName = User.Identity.Name;
                    try
                    {
                        unitOfWork.Save();
                        return Json(new { Success = true, Message = "Membership Closed!!!" }, JsonRequestBehavior.DenyGet);
                    }
                    catch (Exception x)
                    {
                        return Json(new { Success = false, ErrorMessage = "Check error: " + x.Message }, JsonRequestBehavior.DenyGet);
                    }
                }
                return Json(new { Success = false, ErrorMessage = "Closing membership not possible! validation fail!" }, JsonRequestBehavior.DenyGet);
            }
            return Json(new { Success = false, ErrorMessage = "Invalid employee id." }, JsonRequestBehavior.DenyGet);
        }

        public void LedgerOptions(string s = "")
        {
            //all ledger as suggested
            ViewData["LedgerOptions"] = new SelectList(unitOfWork.ACC_LedgerRepository.Get().ToList(), "LedgerName", "LedgerName", s);
        }

        public bool ClosingMembershipPossible(int empID)
        {
            //validation will be  added here
            return true;
        }

        /// <summary>
        /// Pfs the membership closed list.
        /// </summary>
        /// <returns>View</returns>
        /// <ModifiedBy>Avishek</ModifiedBy>
        /// <ModificationDate>24-Feb-2016</ModificationDate>
        [Authorize]
        public ActionResult PFMembershipClosedList()
        {
            ViewBag.branchList = db.LU_tbl_Branch;
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                RedirectToAction("Login", "Account");
            }
            List<VM_Employee> v = new List<VM_Employee>();

            if (DLL.Utility.ApplicationSetting.Branch == true)
            {
                v = unitOfWork.CustomRepository.GetAllEmployeeByBranch(oCode).ToList();

            }
            else {
                v = unitOfWork.CustomRepository.GetAllEmployee(oCode).ToList();
            }
           
            var totalEmployee = v.Count;
            var pfMembers = v.Where(x => x.PFStatusID == 1).ToList();
            var pfDeactivated = v.Where(x => x.PFStatusID == 2).ToList();
            var totalPfMembers = pfMembers.Count;
            ViewBag.Message = totalPfMembers + " person(s) are PF member from " + totalEmployee + " employee";
            ViewBag.Deactivated = pfDeactivated.Count + " member deactivated";
            return View("PFMembershipClosedList", pfDeactivated);
        }

        /// <summary>
        /// Settlements the specified self con.
        /// </summary>
        /// <param name="SelfCon">The self con.</param>
        /// <param name="SelfPro">The self pro.</param>
        /// <param name="MembersFund">The members fund.</param>
        /// <param name="ForfeitureAmount">The forfeiture amount.</param>
        /// <param name="UnpaidLoan">The unpaid loan.</param>
        /// <param name="LedgerName">Name of the ledger.</param>
        /// <param name="EmpID">The emp identifier.</param>
        /// <param name="IdentificationNumber">The identification number.</param>
        /// <param name="Balance">The balance.</param>
        /// <param name="Comment">The comment.</param>
        /// <param name="PFClosingDate">The pf closing date.</param>
        /// <returns>Bool</returns>
        /// <ModifiedBy>Avishek</ModifiedBy>
        /// <ModificationDate>24-Feb-2016</ModificationDate>
        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Settlement(decimal SelfCon, decimal SelfPro, decimal MembersFund, decimal ForfeitureAmount, decimal UnpaidLoan, decimal UnpaidLoanInterest, decimal AdjustedProfit, string LedgerName, int EmpID, string IdentificationNumber, decimal Balance, string Comment, string PFClosingDate)
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                RedirectToAction("Login", "Account");
            }
            _mvcApplication = new MvcApplication();
            string currentUserName = User.Identity.Name;
            Guid currentUserId = unitOfWork.CustomRepository.GetUserID(currentUserName);
            string refMessage = "";
            int voucherId = 0;

            #region validation
            if (oCode < 1)
            {
                return Json(new { Success = false, ErrorMessage = "You must associate with an company to continue this process..." }, JsonRequestBehavior.DenyGet);
            }
            if (MembersFund < 1)
            {
                return Json(new { Success = false, ErrorMessage = "Members Fund less than 1. Process cannot continue..." }, JsonRequestBehavior.DenyGet);
            }
            if (string.IsNullOrEmpty(PFClosingDate))
            {
                return Json(new { Success = false, ErrorMessage = "PF Closing date must not be empty!" }, JsonRequestBehavior.DenyGet);
            }

            if (_mvcApplication.GetNumber(MembersFund + AdjustedProfit) != _mvcApplication.GetNumber(ForfeitureAmount + UnpaidLoan + UnpaidLoanInterest + Balance))
            {
                return Json(new { Success = false, ErrorMessage = "Voucher Debit-Credit amount must be equal!" }, JsonRequestBehavior.DenyGet);
            }
            if (string.IsNullOrEmpty(LedgerName))
            {
                return Json(new { Success = false, ErrorMessage = "You must select a ledger name to complete process..." }, JsonRequestBehavior.DenyGet);
            }
            var empEntity = unitOfWork.EmployeesRepository.GetByID(EmpID);
            if (empEntity.PFStatus != 1)
            {
                return Json(new { Success = false, ErrorMessage = "This employee currently not active. So process cannot continue to deactivate this user!" }, JsonRequestBehavior.DenyGet);
            }
            #endregion

            //List<string> LedgerNameList = new List<string>();
            List<Guid> ledgerIdList = new List<Guid>();
            List<decimal> credit = new List<decimal>();
            List<decimal> debit = new List<decimal>();
            List<string> chqNumber = new List<string>();
            List<string> pfLoanId = new List<string>();
            List<string> pfMemberId = new List<string>();

            List<acc_Chart_of_Account_Maping> accChartOfAccountMaping = unitOfWork.ChartofAccountMapingRepository.Get().ToList();
            ledgerIdList.Add(accChartOfAccountMaping.Where(x => x.MIS_Id == 3 && x.OCode == oCode).Select(x => x.Ledger_Id).FirstOrDefault());
            credit.Add(0);
            debit.Add(_mvcApplication.GetNumber(SelfCon + SelfPro));
            chqNumber.Add("");
            pfMemberId.Add(EmpID + "");
            pfLoanId.Add("");
            //
            ledgerIdList.Add(accChartOfAccountMaping.Where(x => x.MIS_Id == 18 && x.OCode == oCode).Select(x => x.Ledger_Id).FirstOrDefault());
            credit.Add(0);
            debit.Add(_mvcApplication.GetNumber(MembersFund - (SelfCon + SelfPro)));
            chqNumber.Add("");
            pfMemberId.Add(EmpID + "");
            pfLoanId.Add("");
            // For Adjusted Profit
            ledgerIdList.Add(accChartOfAccountMaping.Where(x => x.MIS_Id == 26 && x.OCode == oCode).Select(x => x.Ledger_Id).FirstOrDefault());
            credit.Add(0);
            debit.Add(_mvcApplication.GetNumber(AdjustedProfit/2));
            chqNumber.Add("");
            pfMemberId.Add(EmpID + "");
            pfLoanId.Add("");

            ledgerIdList.Add(accChartOfAccountMaping.Where(x => x.MIS_Id == 26 && x.OCode == oCode).Select(x => x.Ledger_Id).FirstOrDefault());
            credit.Add(0);
            debit.Add(_mvcApplication.GetNumber(AdjustedProfit / 2));
            chqNumber.Add("");
            pfMemberId.Add(EmpID + "");
            pfLoanId.Add("");
            //
            ledgerIdList.Add(accChartOfAccountMaping.Where(x => x.MIS_Id == 6 && x.OCode == oCode).Select(x => x.Ledger_Id).FirstOrDefault());
            credit.Add(_mvcApplication.GetNumber(ForfeitureAmount));
            debit.Add(0);
            chqNumber.Add("");
            pfMemberId.Add(EmpID + "");
            pfLoanId.Add("");
            //
            ledgerIdList.Add(accChartOfAccountMaping.Where(x => x.MIS_Id == 5 && x.OCode == oCode).Select(x => x.Ledger_Id).FirstOrDefault());
            credit.Add(_mvcApplication.GetNumber(UnpaidLoan));
            debit.Add(0);
            chqNumber.Add("");
            pfMemberId.Add(EmpID + "");
            pfLoanId.Add("");

            ledgerIdList.Add(accChartOfAccountMaping.Where(x => x.MIS_Id == 8 && x.OCode == oCode).Select(x => x.Ledger_Id).FirstOrDefault());
            credit.Add(_mvcApplication.GetNumber(UnpaidLoanInterest));
            debit.Add(0);
            chqNumber.Add("");
            pfMemberId.Add(EmpID + "");
            pfLoanId.Add("");

            // For Adjusted Profit to Profit Distribution Expense
            //ledgerIdList.Add(accChartOfAccountMaping.Where(x => x.MIS_Id == 7 && x.OCode == oCode).Select(x => x.Ledger_Id).FirstOrDefault());
            //credit.Add(_mvcApplication.GetNumber(AdjustedProfit));
            //debit.Add(0);
            //chqNumber.Add("");
            //pfMemberId.Add(EmpID + "");
            //pfLoanId.Add("");

            Guid ledgerId = unitOfWork.ACC_LedgerRepository.Get().Where(x => x.LedgerName == LedgerName).Select(x => x.LedgerID).FirstOrDefault();
            ledgerIdList.Add(ledgerId);
            //credit.Add(_mvcApplication.GetNumber(Balance - AdjustedProfit));
            credit.Add(_mvcApplication.GetNumber(Balance));

            debit.Add(0);
            chqNumber.Add("");
            pfMemberId.Add(EmpID + "");
            pfLoanId.Add("");
            //
            string empName = unitOfWork.CustomRepository.GetEmployeeById(EmpID).Select(x => x.EmpName).FirstOrDefault();
            bool isOperationSuccess = unitOfWork.AccountingRepository.DualEntryVoucherById(EmpID, 5, Convert.ToDateTime(PFClosingDate), ref voucherId, "Member settlement ID " + IdentificationNumber + " Name " + empName, ledgerIdList, debit, credit, chqNumber, ref refMessage, currentUserName, currentUserId, pfMemberId, "Pre Settlement", "", "", null, pfLoanId, oCode, "Settlement");
            int VTypeID = 5;
            Guid editUser = currentUserId;
            DateTime editDate = DateTime.Now;
            if (isOperationSuccess)
            {
                try
                {
                    var employee = unitOfWork.EmployeesRepository.GetByID(EmpID);
                   
                    employee.PFDeactivationVoucherID = voucherId;
                    employee.PFStatus = 2;// 2 means closed! 0 inactive and 1 active
                    employee.PFDeactivatedBy = currentUserId;
                    employee.PFDeactivatedByName = currentUserName;
                    employee.PFDeactivationDate = DateTime.ParseExact(PFClosingDate, "dd/MMM/yyyy", CultureInfo.InvariantCulture);
                    employee.Comment += Comment;
                    unitOfWork.EmployeesRepository.Update(employee);
                    
                    ///---------------Check Info ---------
                    if (ApplicationSetting.Chequeue == true)
                    {
                        Ac_Cheque_Print checkP = new Ac_Cheque_Print();
                        checkP.Amount = _mvcApplication.GetNumber(Balance);
                        checkP.AccountNo = "AC-12345";
                        checkP.ChequeDate = Convert.ToDateTime(PFClosingDate);
                        checkP.ClientInfo_id = EmpID;
                        checkP.ChequeNo = "CA-001"; //+ GetMaxVoucherTypeID(VTypeID).ToString().PadLeft(5, '0');
                        checkP.ChequeType = "Cash";
                        checkP.OCode = 1;
                        checkP.BankInfo_Id = 1;
                        checkP.LedgerId = ledgerId;
                        checkP.EditDate = editDate;
                        checkP.EditUser = editUser;
                        //checkP.ChequePrint_id = GetMaxCheckID();
                        //unitOfWork.ChequeRepository.Insert(checkP);
                        db.Ac_Cheque_Print.Add(checkP);
                        try
                        {
                           db.SaveChanges();

                        }
                        catch (Exception x)
                        {
                            return Json(new { Success = false, ErrorMessage = "WOW ERROR: " + x.Message }, JsonRequestBehavior.DenyGet);
                        }
                   
                    }
                    else
                    {
                       db.SaveChanges();
                    }
                    
                    unitOfWork.Save();
                    return Json(new { Success = true, Message = "Member settlement process completed successfully!" }, JsonRequestBehavior.DenyGet);
                }
                catch (Exception x)
                {
                    return Json(new { Success = false, ErrorMessage = "MEMBER SETTLEMENT PROCESS WAS NOT SUCCESSFULL. ERROR: " + x.Message }, JsonRequestBehavior.DenyGet);
                }
            }
            return Json(new { Success = false, ErrorMessage = "TRANSACTION FAILED WITH FOLLOWING ERROR : " + refMessage }, JsonRequestBehavior.DenyGet);
        }

        public int GetMaxVoucherTypeID(int i)
        {
            int max = 0;

            try
            {
                
                    max = db.Ac_Cheque_Print.Select(x => new
                    {
                        ChequeNo = Convert.ToInt32(x.ChequeNo.Substring(3, 8))
                    }).Max(a => a.ChequeNo);
               

                max++;
            }
            catch (Exception ex)
            {
                if (ex.Message == "Sequence contains no elements")
                {
                    max++;
                }
                else
                    throw;
            }

            return max;
        }
        public int GetMaxCheckID()
        {
            int cId = 0;
            try
            {
                cId = db.Ac_Cheque_Print.Select(s => s.ChequePrint_id).Max();
            }
            catch
            {
                cId = 0;
            }
            return cId + 1;
        }
        //handle error
        private void ErrorGenerate(Exception ex)
        {
            tbl_ErrorLog errorLog = new tbl_ErrorLog();
            errorLog.Message = ex.Message;
            errorLog.InnerException = ex.InnerException + "";
            errorLog.UserName = User.Identity.Name;
            errorLog.Time = DateTime.Now;
            try
            {
                errorLog.Terminal = Request.ServerVariables["REMOTE_ADDR"];
            }
            catch
            {
                errorLog.Terminal = System.Web.HttpContext.Current.Request.UserHostAddress;
            }
            errorLog.HostName = Dns.GetHostName();
            try
            {
                unitOfWork.ErrorLogRepository.Insert(errorLog);
                unitOfWork.Save();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Passes the voucher.
        /// </summary>
        /// <returns>View</returns>
        /// <ModifiedBy>Avishek</ModifiedBy>
        /// <ModificationDate>24-Feb-2016</ModificationDate>
        public ActionResult PassVoucher()
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return Json(new { Success = false, ErrorMessage = "You must be under a compnay!" }, JsonRequestBehavior.DenyGet);
            }
            var v = unitOfWork.EmployeesRepository.Get(w => w.PassVoucher == false && w.OCode == oCode).ToList();
            ViewBag.Message = "Following " + v.Count + " Employees' accounting entry pending!";
            if (v.Count > 0)
            {
                ViewBag.TotalCount = v.Count + "";
            }
            return View("PassVoucher", v);
        }

        /// <summary>
        /// Passes the voucher confirm.
        /// </summary>
        /// <returns>bool</returns>
        /// <ModifiedBy>Avishek</ModifiedBy>
        /// <ModificationDate>24-Feb-2016</ModificationDate>
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PassVoucherConfirm()
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                RedirectToAction("Login", "Account");
            }
            _mvcApplication = new MvcApplication();
            if (oCode == 0)
            {
                return Json(new { Success = false, ErrorMessage = "To create a group you must be under a company!" }, JsonRequestBehavior.DenyGet);
            }

            string refMessage = "";
            var v = unitOfWork.EmployeesRepository.Get(w => w.PassVoucher == false && w.OCode == oCode).ToList();
            if (v.Count < 1)
            {
                unitOfWork.Save();
                ViewBag.Message = "No voucher pass pending...";
                return View("PassVoucher", v);
            }


            string currentUserName = User.Identity.Name;
            Guid currentUserId = unitOfWork.CustomRepository.GetUserID(currentUserName);
            int voucherId = 0;

            decimal openingBalance = 0;
            decimal openingLoan = 0;
            decimal openOwnCon = 0;
            decimal openEmpCon = 0;
            foreach (var item in v)
            {
                openingBalance = openingBalance + ((item.opOwnContribution ?? 0) + (item.opEmpContribution ?? 0) + (item.opProfit ?? 0));
                if (item.opLoan != null) openingLoan += (decimal)item.opLoan;
                decimal profit = (item.opProfit / 2) ?? 0;
                openOwnCon = openOwnCon + (item.opOwnContribution ?? 0) + profit;
                openEmpCon = openEmpCon + (item.opEmpContribution ?? 0) + profit;

                List<acc_Chart_of_Account_Maping> accChartOfAccountMaping = unitOfWork.ChartofAccountMapingRepository.Get().ToList();

                //read commnet
                //is this process execute 2nd time then previous value will be sum up with new balance but will not be replaced.
                //if the paramenter PFAdditionalInfo === "Opening Balance" then have to show members opening balance ledger with => ocode+passVoucher is true
                //Added By Avishek Date: Dec-23-2015
                List<Guid> ledgerIdList = new List<Guid>();
                List<decimal> credit = new List<decimal>();
                List<decimal> debit = new List<decimal>();
                List<string> chqNumber = new List<string>();
                List<string> pfLoanId = new List<string>();
                List<string> pfMemberId = new List<string>();

                ledgerIdList.Add(accChartOfAccountMaping.Where(x => x.MIS_Id == 1 && x.OCode == oCode).Select(x => x.Ledger_Id).FirstOrDefault());
                credit.Add(0);
                debit.Add(_mvcApplication.GetNumber(openOwnCon));
                chqNumber.Add("");
                pfMemberId.Add(item.EmpID.ToString() ?? "");
                pfLoanId.Add("");

                ledgerIdList.Add(accChartOfAccountMaping.Where(x => x.MIS_Id == 2 && x.OCode == oCode).Select(x => x.Ledger_Id).FirstOrDefault());
                credit.Add(0);
                debit.Add(_mvcApplication.GetNumber(openEmpCon));
                chqNumber.Add("");
                pfMemberId.Add(item.EmpID.ToString() ?? "");
                pfLoanId.Add("");

                ledgerIdList.Add(accChartOfAccountMaping.Where(x => x.MIS_Id == 3 && x.OCode == oCode).Select(x => x.Ledger_Id).FirstOrDefault());
                credit.Add(_mvcApplication.GetNumber(openOwnCon));
                debit.Add(0);
                chqNumber.Add("");
                pfMemberId.Add(item.EmpID.ToString() ?? "");
                pfLoanId.Add("");

                ledgerIdList.Add(accChartOfAccountMaping.Where(x => x.MIS_Id == 4 && x.OCode == oCode).Select(x => x.Ledger_Id).FirstOrDefault());
                credit.Add(_mvcApplication.GetNumber(openEmpCon));
                debit.Add(0);
                chqNumber.Add("");
                pfMemberId.Add(item.EmpID.ToString() ?? "");
                pfLoanId.Add("");

                bool isVoucherPassed = unitOfWork.AccountingRepository.DualEntryVoucherById(item.EmpID, 5, DateTime.Now, ref voucherId, "Member add in PF" + "" + " Name " + currentUserName, ledgerIdList, debit, credit, chqNumber, ref refMessage, currentUserName, currentUserId, pfMemberId, "", "", "", null, pfLoanId, oCode, "Member add");
                //End

                string voucherPassedMessage = refMessage;

                if (isVoucherPassed)
                {
                    ////update status/message in employee entity
                    item.PassVoucher = true;
                    item.PassVoucherMessage = "Accounting Hit Completed!" + voucherPassedMessage;
                }
            }
            try
            {
                unitOfWork.Save();
                ViewBag.Message = "Transaction Successfull! Members Fund Ledger Opening Balance Updated!!!";
                return View("PassVoucher", v);
            }
            catch (Exception x)
            {
                return Json(new { Success = false, ErrorMessage = "TRANSACTION FAILED WITH FOLLOWING ERROR: " + x.Message + " PLEASE CONTACT SYS ADMIN!" }, JsonRequestBehavior.DenyGet);
            }
        }

        /// <summary>
        /// Pfs the closing report.
        /// </summary>
        /// <param name="EmpID">The emp identifier.</param>
        /// <returns>View</returns>
        /// <ModifiedBy>Avishek</ModifiedBy>
        /// <ModificationDate>24-Feb-2016</ModificationDate>
        public ActionResult PFClosingReport(int EmpID)
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                RedirectToAction("Login", "Account");
            }
            acc_VoucherEntry settlementVoucherEntry = new acc_VoucherEntry();
            List<VM_acc_VoucherDetail> settlementVoucherDetail = new List<VM_acc_VoucherDetail>();
            //var empEntity = unitOfWork.CustomRepository.GetAllEmployee(oCode).Where(f => f.EmpID == EmpID).ToList();
            tbl_Employees tblEmployees = unitOfWork.EmployeesRepository.GetByID(EmpID);
            var empEntity = unitOfWork.CustomRepository.GetAllEmployee(oCode).Where(f => f.EmpID == EmpID).ToList();
            if (empEntity != null)
            {
                if (empEntity[0].PFDeactivationVoucherID > 0)
                {
                    settlementVoucherEntry = unitOfWork.ACC_VoucherEntryRepository.Get(f => f.VoucherID == tblEmployees.PFDeactivationVoucherID).SingleOrDefault();
                    if (settlementVoucherEntry != null)
                    {
                        settlementVoucherDetail = unitOfWork.AccountingRepository.GetVoucherDetail(settlementVoucherEntry.VoucherID).ToList();
                    }
                }
                else
                {
                    return Json(new { Success = false, ErrorMessage = "This member is not deactivated or voucher not passed!" }, JsonRequestBehavior.AllowGet);
                }
            }
            List<tbl_Employees> tblEmployeesList = unitOfWork.EmployeesRepository.Get(x => x.EmpID == EmpID).ToList();
            
            LocalReport lr = new LocalReport();

            string path = Path.Combine(Server.MapPath("~/Reporting/PF/"), "PFDeactivationReport.rdlc");
            if (System.IO.File.Exists(path))
            {
                lr.ReportPath = path;
            }
            else
            {
                return View("Report/ReportPF/Index");
            }

            string userInfo = "This record was modified by " + unitOfWork.UserProfileRepository.Get(w => w.UserID == settlementVoucherEntry.EditUser).SingleOrDefault().UserFullName + " at " + settlementVoucherEntry.EditDate;
            var getCompany = unitOfWork.CompanyInformationRepository.GetByID(oCode);
            ReportParameterCollection reportParameters = new ReportParameterCollection();
            reportParameters.Add(new ReportParameter("rpVoucherNumber", settlementVoucherEntry.VNumber + ""));
            reportParameters.Add(new ReportParameter("rpTransactionDate", settlementVoucherEntry.TransactionDate + ""));
            reportParameters.Add(new ReportParameter("rpCreatedBy", userInfo));
            reportParameters.Add(new ReportParameter("rpNarration", settlementVoucherEntry.Narration + ""));
            reportParameters.Add(new ReportParameter("rpCompanyName", getCompany.CompanyName + ""));
            reportParameters.Add(new ReportParameter("rpCompanyAddress", getCompany.CompanyAddress + ""));
            lr.SetParameters(reportParameters);


            ReportDataSource rd1 = new ReportDataSource("DataSet1", empEntity);
            ReportDataSource rd2 = new ReportDataSource("DataSet2", settlementVoucherDetail);

            lr.DataSources.Add(rd1);
            lr.DataSources.Add(rd2);

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

        public ActionResult GetList(int BranchID)
        {

            ViewBag.branchname = new SelectList(db.LU_tbl_Branch, "BranchID", "BranchLocation");

            LU_tbl_Branch branch = unitOfWork.BranchRepository.GetByID(BranchID);
            LU_tbl_Branch _branch = new LU_tbl_Branch();
            _branch.BranchLocation = branch.BranchLocation;

            //_branch.EmpName = employee.EmpName;
            //_employee.Designation = employee.Designation;
            //_employee.Department = employee.Department;
            return Json(_branch);




            //var branch = new
            //{
            //    BranchShortName = ViewBag.branchname


            //};
            //return Json(new { Success = true, Message = branch }, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> SendMail()
        {
            int OCode = ((int?)Session["OCode"]) ?? 0;
            _mvcApplication = new MvcApplication();
            LocalReport lr = new LocalReport();

            DateTime fdate = Convert.ToDateTime("1900-01-10");
            DateTime tdate = DateTime.Now;

            List<VM_Email> emailModel = new List<VM_Email>();
            var employee = unitOfWork.EmployeesRepository.Get().Select(s => new { Name = s.EmpName, Id = s.EmpID, Email = s.Email }).ToList();


            #region _member_contribution_report
            string path = Path.Combine(Server.MapPath("~/Reporting/PF"), "PFMemberContribution.rdlc");
            if (System.IO.File.Exists(path))
            {
                lr.ReportPath = path;
            }
            else
            {
                return View("Report/ReportPF/Index");
            }
            Task task = new Task(() =>
            {

                foreach (var emp in employee)
                {
                    try
                    {

                        List<VM_PFMonthlyStatus> v = new List<VM_PFMonthlyStatus>();
                        using (unitOfWork = new UnitOfWork())
                        {
                            int? empID = emp.Id;
                            var temp = unitOfWork.CustomRepository.EmpPFMonthlyStatusForReport(OCode, empID ?? 0, fdate, tdate).OrderBy(x => x.ContrebutionDate).ToList();
                            List<tbl_ProfitDistributionDetail> _tbl_ProfitDistributionDetail = unitOfWork.CustomRepository.GetProfitDistributionByEmpId(Convert.ToInt16(empID)).Where(x => x.TransactionDate >= fdate && x.TransactionDate <= tdate).ToList();
                            foreach (VM_PFMonthlyStatus item in temp)
                            {
                                VM_PFMonthlyStatus atbl_ProfitDistributionDetail = new VM_PFMonthlyStatus();
                                atbl_ProfitDistributionDetail.EmpID = item.EmpID;
                                atbl_ProfitDistributionDetail.IdentificationNumber = item.IdentificationNumber;
                                atbl_ProfitDistributionDetail.ProcessRunDate = item.ProcessRunDate;
                                atbl_ProfitDistributionDetail.ContrebutionDate = item.ContrebutionDate;
                                atbl_ProfitDistributionDetail.EmpContribution = _mvcApplication.GetNumber(item.EmpContribution);
                                atbl_ProfitDistributionDetail.SelfContribution = _mvcApplication.GetNumber(item.SelfContribution);
                                atbl_ProfitDistributionDetail.Year = item.Year;
                                atbl_ProfitDistributionDetail.Month = item.Month;
                                atbl_ProfitDistributionDetail.SelfProfit = _mvcApplication.GetNumber(Convert.ToDecimal(_tbl_ProfitDistributionDetail.Where(x => x.TransactionDate.Value.Month == Convert.ToInt32(item.Month) && x.TransactionDate.Value.Year == Convert.ToInt32(item.Year)).Select(x => x.SelfProfit).FirstOrDefault()));
                                atbl_ProfitDistributionDetail.EmpProfit = _mvcApplication.GetNumber(Convert.ToDecimal(_tbl_ProfitDistributionDetail.Where(x => x.TransactionDate.Value.Month == Convert.ToInt32(item.Month) && x.TransactionDate.Value.Year == Convert.ToInt32(item.Year)).Select(x => x.EmpProfit).FirstOrDefault()));
                                v.Add(atbl_ProfitDistributionDetail);
                            }

                            //v = temp.Where(x => x.ProcessRunDate >= fdate && x.ProcessRunDate <= tdate).ToList();
                            List<tbl_Employees> list = unitOfWork.CustomRepository.GetEmployeeById(v.Where(x => x.EmpID != null).Select(x => x.EmpID).FirstOrDefault());
                            var opening = unitOfWork.CustomRepository.EmpPFMonthlyStatusForReport(OCode, empID ?? 0, DateTime.MinValue, fdate).OrderBy(x => x.ProcessRunDate).ToList();
                            List<tbl_ProfitDistributionDetail> _tbl_ProfitDistributionDetailforOpening = unitOfWork.CustomRepository.GetProfitDistributionByEmpId(Convert.ToInt16(empID)).Where(x => x.TransactionDate <= fdate).ToList();
                            decimal se = _mvcApplication.GetNumber((decimal)(opening.Where(x => x.ContrebutionDate <= fdate) == null ? 0 : opening.Where(x => x.ContrebutionDate <= fdate).Sum(x => x.SelfContribution)));
                            decimal em = _mvcApplication.GetNumber((decimal)(opening.Where(x => x.ContrebutionDate <= fdate) == null ? 0 : opening.Where(x => x.ContrebutionDate <= fdate).Sum(x => x.EmpContribution)));
                            decimal ope = _mvcApplication.GetNumber((decimal)list.Sum(x => x.opProfit));
                            decimal oem = _mvcApplication.GetNumber((decimal)list.Sum(x => x.opEmpContribution));
                            decimal ose = _mvcApplication.GetNumber((decimal)list.Sum(x => x.opOwnContribution));
                            decimal olo = _mvcApplication.GetNumber(((decimal)(list.Sum(x => x.opLoan) ?? 0)));
                            decimal pro = _mvcApplication.GetNumber(((decimal)(_tbl_ProfitDistributionDetailforOpening.Where(x => x.TransactionDate < fdate).Sum(x => x.DistributedAmount) ?? 0)));
                            decimal openingBalance = se + em + ope + oem + ose - olo + pro;


                            //Added by Fahim 25/10/2015
                            decimal openingSelfContribution = ose + se; //Added by Fahim 25/10/2015
                            decimal openingEmployeeContribution = oem + em; //Added by Fahim 25/10/2015

                            decimal openingSelfProfit = 0;
                            decimal openingEmployeeProfit = 0;
                            decimal selfProfit = 0;
                            decimal employeeProfit = 0;
                            if (openingSelfContribution + openingEmployeeContribution != 0)
                            {
                                openingSelfProfit = ((ope + pro) * openingSelfContribution) / (openingSelfContribution + openingEmployeeContribution);
                                openingEmployeeProfit = ((ope + pro) * openingEmployeeContribution) / (openingEmployeeContribution + openingSelfContribution);
                                selfProfit = pro / 2;
                                employeeProfit = pro / 2;
                            }
                            else
                            {
                                openingSelfProfit = 0;
                                openingEmployeeProfit = 0;
                                selfProfit = 0;
                                employeeProfit = 0;
                            }
                            var joiningDate = Convert.ToDateTime(list.Select(x => x.JoiningDate).FirstOrDefault()).ToShortDateString();
                            var activationDate = Convert.ToDateTime(list.Select(x => x.PFActivationDate).FirstOrDefault()).ToShortDateString();
                            var deactivationDate = Convert.ToDateTime(list.Select(x => x.PFDeactivationDate).FirstOrDefault()).ToShortDateString();
                            if (deactivationDate == "01 Jan 0001")
                            {
                                deactivationDate = "  ";
                            }
                            int[] duration = getDateDiffByDayMonthYear(Convert.ToDateTime(activationDate), tdate);
                            string JobDuration = duration[0] + " years " + duration[1] + " months " + duration[2] + " days";

                            Decimal LoanableAmount = 0;
                            if (empID > 0)
                            {
                                v = v.Where(w => w.EmpID == empID).ToList();
                            }
                            if (duration[0] >= 1)
                            {
                                var s = v.Where(w => w.EmpID == empID).Select(
                                    a => new
                                    {
                                        a.SelfContribution,
                                    }).ToList();
                                LoanableAmount = s.AsEnumerable().Sum(o => o.SelfContribution);
                                LoanableAmount = ((LoanableAmount * 80) / 100);
                            }

                            CultureInfo cInfo = new CultureInfo("en-IN");
                            decimal totalPayable = 0;
                            bool isOwnPartPayable = false;
                            bool isEmpPartPayable = false;
                            bool isOwnProfitPayable = false;
                            bool isEmpProfitPayable = false;
                            if (list.Count() <= 0) continue;

                            var pfDuration = ((tdate == null ? DateTime.Now.Year : tdate.Year - list.FirstOrDefault().PFActivationDate.Year) * 12) + (tdate == null ? DateTime.Now.Month : tdate.Month) - list.FirstOrDefault().PFActivationDate.Month;
                            var getClosingRules = unitOfWork.MembershipClosingRulesRepository.Get().Where(o => o.OCode == OCode && o.PFDurationInMonth <= pfDuration).OrderByDescending(o => o.PFDurationInMonth).ToList();
                            var getClosingRule = getClosingRules.FirstOrDefault();

                            if (getClosingRule != null && getClosingRule.OwnPartPayable > 0)
                            {
                                isOwnPartPayable = true;
                            }
                            if (getClosingRule != null && getClosingRule.EmployerPartPayable > 0)
                            {
                                isEmpPartPayable = true;
                            }
                            if (getClosingRule != null && (getClosingRule.OwnProfitPercent != null && getClosingRule.OwnProfitPercent > 0))
                            {
                                isOwnProfitPayable = true;
                            }
                            if (getClosingRule != null && (getClosingRule.EmpProfitPercent != null && getClosingRule.EmpProfitPercent > 0))
                            {
                                isEmpProfitPayable = true;
                            }


                            var loan = unitOfWork.CustomRepository.GetAmortizationDetailById(empID ?? 0).ToList();
                            var unpaidloan = loan.Where(x => x.Processed != 1).OrderBy(x => x.InstallmentNumber).FirstOrDefault();


                            if (isOwnPartPayable)
                            {
                                totalPayable += ((v.Sum(x => x.SelfContribution) + list.Sum(x => x.opOwnContribution) ?? 0) * getClosingRule.OwnPartPayable) / 100;
                                totalPayable = totalPayable - (unpaidloan == null ? 0 : (unpaidloan.Interest + unpaidloan.Amount));
                            }
                            if (isEmpPartPayable)
                            {
                                totalPayable += ((v.Sum(x => x.EmpContribution) + list.Sum(x => x.opEmpContribution) ?? 0) * getClosingRule.EmployerPartPayable) / 100;
                            }

                            if (isOwnProfitPayable)
                            {
                                totalPayable += ((v.Sum(x => x.SelfProfit) + ((list.Sum(x => x.opProfit) ?? 0 * v.Sum(x => x.SelfContribution)) / (v.Sum(x => x.EmpContribution) + v.Sum(x => x.SelfContribution)))) * getClosingRule.OwnProfitPercent ?? 0) / 100;
                            }

                            if (isEmpProfitPayable)
                            {

                                totalPayable += ((v.Sum(x => x.EmpProfit) + ((list.Sum(x => x.opProfit) ?? 0 * v.Sum(x => x.EmpContribution)) / (v.Sum(x => x.EmpContribution) + v.Sum(x => x.SelfContribution)))) * getClosingRule.OwnProfitPercent ?? 0) / 100;
                            }


                            var getCompany = unitOfWork.CompanyInformationRepository.GetByID(OCode);
                            ReportParameterCollection reportParameters = new ReportParameterCollection();
                            reportParameters.Add(new ReportParameter("rpCompanyName", getCompany.CompanyName + ""));
                            reportParameters.Add(new ReportParameter("rpCompanyAddress", getCompany.CompanyAddress + ""));
                            reportParameters.Add(new ReportParameter("rpMemberName", list.Select(x => x.EmpName).FirstOrDefault() + ""));
                            reportParameters.Add(new ReportParameter("rpDesignation", list.Select(x => x.opDesignationName).FirstOrDefault() + ""));
                            reportParameters.Add(new ReportParameter("rpID", list.Select(x => x.IdentificationNumber).FirstOrDefault() + ""));
                            reportParameters.Add(new ReportParameter("rpOpeningBalance", _mvcApplication.GetNumber(openingBalance).ToString() + ""));
                            reportParameters.Add(new ReportParameter("rpFromDate", fdate + ""));
                            reportParameters.Add(new ReportParameter("rpToDate", tdate + ""));
                            reportParameters.Add(new ReportParameter("rpuserName", "kds"));
                            //Edited by Fahim 25/10/2015
                            reportParameters.Add(new ReportParameter("rpOpeningSelfContribution", _mvcApplication.GetNumber(openingSelfContribution) + ""));
                            reportParameters.Add(new ReportParameter("rpOpeningEmployeeContribution", _mvcApplication.GetNumber(openingEmployeeContribution) + ""));
                            reportParameters.Add(new ReportParameter("rpOpeningSeflProfit", _mvcApplication.GetNumber(openingSelfProfit) + ""));
                            reportParameters.Add(new ReportParameter("rpSelfProfit", _mvcApplication.GetNumber(selfProfit) + ""));
                            reportParameters.Add(new ReportParameter("rpEmployeeProfit", _mvcApplication.GetNumber(employeeProfit) + ""));
                            reportParameters.Add(new ReportParameter("rpOpeningEmployeeProfit", _mvcApplication.GetNumber(openingEmployeeProfit) + ""));
                            reportParameters.Add(new ReportParameter("rpseviceDuration", JobDuration + ""));
                            reportParameters.Add(new ReportParameter("rpLoanableAmount", LoanableAmount + ""));

                            //New Editer By Fahim 01/11/2015

                            reportParameters.Add(new ReportParameter("rpJoiningDate", joiningDate));
                            reportParameters.Add(new ReportParameter("rpActivationDate", activationDate));
                            reportParameters.Add(new ReportParameter("rpDeactivationDate", deactivationDate));

                            //added by asif 17 jan 2017
                            reportParameters.Add(new ReportParameter("rpPayableToEmployee", totalPayable.ToString()));
                            //end asif

                            //End Fahim
                            lr.SetParameters(reportParameters);
                            //End
                        }
                        ReportDataSource rd = new ReportDataSource("DataSet1", v);
            #endregion
                        lr.DataSources.Add(rd);

                        string reportType = "PDF";
                        string mimeType;
                        string encoding;
                        string fileNameExtension;



                        string deviceInfo =

                        "<DeviceInfo>" +
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
                            out warnings
                            );

                        string reportPath = @"~/ReportFile/" + Guid.NewGuid().ToString("N").ToUpper() + ".pdf";
                        string outputPath = HostingEnvironment.MapPath(reportPath);

                        // save the file
                        using (FileStream fs = new FileStream(outputPath, FileMode.Create))
                        {
                            fs.Write(renderedBytes, 0, renderedBytes.Length);
                            fs.Close();
                        }
                        // string email = ConfigurationManager.AppSettings["Receiver"].ToString();

                        EmailSender.SendEmail(outputPath, emp.Name, emp.Email);

                    }
                    catch (Exception ex)
                    {
                        string logMessage = "Message:" + ex.Message + "\n Inner Exception: " + ex.InnerException;
                        //Log(logMessage);
                        TempData["msg"] = logMessage;
                        continue;
                    }
                }

            });
            TempData["msg"] = "Email is sending...";
            task.Start();
            task.Wait();
            return RedirectToAction("Index");
        }
        public int[] getDateDiffByDayMonthYear(DateTime fromDate, DateTime toDate)
        {
            int fYear = fromDate.Year;
            int fMonth = fromDate.Month;
            int fDay = fromDate.Day;

            int tYear = toDate.Year;
            int tMonth = toDate.Month;
            int tDay = toDate.Day;

            int dYear = 0;
            int dMonth = 0;
            int dDay = 0;

            if (fromDate < toDate)
            {
                if (tDay < fDay)
                {
                    tDay += 30;
                    dDay = tDay - fDay;
                    fMonth += 1;

                    if (tMonth < fMonth)
                    {
                        tMonth += 12;
                        dMonth = tMonth - fMonth;
                        fYear += 1;
                        dYear = tYear - fYear;
                    }
                    else if (tMonth >= fMonth)
                    {
                        dMonth = tMonth - fMonth;
                        dYear = tYear - fYear;
                    }
                }
                else if (tDay >= fDay)
                {
                    dDay = tDay - fDay;

                    if (tMonth < fMonth)
                    {
                        tMonth += 12;
                        dMonth = tMonth - fMonth;
                        fYear += 1;
                        dYear = tYear - fYear;
                    }
                    else if (tMonth >= fMonth)
                    {
                        dMonth = tMonth - fMonth;
                        dYear = tYear - fYear;
                    }
                }
            }

            if (dDay == 30)
            {
                dDay = 0;

                dMonth += 1;
            }

            if (dMonth == 12)
            {
                dMonth = 0;

                dYear += 1;
            }

            int[] diff = { dYear, dMonth, dDay };

            return diff;
        }
        public JsonResult SendMailToSelectedEmployee(string empList)
        {
            List<VM_Emp> emps = new List<VM_Emp>();

            var emplist = JsonConvert.DeserializeObject<VM_empList>(empList);


            var result = emplist.list;

            int OCode = ((int?)Session["OCode"]) ?? 0;
            _mvcApplication = new MvcApplication();
            LocalReport lr = new LocalReport();

            DateTime fdate = Convert.ToDateTime("1900-01-10");
            DateTime tdate = DateTime.Now;

            List<VM_Email> emailModel = new List<VM_Email>();
            List<VM_Employee> employeeList = new List<VM_Employee>();


            foreach (var emp in result)
            {
                VM_Employee employee = unitOfWork.EmployeesRepository.Get().Where(e => e.IdentificationNumber == emp.EmpId)
                                       .Select(s => new VM_Employee { EmpID = s.EmpID, EmpName = s.EmpName, Email = s.Email }).FirstOrDefault();
                employeeList.Add(employee);
            }

            #region _member_contribution_report
            string path = Path.Combine(Server.MapPath("~/Reporting/PF"), "PFMemberContribution.rdlc");
            if (System.IO.File.Exists(path))
            {
                lr.ReportPath = path;
            }
            else
            {
                return Json("Report path not found", JsonRequestBehavior.AllowGet);
            }
            Task task = new Task(() =>
            {

                foreach (var emp in employeeList)
                {
                    try
                    {

                        List<VM_PFMonthlyStatus> v = new List<VM_PFMonthlyStatus>();
                        using (unitOfWork = new UnitOfWork())
                        {
                            int? empID = emp.EmpID;
                            var temp = unitOfWork.CustomRepository.EmpPFMonthlyStatusForReport(OCode, empID ?? 0, fdate, tdate).OrderBy(x => x.ContrebutionDate).ToList();
                            List<tbl_ProfitDistributionDetail> _tbl_ProfitDistributionDetail = unitOfWork.CustomRepository.GetProfitDistributionByEmpId(Convert.ToInt16(empID)).Where(x => x.TransactionDate >= fdate && x.TransactionDate <= tdate).ToList();
                            foreach (VM_PFMonthlyStatus item in temp)
                            {
                                VM_PFMonthlyStatus atbl_ProfitDistributionDetail = new VM_PFMonthlyStatus();
                                atbl_ProfitDistributionDetail.EmpID = item.EmpID;
                                atbl_ProfitDistributionDetail.IdentificationNumber = item.IdentificationNumber;
                                atbl_ProfitDistributionDetail.ProcessRunDate = item.ProcessRunDate;
                                atbl_ProfitDistributionDetail.ContrebutionDate = item.ContrebutionDate;
                                atbl_ProfitDistributionDetail.EmpContribution = _mvcApplication.GetNumber(item.EmpContribution);
                                atbl_ProfitDistributionDetail.SelfContribution = _mvcApplication.GetNumber(item.SelfContribution);
                                atbl_ProfitDistributionDetail.Year = item.Year;
                                atbl_ProfitDistributionDetail.Month = item.Month;
                                atbl_ProfitDistributionDetail.SelfProfit = _mvcApplication.GetNumber(Convert.ToDecimal(_tbl_ProfitDistributionDetail.Where(x => x.TransactionDate.Value.Month == Convert.ToInt32(item.Month) && x.TransactionDate.Value.Year == Convert.ToInt32(item.Year)).Select(x => x.SelfProfit).FirstOrDefault()));
                                atbl_ProfitDistributionDetail.EmpProfit = _mvcApplication.GetNumber(Convert.ToDecimal(_tbl_ProfitDistributionDetail.Where(x => x.TransactionDate.Value.Month == Convert.ToInt32(item.Month) && x.TransactionDate.Value.Year == Convert.ToInt32(item.Year)).Select(x => x.EmpProfit).FirstOrDefault()));
                                v.Add(atbl_ProfitDistributionDetail);
                            }

                            //v = temp.Where(x => x.ProcessRunDate >= fdate && x.ProcessRunDate <= tdate).ToList();
                            List<tbl_Employees> list = unitOfWork.CustomRepository.GetEmployeeById(v.Where(x => x.EmpID != null).Select(x => x.EmpID).FirstOrDefault());
                            var opening = unitOfWork.CustomRepository.EmpPFMonthlyStatusForReport(OCode, empID ?? 0, DateTime.MinValue, fdate).OrderBy(x => x.ProcessRunDate).ToList();
                            List<tbl_ProfitDistributionDetail> _tbl_ProfitDistributionDetailforOpening = unitOfWork.CustomRepository.GetProfitDistributionByEmpId(Convert.ToInt16(empID)).Where(x => x.TransactionDate <= fdate).ToList();
                            decimal se = _mvcApplication.GetNumber((decimal)(opening.Where(x => x.ContrebutionDate <= fdate) == null ? 0 : opening.Where(x => x.ContrebutionDate <= fdate).Sum(x => x.SelfContribution)));
                            decimal em = _mvcApplication.GetNumber((decimal)(opening.Where(x => x.ContrebutionDate <= fdate) == null ? 0 : opening.Where(x => x.ContrebutionDate <= fdate).Sum(x => x.EmpContribution)));
                            decimal ope = _mvcApplication.GetNumber((decimal)list.Sum(x => x.opProfit));
                            decimal oem = _mvcApplication.GetNumber((decimal)list.Sum(x => x.opEmpContribution));
                            decimal ose = _mvcApplication.GetNumber((decimal)list.Sum(x => x.opOwnContribution));
                            decimal olo = _mvcApplication.GetNumber(((decimal)(list.Sum(x => x.opLoan) ?? 0)));
                            decimal pro = _mvcApplication.GetNumber(((decimal)(_tbl_ProfitDistributionDetailforOpening.Where(x => x.TransactionDate < fdate).Sum(x => x.DistributedAmount) ?? 0)));
                            decimal openingBalance = se + em + ope + oem + ose - olo + pro;


                            //Added by Fahim 25/10/2015
                            decimal openingSelfContribution = ose + se; //Added by Fahim 25/10/2015
                            decimal openingEmployeeContribution = oem + em; //Added by Fahim 25/10/2015

                            decimal openingSelfProfit = 0;
                            decimal openingEmployeeProfit = 0;
                            decimal selfProfit = 0;
                            decimal employeeProfit = 0;
                            if (openingSelfContribution + openingEmployeeContribution != 0)
                            {
                                openingSelfProfit = ((ope + pro) * openingSelfContribution) / (openingSelfContribution + openingEmployeeContribution);
                                openingEmployeeProfit = ((ope + pro) * openingEmployeeContribution) / (openingEmployeeContribution + openingSelfContribution);
                                selfProfit = pro / 2;
                                employeeProfit = pro / 2;
                            }
                            else
                            {
                                openingSelfProfit = 0;
                                openingEmployeeProfit = 0;
                                selfProfit = 0;
                                employeeProfit = 0;
                            }
                            var joiningDate = Convert.ToDateTime(list.Select(x => x.JoiningDate).FirstOrDefault()).ToShortDateString();
                            var activationDate = Convert.ToDateTime(list.Select(x => x.PFActivationDate).FirstOrDefault()).ToShortDateString();
                            var deactivationDate = Convert.ToDateTime(list.Select(x => x.PFDeactivationDate).FirstOrDefault()).ToShortDateString();
                            if (deactivationDate == "01 Jan 0001")
                            {
                                deactivationDate = "  ";
                            }
                            int[] duration = getDateDiffByDayMonthYear(Convert.ToDateTime(activationDate), tdate);
                            string JobDuration = duration[0] + " years " + duration[1] + " months " + duration[2] + " days";

                            Decimal LoanableAmount = 0;
                            if (empID > 0)
                            {
                                v = v.Where(w => w.EmpID == empID).ToList();
                            }
                            if (duration[0] >= 1)
                            {
                                var s = v.Where(w => w.EmpID == empID).Select(
                                    a => new
                                    {
                                        a.SelfContribution,
                                    }).ToList();
                                LoanableAmount = s.AsEnumerable().Sum(o => o.SelfContribution);
                                LoanableAmount = ((LoanableAmount * 80) / 100);
                            }

                            CultureInfo cInfo = new CultureInfo("en-IN");
                            decimal totalPayable = 0;
                            bool isOwnPartPayable = false;
                            bool isEmpPartPayable = false;
                            bool isOwnProfitPayable = false;
                            bool isEmpProfitPayable = false;
                            if (list.Count() <= 0) continue;

                            var pfDuration = ((tdate == null ? DateTime.Now.Year : tdate.Year - list.FirstOrDefault().PFActivationDate.Year) * 12) + (tdate == null ? DateTime.Now.Month : tdate.Month) - list.FirstOrDefault().PFActivationDate.Month;
                            var getClosingRules = unitOfWork.MembershipClosingRulesRepository.Get().Where(o => o.OCode == OCode && o.PFDurationInMonth <= pfDuration).OrderByDescending(o => o.PFDurationInMonth).ToList();
                            var getClosingRule = getClosingRules.FirstOrDefault();

                            if (getClosingRule != null && getClosingRule.OwnPartPayable > 0)
                            {
                                isOwnPartPayable = true;
                            }
                            if (getClosingRule != null && getClosingRule.EmployerPartPayable > 0)
                            {
                                isEmpPartPayable = true;
                            }
                            if (getClosingRule != null && (getClosingRule.OwnProfitPercent != null && getClosingRule.OwnProfitPercent > 0))
                            {
                                isOwnProfitPayable = true;
                            }
                            if (getClosingRule != null && (getClosingRule.EmpProfitPercent != null && getClosingRule.EmpProfitPercent > 0))
                            {
                                isEmpProfitPayable = true;
                            }


                            var loan = unitOfWork.CustomRepository.GetAmortizationDetailById(empID ?? 0).ToList();
                            var unpaidloan = loan.Where(x => x.Processed != 1).OrderBy(x => x.InstallmentNumber).FirstOrDefault();


                            if (isOwnPartPayable)
                            {
                                totalPayable += ((v.Sum(x => x.SelfContribution) + list.Sum(x => x.opOwnContribution) ?? 0) * getClosingRule.OwnPartPayable) / 100;
                                totalPayable = totalPayable - (unpaidloan == null ? 0 : (unpaidloan.Interest + unpaidloan.Amount));
                            }
                            if (isEmpPartPayable)
                            {
                                totalPayable += ((v.Sum(x => x.EmpContribution) + list.Sum(x => x.opEmpContribution) ?? 0) * getClosingRule.EmployerPartPayable) / 100;
                            }

                            if (isOwnProfitPayable)
                            {
                                totalPayable += ((v.Sum(x => x.SelfProfit) + ((list.Sum(x => x.opProfit) ?? 0 * v.Sum(x => x.SelfContribution)) / (v.Sum(x => x.EmpContribution) + v.Sum(x => x.SelfContribution)))) * getClosingRule.OwnProfitPercent ?? 0) / 100;
                            }

                            if (isEmpProfitPayable)
                            {

                                totalPayable += ((v.Sum(x => x.EmpProfit) + ((list.Sum(x => x.opProfit) ?? 0 * v.Sum(x => x.EmpContribution)) / (v.Sum(x => x.EmpContribution) + v.Sum(x => x.SelfContribution)))) * getClosingRule.OwnProfitPercent ?? 0) / 100;
                            }


                            var getCompany = unitOfWork.CompanyInformationRepository.GetByID(OCode);
                            ReportParameterCollection reportParameters = new ReportParameterCollection();
                            reportParameters.Add(new ReportParameter("rpCompanyName", getCompany.CompanyName + ""));
                            reportParameters.Add(new ReportParameter("rpCompanyAddress", getCompany.CompanyAddress + ""));
                            reportParameters.Add(new ReportParameter("rpMemberName", list.Select(x => x.EmpName).FirstOrDefault() + ""));
                            reportParameters.Add(new ReportParameter("rpDesignation", list.Select(x => x.opDesignationName).FirstOrDefault() + ""));
                            reportParameters.Add(new ReportParameter("rpID", list.Select(x => x.IdentificationNumber).FirstOrDefault() + ""));
                            reportParameters.Add(new ReportParameter("rpOpeningBalance", _mvcApplication.GetNumber(openingBalance).ToString() + ""));
                            reportParameters.Add(new ReportParameter("rpFromDate", fdate + ""));
                            reportParameters.Add(new ReportParameter("rpToDate", tdate + ""));
                            reportParameters.Add(new ReportParameter("rpuserName", "kds"));
                            //Edited by Fahim 25/10/2015
                            reportParameters.Add(new ReportParameter("rpOpeningSelfContribution", _mvcApplication.GetNumber(openingSelfContribution) + ""));
                            reportParameters.Add(new ReportParameter("rpOpeningEmployeeContribution", _mvcApplication.GetNumber(openingEmployeeContribution) + ""));
                            reportParameters.Add(new ReportParameter("rpOpeningSeflProfit", _mvcApplication.GetNumber(openingSelfProfit) + ""));
                            reportParameters.Add(new ReportParameter("rpSelfProfit", _mvcApplication.GetNumber(selfProfit) + ""));
                            reportParameters.Add(new ReportParameter("rpEmployeeProfit", _mvcApplication.GetNumber(employeeProfit) + ""));
                            reportParameters.Add(new ReportParameter("rpOpeningEmployeeProfit", _mvcApplication.GetNumber(openingEmployeeProfit) + ""));
                            reportParameters.Add(new ReportParameter("rpseviceDuration", JobDuration + ""));
                            reportParameters.Add(new ReportParameter("rpLoanableAmount", LoanableAmount + ""));

                            //New Editer By Fahim 01/11/2015

                            reportParameters.Add(new ReportParameter("rpJoiningDate", joiningDate));
                            reportParameters.Add(new ReportParameter("rpActivationDate", activationDate));
                            reportParameters.Add(new ReportParameter("rpDeactivationDate", deactivationDate));

                            //added by asif 17 jan 2017
                            reportParameters.Add(new ReportParameter("rpPayableToEmployee", totalPayable.ToString()));
                            //end asif

                            //End Fahim
                            lr.SetParameters(reportParameters);
                            //End
                        }
                        ReportDataSource rd = new ReportDataSource("DataSet1", v);
            #endregion
                        lr.DataSources.Add(rd);

                        string reportType = "PDF";
                        string mimeType;
                        string encoding;
                        string fileNameExtension;



                        string deviceInfo =

                        "<DeviceInfo>" +
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
                            out warnings
                            );

                        string reportPath = @"~/ReportFile/" + Guid.NewGuid().ToString("N").ToUpper() + ".pdf";
                        string outputPath = HostingEnvironment.MapPath(reportPath);

                        // save the file
                        using (FileStream fs = new FileStream(outputPath, FileMode.Create))
                        {
                            fs.Write(renderedBytes, 0, renderedBytes.Length);
                            fs.Close();
                        }
                        // string email = ConfigurationManager.AppSettings["Receiver"].ToString();

                        EmailSender.SendEmail(outputPath, emp.EmpName, emp.Email);

                    }
                    catch (Exception ex)
                    {
                        string logMessage = "Message:" + ex.Message + "\n Inner Exception: " + ex.InnerException;
                        //Log(logMessage);
                        TempData["msg"] = logMessage;
                        continue;
                    }
                }

            });
            TempData["msg"] = "Email has been Sent!!!";
            task.Start();
            task.Wait();
            return Json("Mails are sent", JsonRequestBehavior.AllowGet);
        }

        public void Log(string logMessage)
        {
            try
            {
                string outputPath = HostingEnvironment.MapPath(@"~/Log.txt");
                using (StreamWriter w = System.IO.File.AppendText(outputPath))
                {
                    w.Write("\nLog Entry : ");
                    w.Write("{0} {1}", DateTime.Now.ToLongTimeString(),
                        DateTime.Now.ToLongDateString());
                    w.WriteLine("  :{0}", logMessage);
                    w.WriteLine("-------------------------------");
                }
            }
            catch (Exception ex) { throw ex; }
        }
        
    }
}
