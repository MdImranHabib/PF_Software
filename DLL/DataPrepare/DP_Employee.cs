using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DLL.ViewModel;

namespace DLL.DataPrepare
{
    public class DP_Employee
    {
        public tbl_Employees tbl_Employees(VM_Employee x)
        {
            tbl_Employees y = new tbl_Employees();
            y.BirthDate = x.BirthDate;
            y.Branch = x.BranchID;
            y.ContactNumber = x.ContactNumber;
            y.Department = x.DepartmentID;
            y.Designation = x.DesignationID;
            y.Email = x.Email;
            y.EmpID = x.EmpID;
            y.EmpImg = x.EmpImg;
            y.EmpName = x.EmpName;
            y.Gender = x.Gender;
            y.JoiningDate = x.JoiningDate;
            y.NID = x.NID;
            y.PFStatus = x.PFStatusID;
            y.PresentAddress = x.PresentAddress;
            y.SignatureImg = x.SignatureImg;
            y.IdentificationNumber = x.IdentificationNumber;
            y.opOwnContribution = x.opOwnContribution;
            y.opEmpContribution = x.opEmpContribution;
            y.opProfit = x.opProfit;
            y.opLoan = x.opLoan;
            y.PFDeactivationDate = x.PFDeactivationDate;
            y.PFDeactivationVoucherID = x.PFDeactivationVoucherID;
            y.opDepartmentName = x.opDepartmentName;
            y.opDesignationName = x.opDesignationName;
            y.PFActivationDate = x.PFActivationDate ?? DateTime.Now;

            return y;
        }


        public VM_Employee VM_Employee(tbl_Employees x)
        {
            VM_Employee y = new VM_Employee();
            y.BirthDate = x.BirthDate;
            y.BranchID = x.Branch?? 0;
            y.ContactNumber = x.ContactNumber;
            y.DepartmentID = x.Department;
            y.DesignationID = x.Designation;
            y.Email = x.Email;
            y.EmpID = x.EmpID;
            y.EmpImg = x.EmpImg;
            y.EmpName = x.EmpName;
            y.Gender = x.Gender;
            y.JoiningDate = x.JoiningDate;
            y.NID = x.NID;
            y.PFStatusID = x.PFStatus ?? 0;
            y.PresentAddress = x.PresentAddress;
            y.SignatureImg = x.SignatureImg;
            y.IdentificationNumber = x.IdentificationNumber;
            y.opOwnContribution = x.opOwnContribution ?? 0;
            y.opEmpContribution = x.opEmpContribution ?? 0;
            y.opProfit = x.opProfit ?? 0;
            y.opLoan = x.opLoan ?? 0;
            y.PFDeactivationDate = x.PFDeactivationDate;
            y.PFDeactivationVoucherID = x.PFDeactivationVoucherID??0;
            y.opDepartmentName = x.opDepartmentName;
            y.opDesignationName = x.opDesignationName;
            y.PFActivationDate = x.PFActivationDate;
            return y;
        }
    }
}
