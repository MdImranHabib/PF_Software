using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLL.ViewModel
{
    public class VM_InterestRate
    {
        public int RowId { get; set; }
        public string ConYear { get; set; }
        public string ConMonth { get; set; }
        [Required]
        [DisplayName("Interest Rate (%)")]
        public double InterestRate { get; set; }
        public string EditUserName { get; set; }
        public System.DateTime EditDate { get; set; }
        
        public string MonthYear
        {
            get
            {
                if (!string.IsNullOrEmpty(ConMonth) && !string.IsNullOrEmpty(ConYear))
                {
                    return Convert.ToDateTime(ConYear + "/" + ConMonth + "/01").ToString("MMMM, yyyy");
                }
                else
                {
                    return "";
                }
            }
        }

        [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? EffectiveFrom { get; set; }
    }
}
