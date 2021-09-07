using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLL.ViewModel
{
    public class VM_Contribution
    {
        public int EmpID { get; set; }
        public string IdentificationNumber { get; set; }
        public string EmpName { get; set; }

        //Added by fahim 02/11/2015

        public decimal EmpProfit { get; set; }
        public decimal SelfProfit { get; set; }
        public double SeProfit { get; set; }
        public double EmProfit { get; set; }



        //End

        public DateTime JoiningDate { get; set; }
        public DateTime PFActivationDate { get; set; }
        public DateTime? PFDeactivationDate { get; set; }
        public string WorkingDuration { get; set; }
        public string Deginecation { get; set; }
        public string Branch { get; set; }

        public decimal SelfContribution { get; set; }
        public decimal SCInterest { get; set; }
        public string Department { get; set; }
        public decimal EmpContribution { get; set; }
        public decimal ECInterest { get; set; }

        public decimal Salary { get; set; }

        public string ConYear { get; set; }
        public string ConMonth { get; set; }

        public System.DateTime ProcessDate { get; set; }
        public string Message { get; set; }

        public int PFRuleID { get; set; }
        public decimal Total
        {
            get
            { return SelfContribution + EmpContribution; }

        }

        public int ConYearInt { get; set; }
        public int ConMonthInt { get; set; }

        public int PFRulesID { get; set; }

        public Nullable<decimal> SCPercentage { get; set; }
        public Nullable<decimal> ECPercentage { get; set; }
        public Nullable<decimal> InterestRate { get; set; }

        public decimal CumulativeSelfContribution { get; set; }
        public decimal CumulativeEmpContribution { get; set; }

        public string MonthYear
        {
            get
            {
                //return Convert.ToDateTime("25/" + ConMonth + "/" + ConYear).ToString("MMMM, yyyy"); 
                return Convert.ToDateTime(DateTime.ParseExact("13/" + ConMonth + "/" + ConYear, "dd/MM/yyyy", CultureInfo.InvariantCulture)).ToString("MMMM, yyyy");
            }
        }
        //return Convert.ToDateTime(DateTime.ParseExact("13/" + Month + "/" + Year, "dd/MM/yyyy", CultureInfo.InvariantCulture)).ToString("MMMM, yyyy"); 
        public decimal DistributedAmount { get; set; }
        public decimal opOwnContribution { get; set; }
        public decimal opProfit { get; set; }
        public decimal opLoan { get; set; }
        public decimal opEmpContribution { get; set; }
        public int OCode { get; set; }
        public decimal Profit { get; set; }
        public DateTime ContributionFromDate { get; set; }
        public DateTime ContributionToDate { get; set; }
        public DateTime ContributionDate { get; set; }

        public int TotalPendingMonth { get; set; }
    }
}
