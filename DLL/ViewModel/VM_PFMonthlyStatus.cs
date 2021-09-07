using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLL.ViewModel
{
    public class VM_PFMonthlyStatus
    {
        public int EmpID { get; set; }
        public string Month { get; set; }
        public string Branch { get; set; }

        public string Year { get; set; }
        [DisplayFormat(DataFormatString = "{0:#,##,##,##0.00}", ApplyFormatInEditMode = true)]
        public decimal SelfContribution { get; set; }
        [DisplayFormat(DataFormatString = "{0:#,##,##,##0.00}", ApplyFormatInEditMode = true)]
        public decimal EmpContribution { get; set; }
        public DateTime ProcessRunDate { get; set; }
        [DisplayFormat(DataFormatString = "{0:#,##,##,##0.00}", ApplyFormatInEditMode = true)]
        public decimal Total { get { return SelfContribution + EmpContribution + EmpProfit + SelfProfit; } }
        //public string Total { get { return (SelfContribution + EmpContribution).ToString("#,##,##,##0.00"); } }
        public string MonthYear
        {
            get;
            set;
            //get
            //{
            //    return Convert.ToDateTime(DateTime.ParseExact("13/" + Month??null + "/" + Year??null, "dd/MM/yyyy", CultureInfo.DefaultThreadCurrentCulture)).ToString("MMMM, yyyy");
            //}
            //set { }
        }
        public Nullable<decimal> Salary { get; set; }
        public Nullable<decimal> SCPercentage { get; set; }
        public Nullable<decimal> ECPercentage { get; set; }
        public Nullable<decimal> InterestRate { get; set; }
        [DisplayFormat(DataFormatString = "{0:#,##,##,##0.00}", ApplyFormatInEditMode = true)]
        public decimal CumulativeSelfContribution { get; set; }
        [DisplayFormat(DataFormatString = "{0:#,##,##,##0.00}", ApplyFormatInEditMode = true)]
        public decimal CumulativeEmpContribution { get; set; }
        public decimal SCInterest { get; set; }
        public decimal ECInterest { get; set; }
        public string IdentificationNumber { get; set; }
        public string EmpName { get; set; }
        public string Designation { get; set; }
        [DisplayFormat(DataFormatString = "{0:#,##,##,##0.00}", ApplyFormatInEditMode = true)]
        public decimal OSelfContribution { get; set; }
        [DisplayFormat(DataFormatString = "{0:#,##,##,##0.00}", ApplyFormatInEditMode = true)]
        public decimal OEmpContribution { get; set; }
        public string SUM { get; set; }
        public string Self { get; set; }
        public string EMP { get; set; }
        public string Profit { get; set; }
        public decimal SelfProfit { get; set; }
        public decimal EmpProfit { get; set; }
        public string CSC { get; set; }
        public string CEC { get; set; }
        public DateTime ContrebutionDate { get; set; }
        [DisplayFormat(DataFormatString = "{0:#,##,##,##0.00}", ApplyFormatInEditMode = true)]
        public decimal Principle { get; set; }
        [DisplayFormat(DataFormatString = "{0:#,##,##,##0.00}", ApplyFormatInEditMode = true)]
        public decimal PaidAmount { get; set; }
        [DisplayFormat(DataFormatString = "{0:#,##,##,##0.00}", ApplyFormatInEditMode = true)]
        public decimal LoanAmount { get; set; }
    }
}
