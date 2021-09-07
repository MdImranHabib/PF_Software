using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLL.ViewModel
{
    public class ChequeR
    {
        public string BankName { get; set; }
        public string ChequeNo { get; set; }
        public string AccountNo { get; set; }
        public decimal? Amount { get; set; }
        public string ClientName { get; set; }
        public DateTime? ChequeDate { get; set; }
        public string AmountInWord { get; set; }

    }
}
