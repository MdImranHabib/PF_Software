using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLL.ViewModel
{
    public class VM_NomineeInformation
    {
        public int EmpID { get; set; }
        public int NomineeID { get; set; }
        [Required]
        public string NomineeName { get; set; }
        public string NomineeAddress { get; set; }
        [Required]
        public string Relation { get; set; }
        public Nullable<System.DateTime> RegistrationDate { get; set; }
        
        public string NomineeImageFileName { get; set; }
        [Required]
        public string NomineeNationalID { get; set; }
        [Required]
        [Range(0,101, ErrorMessage="Nominee percentage range should be withing 100")]
        public decimal Nomineepercentage { get; set; }
    }
}
