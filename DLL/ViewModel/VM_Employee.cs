using DLL.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLL.ViewModel
{
    public class VM_Employee
    {

        public int EmpID { get; set; }
        public string EmployeeID { get; set; }
        [Required(AllowEmptyStrings = false)]
        public string IdentificationNumber { get; set; }
        [Required]
        [Display(Name = "Employee Name")]
        public string EmpName { get; set; }
        public string DesignationID { get; set; }
        public string DepartmentID { get; set; }
        public int BranchID { get; set; }
        [Display(Name = "Designation Name")]
        public string DesignationName { get; set; }
        [Display(Name = "Department Name")]
        public string DepartmentName { get; set; }
        [Display(Name = "Branch Name")]
        [DisplayFormat]
        public string BranchName { get; set; }
        public string Gender { get; set; }
        [Display(Name = "Present Address")]
        public string PresentAddress { get; set; }
        [Display(Name = "Mobile")]
        public string ContactNumber { get; set; }
        [Display(Name = "Email")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Please input valid email address")]
        public string Email { get; set; }
        [Display(Name = "Birth Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? BirthDate { get; set; }
        [Display(Name = "National ID")]
        public string NID { get; set; }
        //[DisplayFormat(DataFormatString = "{0:dd-MMM-yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Joining Date")]
        [DataType(DataType.DateTime)]
        public DateTime? JoiningDate { get; set; }
        [Display(Name = "PF Status")]
        public int PFStatusID { get; set; }
        public decimal OwnProfit { get; set; }
        public decimal EmpProfit { get; set; }
        public decimal TransferCompanyAmount { get; set; }
        public string FirstName { get; set; }
        public string Lastname { get; set; }
        [Display(Name = "PF Status")]
        public string PFStatus
        {
            get
            {
                if (PFStatusID == 2)
                {
                    return "Deactivated";
                }
                else if (PFStatusID == 1)
                {
                    return "Active";
                }
                else return "Inactive";
            }
        }
        public string EmpImg { get; set; }
        public string PreImportMessage { get; set; }
        public string SignatureImg { get; set; }
        [Display(Name = "Section Name")]        
        public string SectionName { get; set; }
        //added by sohana
        [Display(Name = "Basic Salary")]
        public decimal Basic { get; set; }
        public string LocationName { get; set; }
        public string PFDeactivatedByName { get; set; }
        public int PassVoucherEntryID { get; set; }
        public int PFRuleUsedForDeactivation { get; set; }
        //ended by sohana
        //=========
        //[Required]
        //[Display(Name = "Basic Salary")]
        //public decimal Basic { get; set; }
        //[Required]
        //[Display(Name = "Effective Date")]
        //[DataType(DataType.DateTime)]
        //public System.DateTime? EffectiveDate { get; set; }
        //============
        [Required]
        public DateTime? PFActivationDate { get; set; }
        public DateTime? PFDeactivationDate { get; set; }
        public DateTime? RetirementDate { get; set; }
        public bool IsPFMember
        {
            get
            {
                if (PFStatusID == 1)
                    return true;
                else return false;
            }
        }
        public System.Guid EditUser { get; set; }
        public string EditUserName { get; set; }
        public string PFDeactivatedBy { get; set; }
        public string EmpAddedBy { get; set; }
        public System.DateTime EditDate { get; set; }
        public string PFDuration
        {
            get
            {
                if (PFDeactivationDate != null)
                {
                    if (ApplicationSetting.JoiningDate == true)
                    {
                        string s = "";
                        s += PFDeactivationDate.Value.Year - JoiningDate.Value.Year + " years ";
                        s += PFDeactivationDate.Value.Month - JoiningDate.Value.Month + " months ";

                        return s;
                    }
                    else
                    {
                        string s = "";

                        s += PFDeactivationDate.Value.Year - PFActivationDate.Value.Year + " years ";
                        s += PFDeactivationDate.Value.Month - PFActivationDate.Value.Month + " months ";
                        return s;
                    }
                }
                else
                {
                    return string.Empty;
                }
            }
        }
        [DisplayFormat(DataFormatString = "{0:0}",ApplyFormatInEditMode = true)]
        public decimal opOwnContribution { get; set; }
        [DisplayFormat(DataFormatString = "{0:0}",ApplyFormatInEditMode = true)]
        public decimal opEmpContribution { get; set; }
        [DisplayFormat(DataFormatString = "{0:0}",ApplyFormatInEditMode = true)]
        public decimal opProfit { get; set; }
        [DisplayFormat(DataFormatString = "{0:0}",ApplyFormatInEditMode = true)]
        public decimal opLoan { get; set; }
        [Display(Name = "Designation Name")]
        public string opDesignationName { get; set; } // meaning exporting as plain text
        [Display(Name = "Department Name")]
        public string opDepartmentName { get; set; } // same ^ \
        [DisplayFormat(DataFormatString = "{0:0}", ApplyFormatInEditMode = true)]
        public decimal OpeningBalance { get;set; }
        //public decimal OpeningBalance
        //{
        //    get
        //    {
        //        return Math.Round(opOwnContribution + opEmpContribution + opProfit - opLoan);
        //    }
        //}
        public string Comment { get; set; }
        public bool? PassVoucher { get; set; }
        public string PassVoucherMessage { get; set; }
        public int PFDeactivationVoucherID { get; set; }
        public int OCode { get; set; }
        [DisplayFormat(DataFormatString = "{0:0}",ApplyFormatInEditMode = true)]
        public decimal OwnCont { get; set; }
        [DisplayFormat(DataFormatString = "{0:0}",ApplyFormatInEditMode = true)]
        public decimal EmpCont { get; set; }
        [DisplayFormat(DataFormatString = "{0:0}",ApplyFormatInEditMode = true)]
        public decimal Profit { get; set; }
        [DisplayFormat(DataFormatString = "{0:0}",ApplyFormatInEditMode = true)]
        public decimal Withdrawal { get; set; }
        [DisplayFormat(DataFormatString = "{0:0}",ApplyFormatInEditMode = true)]
        public decimal LoanAdjustment { get; set; }
        [DisplayFormat(DataFormatString = "{0:0}",ApplyFormatInEditMode = true)]
        public decimal Forfeiture { get; set; }
        [DisplayFormat(DataFormatString = "{0:0}",ApplyFormatInEditMode = true)]
        public decimal ShowSummaryBalance { get; set; }

        public decimal distributedInterestIncomeProfit { get; set; }

        private decimal summaryBalance = -1;
        [DisplayFormat(DataFormatString = "{0:0}",ApplyFormatInEditMode = true)]
        public decimal SummaryBalance
        {
            get
            {
                if (summaryBalance != 0)
                {
                    return (OpeningBalance + OwnCont + EmpCont + Profit - Withdrawal - LoanAdjustment - Forfeiture);
                    //return (OpeningBalance + OwnCont + EmpCont - Withdrawal - LoanAdjustment - Forfeiture);
                }

                else
                {
                    return summaryBalance;
                }
            }

            set
            {
                summaryBalance = value;
            }
        }
        public string LoanId { get; set; }
        public DateTime TracsactionDate { get; set; }
        [DisplayFormat(DataFormatString = "{#,###}")]
        public decimal SelfContribution { get; set; }
        public decimal TransferAmount { get; set; }
        public bool isSelected { set; get; }
        //public decimal TransferCompanyAmount { get; set; }
    }
}
