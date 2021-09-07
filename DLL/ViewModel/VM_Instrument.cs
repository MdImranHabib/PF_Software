using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Web;
using System.Threading.Tasks;
using System.IO;


namespace DLL.ViewModel
{
     public class VM_Instrument
    {
        public int InstrumentID { get; set; }
        [Required]
        public string InstrumentType { get; set; }
        //[Required]
        public string InstrumentNumber { get; set; }
        [Required]
        public string Institution { get; set; }
        [Required]
        public string Branch { get; set; }
        [Required]
        public System.DateTime? DepositDate { get; set; }
        //[Required]
        public System.DateTime? StartDate { get; set; }
        [Required]
        public int MaturityPeriod { get; set; }
        public Nullable<System.DateTime> MaturityDate { get; set; }
        [Required]
        public decimal Amount { get; set; }
        [Required]
        public decimal InterestRate { get; set; }
        public Nullable<System.Guid> EditUser { get; set; }
        public string EditUserName { get; set; }
        public Nullable<System.DateTime> EditDate { get; set; }
        public string Comment { get; set; }
        public Guid? LedgerID { get; set; }
        public string LedgerName { get; set; }
        public double AccuredInterest { get; set; }
        public decimal InterestIncome { get; set; }
        public string LastProcessShow { get; set; }
        public DateTime EncashmentDate { get; set; }
        public bool Renew { get; set; }    // added by shohid 23 Aug 2016  for Renew Investment
        public int? RenewInstrumentID { get; set; }
        public decimal EncashmentAmount { get; set; }
        public decimal FDRTax { get; set; }
        public decimal BankCharge { get; set; }
        public decimal TDSonFDR { get; set; }
      //  public Guid? InstrumentID { get; set; }
        public string EntryForPF { get; set; }
        public bool Closed { get; set; }
        /// <summary>
        /// /For File Upload
        /// </summary>
        public string file_directory { get; set; }
        public string file_name { get; set; }

        //public HttpPostedFileBase file_data { get; set; }
        //public string file_data { get; set; }

        public string photo { get; set; }
        public string file_full_path
        {
            get
            {
                if (file_type == "VIDEO")
                    return file_directory.Replace("~", "") + (file_directory.Last() == '/' ? "" : "/") + file_name;
                else
                    return file_directory.Replace("~", "") + (file_directory.Last() == '/' ? "" : "/") + file_name;
            }
        }


        public string file_extenstion
        {
            get
            {
                return file_name.Split('.').LastOrDefault();
            }
        }

        public string file_type
        {
            get
            {
                switch (file_extenstion.ToLower())
                {
                    
                    case "jpeg":
                    case "jpg":
                    case "png":
                    case "gif":
                    case "bmp":
                        return "IMAGE";
                    case "docx":
                    case "doc":
                        return "Docx";
                    default:
                        return "";
                }
            }
        }
    }
}
