using DLL.ViewModel;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLL.Utility
{
    public class Cheque_BankInfo_DAL
    {
        PFTMEntities _context = new PFTMEntities();

        //internal int InsertBankInfo(Ac_Cheque_BankInfo aAc_Cheque_BankInfo)
        //{
        //    try
        //    {
        //        _context.Ac_Cheque_BankInfo.AddObject(aAc_Cheque_BankInfo);
        //        _context.SaveChanges();
        //        return 1;
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }

        //}

        internal List<Ac_Cheque_BankInfo> GetBankInfo(string Ocode)
        {
            try
            {
                var Banks = (from Bank in _context.Ac_Cheque_BankInfo
                             where Bank.OCode == Ocode
                             select Bank).OrderBy(x => x.BankInfo_Id);
                return Banks.ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        //internal int InsertChequeInfo(Ac_Cheque_Print aAc_Cheque_Print)
        //{
        //    try
        //    {
        //        _context.Ac_Cheque_Print.AddObject(aAc_Cheque_Print);
        //        _context.SaveChanges();
        //        return 1;
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        internal List<Ac_Cheque_BankInfo> GetBankName(string OCODE)
        {
            try
            {
                var query = (from bank in _context.Ac_Cheque_BankInfo
                             where bank.OCode == OCODE
                             select bank).OrderBy(x => x.BankInfo_Id);


                return query.ToList();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message.ToString());
            }

        }

        internal List<Ac_Cheque_ClientInfo> GetParty(string OCODE)
        {
            try
            {
                var query = (from bank in _context.Ac_Cheque_ClientInfo
                             where bank.OCode == OCODE
                             select bank).OrderBy(x => x.ClientInfo_id);


                return query.ToList();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message.ToString());
            }
        }

        internal IEnumerable<ChequeR> GetChequeDetails(string chequeno, int OCODE)
        {
            List<ChequeR> cheque = (from p in _context.Ac_Cheque_Print
                                   join b in _context.Ac_Cheque_BankInfo on p.BankInfo_Id equals b.BankInfo_Id
                                   join c in _context.Ac_Cheque_ClientInfo on p.ClientInfo_id equals c.ClientInfo_id
                                   where p.ChequeNo == chequeno 
                                   &&
                                         p.OCode == OCODE

                                   select new ChequeR
                                   {
                                       BankName = b.BankName,
                                       ChequeNo = p.ChequeNo,
                                       AccountNo = b.AccountNo,
                                       Amount = p.Amount,
                                       ClientName = c.ClientName,
                                       ChequeDate = p.ChequeDate
                                   }).ToList();
            return cheque;
        }

        internal List<ChequeR> GetAc_Rpt_ChequePrint(int empid, int OCODE)
        {
            try
            {
                using (var _context = new PFTMEntities())
                {
                    var cheque_no = new SqlParameter("@EmpId", empid);

                    var O_CODE = new SqlParameter("@OCODE", OCODE);
                    //string SP_SQL = "Ac_Rpt_ChequePrint @ChequeNo,@OCODE";
                    var data = _context.Database.SqlQuery<ChequeR>("Ac_Rpt_ChequePrint @EmpId, @OCODE", empid, OCODE).ToList();
                    return data;
                        //(_context.ExecuteStoreQuery<ChequeR>(SP_SQL, cheque_no, O_CODE)).ToList();
                }

            }
            catch (Exception)
            {
                throw;
            }
        }

        //internal int InsertClientInfo(Ac_Cheque_ClientInfo aAc_Cheque_ClientInfo)
        //{
        //    _context.Ac_Cheque_ClientInfo.AddObject(aAc_Cheque_ClientInfo);
        //    _context.SaveChanges();
        //    return 1;
        //}

        internal List<Ac_Cheque_ClientInfo> GetClientInfo(string Ocode)
        {
            try
            {
                var quary = (from client in _context.Ac_Cheque_ClientInfo
                             where client.OCode == Ocode
                             select client).OrderBy(x => x.ClientInfo_id);
                return quary.ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        internal List<Ac_Cheque_BankInfo> GetBankInfoByIdandOcode(string bankId, string OCODE)
        {

            try
            {
                int bId = Convert.ToInt32(bankId);
                var query = (from bnk in _context.Ac_Cheque_BankInfo
                             where bnk.OCode == OCODE && bnk.BankInfo_Id == bId
                             select bnk).OrderBy(bnk => bnk.BankInfo_Id);

                return query.ToList();



            }
            catch (Exception)
            {

                throw;
            }
        }

        internal int UpdateBankInfo(Ac_Cheque_BankInfo aAc_Cheque_BankInfo, int bankId)
        {
            Ac_Cheque_BankInfo aAc_Cheque_Bank = _context.Ac_Cheque_BankInfo.First(x => x.BankInfo_Id == bankId);
            aAc_Cheque_Bank.BankName = aAc_Cheque_BankInfo.BankName;
            aAc_Cheque_Bank.AccountName = aAc_Cheque_BankInfo.AccountName;
            aAc_Cheque_Bank.AccountNo = aAc_Cheque_BankInfo.AccountNo;
            aAc_Cheque_Bank.EditDate = aAc_Cheque_BankInfo.EditDate;
            aAc_Cheque_Bank.EditUser = aAc_Cheque_BankInfo.EditUser;
            aAc_Cheque_Bank.OCode = aAc_Cheque_BankInfo.OCode;
            _context.SaveChanges();
            return 1;
        }

        internal List<Ac_Cheque_ClientInfo> GetClientByIdandOcode(string ClientId, string OCODE)
        {
            int clnID = Convert.ToInt32(ClientId);
            var quary = (from client in _context.Ac_Cheque_ClientInfo
                         where client.ClientInfo_id == clnID && client.OCode == OCODE
                         select client).OrderBy(x => x.ClientInfo_id);

            return quary.ToList();

        }

        internal int UpdateClientInfo(Ac_Cheque_ClientInfo aAc_Cheque_ClientInfo, int clintid)
        {
            Ac_Cheque_ClientInfo aAc_Cheque_Client = _context.Ac_Cheque_ClientInfo.First(x => x.ClientInfo_id == clintid);

            aAc_Cheque_Client.ClientName = aAc_Cheque_ClientInfo.ClientName;
            aAc_Cheque_Client.Address = aAc_Cheque_ClientInfo.Address;
            aAc_Cheque_Client.ContactNumber = aAc_Cheque_ClientInfo.ContactNumber;
            aAc_Cheque_Client.EditDate = aAc_Cheque_ClientInfo.EditDate;
            aAc_Cheque_Client.EditUser = aAc_Cheque_ClientInfo.EditUser;
            aAc_Cheque_Client.OCode = aAc_Cheque_ClientInfo.OCode;
            _context.SaveChanges();
            return 1;
        }
    }
}
