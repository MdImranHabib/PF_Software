using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLL.Utility
{
    public class ApplicationSetting
    
    {
        public static bool JoiningDate
        {
            get
            {
                return (ConfigurationManager.AppSettings.Get("CalculateByJoiningDate").ToLower() == "true");
                //return ConfigurationManager.AppSettings.Get("JoiningDate");
            }
        }
        public static bool Branch
        {
            get
            {
                return (ConfigurationManager.AppSettings.Get("UsingBranch").ToLower() == "true");
                
            }
        }
        public static bool GenerateAmortization
        {
            get
            {
                return (ConfigurationManager.AppSettings.Get("GenerateAmortization").ToLower() == "true");

            }
        }
        public static bool Chequeue
        {
            get
            {
                return (ConfigurationManager.AppSettings.Get("Chequeue").ToLower() == "true");

            }
        }
        
        public static bool LoanPaidandAmortization
        {
            get
            {
                return (ConfigurationManager.AppSettings.Get("LoanPaidandAmortization").ToLower() == "true");

            }
        }
        public static bool ReceivePaymentReport
        {
            get
            {
                return (ConfigurationManager.AppSettings.Get("ReceivePaymentReport").ToLower() == "true");

            }
        }
        public static bool ContributionFromPayroll
        {
            get
            {
                return (ConfigurationManager.AppSettings.Get("ContributionFromPayroll").ToLower() == "true");

            }
        }
        public static bool InstrumentAccruedProcess
        {
            get
            {
                return (ConfigurationManager.AppSettings.Get("InstrumentAccruedProcess").ToLower() == "true");

            }
        }

        public static bool Forfeiture
        {
            get
            {
                return (ConfigurationManager.AppSettings.Get("Forfeiture").ToLower() == "true");

            }
        }
        public static string DbBackUpPath
        {
            get
            {
                return (ConfigurationManager.AppSettings.Get("DbBackUpPath"));

            }
        }
        public static string DbBackUpConnection
        {
            get
            {
                return (ConfigurationManager.AppSettings.Get("DbBackUpConnection"));

            }
        }

        public static bool CashFlow
        {
            get
            {
                return (ConfigurationManager.AppSettings.Get("CashFlow").ToLower() == "true");

            }
        }
        public static bool Subsidiary
        {
            get
            {
                return (ConfigurationManager.AppSettings.Get("Subsidiary").ToLower() == "true");

            }
        }
        public static bool CheckPrint
        {
            get
            {
                return (ConfigurationManager.AppSettings.Get("CheckPrint").ToLower() == "true");

            }
        }
    }
}
