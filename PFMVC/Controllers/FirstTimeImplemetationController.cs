using DLL;
using DLL.Repository;
using DLL.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace PFMVC.Controllers
{
    public class FirstTimeImplemetationController : Controller
    {
        OleDbConnection excelConnection;
        DataSet dataset = new DataSet();
        UnitOfWork unitOfWork = new UnitOfWork();

        public ActionResult Settlement()
        {
            int oCode = ((int?)Session["OCode"]) ?? 0;
            if (oCode == 0)
            {
                RedirectToAction("Login", "Account");
            }
            return View();
        }

        [Authorize]
        public ActionResult ImportExcel(string date)
        {
            int OCode = ((int?)Session["OCode"]) ?? 0;
            if (OCode == 0)
            {
                return Json(new { Success = false, ErrorMessage = "You must be under a compnay!" }, JsonRequestBehavior.DenyGet);
            }
            List<VM_Settlement_FirstTime> _VM_Settlement_FirstTimeList = new List<VM_Settlement_FirstTime>();
            int no_of_excel_member = 0;
            int no_of_system_pf_member = 0;
            int no_of_invalid_member = 0;
            int no_of_excel_pf_member = 0;
            string extension = "";

            try
            {
                extension = System.IO.Path.GetExtension(Request.Files["FileUpload1"].FileName);
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

                    string path1 = string.Format("{0}/{1}", Server.MapPath("~/ImportedExcel/Salary"), extension);
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
                    try
                    {
                        cmd.CommandText = "Select [Identification No],[Sattlement Amount] from [Sheet1$]";
                        oleda = new OleDbDataAdapter(cmd);
                        oleda.Fill(dataset, "SalaryData");
                    }
                    catch (Exception x)
                    {
                        return RedirectToAction("Settlement");
                    }
                    finally
                    {
                        OleDbConnection.ReleaseObjectPool();
                        excelConnection.Close();
                        excelConnection.Dispose();
                        if (System.IO.File.Exists(path1))
                            System.IO.File.Delete(path1);
                    }
                    try
                    {
                        DataTable dt = dataset.Tables["SalaryData"];
                        var query = dt.AsEnumerable().Select(s => new
                        {
                            EmployeeIdNo = s.Field<string>("Identification No"),
                            SattlementAmount = s.Field<object>("Sattlement Amount")
                        }).ToList();

                        no_of_excel_member = dt.Rows.Count;


                        foreach (var item in query)
                        {
                            VM_Settlement_FirstTime _VM_Settlement_FirstTime = new VM_Settlement_FirstTime();
                            if (item.EmployeeIdNo != "")
                            {
                                string loan = "";
                                tbl_Employees _tbl_Employees = unitOfWork.CustomRepository.GetEmployeeByIdentificationNumber(item.EmployeeIdNo.Trim()).FirstOrDefault();
                                if (_tbl_Employees != null)
                                {
                                    List<tbl_Contribution> _tbl_Contribution = unitOfWork.CustomRepository.ValidContributionDetail(_tbl_Employees.EmpID).ToList();
                                    List<tbl_ProfitDistributionDetail> _tbl_ProfitDistributionDetail = unitOfWork.CustomRepository.GetProfitDistributionByEmpId(_tbl_Employees.EmpID).ToList();
                                    tbl_PFL_Amortization _tbl_PFL_Amortization = unitOfWork.CustomRepository.GetUnpaidLoanByEmpId(_tbl_Employees.EmpID).Where(x => x.Processed == 0).FirstOrDefault();

                                    decimal selfCon = _tbl_Contribution.Where(w=>w.ContributionDate<= DateTime.Now.Date ).Sum(x => x.SelfContribution);
                                    decimal empfCon = _tbl_Contribution.Where(w => w.ContributionDate <= DateTime.Now.Date).Sum(x => x.EmpContribution);
                                    decimal balance = 0;
                                    decimal profit = Convert.ToDecimal(_tbl_ProfitDistributionDetail.Sum(x => x.DistributedAmount));
                                    if (_tbl_Employees != null)
                                    {
                                        _VM_Settlement_FirstTime.IdentificationNo = item.EmployeeIdNo.Trim();
                                    }
                                    else
                                    {
                                        _VM_Settlement_FirstTime.Message = "This is not a PF Member";
                                    }
                                    if (_tbl_PFL_Amortization == null)
                                    {
                                        balance = 0;
                                    }
                                    else
                                    {
                                        balance = _tbl_PFL_Amortization.Balance;
                                    }
                                    //if (_tbl_ProfitDistributionDetail.Count == 0)
                                    //{
                                    //    profit = 0;
                                    //}
                                    //else
                                    //{
                                    //    profit = Convert.ToDecimal(_tbl_ProfitDistributionDetail.Sum(x => x.DistributedAmount));
                                    //}
                                    var o = _tbl_Employees.opEmpContribution
                                            + _tbl_Employees.opOwnContribution
                                            + _tbl_Employees.opProfit
                                            - _tbl_Employees.opLoan
                                            + selfCon
                                            + empfCon
                                            + profit
                                            - balance;
                                    if (o != null)
                                        _VM_Settlement_FirstTime.Balance = (decimal)o;
                                    if (item.SattlementAmount == null || item.SattlementAmount == "")
                                        _VM_Settlement_FirstTime.Message = "Salttelement amount is not in here";
                                    else
                                        _VM_Settlement_FirstTime.SettlementAmount = Convert.ToDecimal(item.SattlementAmount);
                                    _VM_Settlement_FirstTimeList.Add(_VM_Settlement_FirstTime);
                                }
                                else
                                {
                                    _VM_Settlement_FirstTime.IdentificationNo = item.EmployeeIdNo.Trim();
                                    _VM_Settlement_FirstTime.Balance = 0;
                                    _VM_Settlement_FirstTime.SettlementAmount = 0;
                                    _VM_Settlement_FirstTime.Message = "This is not a PF Member";
                                    _VM_Settlement_FirstTimeList.Add(_VM_Settlement_FirstTime);
                                }
                            }
                        }
                    }
                    catch (Exception x)
                    {
                        return RedirectToAction("Settlement");
                    }
                }
                else
                {
                    return RedirectToAction("Settlement");
                }
            }
            else
            {
                return RedirectToAction("Settlement");
            }
            return View("_SettlementForFirstTimeDataGrid", _VM_Settlement_FirstTimeList);
        }


        public ActionResult SettelmentSubmit(List<VM_Settlement_FirstTime> v)
        {
            try
            {
                int OCode = ((int?)Session["OCode"]) ?? 0;
                if (OCode == 0)
                {
                    return Json(new { Success = false, ErrorMessage = "You must be under a compnay!" }, JsonRequestBehavior.DenyGet);
                }

                VM_Settlement_FirstTime validation = v.Where(x => x.Message == "This is not a PF Member").FirstOrDefault();
                if (validation != null)
                {
                    TempData["shortMessage"] = "This is not a PF Member";
                    return RedirectToAction("Settlement");
                }

                foreach (var item in v)
                {
                    string refMessage = "";
                    int voucherID = 0;
                    List<string> LedgerNameList = new List<string>();
                    List<decimal> Credit = new List<decimal>();
                    List<decimal> Debit = new List<decimal>();
                    List<string> ChqNumber = new List<string>();
                    List<string> PFLoanID = new List<string>();
                    List<string> PFMemberID = new List<string>();

                    tbl_Employees _tbl_Employees = unitOfWork.CustomRepository.GetEmployeeByIdentificationNumber(item.IdentificationNo.Trim()).FirstOrDefault();
                    tbl_PFL_Amortization _tbl_PFL_Amortization = unitOfWork.CustomRepository.GetUnpaidLoanByEmpId(_tbl_Employees.EmpID).Where(x => x.Processed == 0).FirstOrDefault();
                    decimal finalBalance = item.Balance - item.SettlementAmount;
                    decimal UnpaidLoan = _tbl_PFL_Amortization == null ? 0 : _tbl_PFL_Amortization.Balance;

                    LedgerNameList.Add("Members Fund");
                    Credit.Add(0);
                    Debit.Add(item.Balance);
                    ChqNumber.Add("");
                    PFMemberID.Add(_tbl_Employees.EmpID + "");
                    PFLoanID.Add("");

                    LedgerNameList.Add("Forfeiture");
                    Credit.Add(finalBalance);
                    Debit.Add(0);
                    ChqNumber.Add("");
                    PFMemberID.Add(_tbl_Employees.EmpID + "");
                    PFLoanID.Add("");

                    LedgerNameList.Add("Loan");
                    Credit.Add(0);
                    Debit.Add(0);
                    ChqNumber.Add("");
                    PFMemberID.Add(_tbl_Employees.EmpID + "");
                    PFLoanID.Add("");

                    LedgerNameList.Add("Company Current Account");
                    Credit.Add(item.SettlementAmount);
                    Debit.Add(0);
                    ChqNumber.Add("");
                    PFMemberID.Add(_tbl_Employees.EmpID + "");
                    PFLoanID.Add("");

                    Guid currentUserID = unitOfWork.CustomRepository.GetUserID("admin");
                    bool isOperationSuccess = unitOfWork.AccountingRepository.DualEntryVoucher(_tbl_Employees.EmpID, 5, DateTime.Now, ref voucherID, "Member settlement " + _tbl_Employees.IdentificationNumber, LedgerNameList, Debit, Credit, ChqNumber, ref refMessage, "Implement", currentUserID, PFMemberID, "", "", "", null, PFLoanID, OCode, "Settlement");
                    if (isOperationSuccess)
                    {
                        var employee = unitOfWork.EmployeesRepository.GetByID(_tbl_Employees.EmpID);
                        employee.PFDeactivationVoucherID = voucherID;
                        employee.PFStatus = 2;
                        employee.PFDeactivatedBy = currentUserID;
                        employee.PFDeactivatedByName = "Implement";
                        employee.PFDeactivationDate = DateTime.Now;
                        employee.Comment = "Sattlement from excel";
                        unitOfWork.EmployeesRepository.Update(employee);
                        unitOfWork.Save();
                    }

                }
            }
            catch (Exception ex)
            {
                TempData["shortMessage"] = "Error";
                return RedirectToAction("Settlement");
            }
            TempData["shortMessage"] = "Sucess";
            return RedirectToAction("Settlement");
        }


    }
}
