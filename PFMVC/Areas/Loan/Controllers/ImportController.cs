using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using DLL.Repository;
using DLL.ViewModel;
using PFMVC.common;
using DLL;
using System.Globalization;

namespace PFMVC.Areas.Loan.Controllers
{
    public class ImportController : Controller
    {
        MvcApplication _MvcApplication;
        int PageID = 7;
        private UnitOfWork unitOfWork = new UnitOfWork();
        OleDbConnection excelConnection;
        DataSet dataset = new DataSet();
        string errorMessage = "";


        /// <summary>
        /// Indexes this instance.
        /// </summary>
        /// <returns></returns>
        /// <ModifiedBy>Avishek</ModifiedBy>
        /// <DateofModification>Feb-24-2016</DateofModification>
        [Authorize]
        public ActionResult Index()
        {
            //Added By Avishek Date:Jan-19-2016
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End
            RecentUploaded();
            ViewBag.PageName = "Import Loan Payment";

            bool b = PagePermission.HasPermission(User.Identity.Name, PageID, 3);
            if (b)
            {
                ViewBag.ErrorMessage = errorMessage;
                return View();
            }
            ViewBag.PageName = "Import Loan Payment";
            return View("Unauthorized");
        }

        /// <summary>
        /// Imports the excel.
        /// </summary>
        /// <returns></returns>
        /// <ReviewBy>Avishek</ReviewBy>
        /// <ReviewDate>Feb-24-2016</ReviewDate>
        public ActionResult ImportExcel() //HttpPostedFileBase ExcelFile
        {

            #region check excel file validity
            //Added By Avishek Date:Jan-19-2016
            int OCode = ((int?)Session["OCode"]) ?? 0;
            if (OCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End
            try
            {
                if (Request.Files["FileUpload1"].ContentType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                }
                else
                {
                    errorMessage = "Please upload valid excel file only...Your uploaded file type " + Request.Files["FileUpload1"].ContentType + "";
                    return RedirectToAction("Index");
                }
            }
            catch
            {
                errorMessage = "Please upload valid excel file only...";
                return RedirectToAction("Index");
            }
            #endregion

            if (Request.Files["FileUpload1"].ContentLength > 0)
            {

                try
                {
                    string fileName = Request.Files["FileUpload1"].FileName;
                    fileName = fileName.Remove(fileName.LastIndexOf('.'));
                    int process_number = GetCurrentAmortizationProcessNumber();
                    fileName = process_number + "_" + fileName;
                    ViewBag.ProcessNumber = process_number;
                    string extension = System.IO.Path.GetExtension(Request.Files["FileUpload1"].FileName);
                    string path1 = string.Format("{0}/{1}", Server.MapPath("~/ImportedExcel/LoanPayment"), fileName + extension);
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

                    cmd.CommandText = "Select [Identification No], [Loan ID], [Payment Amount], [Payment Year], [Payment Month], [Payment Date]   from [Sheet1$]";
                    //cmd.CommandText = "Select [Identification No], [Loan ID], [Installment Amount],[Interest], [Payment Year], [Payment Month], [Payment Date]   from [Sheet1$]";


                    oleda = new OleDbDataAdapter(cmd);
                    oleda.Fill(dataset, "LoanPayment");
                }
                catch (Exception x)
                {
                    errorMessage = x.Message;
                    return RedirectToAction("Index", new { MessageToCarry = errorMessage });
                }
                finally
                {
                    OleDbConnection.ReleaseObjectPool();
                    excelConnection.Close();
                    excelConnection.Dispose();
                }

                try
                {
                    DataTable dt = dataset.Tables["LoanPayment"];

                    var query = dt.AsEnumerable().Select(s => new
                    {
                        EmpIdentificationNumber = s.Field<string>("Identification No"), // excel empid id identification number actually
                        LoanID = s.Field<object>("Loan ID"),
                        PaymentAmount = s.Field<object>("Payment Amount"),
                        //PaymentAmount = s.Field<object>("Installment Amount"),
                        //Interest = s.Field<object>("Interest"),

                        PaymentYear = s.Field<object>("Payment Year"),
                        PaymentMonth = s.Field<object>("Payment Month").ToString(),
                        PaymentDate = s.Field<DateTime>("Payment Date")
                    });

                    double u = query.Count();
                    bool flag = true;
                    VM_PFLoanPayment vm_pfLoanPayment;
                    List<VM_PFLoanPayment> lst_vm_pfLoanPayment = new List<VM_PFLoanPayment>();
                    foreach (var item in query)
                    {

                        vm_pfLoanPayment = new VM_PFLoanPayment();
                        vm_pfLoanPayment.IdentificationNumber = item.EmpIdentificationNumber; //look, here employee id is employee identification number actually
                        vm_pfLoanPayment.LoanID = item.LoanID.ToString();
                        vm_pfLoanPayment.PaymentDate = Convert.ToDateTime(item.PaymentDate);
                        vm_pfLoanPayment.ConYear = item.PaymentYear.ToString();
                        vm_pfLoanPayment.ConMonth = item.PaymentMonth.ToString();

                        tbl_Employees _tbl_Employees = unitOfWork.CustomRepository.GetEmployeeByIdentificationNo(item.EmpIdentificationNumber).FirstOrDefault();
                        string month = "";
                        if (item.PaymentMonth.Length == 1)
                        {
                            month = '0' + item.PaymentMonth;
                        }
                        else
                        {
                            month = item.PaymentMonth;
                        }

                        tbl_PFL_Amortization _tbl_PFL_Amortization = unitOfWork.CustomRepository.GetPaymentofEmployee(Convert.ToInt32(_tbl_Employees.EmpID), (string)item.LoanID).Where(x => x.Processed != 1).OrderBy(x => x.InstallmentNumber).FirstOrDefault();

                        try
                        {
                            _MvcApplication = new MvcApplication();
                            //if (_tbl_PFL_Amortization != null && _tbl_PFL_Amortization.Processed != 1 && _tbl_PFL_Amortization.ConYear == vm_pfLoanPayment.ConYear && _tbl_PFL_Amortization.ConMonth == month)
                                if (_tbl_PFL_Amortization != null && _tbl_PFL_Amortization.Processed != 1 && _tbl_PFL_Amortization.ConYear == vm_pfLoanPayment.ConYear)

                            {
                                //decimal payment = _MvcApplication.GetNumber(Convert.ToDecimal(item.PaymentAmount));
                                decimal payment = _MvcApplication.GetNumber(Convert.ToDecimal(item.PaymentAmount));
                                decimal interest = _MvcApplication.GetNumber(Convert.ToDecimal(_tbl_PFL_Amortization.Interest));
                                decimal principal = _MvcApplication.GetNumber(Convert.ToDecimal(_tbl_PFL_Amortization.Principal));

                                
                                decimal tableData = _MvcApplication.GetNumber(Convert.ToDecimal(_tbl_PFL_Amortization.Principal + _tbl_PFL_Amortization.Interest));
                                if (tableData == payment)
                                //if (tableData == payment + interest)

                                {
                                    vm_pfLoanPayment.PaymentAmount = _MvcApplication.GetNumber(Convert.ToDecimal(item.PaymentAmount));
                                    vm_pfLoanPayment.Interest = _MvcApplication.GetNumber(Convert.ToDecimal(_tbl_PFL_Amortization.Interest));
                                    vm_pfLoanPayment.PrincipalAmount = _MvcApplication.GetNumber(Convert.ToDecimal(_tbl_PFL_Amortization.Principal)); // Added By Kamrul
                                    // Added By Kamrul

                                }
                                else
                                {
                                    vm_pfLoanPayment.PaymentAmount = _MvcApplication.GetNumber(Convert.ToDecimal(item.PaymentAmount));
                                    vm_pfLoanPayment.Interest = _MvcApplication.GetNumber(Convert.ToDecimal(_tbl_PFL_Amortization.Interest));
                                    vm_pfLoanPayment.PrincipalAmount = _MvcApplication.GetNumber(Convert.ToDecimal(_tbl_PFL_Amortization.Principal));
                                    //vm_pfLoanPayment.Interest = _MvcApplication.GetNumber(Convert.ToDecimal(item.Interest));
                                    vm_pfLoanPayment.PreImportMessage += "Payment amount is not match with installment amount";
                                }
                            }
                            else
                            {
                                vm_pfLoanPayment.PreImportMessage += "Loan amount is already paid";
                            }
                        }
                        catch
                        {
                            vm_pfLoanPayment.PreImportMessage += "Payment not in correct format";
                            flag = false;
                        }

                        lst_vm_pfLoanPayment.Add(vm_pfLoanPayment);
                    }

                    if (flag)
                    {
                        ViewBag.Message = "Files contain no error and can uploaded safely...";
                        return View("LoanPaymentNoError", lst_vm_pfLoanPayment);
                    }
                    else
                    {
                        ViewBag.Message = "You must solve the following error to save these data...";
                        return View("LoanPaymentErrorFound", lst_vm_pfLoanPayment);
                    }
                }
                catch (Exception x)
                {
                    errorMessage = x.Message;
                    return RedirectToAction("Index", new { MessageToCarry = errorMessage });
                }

            }
            errorMessage = "No file uploaded...";
            return RedirectToAction("Index", new { MessageToCarry = errorMessage });
        }


        /// <summary>
        /// Imports the excel save.
        /// </summary>
        /// <param name="lst_vm_pfLoanPayment">The LST_VM_PF loan payment.</param>
        /// <param name="process_number">The process_number.</param>
        /// <returns>bool</returns>
        /// <CreatedBy>Avishek</CreatedBy>
        /// <ReviewBy>Avishek</ReviewBy>
        /// <ReviewDate>Feb-24-2016</ReviewDate>
        [HttpPost]
        public ActionResult ImportExcelSave(List<VM_PFLoanPayment> lst_vm_pfLoanPayment, int process_number = 0)
        {
            //Added By Avishek Date:Jan-19-2016
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            //End
            //process number should not be less than 1.
            if (process_number < 1)
            {
                return Json(new { Success = false, ErrorMessage = "Invalid process number! try again..." }, JsonRequestBehavior.DenyGet);
            }
            try
            {
                _MvcApplication = new MvcApplication();
                foreach (var item in lst_vm_pfLoanPayment)
                {
                    tbl_Employees tblEmployees = unitOfWork.CustomRepository.GetEmployeeByIdentificationNo(item.IdentificationNumber).FirstOrDefault();
                    List<tbl_PFL_Amortization> empLoanPayment = unitOfWork.CustomRepository.GetPaymentofEmployee(tblEmployees.EmpID, item.LoanID).ToList();
                    string month = "";
                    if (item.ConMonth.Length == 1)
                    {
                        month = '0' + item.ConMonth;
                    }
                    else
                    {
                        month = item.ConMonth;
                    }
                    {
                        tbl_PFL_Amortization tblPflAmortization = empLoanPayment.FirstOrDefault(x => x.Processed != 1 && Convert.ToDouble(x.ConMonth) <= Convert.ToDouble(item.ConMonth) && Convert.ToDouble(x.ConYear) <= Convert.ToDouble(item.ConYear));
                        //if (tblPflAmortization != null && (empLoanPayment.Where(x => x.Processed != 1).Select(x => x.Balance).LastOrDefault() != null && empLoanPayment.Where(x => x.Processed != 1).Select(x => x.Balance).LastOrDefault() >= 0) && _MvcApplication.GetNumber(tblPflAmortization.Principal + tblPflAmortization.Interest) == item.PaymentAmount)//Modified By Avishek Date:May-17-2015, Installmant amount & Payment amount check in save

                            if (tblPflAmortization != null && (empLoanPayment.Where(x => x.Processed != 1).Select(x => x.Balance).LastOrDefault() != null && empLoanPayment.Where(x => x.Processed != 1).Select(x => x.Balance).LastOrDefault() >= 0) )
                            {
                            var emp_loan_payment = empLoanPayment.FirstOrDefault(x => x.ConMonth == month && x.ConYear == item.ConYear);
                            emp_loan_payment.Processed = 1;
                            emp_loan_payment.PFLoanID = item.LoanID;
                            emp_loan_payment.PaymentDate = item.PaymentDate;
                            emp_loan_payment.ConMonth = item.ConMonth;
                            emp_loan_payment.ConYear = item.ConYear;
                            emp_loan_payment.ProcessNumber = process_number;
                            emp_loan_payment.PaymentAmount = _MvcApplication.GetNumber(item.PrincipalAmount + item.Interest);
                            emp_loan_payment.PaidAmount = _MvcApplication.GetNumber(item.PrincipalAmount + item.Interest);
                            emp_loan_payment.OCode = oCode;
                            unitOfWork.Save();
                            tbl_PFLoan tblPfLoan = unitOfWork.CustomRepository.EmpLoanInfo(emp_loan_payment.EmpID, item.LoanID).FirstOrDefault();
                            if (tblPfLoan != null)
                                LoanVoucherPass(emp_loan_payment, tblPfLoan.CashLedgerID, "LoanSettlement");
                        }
                    }
                }
                return Json(new { Success = true, Message = "Data saved successfully!" }, JsonRequestBehavior.DenyGet);
            }
            catch (Exception x)
            {
                return Json(new { Success = false, ErrorMessage = "Check error : " + x.Message }, JsonRequestBehavior.DenyGet);
            }
        }

        /// <summary>
        /// Autocompletes the suggestions.
        /// </summary>
        /// <param name="term">The term.</param>
        /// <returns>list</returns>
        /// <CreatedBy>Avishek</CreatedBy>
        /// <createdDate>May-17-2015</createdDate>
        private void LoanVoucherPass(tbl_PFL_Amortization emp_loan_payment, Guid? cashLedgerID, string Status)
        {
            _MvcApplication = new MvcApplication();
            int oCode = ((int?)Session["OCode"]) ?? 0;
            int voucherId = 0;
            //List<string> LedgerNameList = new List<string>();
            List<Guid> ledgerIdList = new List<Guid>();
            List<decimal> credit = new List<decimal>();
            List<decimal> debit = new List<decimal>();
            List<string> chqNumber = new List<string>();
            List<string> pfLoanId = new List<string>();
            List<string> pfMemberId = new List<string>();
            string refMessage = "";
            //string ledgerName = unitOfWork.ACC_LedgerRepository.GetByID(cashLedgerID).LedgerName;

            List<acc_Chart_of_Account_Maping> accChartOfAccountMaping = unitOfWork.ChartofAccountMapingRepository.Get().ToList();
            ledgerIdList.Add(accChartOfAccountMaping.Where(x => x.MIS_Id == 2 && x.OCode == oCode).Select(x => x.Ledger_Id).FirstOrDefault());
            debit.Add(_MvcApplication.GetNumber(emp_loan_payment.Interest));
            credit.Add(0);
            chqNumber.Add("");
            pfMemberId.Add(emp_loan_payment.EmpID + "");
            pfLoanId.Add(emp_loan_payment.PFLoanID);

            //LedgerNameList.Add("Member Loan Interest");
            ledgerIdList.Add(accChartOfAccountMaping.Where(x => x.MIS_Id == 14 && x.OCode == oCode).Select(x => x.Ledger_Id).FirstOrDefault());
            credit.Add(_MvcApplication.GetNumber(emp_loan_payment.Interest));
            debit.Add(0);
            chqNumber.Add("");
            pfMemberId.Add(emp_loan_payment.EmpID + "");
            pfLoanId.Add(emp_loan_payment.PFLoanID);

            //bool isOperationSuccess = unitOfWork.AccountingRepository.DualEntryVoucher(emp_loan_payment.EmpID, 7, emp_loan_payment.EditDate, ref voucherID, "Loan installment ", LedgerNameList, Debit, Credit, ChqNumber, ref refMessage, User.Identity.Name, unitOfWork.CustomRepository.GetUserID(User.Identity.Name), PFMemberID, "Loan installment", "", "", null, PFLoanID, OCode, "Loan installment");
            bool isOperationSuccess = unitOfWork.AccountingRepository.DualEntryVoucherById(emp_loan_payment.EmpID, 5, Convert.ToDateTime(emp_loan_payment.PaymentDate), ref voucherId, "Loan installment ", ledgerIdList, debit, credit, chqNumber, ref refMessage, User.Identity.Name, unitOfWork.CustomRepository.GetUserID(User.Identity.Name), pfMemberId, "Loan installment", "", "", null, pfLoanId, oCode, "Loan installment");
            // Convert.ToDateTime(emp_loan_payment.PaymentDate)  will be replaced by DateTime.Now after Back Dated entry Date:Feb-24-2016
           
            if (isOperationSuccess)
            {
                ledgerIdList = new List<Guid>();
                credit = new List<decimal>();
                debit = new List<decimal>();
                chqNumber = new List<string>();
                pfLoanId = new List<string>();
                pfMemberId = new List<string>();
                voucherId = 0;

                //LedgerIdList.Add("Company Current Account");
                ledgerIdList.Add(accChartOfAccountMaping.Where(x => x.MIS_Id == 2 && x.OCode == oCode).Select(x => x.Ledger_Id).FirstOrDefault());
                debit.Add(Status == "LoanSettlement"
                    ? _MvcApplication.GetNumber(Convert.ToDecimal(emp_loan_payment.Principal))
                    : _MvcApplication.GetNumber(Convert.ToDecimal(emp_loan_payment.PaymentAmount-emp_loan_payment.Interest)));
                credit.Add(0);
                chqNumber.Add("");
                pfMemberId.Add(emp_loan_payment.EmpID + "");
                pfLoanId.Add(emp_loan_payment.PFLoanID);
                
                //
                //if (cashLedgerID != null)
                //{
                //    ledgerIdList.Add((Guid)cashLedgerID);
                //}
                //else
                //{
                //    ledgerIdList.Add(
                //        accChartOfAccountMaping.Where(x => x.MIS_Id == 11 && x.OCode == oCode)
                //            .Select(x => x.Ledger_Id)
                //            .FirstOrDefault());
                //}
                //debit.Add(Status == "LoanSettlement"
                //    ? _MvcApplication.GetNumber(Convert.ToDecimal(emp_loan_payment.Amount + emp_loan_payment.Interest))
                //    : _MvcApplication.GetNumber(Convert.ToDecimal(emp_loan_payment.PaymentAmount)));
                //credit.Add(0);
                //chqNumber.Add("");
                //pfMemberId.Add(emp_loan_payment.EmpID + "");
                //pfLoanId.Add(emp_loan_payment.PFLoanID);

                //LedgerNameList.Add("Member Loan");
                ledgerIdList.Add(accChartOfAccountMaping.Where(x => x.MIS_Id == 5 && x.OCode == oCode).Select(x => x.Ledger_Id).FirstOrDefault());
                credit.Add(Status == "LoanSettlement"
                    ? _MvcApplication.GetNumber(Convert.ToDecimal(emp_loan_payment.Principal))
                    : _MvcApplication.GetNumber(Convert.ToDecimal(emp_loan_payment.PaymentAmount - emp_loan_payment.Interest)));
                debit.Add(0);
                chqNumber.Add("");
                pfMemberId.Add(emp_loan_payment.EmpID + "");
                pfLoanId.Add(emp_loan_payment.PFLoanID);

                //bool isSuccess = unitOfWork.AccountingRepository.DualEntryVoucher(emp_loan_payment.EmpID, 7, DateTime.Now, ref voucherID, "Loan adjustment ", LedgerNameList, Debit, Credit, ChqNumber, ref refMessage, User.Identity.Name, unitOfWork.CustomRepository.GetUserID(User.Identity.Name), PFMemberID, "Loan adjustment", "", "", null, PFLoanID, OCode, "Loan adjustment");
                unitOfWork.AccountingRepository.DualEntryVoucherById(emp_loan_payment.EmpID, 7, Convert.ToDateTime(emp_loan_payment.PaymentDate), ref voucherId, "Loan adjustment ", ledgerIdList, debit, credit, chqNumber, ref refMessage, User.Identity.Name, unitOfWork.CustomRepository.GetUserID(User.Identity.Name), pfMemberId, "Loan adjustment", "", "", null, pfLoanId, oCode, "Loan adjustment");
                // Convert.ToDateTime(emp_loan_payment.PaymentDate)  will be replaced by DateTime.Now after Back Dated entry Date:Feb-24-2016
            }
        }

        public int GetCurrentAmortizationProcessNumber()
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            int n = unitOfWork.AmortizationRepository.Get().Where(p => p.ProcessNumber > 0 && p.OCode == oCode).Max(m => m.ProcessNumber) ?? 0;
            return n + 1;
        }


        public void RecentUploaded()
        {
            string[] excelFiles = Directory.GetFiles(Server.MapPath("~/ImportedExcel/LoanPayment"), "*.xlsx")
                                     .Select(path => Path.GetFileName(path))
                                     .ToArray();
            ViewBag.RecentUploaded = excelFiles;
        }

        public ActionResult RollbackProcess(string FileName)
        {
            int processNumber = 0;
            try
            {
                processNumber = Convert.ToInt32(FileName.Remove(FileName.IndexOf('_')));
            }
            catch
            {
                return View("Index");
            }
            var v = unitOfWork.AmortizationRepository.Get().Where(w => w.ProcessNumber == processNumber).ToList();
            List<VM_PFLoanPayment> lstVmPfLoanPayment = new List<VM_PFLoanPayment>();
            foreach (var item in v)
            {
                VM_PFLoanPayment vmPfLoanPayment = new VM_PFLoanPayment();
                vmPfLoanPayment.EmployeeID = item.EmpID;
                vmPfLoanPayment.LoanID = item.PFLoanID;
                vmPfLoanPayment.PaymentAmount = item.PaymentAmount ?? 0;
                vmPfLoanPayment.InstallmentNumber = item.InstallmentNumber;
                vmPfLoanPayment.PaymentDate = item.PaymentDate ?? DateTime.MaxValue;
                lstVmPfLoanPayment.Add(vmPfLoanPayment);
            }

            ViewBag.FileName = FileName;
            ViewBag.Message = "Would you like to delete the following records (very careful...) ???";
            return View("Rollback", lstVmPfLoanPayment);
        }

        [HttpPost]
        public ActionResult RollbackConfirm(string FileName)
        {
            int processNumber = 0;
            try
            {
                processNumber = Convert.ToInt32(FileName.Remove(FileName.IndexOf('_')));
            }
            catch
            {
                return Json(new { Success = false, ErrorMessage = "Invalid process number! try again..." }, JsonRequestBehavior.DenyGet);
            }
            if (processNumber < 1)
            {
                return Json(new { Success = false, ErrorMessage = "Invalid process number! try again..." }, JsonRequestBehavior.DenyGet);
            }


            var v = unitOfWork.AmortizationRepository.Get().Where(w => w.ProcessNumber == processNumber).ToList();
            foreach (var item in v)
            {
                //unitOfWork.AmortizationRepository.Delete(item); //Commitout by Avishek date:May-17-2015 Reason that this code delete amoortization & also not hit in account 
            }
            try
            {
                string path1 = string.Format("{0}/{1}", Server.MapPath("~/ImportedExcel/LoanPayment"), FileName);
                System.IO.File.Delete(path1);
                unitOfWork.Save();
                return Json(new { Success = true, Message = "Rollback completed!!!" }, JsonRequestBehavior.DenyGet);
            }
            catch (Exception x)
            {
                return Json(new { Success = false, ErrorMessage = "Check error : " + x.Message }, JsonRequestBehavior.DenyGet);
            }
        }

        /// <summary>
        /// Autocompletes the suggestions.
        /// </summary>
        /// <param name="term">The term.</param>
        /// <returns>list</returns>
        /// <CreatedBy>Avishek</CreatedBy>
        /// <createdDate>Mar-9-2015</createdDate>
        public JsonResult AutocompleteSuggestionsForEmpId(string term)
        {
            try
            {
                int oCode = ((int?)Session["OCode"]) ?? 0;
                var suggestions = unitOfWork.CustomRepository.EmployeeWithLoan(oCode, term).Select(s => new
                {
                    value = s.LoanID,
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
        /// <createdDate>Mar-9-2015</createdDate>
        public JsonResult AutocompleteSuggestionsForLoanId(string term)
        {
            try
            {
                int oCode = ((int?)Session["OCode"]) ?? 0;
                var suggestions = unitOfWork.CustomRepository.EmployeeWithLoanByLoanId(oCode, term).Select(s => new
                {
                    label = s.LoanID,
                    value = s.IdentificationNumber
                }).GroupBy(x => x.value).Select(g => g.FirstOrDefault()).ToList();
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
        /// <returns>object</returns>
        /// <CreatedBy>Avishek</CreatedBy>
        /// <createdDate>May-13-2015</createdDate>
        [HttpPost]
        public JsonResult GetAmountAndInstallmentNo(string empId, string loanId)
        {
            try
            {
                CultureInfo CInfo = new CultureInfo("en-IN");
                _MvcApplication = new MvcApplication();
                tbl_PFL_Amortization tblPflAmortization = unitOfWork.CustomRepository.GetPaymentof(empId, loanId).OrderBy(x => x.InstallmentNumber).Where(x => x.Processed == 0).FirstOrDefault();
                tbl_PFL_Amortization atblPflAmortization = new tbl_PFL_Amortization();
                atblPflAmortization.InstallmentNumber = tblPflAmortization.InstallmentNumber;
                atblPflAmortization.PaymentAmount = Convert.ToDecimal(_MvcApplication.GetNumber(tblPflAmortization.Interest + tblPflAmortization.Principal).ToString("N", CInfo));
                atblPflAmortization.Amount = Convert.ToDecimal(_MvcApplication.GetNumber(tblPflAmortization.Amount).ToString("N", CInfo));
                atblPflAmortization.Interest = Convert.ToDecimal(_MvcApplication.GetNumber(tblPflAmortization.Interest).ToString("N", CInfo));
                return Json(atblPflAmortization);
            }
            catch (Exception ex)
            {
                return Json(ex);
            }
        }

        /// <summary>
        /// Autocompletes the suggestions.
        /// </summary>
        /// <param name="term">The term.</param>
        /// <returns>object</returns>
        /// <CreatedBy>Avishek</CreatedBy>
        /// <createdDate>May-13-2015</createdDate>
        [HttpPost]
        public JsonResult Save(VM_PFLoanPayment lst_vm_pfLoanPayment)
        {
            //Added By Avishek Date:Jan-19-2016
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return Json(new { Success = false, Message = "Check error : " }, JsonRequestBehavior.DenyGet);
            }
            //End
            try
            {
                _MvcApplication = new MvcApplication();
                tbl_Employees tblEmployees = unitOfWork.CustomRepository.GetEmployeeByIdentificationNo(lst_vm_pfLoanPayment.IdentificationNumber).FirstOrDefault();
                var empLoanPayment = unitOfWork.CustomRepository.GetPaymentofEmployee(tblEmployees.EmpID, lst_vm_pfLoanPayment.LoanID).Where(x => x.Processed == 0 && x.InstallmentNumber == Convert.ToInt32(lst_vm_pfLoanPayment.InstallmentNumber)).OrderBy(x => x.InstallmentNumber).FirstOrDefault();

                if (empLoanPayment != null)
                {
                    if (empLoanPayment.Balance >= 0)
                    {
                        empLoanPayment.Processed = 1;
                        empLoanPayment.PFLoanID = lst_vm_pfLoanPayment.LoanID;
                        empLoanPayment.InstallmentNumber = lst_vm_pfLoanPayment.InstallmentNumber;
                        empLoanPayment.PaymentDate = lst_vm_pfLoanPayment.PaymentDate;
                        empLoanPayment.ProcessNumber = GetCurrentAmortizationProcessNumber();
                        //_emp_loan_payment.PaymentAmount = lst_vm_pfLoanPayment.InstallmentAmount;
                        //_emp_loan_payment.PaidAmount = lst_vm_pfLoanPayment.InstallmentAmount;
                        empLoanPayment.PaymentAmount = empLoanPayment.Interest + empLoanPayment.Principal;
                        empLoanPayment.PaidAmount = empLoanPayment.Interest + empLoanPayment.Principal;
                        empLoanPayment.Amount = _MvcApplication.GetNumber(empLoanPayment.Amount);
                        empLoanPayment.Balance = _MvcApplication.GetNumber(empLoanPayment.Balance);
                        empLoanPayment.Interest = _MvcApplication.GetNumber(empLoanPayment.Interest);
                        empLoanPayment.Principal = _MvcApplication.GetNumber(empLoanPayment.Principal);
                        empLoanPayment.OCode = oCode;
                        unitOfWork.AmortizationRepository.Update(empLoanPayment);
                        unitOfWork.Save();
                        tbl_PFLoan tblPfLoan = unitOfWork.CustomRepository.EmpLoanInfo(empLoanPayment.EmpID, empLoanPayment.PFLoanID).FirstOrDefault();
                        LoanVoucherPass(empLoanPayment, tblPfLoan.CashLedgerID, "MonthlyInstallment");
                    }
                }
                else
                {
                    return Json(new { Success = false, Message = "Unstable state!!! Emp ID " + lst_vm_pfLoanPayment.EmployeeID + " & Loan ID " + lst_vm_pfLoanPayment.LoanID + " record not found! please reupload data!!" }, JsonRequestBehavior.DenyGet);
                }

                return Json(new { Success = true, Message = "Data saved successfully!" }, JsonRequestBehavior.DenyGet);
            }
            catch (Exception x)
            {
                return Json(new { Success = false, Message = "Check error : " + x.Message }, JsonRequestBehavior.DenyGet);
            }
        }

        /// <summary>
        /// Autocompletes the suggestions.
        /// </summary>
        /// <param name="lst_vm_pfLoanPayment">The LST_VM_PF loan payment.</param>
        /// <returns>object</returns>
        /// <CreatedBy>Avishek</CreatedBy>
        /// <createdDate>Jul-6-2015</createdDate>
        [HttpPost]
        public JsonResult SaveForLoanSettlement(VM_PFLoanPayment lst_vm_pfLoanPayment)
        {
            //Added By Avishek Date:Jan-19-2016
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                return Json(new { Success = false, Message = "Check error : " }, JsonRequestBehavior.DenyGet);
            }
            //End
            try
            {
                _MvcApplication = new MvcApplication();
                tbl_Employees tblEmployees = unitOfWork.CustomRepository.GetEmployeeByIdentificationNo(lst_vm_pfLoanPayment.IdentificationNumber).FirstOrDefault();
                List<tbl_PFL_Amortization> empLoanPaymentList = unitOfWork.CustomRepository.GetPaymentofEmployee(tblEmployees.EmpID, lst_vm_pfLoanPayment.LoanID).Where(x => x.Processed == 0).OrderBy(x => x.InstallmentNumber).ToList();
                tbl_PFL_Amortization empLoanPayment = empLoanPaymentList.FirstOrDefault();
                if (empLoanPayment != null)
                {
                    if (empLoanPayment.Balance > 0)
                    {
                        empLoanPayment.ProcessNumber = GetCurrentAmortizationProcessNumber();
                        tbl_PFLoan tblPfLoan = unitOfWork.CustomRepository.EmpLoanInfo(empLoanPayment.EmpID, empLoanPayment.PFLoanID).FirstOrDefault();
                        LoanVoucherPass(empLoanPayment, tblPfLoan.CashLedgerID, "LoanSettlement");
                        int i = 0;
                        foreach (var item in empLoanPaymentList)
                        {
                            item.Processed = 1;
                            item.PFLoanID = lst_vm_pfLoanPayment.LoanID;
                            item.PaymentDate = lst_vm_pfLoanPayment.PaymentDate;
                            item.PaymentAmount = i == 0 ? _MvcApplication.GetNumber(item.Amount + item.Interest) : 0;
                            item.PaidAmount = i == 0 ? _MvcApplication.GetNumber(item.Amount + item.Interest) : 0;
                            item.Amount = _MvcApplication.GetNumber(item.Amount);
                            item.Balance = _MvcApplication.GetNumber(item.Balance);
                            item.Interest = _MvcApplication.GetNumber(item.Interest);
                            item.Principal = _MvcApplication.GetNumber(item.Principal);
                            item.OCode = oCode;
                            item.ProcessNumber = empLoanPayment.ProcessNumber;
                            unitOfWork.AmortizationRepository.Update(item);
                            unitOfWork.Save();
                            i++;
                        }
                    }
                }
                else
                {
                    return Json(new { Success = false, Message = "Unstable state!!! Emp ID " + lst_vm_pfLoanPayment.EmployeeID + " & Loan ID " + lst_vm_pfLoanPayment.LoanID + " record not found! please reupload data!!" }, JsonRequestBehavior.DenyGet);
                }

                return Json(new { Success = true, Message = "Data saved successfully!" }, JsonRequestBehavior.DenyGet);
            }
            catch (Exception x)
            {
                return Json(new { Success = false, Message = "Check error : " + x.Message }, JsonRequestBehavior.DenyGet);
            }
        }

        /// <summary>
        /// Updates the unit of work.
        /// </summary>
        /// <param name="v">The v.</param>
        /// <returns>string</returns>
        /// <CreaterdBy>Avishek</CreaterdBy>
        /// <CreatedDate>Jul-7-2015</CreatedDate>
        private string UpdateUnitOfWork(tbl_PFL_Amortization v)
        {
            try
            {
                unitOfWork = new UnitOfWork();
                unitOfWork.AmortizationRepository.Update(v);
                return "Sucess";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

    }
}
