using DLL.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLL.Utility
{
    public class Cheque_BankInfo_BLL
    {
        Cheque_BankInfo_DAL aCheque_BankInfo_DAL = new Cheque_BankInfo_DAL();

        //internal int InsertBankInfo(Ac_Cheque_BankInfo aAc_Cheque_BankInfo)
        //{
        //    return aCheque_BankInfo_DAL.InsertBankInfo(aAc_Cheque_BankInfo);
        //}

        internal List<Ac_Cheque_BankInfo> GetBankInfo(string Ocode)
        {
            return aCheque_BankInfo_DAL.GetBankInfo(Ocode);
        }

        //internal int InsertChequeInfo(Ac_Cheque_Print aAc_Cheque_Print)
        //{
        //    return aCheque_BankInfo_DAL.InsertChequeInfo(aAc_Cheque_Print);
        //}

        internal virtual List<Ac_Cheque_BankInfo> GetBankName(string OCODE)
        {
            return aCheque_BankInfo_DAL.GetBankName(OCODE);
        }

        internal virtual List<Ac_Cheque_ClientInfo> GetParty(string OCODE)
        {
            return aCheque_BankInfo_DAL.GetParty(OCODE);
        }

        internal IEnumerable<ChequeR> GetChequeDetails(string chequeno, string OCODE)
        {
            return aCheque_BankInfo_DAL.GetChequeDetails(chequeno, OCODE);
        }

        //internal List<ChequeR> GetAc_Rpt_ChequePrint(string chequeno, string OCODE)
        //{
        //    return aCheque_BankInfo_DAL.GetAc_Rpt_ChequePrint(chequeno, OCODE);
        //}
        //internal List<ChequeR> GetAc_Rpt_ChequePrint(int empID, int OCODE)
        //{
        //    return aCheque_BankInfo_DAL.GetAc_Rpt_ChequePrint(empID, OCODE);
        //}
        internal List<ChequeR> GetChequePrint(int empId, int OCODE)
        {
            return aCheque_BankInfo_DAL.GetAc_Rpt_ChequePrint(empId, OCODE);
        }

        //internal int InsertClientInfo(Ac_Cheque_ClientInfo aAc_Cheque_ClientInfo)
        //{
        //    return aCheque_BankInfo_DAL.InsertClientInfo(aAc_Cheque_ClientInfo);
        //}

        internal List<Ac_Cheque_ClientInfo> GetClientInfo(string Ocode)
        {
            return aCheque_BankInfo_DAL.GetClientInfo(Ocode);
        }

        internal List<Ac_Cheque_BankInfo> GetBankInfoByIdandOcode(string bankId, string OCODE)
        {
            return aCheque_BankInfo_DAL.GetBankInfoByIdandOcode(bankId, OCODE);
        }
        internal int UpdateBankInfo(Ac_Cheque_BankInfo aAc_Cheque_BankInfo, int bankId)
        {
            return aCheque_BankInfo_DAL.UpdateBankInfo(aAc_Cheque_BankInfo, bankId);
        }

        internal List<Ac_Cheque_ClientInfo> GetClientByIdandOcode(string ClientId, string OCODE)
        {
            return aCheque_BankInfo_DAL.GetClientByIdandOcode(ClientId, OCODE);
        }



        internal int UpdateClientInfo(Ac_Cheque_ClientInfo aAc_Cheque_ClientInfo, int clintid)
        {
            return aCheque_BankInfo_DAL.UpdateClientInfo(aAc_Cheque_ClientInfo, clintid);
        }
    }
}
