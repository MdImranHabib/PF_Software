//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DLL.PayRollAccess
{
    using System;
    using System.Collections.Generic;
    
    public partial class HRM_Pay_Salary
    {
        public int PaySalary_ID { get; set; }
        public Nullable<long> EmpId { get; set; }
        public string EID { get; set; }
        public Nullable<int> Worked_Day { get; set; }
        public Nullable<int> Over_Time { get; set; }
        public Nullable<int> Total_Day_Of_Month { get; set; }
        public Nullable<int> Work_Holiday { get; set; }
        public Nullable<int> Other_Holiday { get; set; }
        public Nullable<int> Total_Leave { get; set; }
        public Nullable<System.DateTime> Salary_Month { get; set; }
        public Nullable<System.DateTime> Date_Processed { get; set; }
        public Nullable<decimal> Total_Basic_New { get; set; }
        public Nullable<decimal> Total_Gross_Sal { get; set; }
        public Nullable<decimal> Net_Payable { get; set; }
        public Nullable<decimal> Total_Tax { get; set; }
        public Nullable<decimal> PF_Contribution { get; set; }
        public Nullable<decimal> Total_Bonus { get; set; }
        public Nullable<decimal> Total_LateDeduction_Cost { get; set; }
        public Nullable<decimal> Total_Leave_Cost { get; set; }
        public Nullable<decimal> Total_Absent_Cost { get; set; }
        public Nullable<bool> Pay_Status { get; set; }
        public Nullable<System.Guid> Edit_User { get; set; }
        public Nullable<System.DateTime> Edit_Date { get; set; }
        public string OCode { get; set; }
        public Nullable<decimal> OT_Rate { get; set; }
        public Nullable<decimal> OT_Taka { get; set; }
        public Nullable<decimal> Attendance_Bonus { get; set; }
        public Nullable<int> P { get; set; }
        public Nullable<int> L { get; set; }
        public Nullable<int> SL { get; set; }
        public Nullable<int> CL { get; set; }
        public Nullable<int> ML { get; set; }
        public Nullable<int> AL { get; set; }
        public Nullable<int> LWP { get; set; }
        public Nullable<int> Absent_Day { get; set; }
        public Nullable<decimal> Absent_Deduction { get; set; }
        public Nullable<double> TotalPunishmentDay { get; set; }
        public Nullable<decimal> Punishment_Deduction { get; set; }
        public Nullable<double> TotalDeductDay { get; set; }
        public Nullable<decimal> Other_Deduction { get; set; }
        public Nullable<decimal> Total_Benifit { get; set; }
        public Nullable<decimal> Stamp { get; set; }
        public Nullable<decimal> AdvanceDeduction { get; set; }
        public Nullable<decimal> Total_Deduction { get; set; }
        public Nullable<bool> IsSalaryHeldup { get; set; }
        public Nullable<bool> IsSalaryCancel { get; set; }
        public Nullable<bool> IsSalaryRelease { get; set; }
    }
}