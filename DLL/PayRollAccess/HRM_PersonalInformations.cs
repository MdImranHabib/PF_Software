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
    
    public partial class HRM_PersonalInformations
    {
        public long EmpId { get; set; }
        public string EID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string BanFullName { get; set; }
        public Nullable<int> RegionsId { get; set; }
        public Nullable<int> OfficeId { get; set; }
        public Nullable<int> DepartmentId { get; set; }
        public Nullable<int> SectionId { get; set; }
        public Nullable<int> SubSectionId { get; set; }
        public Nullable<int> DesginationId { get; set; }
        public Nullable<int> Grade_Id { get; set; }
        public string BIO_MATRIX_ID { get; set; }
        public string Gender { get; set; }
        public Nullable<System.DateTime> DateOfBrith { get; set; }
        public string Birth_Place { get; set; }
        public string BloodGroup { get; set; }
        public string FatherName { get; set; }
        public string MotherName { get; set; }
        public string MaritalStatus { get; set; }
        public string Religion { get; set; }
        public string Emp_Height { get; set; }
        public string Emp_Weight { get; set; }
        public string Emp_IdentificationSign { get; set; }
        public string NationalID { get; set; }
        public string E_TIN { get; set; }
        public string Nationality { get; set; }
        public string ContactNumber { get; set; }
        public string EmergencyContactNo { get; set; }
        public string EmergencyContactPerson { get; set; }
        public Nullable<System.DateTime> EmergencyCPDoB { get; set; }
        public string EmergencyCPAge { get; set; }
        public string EmergencyN_ID { get; set; }
        public string EmergencyAddress { get; set; }
        public string FatherAge { get; set; }
        public string FatherProfession { get; set; }
        public string MotherAge { get; set; }
        public string MotherProfession { get; set; }
        public string SpouseName { get; set; }
        public string SpouseAge { get; set; }
        public string SpouseProfession { get; set; }
        public string NumberOfChildren { get; set; }
        public string ChildrenNameRemark { get; set; }
        public string Email { get; set; }
        public string PresentAddress { get; set; }
        public string Present_Vill_HouseNo { get; set; }
        public string Present_PostOffice { get; set; }
        public string Present_Thana { get; set; }
        public string Present_District { get; set; }
        public string PermanenAddress { get; set; }
        public string Permanent_Vill_HouseNo { get; set; }
        public string Permanent_PostOffice { get; set; }
        public string Permanent_Thana { get; set; }
        public string Permanent_District { get; set; }
        public string TelegraphOffice { get; set; }
        public string AlternativEmailAddress { get; set; }
        public string NomineeName { get; set; }
        public string NomineeAge { get; set; }
        public string NomineeRelation { get; set; }
        public Nullable<int> Step { get; set; }
        public string Grade { get; set; }
        public Nullable<decimal> EffectiveSlary { get; set; }
        public Nullable<decimal> Salary { get; set; }
        public Nullable<decimal> GrossRate { get; set; }
        public Nullable<decimal> OldSalary { get; set; }
        public Nullable<System.DateTime> SalaryUpdateDate { get; set; }
        public Nullable<bool> OTApplicable { get; set; }
        public Nullable<System.DateTime> ProbationPeriodFrom { get; set; }
        public Nullable<System.DateTime> ProbationPeriodTo { get; set; }
        public Nullable<System.DateTime> ConfiramtionDate { get; set; }
        public Nullable<bool> ConfiramtionDateStatus { get; set; }
        public Nullable<int> EmployeeType { get; set; }
        public Nullable<int> EmpCategoryId { get; set; }
        public Nullable<int> ShiftId { get; set; }
        public string ShiftCode { get; set; }
        public string ReportingBossId { get; set; }
        public string SecondReportingBossId { get; set; }
        public string ThirdReportingBossId { get; set; }
        public string JobResponsibility { get; set; }
        public Nullable<System.DateTime> JoiningDate { get; set; }
        public string ContactPersonRelationName { get; set; }
        public Nullable<bool> EMP_TERMIN_STATUS { get; set; }
        public Nullable<bool> EMP_TRANSFER_STATUS { get; set; }
        public Nullable<bool> EMP_Retired_Status { get; set; }
        public Nullable<bool> EMP_Resignation_Status { get; set; }
        public Nullable<bool> EMP_Dismissal_Status { get; set; }
        public Nullable<bool> EMP_Died_Status { get; set; }
        public Nullable<bool> EMP_Dis_Continution_Status { get; set; }
        public Nullable<bool> EMP_Other { get; set; }
        public Nullable<System.DateTime> Seperation_Date { get; set; }
        public byte[] EMP_PHOTO { get; set; }
        public byte[] EMP_Singnature1 { get; set; }
        public byte[] EMP_Singnature2 { get; set; }
        public byte[] EMP_Singnature3 { get; set; }
        public byte[] EMP_Singnature4 { get; set; }
        public byte[] Nomine_Photo { get; set; }
        public byte[] Nomine_Singnature { get; set; }
        public Nullable<int> P { get; set; }
        public Nullable<int> L { get; set; }
        public Nullable<int> A { get; set; }
        public Nullable<int> OT { get; set; }
        public Nullable<System.TimeSpan> LateHour { get; set; }
        public Nullable<int> WH { get; set; }
        public Nullable<int> GV_H { get; set; }
        public Nullable<int> PV_H { get; set; }
        public Nullable<int> OH { get; set; }
        public Nullable<int> EX { get; set; }
        public Nullable<bool> EffectiveSalaryStatus { get; set; }
        public Nullable<bool> Attendance_Bouns { get; set; }
        public Nullable<bool> Late_Applicable { get; set; }
        public Nullable<bool> Absence_Applicable { get; set; }
        public Nullable<bool> Tax_Applicable { get; set; }
        public Nullable<bool> PF_Applicable { get; set; }
        public Nullable<bool> Increment_Applicable { get; set; }
        public Nullable<bool> On_Amount { get; set; }
        public Nullable<int> SeniorId { get; set; }
        public string Substitute_Employee_EID { get; set; }
        public Nullable<double> TotalJobYear { get; set; }
        public Nullable<decimal> PF_Contribution_Amount { get; set; }
        public Nullable<decimal> CPF_Loan_Amount { get; set; }
        public Nullable<decimal> Credit_Union_Amount { get; set; }
        public Nullable<decimal> Tax_Amount { get; set; }
        public Nullable<System.Guid> EDIT_USER { get; set; }
        public Nullable<System.DateTime> EDIT_DATE { get; set; }
        public string OCODE { get; set; }
    }
}