using System;
using System.Collections.Generic;
using System.Linq;
using DLL.ViewModel;
using System.Data.SqlClient;
using System.Data;

namespace DLL.Repository
{
    public class AccountingRepository : IDisposable
    {
        private PFTMEntities context;

        //<ModifyBy>Avishek<ModifyBy>
        //<ModificationDate>6-Mar-2016<ModificationDate>
        #region start Modification
        public AccountingRepository(PFTMEntities context)
        {
            this.context = context;
        }

        public IQueryable<VM_acc_group> GetGroup()
        {
            var v = from a in context.acc_Group
                    join b in context.acc_Nature on a.NatureID equals b.NatureID into lfNatureGroup
                    from e in lfNatureGroup.DefaultIfEmpty()
                    select new VM_acc_group
                    {
                        GroupID = a.GroupID,
                        GroupName = a.GroupName,
                        NatureID = e == null ? 0 : e.NatureID,
                        NatureName = e == null ? string.Empty : e.NatureName,
                        ParentGroupID = a.ParentGroupID,
                        EditDate = a.EditDate ?? DateTime.MinValue,
                        EditUser = a.EditUser ?? Guid.Empty,
                        OCode = a.OCode,
                        GroupCode = a.GroupCode
                    };
            return v;
        }

        public IQueryable<VM_acc_ledger> GetLedger()
        {
            var v = from a in context.acc_Ledger
                    join b in context.acc_Group on a.GroupID equals b.GroupID
                    select new VM_acc_ledger
                    {
                        GroupID = b.GroupID,
                        LedgerID = a.LedgerID,
                        LedgerName = a.LedgerName,
                        GroupName = b.GroupName,
                        InitialBalance = a.InitialBalance,
                        BalanceType = a.BalanceType,
                        EditDate = a.EditDate ?? DateTime.MinValue,
                        EditUser = a.EditUser ?? Guid.Empty,
                        LedgerCode = a.LedgerCode
                    };
            return v;
        }

        public IEnumerable<VM_acc_VoucherDetail> GetVoucherDetail(int voucherId)
        {
            var v = (from a in context.acc_VoucherDetail
                     join b in context.acc_Ledger on a.LedgerID equals b.LedgerID
                     where a.VoucherID == voucherId
                     select new VM_acc_VoucherDetail
                     {
                         VoucherDetailID = a.VoucherDetailID,
                         ChequeNumber = a.ChequeNumber,
                         Credit = a.Credit ?? 0,
                         Debit = a.Debit ?? 0,
                         LedgerName = b.LedgerName,
                         LedgerID = a.LedgerID,
                         VoucherID = voucherId,
                         EditDate = a.EditDate ?? DateTime.MinValue,
                         EditUser = a.EditUser ?? Guid.Empty
                     }).OrderByDescending(x => x.Debit).Where(x => x.Credit != 0 || x.Debit != 0);
            return v;
        }

        public IEnumerable<VM_acc_VoucherDetail> GetVoucherDetail(int voucherId, int ocode)
        {
            var v = (from a in context.acc_VoucherDetail
                     where a.OCode == ocode
                     join b in context.acc_Ledger on a.LedgerID equals b.LedgerID
                     where a.VoucherID == voucherId
                     select new VM_acc_VoucherDetail
                     {
                         VoucherDetailID = a.VoucherDetailID,
                         ChequeNumber = a.ChequeNumber,
                         Credit = a.Credit ?? 0,
                         Debit = a.Debit ?? 0,
                         LedgerName = b.LedgerName,
                         LedgerID = a.LedgerID,
                         VoucherID = voucherId,
                         EditDate = a.EditDate ?? DateTime.MinValue,
                         EditUser = a.EditUser ?? Guid.Empty
                     }).OrderByDescending(x => x.Debit).Where(x => x.Credit != 0 || x.Debit != 0);
            return v;
        }

        public void sp_GetDifferenceInitialBalance(out decimal amount)
        {
            SqlParameter oAmount = new SqlParameter("oAmount", SqlDbType.Decimal)
            {
                Direction = ParameterDirection.Output
            };

            context.Database.ExecuteSqlCommand("sp_GetDifferenceInitialBalance @oAmount out", oAmount);

            amount = (decimal)oAmount.Value;
        }

        public IEnumerable<VM_acc_VoucherDetail> sp_GetLedgerTransactionDetail(Guid ledgerId, DateTime fromDate, DateTime toDate, int OCode)
        {
            SqlParameter lID = new SqlParameter("@ledgerId", new Guid(ledgerId + ""));
            SqlParameter f = new SqlParameter("@fromDate", fromDate);
            SqlParameter t = new SqlParameter("@toDate", toDate);
            SqlParameter oCode = new SqlParameter("@oCode", OCode);
            var result = context.Database.SqlQuery<VM_acc_VoucherDetail>("sp_GetLedgerTransactionDetail @ledgerId, @fromDate, @toDate, @oCode", lID, f, t, oCode).ToList();

            //now I need particulars information for based on voucherID from above result set

            //now find distinct voucher id used in above transaction because I need all related ledgerName which are related to above record! 
            //this related ledger name will be showed as particular!
            List<int> voucherIdList = new List<int>();
            foreach (var r in result)
            {
                if (!voucherIdList.Contains(r.VoucherID))
                {
                    voucherIdList.Add(r.VoucherID);
                }
            }

            var relatedLedger = from a in context.acc_VoucherDetail
                                join b in context.acc_Ledger on a.LedgerID equals b.LedgerID
                                where voucherIdList.Contains(a.VoucherID)
                                select new
                                {
                                    ldgrID = a.LedgerID,
                                    vouchrID = a.VoucherID,
                                    ldgrName = b.LedgerName
                                };
            foreach (var r in result)
            {
                var temp = relatedLedger.Where(w => w.vouchrID == r.VoucherID);
                string temp_s = "";
                foreach (var s in temp)
                {
                    if (r.LedgerID != s.ldgrID)
                    {
                        temp_s += s.ldgrName + ", ";
                    }
                }
                temp_s = temp_s.Trim(' ').Trim(',');
                r.Particulars = temp_s;
            }
            return result;
        }

        public void sp_GetTransactionBalanceBeforeDate(Guid ledgerID, DateTime fromDate, out decimal initialBalance, out int initialBalanceType, out decimal creditBalanceBeforeDate, out decimal debitBalanceBeforeDate, out string groupName, int OCode)
        {
            SqlParameter lID = new SqlParameter("@ledgerId", new Guid(ledgerID + ""));
            SqlParameter f = new SqlParameter("@fromDate", fromDate);
            SqlParameter oCode = new SqlParameter("@oCode", (int)OCode);
            SqlParameter oInitialBalance = new SqlParameter("oInitialBalance", SqlDbType.Decimal)
            {
                Direction = ParameterDirection.Output
            };
            SqlParameter oInitialBalanceType = new SqlParameter("oInitialBalanceType", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };
            SqlParameter oCreditBalanceBeforeDate = new SqlParameter("oCreditBalanceBeforeDate", SqlDbType.Decimal)
            {
                Direction = ParameterDirection.Output
            };
            SqlParameter oDebitBalanceBeforeDate = new SqlParameter("oDebitBalanceBeforeDate", SqlDbType.Decimal)
            {
                Direction = ParameterDirection.Output
            };
            SqlParameter oGroupName = new SqlParameter("oGroupName", SqlDbType.VarChar, 50)
            {
                Direction = ParameterDirection.Output
            };

            context.Database.ExecuteSqlCommand("sp_GetTransactionBalanceBeforeDate @ledgerId, @fromDate, @oCode, @oInitialBalance out, @oInitialBalanceType out, @oCreditBalanceBeforeDate out, @oDebitBalanceBeforeDate out, @oGroupName out", lID, f, oCode, oInitialBalance, oInitialBalanceType, oCreditBalanceBeforeDate, oDebitBalanceBeforeDate, oGroupName);
            initialBalance = (decimal)oInitialBalance.Value;
            creditBalanceBeforeDate = (decimal)oCreditBalanceBeforeDate.Value;
            debitBalanceBeforeDate = (decimal)oDebitBalanceBeforeDate.Value;
            initialBalanceType = (int)oInitialBalanceType.Value;
            groupName = (string)oGroupName.Value;
        }

        //difference between cashbook and cashbalance is cashbook contains detail but cash balance contains groupby ledger sum the balance
        public List<VM_acc_VoucherDetail> GenerateCashBalance(DateTime fromDate, DateTime toDate, int oCode)
        {
            List<int> groupIdList = CashAccountGroupList();
            //now find all those ledger which are related to the above group list
            List<Guid> ledgerList = context.acc_Ledger.Where(w => groupIdList.Contains(w.GroupID)).Select(s => s.LedgerID).ToList();
            var v = from a in context.acc_Ledger
                    join b in context.acc_VoucherDetail.Where(w => w.TransactionDate <= toDate && w.TransactionDate >= fromDate && ledgerList.Contains(w.LedgerID) && w.OCode == oCode) on a.LedgerID equals b.LedgerID
                    group b by new { b.LedgerID, a.LedgerName, a.InitialBalance, a.GroupID } into grp
                    select new VM_acc_VoucherDetail
                    {
                        LedgerID = grp.Key.LedgerID,
                        LedgerName = grp.Key.LedgerName,
                        InitialBalance = grp.Key.InitialBalance,
                        GroupID = grp.Key.GroupID,
                        Credit = grp.Sum(s => s.Credit) ?? 0,
                        Debit = grp.Sum(s => s.Debit) ?? 0
                    };
            return v.ToList();
        }

        #region Generate cashbook : Need to review this process
        public List<VM_acc_VoucherDetail> GenerateCacheBook(DateTime fromDate, DateTime toDate, int OCode)
        {
            //first I need all th0ose group which are under ParentGroup => cash and cash equity
            List<int> groupIdList = CashAccountGroupList();
            //now find all those ledger which are related to the above group list
            List<Guid> ledgerList = context.acc_Ledger.Where(w => groupIdList.Contains(w.GroupID)).Select(s => s.LedgerID).ToList();

            //now find transaction detail for all those ledger
            var v = from a in context.acc_VoucherDetail
                    join b in context.acc_Ledger on a.LedgerID equals b.LedgerID
                    join d in context.acc_Group on b.GroupID equals d.GroupID
                    join c in context.acc_VoucherEntry on a.VoucherID equals c.VoucherID
                    where ledgerList.Contains(a.LedgerID)
                    && a.TransactionDate >= fromDate && a.TransactionDate <= toDate
                    && c.OCode == OCode

                    select new VM_acc_VoucherDetail
                    {
                        VoucherDetailID = a.VoucherDetailID,
                        ChequeNumber = a.ChequeNumber,
                        Credit = a.Credit ?? 0,
                        Debit = a.Debit ?? 0,
                        LedgerName = b.LedgerName,
                        LedgerID = a.LedgerID,
                        VoucherID = c.VoucherID,
                        VNumber = c.VNumber,
                        TransactionDate = c.TransactionDate ?? c.TransactionDate.Value,
                        InitialBalance = b.InitialBalance ?? 0,
                        Narration = c.Narration,
                        GroupName = d.GroupName
                    };
            List<VM_acc_VoucherDetail> result = v.ToList();


            //now find distinct voucher id used in above transaction because I need all related ledgerName which are related to above record! 
            //this related ledger name will be showed as particular!
            List<int> voucherIdList = new List<int>();
            foreach (var f in result.Where(f => !voucherIdList.Contains(f.VoucherID)))
            {
                voucherIdList.Add(f.VoucherID);
            }

            var relatedLedger = from a in context.acc_VoucherDetail
                                join b in context.acc_Ledger on a.LedgerID equals b.LedgerID
                                where voucherIdList.Contains(a.VoucherID)
                                select new
                                {
                                    ldgrID = a.LedgerID,
                                    vouchrID = a.VoucherID,
                                    ldgrName = b.LedgerName
                                };
            foreach (var f in result)
            {
                var temp = relatedLedger.Where(w => w.vouchrID == f.VoucherID);
                string tempS = "";
                foreach (var s in temp)
                {
                    if (f.LedgerID != s.ldgrID)
                    {
                        tempS += s.ldgrName + ", ";
                    }
                }
                tempS = tempS.Trim(' ').Trim(',');
                f.Particulars = tempS;
            }
            return result;
        }
        #endregion

        public List<VM_acc_VoucherDetail> GenerateBalanceBook(Guid? ledgerID, int? natureID, int? groupID, DateTime fromDate, DateTime toDate, int OCode)
        {
            var v = from aa in
                        (
                            from a in context.acc_Ledger
                            join b in context.acc_VoucherDetail.Where(w => w.TransactionDate >= fromDate && w.TransactionDate <= toDate && w.OCode == OCode) on a.LedgerID equals b.LedgerID into grp1
                            from c in grp1.DefaultIfEmpty()
                            group c by new { a.LedgerID, a.LedgerName, a.InitialBalance, a.GroupID, a.ParentLedgerID } into grp
                            select new
                            {
                                ldgerID = grp.Key.LedgerID,
                                ldgerName = grp.Key.LedgerName,
                                initBalance = grp.Key.InitialBalance,
                                grpID = grp.Key.GroupID,
                                crdt = grp.Sum(s => s.Credit),
                                dbt = grp.Sum(s => s.Debit),
                                prntLedgerID = grp.Key.ParentLedgerID
                            }
                        )
                    join c in context.acc_Group on aa.grpID equals c.GroupID
                    join d in context.acc_Nature on c.NatureID equals d.NatureID

                    select new VM_acc_VoucherDetail
                    {
                        InitialBalance = aa.initBalance,
                        Credit = aa.crdt ?? 0,
                        Debit = aa.dbt ?? 0,
                        LedgerName = aa.ldgerName,
                        LedgerID = aa.ldgerID,
                        GroupName = c.GroupName,
                        GroupID = c.GroupID,
                        NatureName = d.NatureName,
                        NatureID = d.NatureID,
                        ParentLedgerID = aa.prntLedgerID
                    };
            if (ledgerID != null)
            {
                v = v.Where(w => w.LedgerID == ledgerID);
            }
            if (natureID != null)
            {
                v = v.Where(w => w.NatureID == natureID);
            }
            if (groupID != null)
            {
                v = v.Where(w => w.GroupID == groupID);
            }

            var result = v.ToList();
            var onlyParentLedger = result.Where(w => w.ParentLedgerID == null).ToList();
            foreach (var item in onlyParentLedger)
            {
                item.Credit += result.Where(p => p.ParentLedgerID == item.LedgerID).GroupBy(g => g.ParentLedgerID).Select(s => s.Sum(f => f.Credit)).FirstOrDefault();
                item.Debit += result.Where(p => p.ParentLedgerID == item.LedgerID).GroupBy(g => g.ParentLedgerID).Select(s => s.Sum(f => f.Debit)).FirstOrDefault();
                item.InitialBalance += (result.Where(p => p.ParentLedgerID == item.LedgerID).GroupBy(g => g.ParentLedgerID).Select(s => s.Sum(f => f.InitialBalance)).FirstOrDefault()) ?? 0;
            }

            return onlyParentLedger;
        }

        #region Generate balance book version 2
        public List<VM_acc_VoucherDetail> GenerateBalanceBook2(Guid? ledgerID, int? natureID, int? groupID, DateTime fromDate, DateTime toDate, int OCode)
        {
            var v = from aa in
                        (
                            from a in context.acc_Ledger
                            join b in context.acc_VoucherDetail.Where(w => w.TransactionDate >= fromDate && w.TransactionDate <= toDate && w.OCode == OCode) on a.LedgerID equals b.LedgerID

                            group b by new { a.LedgerID, a.LedgerName, a.InitialBalance, a.GroupID, a.ParentLedgerID } into grp
                            select new
                            {
                                ldgerID = grp.Key.LedgerID,
                                ldgerName = grp.Key.LedgerName,
                                //initBalance = grp.Key.InitialBalance, //in trial balance concept initial balance not required
                                grpID = grp.Key.GroupID,
                                crdt = grp.Sum(s => s.Credit),
                                dbt = grp.Sum(s => s.Debit),
                                prntLedgerID = grp.Key.ParentLedgerID
                            }
                        )
                    join c in context.acc_Group on aa.grpID equals c.GroupID
                    join d in context.acc_Nature on c.NatureID equals d.NatureID

                    select new VM_acc_VoucherDetail
                    {
                        //InitialBalance = aa.initBalance,
                        Credit = aa.crdt ?? 0,
                        Debit = aa.dbt ?? 0,
                        LedgerName = aa.ldgerName,
                        LedgerID = aa.ldgerID,
                        GroupName = c.GroupName,
                        GroupID = c.GroupID,
                        NatureName = d.NatureName,
                        NatureID = d.NatureID,
                        ParentLedgerID = aa.prntLedgerID
                    };
            if (ledgerID != null)
            {
                v = v.Where(w => w.LedgerID == ledgerID);
            }
            if (natureID != null)
            {
                v = v.Where(w => w.NatureID == natureID);
            }
            if (groupID != null)
            {
                v = v.Where(w => w.GroupID == groupID);
            }

            var result = v.ToList();
            var onlyParentLedger = result.Where(w => w.ParentLedgerID == null).ToList();
            foreach (var item in onlyParentLedger)
            {
                item.Credit += result.Where(p => p.ParentLedgerID == item.LedgerID).GroupBy(g => g.ParentLedgerID).Select(s => s.Sum(f => f.Credit)).FirstOrDefault();
                item.Debit += result.Where(p => p.ParentLedgerID == item.LedgerID).GroupBy(g => g.ParentLedgerID).Select(s => s.Sum(f => f.Debit)).FirstOrDefault();
                item.InitialBalance += (result.Where(p => p.ParentLedgerID == item.LedgerID).GroupBy(g => g.ParentLedgerID).Select(s => s.Sum(f => f.InitialBalance)).FirstOrDefault()) ?? 0;
            }

            return onlyParentLedger;
        }
        #endregion

        public decimal GetRetainEarningOpening(DateTime toDate, int OCode) // :P 
        {
            var v = from aa in
                        (
                            from a in context.acc_Ledger
                            join b in context.acc_VoucherDetail.Where(w => w.TransactionDate <= toDate && w.OCode == OCode) on a.LedgerID equals b.LedgerID
                            group b by new { b.LedgerID, a.LedgerName, a.InitialBalance, a.GroupID } into grp
                            select new
                            {
                                ldgerID = grp.Key.LedgerID,
                                ldgerName = grp.Key.LedgerName,
                                initBalance = grp.Key.InitialBalance,
                                grpID = grp.Key.GroupID,
                                crdt = grp.Sum(s => s.Credit),
                                dbt = grp.Sum(s => s.Debit)
                            }
                        )
                    join c in context.acc_Group on aa.grpID equals c.GroupID
                    join d in context.acc_Nature on c.NatureID equals d.NatureID
                    select new VM_acc_VoucherDetail
                    {
                        InitialBalance = aa.initBalance,
                        Credit = aa.crdt ?? 0,
                        Debit = aa.dbt ?? 0,
                        NatureID = d.NatureID
                    };
            var rv = v.Where(w => w.NatureID == 4).ToList(); // 4 for rev
            decimal income = rv.Aggregate<VM_acc_VoucherDetail, decimal>(0, (current, r) => current + ((r.InitialBalance ?? 0) + r.Credit - r.Debit));

            var ex = v.Where(w => w.NatureID == 3).ToList(); // 3 for exp
            decimal expense = ex.Aggregate<VM_acc_VoucherDetail, decimal>(0, (current, e) => current + ((e.InitialBalance ?? 0) - e.Credit + e.Debit));

            return income - expense;
        }
        // Added By Kamrul 2019-02-27 For showing Retained Earning from Ledger Initial Balance
        public decimal GetRetainEarningOpeningLedger(int OCode) // :P 
        {
            decimal v = context.acc_Ledger.Where(t => t.LedgerName == "Retain earning opening").FirstOrDefault().InitialBalance ?? 0;
                           
                           
            return v;
        }
        // End Kamrul
        public decimal GetRetainEarningPeriod(DateTime fromDate, DateTime toDate, int OCode) // :P 
        {
            toDate = toDate.AddHours(23).AddMinutes(59).AddSeconds(59);

            var v = from aa in
                        (
                            from a in context.acc_Ledger
                            join b in context.acc_VoucherDetail.Where(w => w.TransactionDate <= toDate && w.TransactionDate >= fromDate && w.OCode == OCode) on a.LedgerID equals b.LedgerID
                            group b by new { b.LedgerID, a.LedgerName, a.InitialBalance, a.GroupID } into grp
                            select new
                            {
                                ldgerID = grp.Key.LedgerID,
                                ldgerName = grp.Key.LedgerName,
                                initBalance = grp.Key.InitialBalance,
                                grpID = grp.Key.GroupID,
                                crdt = grp.Sum(s => s.Credit),
                                dbt = grp.Sum(s => s.Debit)
                            }
                        )
                    join c in context.acc_Group on aa.grpID equals c.GroupID
                    join d in context.acc_Nature on c.NatureID equals d.NatureID
                    select new VM_acc_VoucherDetail
                    {
                        InitialBalance = aa.initBalance,
                        Credit = aa.crdt ?? 0,
                        Debit = aa.dbt ?? 0,
                        NatureID = d.NatureID
                    };
            var rv = v.Where(w => w.NatureID == 4).ToList();
            //remember revenue credit nature
            decimal income = rv.Aggregate<VM_acc_VoucherDetail, decimal>(0, (current, r) => current + ((r.InitialBalance ?? 0) + r.Credit - r.Debit));

            var ex = v.Where(w => w.NatureID == 3).ToList();

            //remember expense is debit nature
            decimal expense = ex.Aggregate<VM_acc_VoucherDetail, decimal>(0, (current, e) => current + ((e.InitialBalance ?? 0) - e.Credit + e.Debit));
            return income - expense;
        }

        public IEnumerable<VM_acc_VoucherDetail> sp_GenerateTrialBalance(Guid? ledgerID, int? natureID, int? groupID, int OCode)
        {
            SqlParameter lID = new SqlParameter("@ledgerId", ledgerID);
            SqlParameter g = new SqlParameter("@groupID", groupID);
            SqlParameter n = new SqlParameter("@natureID", natureID);
            SqlParameter f = new SqlParameter("@fromDate", "01/01/2014");
            SqlParameter t = new SqlParameter("@toDate", "01/01/2018");
            SqlParameter oCode = new SqlParameter("@oCode", (int)OCode);
            var result = context.Database.SqlQuery<VM_acc_VoucherDetail>("sp_GenerateTrialBalance @ledgerId, @groupID, @natureID, @fromDate, @toDate, @oCode", lID, groupID, natureID, f, t, oCode).ToList();
            return result;
        }

        public List<int> CashAccountGroupList()
        {
            string query = @"with [CTE] as (
                                select GroupID from [dbo].[acc_Group] c where c.[ParentGroupID] = 1 or c.GroupID = 1
                                union all
                                select c.GroupID from [CTE] p, [dbo].[acc_Group] c where c.[ParentGroupID] = p.[GroupID]
                                and c.[ParentGroupID] <> c.[GroupID]
                                )
                                select * from [CTE] order by GroupID";

            var res = context.Database.SqlQuery<int>(query).ToList();

            List<int> groupList = res.ToList<int>();
            return groupList;
        }

        public List<int> CashAccountGroupList(int OCode)
        {
            string query = @"with [CTE] as (
                                select GroupID from [dbo].[acc_Group] c where c.[ParentGroupID] = 1 or c.GroupID = 1
                                union all
                                select c.GroupID from [CTE] p, [dbo].[acc_Group] c where c.[ParentGroupID] = p.[GroupID]
                                and c.[ParentGroupID] <> c.[GroupID]
                                )
                                select * from [CTE] order by GroupID";

            var res = context.Database.SqlQuery<int>(query).ToList();

            List<int> groupList = res.ToList<int>();
            return groupList;
        }

        public List<VM_acc_VoucherDetail> GenerateRP(DateTime fromDate, DateTime toDate, int VTypeID, int OCode)
        {
            var v = from aa in
                        (
                            from a in context.acc_Ledger
                            join b in context.acc_VoucherDetail.Where(w => w.TransactionDate <= toDate && w.TransactionDate >= fromDate && w.VTypeID == VTypeID && w.OCode == OCode) on a.LedgerID equals b.LedgerID
                            group b by new { b.LedgerID, a.LedgerName, a.InitialBalance, a.GroupID } into grp
                            select new
                            {
                                ldgerID = grp.Key.LedgerID,
                                ldgerName = grp.Key.LedgerName,
                                initBalance = grp.Key.InitialBalance,
                                grpID = grp.Key.GroupID,
                                crdt = grp.Sum(s => s.Credit),
                                dbt = grp.Sum(s => s.Debit),

                            }
                        )
                    //join c in context.acc_Group on aa.grpID equals c.GroupID
                    //join d in context.acc_Nature on c.NatureID equals d.NatureID

                    select new VM_acc_VoucherDetail
                    {
                        InitialBalance = aa.initBalance,
                        Credit = aa.crdt ?? 0,
                        Debit = aa.dbt ?? 0,
                        LedgerName = aa.ldgerName,
                        LedgerID = aa.ldgerID,
                    };
            return v.ToList();
        }

        public List<VM_acc_ledger> sp_ChartOfAccount(int OCode)
        {
            var result = context.Database.SqlQuery<VM_acc_ledger>("sp_ChartOfAccount").Where(x => x.OCode == OCode).ToList();
            return result;
        }

        public List<VM_acc_group> sp_GroupTree()
        {
            var result = context.Database.SqlQuery<VM_acc_group>("sp_GroupTree").ToList();
            return result;
        }

        public List<VM_GroupSummary> sp_GetGroupSummaryByNatureID(DateTime fromDate, DateTime toDate, int natureID, int OCode)
        {
            SqlParameter nID = new SqlParameter("@natureID", (int)natureID);
            SqlParameter fDate = new SqlParameter("@fromDate", (DateTime)fromDate);
            SqlParameter tDate = new SqlParameter("@toDate", (DateTime)toDate);
            SqlParameter oCode = new SqlParameter("@oCode", (int)OCode);
            var result = context.Database.SqlQuery<VM_GroupSummary>("sp_GetGroupSummary  @fromDate, @toDate, @natureID, @oCode", fDate, tDate, nID, oCode).ToList();
            return result;
        }

        public List<VM_GroupSummary> sp_GetGroupSummaryAll(DateTime fromDate, DateTime toDate, int OCode)
        {

            SqlParameter fDate = new SqlParameter("@fromDate", (DateTime)fromDate);
            SqlParameter tDate = new SqlParameter("@toDate", (DateTime)toDate);
            SqlParameter oCode = new SqlParameter("@oCode", (int)OCode);
            var result = context.Database.SqlQuery<VM_GroupSummary>("sp_GetGroupSummary  @fromDate, @toDate, @oCode", fDate, tDate, oCode).ToList();
            return result;
        }

        public List<VM_acc_ledger> sp_GetLedgerSummaryAll(DateTime fromDate, DateTime toDate, int OCode)
        {
            SqlParameter fDate = new SqlParameter("@fromDate", (DateTime)fromDate);
            SqlParameter tDate = new SqlParameter("@toDate", (DateTime)toDate);
            SqlParameter oCode = new SqlParameter("@oCode", (int)OCode);
            var result = context.Database.SqlQuery<VM_acc_ledger>("sp_GetLedgerSummaryAll  @fromDate, @toDate, @oCode", fDate, tDate, oCode).ToList();
            return result;
        }

        public List<VM_acc_ledger> sp_GetLedgerSummary(DateTime fromDate, DateTime toDate, int? natureID, int OCode)
        {
            SqlParameter nID = new SqlParameter("@natureID", natureID);
            nID.IsNullable = true;
            SqlParameter fDate = new SqlParameter("@fromDate", (DateTime)fromDate);
            SqlParameter tDate = new SqlParameter("@toDate", (DateTime)toDate);
            SqlParameter oCode = new SqlParameter("@oCode", (int)OCode);

            var result = context.Database.SqlQuery<VM_acc_ledger>("sp_GetLedgerSummary  @fromDate, @toDate, @natureID, @oCode", fDate, tDate, nID, oCode).ToList();
            return result;
        }

        public List<VM_GroupDetail> sp_GetGroupDetail(DateTime fromDate, DateTime toDate, int? groupID)
        {
            List<VM_GroupDetail> result;
            SqlParameter gID = new SqlParameter("@groupID", groupID);
            SqlParameter fDate = new SqlParameter("@fromDate", fromDate);
            SqlParameter tDate = new SqlParameter("@toDate", toDate);
            if (groupID != null)
            {
                result = context.Database.SqlQuery<VM_GroupDetail>("sp_GetGroupDetail  @fromDate, @toDate, @groupID", fDate, tDate, gID).ToList();
            }
            else
            {
                result = context.Database.SqlQuery<VM_GroupDetail>("sp_GetGroupDetail  @fromDate, @toDate", fDate, tDate).ToList();
            }
            return result;
        }

        //public List<VM_acc_VoucherDetail> sp_GetLedgerDetail(DateTime fromDate, DateTime toDate, Guid ledgerId, ref string message)
        //{
        //    SqlParameter lID = new SqlParameter("@ledgerId", new Guid(ledgerId+""));
        //    SqlParameter fDate = new SqlParameter("@fromDate", (DateTime)fromDate);
        //    SqlParameter tDate = new SqlParameter("@toDate", (DateTime)toDate);
        //    try
        //    {
        //        var result = context.Database.SqlQuery<VM_acc_VoucherDetail>("sp_GetLedgerDetail  @fromDate, @toDate, @ledgerId", fDate, tDate, lID).ToList();
        //        return result;
        //    }
        //    catch(Exception x)
        //    {
        //        message = "Error: "+x.Message+"\nInnerException: "+x.InnerException;
        //        return new List<VM_acc_VoucherDetail>();
        //    }

        //}

        #region Accounting API
        /// <summary>
        /// Accounting Ledger Entry! If ledger not exist it will be created! but if group not exist it will not be created! you have to use another method to create group!
        /// </summary>
        /// <param name="ParentLedgerName">Parent Ledger name spelling should be correct. It should be exist in database.</param>
        /// <param name="LedgerName">Ledger name spelling should be correct. It may be exist in database. if not exist it will be created.</param>
        /// <param name="TranType">Transaction Type: Credit or Debit</param>
        /// <param name="TranAmount">Transaction Amount</param>
        /// <param name="Narration">Narration must</param>
        /// <param name="TransactionDate">Datetime</param>
        /// <param name="comment">In ledger table you can keep your comment!</param>
        /// <param name="message">Reference type string. contains any information about operation caller should know</param>
        /// <returns></returns>
        public bool CreateAccountingLedgerEntry(string ParentLedgerName, string LedgerName, string GroupName, string TranType, decimal InitAmount, string comment, ref string message, string EditUserName, Guid EditUserID, string PFMemberID, string PFAdditionalInfo, bool restrictDelete, int OCode)
        {
            //Pre
            Guid editUser = EditUserID;
            Guid parentLedgerId = Guid.Empty;
            //here i've am not using oCode investigate the reason
            int groupId = GetGroupID(GroupName);
            {
                if (groupId == 0)
                {
                    message = "Group ID not exist. you must create " + GroupName + " to continue...";
                    return false;
                }
            }                          
            if (!string.IsNullOrEmpty(ParentLedgerName))
            {
                parentLedgerId = GetLedgerID(ParentLedgerName, OCode);
                if (parentLedgerId == Guid.Empty)
                {
                    message = "Parent Ledger not found! Please create ledger and try again!";
                    return false;
                }
            }
            Guid ledgerId = GetLedgerID(LedgerName, OCode);
            //Ledger need to be created!
            if (ledgerId == Guid.Empty)
            {
                //new ledger entry
                acc_Ledger ledger = new acc_Ledger();
                ledger.LedgerID = Guid.NewGuid();
                ledger.GroupID = groupId;
                ledger.LedgerName = LedgerName;
                ledger.InitialBalance = InitAmount;
                ledger.RestrictDelete = restrictDelete; // this can be deleted!
                if (TranType.ToLower() == "credit")
                {
                    ledger.BalanceType = 1; // 1 for credit
                }
                else if (TranType.ToLower() == "debit")
                {
                    ledger.BalanceType = 2; // 2 for debit
                }
                if (parentLedgerId != Guid.Empty)
                {
                    ledger.ParentLedgerID = parentLedgerId;
                }
                ledger.Comment = comment;
                ledger.EditDate = DateTime.Now;
                ledger.EditUser = editUser;
                ledger.EditUserName = EditUserName;
                ledger.PFMemberID = PFMemberID;
                ledger.PFAdditionalInformation = PFAdditionalInfo;
                ledger.RestrictDelete = true;
                ledger.OCode = OCode;
                context.acc_Ledger.Add(ledger);
            }
            else
            {
                acc_Ledger ledger = context.acc_Ledger.Find(ledgerId);
                //amount is updated not replacing the previous value...
                ledger.InitialBalance = (ledger.InitialBalance ?? 0) + InitAmount;
                if (TranType.ToLower() == "credit")
                {
                    ledger.BalanceType = 1;
                }
                else if (TranType.ToLower() == "debit")
                {
                    ledger.BalanceType = 2;
                }
                ledger.Comment = comment;
                ledger.EditDate = DateTime.Now;
                ledger.EditUser = editUser;
                ledger.EditUserName = EditUserName;
                ledger.PFMemberID = PFMemberID;
                ledger.PFAdditionalInformation = PFAdditionalInfo;
                context.Entry(ledger).State = EntityState.Modified;
            }
            return true;
        }

        public Guid GetLedgerID(string ledgerName, int OCode)
        {
            //oCode added for multi-company functionality
            var v = context.acc_Ledger.Where(w => w.LedgerName == ledgerName.Trim()).ToList();
            if (v.Count == 0)
            {
                return Guid.Empty;
            }
            //let's find it with oCode
            var result = v.SingleOrDefault(f => f.OCode == OCode);
            if (result != null)
            {
                return result.LedgerID;
            }
            //if with oCode not exist let'f find without oCode
            acc_Ledger ledger = v.Where(f => f.OCode == null || OCode == 0).SingleOrDefault();
            if (ledger != null)
            {
                ledger.OCode = OCode;
                return ledger.LedgerID;
            }
            return Guid.Empty;
        }

        public int GetGroupID(string groupName)
        {
            var v = context.acc_Group.FirstOrDefault(w => w.GroupName == groupName.Trim());
            if (v == null)
            {
                return 0;
            }
            return v.GroupID;
        }

        /// <summary>
        /// This method will create a Double Entry Voucher. Say, system voucher, this voucher should not be deleted by user as it has been created from system!
        /// </summary>
        /// <param name="VTypeID">PV(1) RV(2) CV(3) JV(4) SV(5)</param>
        /// <param name="TransactionDate">Date of transaction</param>
        /// <param name="VoucherID">if 0 then new create or will update the existing voucher!</param>
        /// <param name="Narration">narration of that voucher</param>
        /// <param name="LedgerName">name of ledger in list form</param>
        /// <param name="Debit">debit list</param>
        /// <param name="Credit">credit</param>
        /// <param name="ChequeNumber">cheque number as list</param>
        /// <param name="messageToReturn">this message variable must be reference type and you will get the messsae here if any error occured!</param>
        /// <param name="EditUserName">Name of the user</param>
        /// <param name="EditUserID">GUID of that user.</param>
        /// <returns></returns>
        public bool DualEntryVoucher(int EmpID, int VTypeID, DateTime TransactionDate, ref int VoucherID, string Narration, IList<string> LedgerName, IList<decimal> Debit, IList<decimal> Credit, IList<string> ChequeNumber, ref string messageToReturn, string EditUserName, Guid EditUserID, List<string> PFMemberID, string PFAdditionalInfo, string PFMonth, string PFYear, int? processID, List<string> PFLoanID, int OCode, string actionName)
        {
            List<Guid> ledgerId = new List<Guid>();
            if (!(LedgerName.Count == Credit.Count && Credit.Count == Debit.Count && Debit.Count == ChequeNumber.Count))
            {
                messageToReturn = "Ledger name, debit, credit count mismatch!";
                return false;
            }
            foreach (var item in LedgerName)
            {
                Guid guid = GetLedgerID(item, OCode);
                if (guid == Guid.Empty)
                {
                    messageToReturn = item + " - this ledger name not found! you have to create this ledger and try again!";
                    return false;
                }
                ledgerId.Add(guid);
            }
            //now check if debit and credit id equal for contra and journal voucher
            decimal totalDebit = 0;
            decimal totalCredit = 0;
            for (int i = 0; i < Debit.Count; i++)
            {
                if (ledgerId[i] != Guid.Empty) // it's important
                {
                    totalDebit += Debit[i];
                    totalCredit += Credit[i];
                }
            }
            if (totalDebit != totalCredit)
            {
                messageToReturn = "DEBIT and CREDIT should be equal.";
                return false;
            }
            if ((totalCredit < 0) || (totalDebit < 0))
            {
                messageToReturn = "Debit or credit value cannot be 0 or LESS.";
                return false;
            }

            bool atLeastOneEntryFound = false;
            string voucherNumber = "";
            Guid editUser = EditUserID;
            DateTime editDate = DateTime.Now;

            if (VoucherID == 0)
            {
                #region New Voucher Entry
                acc_VoucherEntry vEntry = new acc_VoucherEntry();
                vEntry.EmpID = EmpID;
                vEntry.TransactionDate = TransactionDate;
                if (actionName == "Early Profit Distribution")
                {
                    vEntry.VNumber = "JV-" + GetMaxVoucherTypeID(VTypeID).ToString().PadLeft(8, '0'); // system voucher! delete restriced...
                    vEntry.VTypeID = 4;//journal Voucher type
                }
                else
                {
                    vEntry.VNumber = "SV-" + GetMaxVoucherTypeID(VTypeID).ToString().PadLeft(8, '0'); // system voucher! delete restriced...
                    vEntry.VTypeID = VTypeID; // voucher type. payment 1, receipt 2, contra 3 or journal 4 or system 5 
                }
                vEntry.VoucherID = GetMaxVoucherID();
                VoucherID = vEntry.VoucherID;
                vEntry.VoucherName = "";
                vEntry.EditDate = editDate;
                vEntry.EditUser = editUser;
                vEntry.Narration = Narration;
                vEntry.RestrictDelete = true;
                vEntry.OCode = OCode;
                vEntry.ActionName = actionName;
                vEntry.PFMonth = string.IsNullOrEmpty(PFMonth) ? "" : PFMonth.PadLeft(2, '0');
                vEntry.PFYear = PFYear;
                vEntry.ProfitDistributionProcessID = processID ?? null;
                context.acc_VoucherEntry.Add(vEntry);
                //context.SaveChanges();



                acc_VoucherDetail vDetail;
                for (int i = 0; i < ledgerId.Count; i++)
                {
                    if (ledgerId[i] != Guid.Empty)
                    {
                         vDetail = new acc_VoucherDetail();


                        vDetail.Credit = Credit[i];
                        vDetail.Debit = Debit[i];
                        vDetail.VoucherDetailID = Guid.NewGuid();
                        vDetail.LedgerID = ledgerId[i];
                        vDetail.VoucherID = vEntry.VoucherID;
                        vDetail.EditDate = editDate;
                        vDetail.EditUser = editUser;
                        vDetail.ChequeNumber = ChequeNumber[i] + "";
                        vDetail.TransactionDate = TransactionDate;
                        //vEntry.VTypeID = actionName == "Early Profit Distribution" ? 4 : VTypeID;
                        vDetail.VTypeID = actionName == "Early Profit Distribution" ? 4 : VTypeID;
                        vDetail.OCode = OCode;
                        if (PFMemberID[i] != null && PFMemberID[i] !="")
                        {
                            //vDetail.PFMemberID = Convert.ToInt32(PFMemberID[i]);
                            vDetail.PFMemberID = EmpID;
                        }
                        vDetail.PFLoanID = PFLoanID[i];
                        vDetail.PFAdditionalInformation = PFAdditionalInfo;
                        context.acc_VoucherDetail.Add(vDetail);
                        atLeastOneEntryFound = true;
                       
                    }
                    
                }
                context.SaveChanges();
                #endregion
            }
            else if (VoucherID > 0)
            {
                #region Update existing voucher
                acc_VoucherEntry vEntry = context.acc_VoucherEntry.Find(VoucherID);
                if (vEntry == null)
                {
                    messageToReturn = "The voucher which id " + VoucherID + " not found!";
                    return false;
                }
                vEntry.TransactionDate = TransactionDate;
                vEntry.EditDate = editDate;
                vEntry.EditUser = editUser;
                vEntry.Narration = Narration;
                vEntry.RestrictDelete = true;
                int vID = VoucherID;// because in annonymous method i can not use ref parameter
                var delvDetail = context.acc_VoucherDetail.Where(w => w.VoucherID == vID).ToList();
                delvDetail.ForEach(f => context.acc_VoucherDetail.Remove(f));

                acc_VoucherDetail vDetail;
                for (int i = 0; i < ledgerId.Count; i++)
                {
                    if (ledgerId[i] != Guid.Empty)
                    {
                        vDetail = new acc_VoucherDetail();

                        vDetail.Credit = Credit[i];
                        vDetail.Debit = Debit[i];
                        vDetail.VoucherDetailID = Guid.NewGuid();
                        vDetail.LedgerID = ledgerId[i];
                        vDetail.ChequeNumber = ChequeNumber[i] + "";
                        vDetail.VoucherID = vEntry.VoucherID;
                        vDetail.EditDate = editDate;
                        vDetail.EditUser = editUser;
                        vDetail.TransactionDate = TransactionDate;
                        vDetail.VTypeID = VTypeID;
                        if (PFMemberID[i] != null && PFMemberID[i] != "")
                        {
                            vDetail.PFMemberID = Convert.ToInt32(PFMemberID[i]);
                        }
                        vDetail.PFLoanID = PFLoanID[i];
                        vDetail.PFAdditionalInformation = PFAdditionalInfo;
                        context.acc_VoucherDetail.Add(vDetail);
                        context.SaveChanges();
                        atLeastOneEntryFound = true;
                    }
                }
                #endregion
            }
            if (atLeastOneEntryFound)
            {
                messageToReturn = "System voucher generate";
                return true;
            }
            messageToReturn = "No data found to save!";
            return false;
        }

        //Max primary key Note: if you chage this method then you have to do the same in AccountigAPI
        public int GetMaxVoucherID()
        {
            int vId = 0;
            try
            {
                vId = context.acc_VoucherEntry.Select(s => s.VoucherID).Max();
            }
            catch
            {
                vId = 0;
            }
            return vId + 1;
        }

        public int GetMaxVoucherID(int ocode)
        {
            int vId = 0;
            try
            {
                vId = context.acc_VoucherEntry.Select(s => s.VoucherID).Max();
            }
            catch
            {
                vId = 0;
            }
            return vId + 1;
        }

        //Max voucher count for specific voucher
        public int GetMaxVoucherTypeID(int i)
        {
            int max = 0;

            try
            {
                if (i == 5 || i == 7 || i == 6)
                {
                    max = context.acc_VoucherEntry.Where(z => z.VTypeID == 5 || z.VTypeID == 7 || z.VTypeID == 6).AsEnumerable().Select(x => new
                    {
                        vnumber = Convert.ToInt32(x.VNumber.Substring(3, 8))
                        //vnumber = Convert.ToInt32(x.VNumber.Substring(9, 6))
                    }).Max(a => a.vnumber);
                }
                else
                {
                    max = context.acc_VoucherEntry.Where(z => z.VTypeID == i).AsEnumerable().Select(x => new
                    {
                        vnumber = Convert.ToInt32(x.VNumber.Substring(3, 8))
                        //vnumber = Convert.ToInt32(x.VNumber.Substring(9, 6))
                    }).Max(a => a.vnumber);
                }

                max++;
            }
            catch (Exception ex)
            {
                if (ex.Message == "Sequence contains no elements")
                {
                    max++;
                }
                else
                    throw;
            }

            return max;
        }

        public int GetMaxVoucherTypeID(int i, int OCode)
        {
            int max = 0;

            try
            {
                if (i == 5 || i == 7 || i == 6)
                {
                    max = context.acc_VoucherEntry.Where(z => z.VTypeID == 5 || z.VTypeID == 7 || z.VTypeID == 6).AsEnumerable().Select(x => new
                    {
                        vnumber = Convert.ToInt32(x.VNumber.Substring(3, 8))
                    }).Max(a => a.vnumber);
                }
                else
                {
                    max = context.acc_VoucherEntry.Where(z => z.VTypeID == i).AsEnumerable().Select(x => new
                    {
                        vnumber = Convert.ToInt32(x.VNumber.Substring(3, 8))
                    }).Max(a => a.vnumber);
                }

                max++;
            }
            catch (Exception ex)
            {
                if (ex.Message == "Sequence contains no elements")
                {
                    max++;
                }
                else
                    throw;
            }

            return max;
        }
        #endregion

        public void Save()
        {
            context.SaveChanges();
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    context.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public string GetLedgerNatureType(Guid LedgerID)
        {
            var type = (from a in context.acc_Ledger
                        join b in context.acc_Group on a.GroupID equals b.GroupID
                        join c in context.acc_Nature on b.NatureID equals c.NatureID
                        where a.LedgerID == LedgerID
                        select new
                        {
                            NatureType = c.NatureName
                        }).FirstOrDefault();
            return type.NatureType;
        }

        public bool SingleEntryVoucher(int EmpID, int VTypeID, DateTime TransactionDate, ref int VoucherID, string Narration, IList<string> LedgerName, IList<decimal> Debit, string EditUserName, Guid EditUserID, string PFMemberID, string PFAdditionalInfo, string PFMonth, string PFYear, int? processID, string PFLoanID, int OCode, string actionName)
        {
            List<Guid> ledgerId = new List<Guid>();
            foreach (var item in LedgerName)
            {
                Guid guid = GetLedgerID(item, OCode);
                if (guid == Guid.Empty)
                {
                    return false;
                }
                ledgerId.Add(guid);
            }
            for (int i = 0; i < Debit.Count; i++)
            {
                if (ledgerId[i] != Guid.Empty)
                {
                }
            }
            string voucherNumber = "";
            Guid editUser = EditUserID;
            DateTime editDate = DateTime.Now;

            if (VoucherID == 0)
            {
                #region New Voucher Entry
                acc_VoucherEntry vEntry = new acc_VoucherEntry();
                vEntry.EmpID = EmpID;
                vEntry.TransactionDate = TransactionDate;
                vEntry.VNumber = "SV-" + GetMaxVoucherTypeID(VTypeID).ToString().PadLeft(8, '0');
                vEntry.VoucherID = GetMaxVoucherID();
                VoucherID = vEntry.VoucherID;
                vEntry.VoucherName = "";
                vEntry.VTypeID = VTypeID;
                vEntry.EditDate = editDate;
                vEntry.EditUser = editUser;
                vEntry.Narration = Narration;
                vEntry.RestrictDelete = true;
                vEntry.OCode = OCode;
                vEntry.ActionName = actionName;
                vEntry.PFMonth = string.IsNullOrEmpty(PFMonth) ? "" : PFMonth.PadLeft(2, '0');
                vEntry.PFYear = PFYear;
                vEntry.ProfitDistributionProcessID = processID ?? null;
                context.acc_VoucherEntry.Add(vEntry);

                acc_VoucherDetail vDetail;
                for (int i = 0; i < ledgerId.Count; i++)
                {
                    if (ledgerId[i] != Guid.Empty)
                    {
                        vDetail = new acc_VoucherDetail();
                        vDetail.Credit = 0;
                        vDetail.Debit = Debit[i];
                        vDetail.VoucherDetailID = Guid.NewGuid();
                        vDetail.LedgerID = ledgerId[i];
                        vDetail.VoucherID = vEntry.VoucherID;
                        vDetail.EditDate = editDate;
                        vDetail.EditUser = editUser;
                        vDetail.TransactionDate = TransactionDate;
                        vDetail.VTypeID = VTypeID;
                        vDetail.OCode = OCode;
                        vDetail.PFMemberID = Convert.ToInt32(PFMemberID);
                        vDetail.PFLoanID = PFLoanID;
                        vDetail.PFAdditionalInformation = PFAdditionalInfo;
                        context.acc_VoucherDetail.Add(vDetail);
                        context.SaveChanges();
                    }
                }
                #endregion
            }
            else if (VoucherID > 0)
            {
                #region Update existing voucher
                acc_VoucherEntry vEntry = context.acc_VoucherEntry.Find(VoucherID);
                if (vEntry == null)
                {
                    return false;
                }
                vEntry.TransactionDate = TransactionDate;
                vEntry.EditDate = editDate;
                vEntry.EditUser = editUser;
                vEntry.Narration = Narration;
                vEntry.RestrictDelete = true;
                int vId = VoucherID;
                var delvDetail = context.acc_VoucherDetail.Where(w => w.VoucherID == vId).ToList();
                delvDetail.ForEach(f => context.acc_VoucherDetail.Remove(f));

                acc_VoucherDetail vDetail;
                for (int i = 0; i < ledgerId.Count; i++)
                {
                    if (ledgerId[i] != Guid.Empty)
                    {
                        vDetail = new acc_VoucherDetail();
                        vDetail.Credit = 0;
                        vDetail.Debit = Debit[i];
                        vDetail.VoucherDetailID = Guid.NewGuid();
                        vDetail.LedgerID = ledgerId[i];
                        vDetail.VoucherID = vEntry.VoucherID;
                        vDetail.EditDate = editDate;
                        vDetail.EditUser = editUser;
                        vDetail.TransactionDate = TransactionDate;
                        vDetail.VTypeID = VTypeID;
                        vDetail.PFMemberID = Convert.ToInt32(PFMemberID);
                        vDetail.PFLoanID = PFLoanID;
                        vDetail.PFAdditionalInformation = PFAdditionalInfo;
                        context.acc_VoucherDetail.Add(vDetail);
                        context.SaveChanges();
                    }
                }
                #endregion
            }
            return true;
        }

        /// <summary>
        /// Duals the entry voucher.
        /// </summary>
        /// <param name="EmpID">The emp identifier.</param>
        /// <param name="VTypeID">The v type identifier.</param>
        /// <param name="TransactionDate">The transaction date.</param>
        /// <param name="VoucherID">The voucher identifier.</param>
        /// <param name="Narration">The narration.</param>
        /// <param name="LedgerName">Name of the ledger.</param>
        /// <param name="Debit">The debit.</param>
        /// <param name="Credit">The credit.</param>
        /// <param name="ChequeNumber">The cheque number.</param>
        /// <param name="messageToReturn">The message to return.</param>
        /// <param name="EditUserName">Name of the edit user.</param>
        /// <param name="EditUserID">The edit user identifier.</param>
        /// <param name="PFMemberID">The pf member identifier.</param>
        /// <param name="PFAdditionalInfo">The pf additional information.</param>
        /// <param name="PFMonth">The pf month.</param>
        /// <param name="PFYear">The pf year.</param>
        /// <param name="processID">The process identifier.</param>
        /// <param name="PFLoanID">The pf loan identifier.</param>
        /// <param name="OCode">The o code.</param>
        /// <param name="actionName">Name of the action.</param>
        /// <returns>bool</returns>
        /// <CreatedBy>Avishek</CreatedBy>
        /// <createdDate>Dec-20-2015</createdDate>
        /// <ReasonOfCreation>Voucher Entry with Ledger ID</ReasonOfCreation>
        public bool DualEntryVoucherById(int EmpID, int VTypeID, DateTime TransactionDate, ref int VoucherID, string Narration, IList<Guid> LedgerId, IList<decimal> Debit, IList<decimal> Credit, IList<string> ChequeNumber, ref string messageToReturn, string EditUserName, Guid EditUserID, List<string> PFMemberID, string PFAdditionalInfo, string PFMonth, string PFYear, int? processID, List<string> PFLoanID, int OCode, string actionName)
        {
            if (!(LedgerId.Count == Credit.Count && Credit.Count == Debit.Count && Debit.Count == ChequeNumber.Count))
            {
                messageToReturn = "Ledger name, debit, credit count mismatch!";
                return false;
            }
            
            //now check if debit and credit id equal for contra and journal voucher
            decimal totalDebit = 0;
            decimal totalCredit = 0;
            for (int i = 0; i < Debit.Count; i++)
            {
                if (LedgerId[i] != Guid.Empty) // it's important
                {
                    totalDebit += Debit[i];
                    totalCredit += Credit[i];
                }
            }
            if (Math.Floor(totalDebit) != Math.Floor(totalCredit))
            {
                messageToReturn = "DEBIT and CREDIT should be equal.";
                return false;
            }
            if ((totalCredit < 0) || (totalDebit < 0))
            {
                messageToReturn = "Debit or credit value cannot be 0 or LESS.";
                return false;
            }

            bool atLeastOneEntryFound = false;
            Guid editUser = EditUserID;
            DateTime editDate = DateTime.Now;

            if (VoucherID == 0)
            {
                #region New Voucher Entry
                acc_VoucherEntry vEntry = new acc_VoucherEntry();
                vEntry.EmpID = EmpID;
                vEntry.TransactionDate = TransactionDate;
                if (actionName == "Early Profit Distribution")
                {
                    vEntry.VNumber = "JV-" + GetMaxVoucherTypeID(VTypeID).ToString().PadLeft(8, '0'); // system voucher! delete restriced...
                    vEntry.VTypeID = 3;//journal Voucher type
                }
                else
                {
                    vEntry.VNumber = "SV-" + GetMaxVoucherTypeID(VTypeID).ToString().PadLeft(8, '0'); // system voucher! delete restriced...
                    vEntry.VTypeID = VTypeID; // voucher type. payment 1, receipt 2, contra 3 or journal 4 or system 5 
                }
                vEntry.VoucherID = GetMaxVoucherID();
                VoucherID = vEntry.VoucherID;
                vEntry.VoucherName = "";
                vEntry.EditDate = editDate;
                vEntry.EditUser = editUser;
                vEntry.Narration = Narration;
                vEntry.RestrictDelete = true;
                vEntry.OCode = OCode;
                vEntry.ActionName = actionName;
                vEntry.PFMonth = string.IsNullOrEmpty(PFMonth) ? "" : PFMonth.PadLeft(2, '0');
                vEntry.PFYear = PFYear;
                vEntry.ProfitDistributionProcessID = processID ?? null;
                context.acc_VoucherEntry.Add(vEntry);

                acc_VoucherDetail vDetail;
                for (int i = 0; i < LedgerId.Count; i++)
                {
                    if (LedgerId[i] != Guid.Empty)
                    {
                        vDetail = new acc_VoucherDetail();
                        vDetail.Credit = Credit[i];
                        vDetail.Debit = Debit[i];
                        vDetail.VoucherDetailID = Guid.NewGuid();
                        vDetail.LedgerID = LedgerId[i];
                        vDetail.VoucherID = vEntry.VoucherID;
                        vDetail.EditDate = editDate;
                        vDetail.EditUser = editUser;
                        vDetail.ChequeNumber = ChequeNumber[i] + "";
                        vDetail.TransactionDate = Convert.ToDateTime(vEntry.TransactionDate);
                        vEntry.VTypeID = actionName == "Early Profit Distribution" ? 4 : VTypeID;
                        vDetail.OCode = OCode;
                        if (PFMemberID[i] != null && PFMemberID[i] != "")
                        {
                            vDetail.PFMemberID = Convert.ToInt32(PFMemberID[i]); 
                        }
                        vDetail.PFLoanID = PFLoanID[i];
                        vDetail.PFAdditionalInformation = PFAdditionalInfo;
                        context.acc_VoucherDetail.Add(vDetail);
                        atLeastOneEntryFound = true;
                    }
                }
                try    
                {
                    context.SaveChanges();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                #endregion
            }
            else if (VoucherID > 0)
            {
                #region Update existing voucher
                acc_VoucherEntry vEntry = context.acc_VoucherEntry.Find(VoucherID);
                if (vEntry == null)
                {
                    messageToReturn = "The voucher which id " + VoucherID + " not found!";
                    return false;
                }
                vEntry.TransactionDate = TransactionDate;
                vEntry.EditDate = editDate;
                vEntry.EditUser = editUser;
                vEntry.Narration = Narration;
                vEntry.RestrictDelete = true;

                int vId = VoucherID;// because in annonymous method i can not use ref parameter
                var delvDetail = context.acc_VoucherDetail.Where(w => w.VoucherID == vId).ToList();
                delvDetail.ForEach(f => context.acc_VoucherDetail.Remove(f));

                acc_VoucherDetail vDetail;
                for (int i = 0; i < LedgerId.Count; i++)
                {
                    if (LedgerId[i] != Guid.Empty)
                    {
                        vDetail = new acc_VoucherDetail();

                        vDetail.Credit = Credit[i];
                        vDetail.Debit = Debit[i];
                        vDetail.VoucherDetailID = Guid.NewGuid();
                        vDetail.LedgerID = LedgerId[i];
                        vDetail.ChequeNumber = ChequeNumber[i] + "";
                        vDetail.VoucherID = vEntry.VoucherID;
                        vDetail.EditDate = editDate;
                        vDetail.EditUser = editUser;
                        vDetail.TransactionDate = TransactionDate;
                        vDetail.VTypeID = VTypeID;
                        if (PFMemberID[i] != null && PFMemberID[i] != "")
                        {
                            vDetail.PFMemberID = Convert.ToInt32(PFMemberID[i]);
                        }
                        vDetail.PFLoanID = PFLoanID[i];
                        vDetail.PFAdditionalInformation = PFAdditionalInfo;
                        context.acc_VoucherDetail.Add(vDetail);
                        context.SaveChanges();
                        atLeastOneEntryFound = true;
                    }
                }
                #endregion
            }
            if (atLeastOneEntryFound)
            {
                messageToReturn = "System voucher generate";
                return true;
            }
            messageToReturn = "No data found to save!";
            return false;
        }
        public bool DualEntryVoucherByIdForInvestment(int empId, int vTypeId, DateTime transactionDate, ref int VoucherID, string Narration, IList<Guid> LedgerId, IList<decimal> Debit, IList<decimal> Credit, IList<string> ChequeNumber, ref string messageToReturn, string EditUserName, Guid EditUserID, List<string> PFMemberID, string PFAdditionalInfo, string PFMonth, string PFYear, int? processID, List<string> PFLoanID, int instrumentId, int OCode, string actionName)
        {
            if (!(LedgerId.Count == Credit.Count && Credit.Count == Debit.Count && Debit.Count == ChequeNumber.Count))
            {
                messageToReturn = "Ledger name, debit, credit count mismatch!";
                return false;
            }
            //now check if debit and credit id equal for contra and journal voucher
            decimal totalDebit = 0;
            decimal totalCredit = 0;
            for (int i = 0; i < Debit.Count; i++)
            {
                if (LedgerId[i] != Guid.Empty) // it's important
                {
                    totalDebit += Debit[i];
                    totalCredit += Credit[i];
                }
            }
            if (totalDebit != totalCredit)
            {
                messageToReturn = "DEBIT and CREDIT should be equal.";
                return false;
            }
            if ((totalCredit < 0) || (totalDebit < 0))
            {
                messageToReturn = "Debit or credit value cannot be 0 or LESS.";
                return false;
            }

            bool atLeastOneEntryFound = false;
            Guid editUser = EditUserID;
            DateTime editDate = DateTime.Now;

            if (VoucherID == 0)
            {
                #region New Voucher Entry
                acc_VoucherEntry vEntry = new acc_VoucherEntry();
                vEntry.EmpID = empId;
                vEntry.TransactionDate = transactionDate;
                if (actionName == "Early Profit Distribution")
                {
                    vEntry.VNumber = "JV-" + GetMaxVoucherTypeID(vTypeId).ToString().PadLeft(8, '0'); // system voucher! delete restriced...
                    vEntry.VTypeID = 4;//journal Voucher type
                }
                else
                {
                    vEntry.VNumber = "SV-" + GetMaxVoucherTypeID(vTypeId).ToString().PadLeft(8, '0'); // system voucher! delete restriced...
                    vEntry.VTypeID = vTypeId; // voucher type. payment 1, receipt 2, contra 3 or journal 4 or system 5 
                }
                vEntry.VoucherID = GetMaxVoucherID();
                VoucherID = vEntry.VoucherID;
                vEntry.VoucherName = "";
                vEntry.EditDate = editDate;
                vEntry.EditUser = editUser;
                vEntry.Narration = Narration;
                vEntry.RestrictDelete = true;
                vEntry.OCode = OCode;
                vEntry.ActionName = actionName;
                vEntry.PFMonth = string.IsNullOrEmpty(PFMonth) ? "" : PFMonth.PadLeft(2, '0');
                vEntry.PFYear = PFYear;
                //vEntry.UsedProject = "PF";
                vEntry.ProfitDistributionProcessID = processID;
                //vEntry.InstrumentID = instrumentId; //Added By Avishek Date:Apr-2-2016
                context.acc_VoucherEntry.Add(vEntry);

                for (int i = 0; i < LedgerId.Count; i++)
                {
                    if (LedgerId[i] != Guid.Empty)
                    {
                        var vDetail = new acc_VoucherDetail();
                        vDetail.Credit = Credit[i];
                        vDetail.Debit = Debit[i];
                        vDetail.VoucherDetailID = Guid.NewGuid();
                        vDetail.LedgerID = LedgerId[i];
                        vDetail.VoucherID = vEntry.VoucherID;
                        vDetail.EditDate = editDate;
                        vDetail.EditUser = editUser;
                        vDetail.ChequeNumber = ChequeNumber[i] + "";
                        vDetail.TransactionDate = transactionDate;
                        //vDetail.UsedProject = "PF";
                        vDetail.VTypeID = actionName == "Early Profit Distribution" ? 4 : vTypeId;
                        vDetail.OCode = OCode;
                        //vDetail.PFMemberID = PFMemberID[i];
                        vDetail.PFLoanID = PFLoanID[i];
                        vDetail.PFAdditionalInformation = PFAdditionalInfo;
                        //vDetail.InstrumentID = instrumentId;//Added By Avishek Date:Apr-2-2016
                        context.acc_VoucherDetail.Add(vDetail);
                        context.SaveChanges();
                        atLeastOneEntryFound = true;
                    }
                }
                #endregion
            }
            else if (VoucherID > 0)
            {
                #region Update existing voucher
                acc_VoucherEntry vEntry = context.acc_VoucherEntry.Find(VoucherID);
                if (vEntry == null)
                {
                    messageToReturn = "The voucher which id " + VoucherID + " not found!";
                    return false;
                }
                vEntry.TransactionDate = transactionDate;
                vEntry.EditDate = editDate;
                vEntry.EditUser = editUser;
                vEntry.Narration = Narration;
                vEntry.RestrictDelete = true;
                vEntry.VNumber = vEntry.VNumber;

                int vId = VoucherID;// because in annonymous method i can not use ref parameter
                var delvDetail = context.acc_VoucherDetail.Where(w => w.VoucherID == vId).ToList();
                delvDetail.ForEach(f => context.acc_VoucherDetail.Remove(f));

                for (int i = 0; i < LedgerId.Count; i++)
                {
                    if (LedgerId[i] != Guid.Empty)
                    {
                        var vDetail = new acc_VoucherDetail
                        {
                            Credit = Credit[i],
                            Debit = Debit[i],
                            VoucherDetailID = Guid.NewGuid(),
                            LedgerID = LedgerId[i],
                            ChequeNumber = ChequeNumber[i] + "",
                            VoucherID = vEntry.VoucherID,
                            EditDate = editDate,
                            EditUser = editUser,
                            TransactionDate = transactionDate,
                            VTypeID = vTypeId,
                            //UsedProject = "PF",
                            //PFMemberID = PFMemberID[i],
                            PFLoanID = PFLoanID[i],
                            PFAdditionalInformation = PFAdditionalInfo
                        };

                        context.acc_VoucherDetail.Add(vDetail);
                        context.SaveChanges();
                        atLeastOneEntryFound = true;
                    }
                }
                #endregion
            }
            if (atLeastOneEntryFound)
            {
                messageToReturn = "System voucher generate";
                return true;
            }
            messageToReturn = "No data found to save!";
            return false;
        }

        public bool DualEntrySettlementVoucher(int EmpID, int VTypeID, DateTime TransactionDate, ref int VoucherID, string Narration, IList<string> LedgerName, IList<decimal> Debit, IList<decimal> Credit, IList<string> ChequeNumber, ref string messageToReturn, string EditUserName, Guid EditUserID, List<string> PFMemberID, string PFAdditionalInfo, string PFMonth, string PFYear, int? processID, List<string> PFLoanID, int OCode, string actionName)
        {

            // LedgerName = new HashSet<string>(LedgerName).ToList();

            List<Guid> LedgerID = new List<Guid>();
            if (!(LedgerName.Count == Credit.Count && Credit.Count == Debit.Count && Debit.Count == ChequeNumber.Count))
            {
                messageToReturn = "Ledger name, debit, credit count mismatch!";
                return false;
            }
            foreach (var item in LedgerName)
            {
                Guid guid = GetLedgerID(item, OCode);
                if (guid == Guid.Empty)
                {
                    messageToReturn = item + " - this ledger name not found! you have to create this ledger and try again!";
                    return false;
                }
                else
                {
                    LedgerID.Add(guid);
                }
            }
            //now check if debit and credit id equal for contra and journal voucher
            decimal total_debit = 0;
            decimal total_credit = 0;
            for (int i = 0; i < Debit.Count; i++)
            {
                if (LedgerID[i] != Guid.Empty) // it's important
                {
                    total_debit += Debit[i];
                    total_credit += Credit[i];
                }
            }
            if (total_debit != total_credit)
            {
                messageToReturn = "DEBIT and CREDIT should be equal.";
                return false;
            }
            else if ((total_credit < 0) || (total_debit < 0))
            {
                messageToReturn = "Debit or credit value cannot be 0 or LESS.";
                return false;
            }

            bool atLeastOneEntryFound = false;
            string VoucherNumber = "";
            Guid EditUser = EditUserID;
            DateTime EditDate = DateTime.Now;

            if (VoucherID == 0)
            {
                #region New Voucher Entry
                acc_VoucherEntry vEntry = new acc_VoucherEntry();
                vEntry.EmpID = EmpID;
                vEntry.TransactionDate = TransactionDate;
                if (actionName == "Early Profit Distribution")
                {
                    vEntry.VNumber = "JV-" + GetMaxVoucherTypeID(VTypeID).ToString().PadLeft(8, '0'); // system voucher! delete restriced...
                    vEntry.VTypeID = 4;//journal Voucher type
                }
                else
                {
                    vEntry.VNumber = "SV-" + GetMaxVoucherTypeID(VTypeID).ToString().PadLeft(8, '0'); // system voucher! delete restriced...
                    vEntry.VTypeID = VTypeID; // voucher type. payment 1, receipt 2, contra 3 or journal 4 or system 5 
                }
                VoucherNumber = vEntry.VNumber;
                vEntry.VoucherID = GetMaxVoucherID();
                VoucherID = vEntry.VoucherID;
                vEntry.VoucherName = "";
                vEntry.EditDate = EditDate;
                vEntry.EditUser = EditUser;
                vEntry.Narration = Narration;
                vEntry.RestrictDelete = true;
                vEntry.OCode = OCode;
                vEntry.ActionName = actionName;
                vEntry.PFMonth = string.IsNullOrEmpty(PFMonth) ? "" : PFMonth.PadLeft(2, '0');
                vEntry.PFYear = PFYear;
                vEntry.ProfitDistributionProcessID = processID ?? null;
                context.acc_VoucherEntry.Add(vEntry);

                acc_VoucherDetail vDetail;
                for (int i = 0; i < LedgerID.Count; i++)
                {
                    if (LedgerID[i] != Guid.Empty)
                    {
                        vDetail = new acc_VoucherDetail();
                        vDetail.Credit = Credit[i];
                        vDetail.Debit = Debit[i];
                        vDetail.VoucherDetailID = Guid.NewGuid();
                        vDetail.LedgerID = LedgerID[i];
                        vDetail.VoucherID = vEntry.VoucherID;
                        vDetail.EditDate = EditDate;
                        vDetail.EditUser = EditUser;
                        vDetail.ChequeNumber = ChequeNumber[i] + "";
                        vDetail.TransactionDate = TransactionDate;
                        if (actionName == "Early Profit Distribution")
                        {
                            vEntry.VTypeID = 4;//journal Voucher type
                        }
                        else
                        {
                            vEntry.VTypeID = VTypeID; // voucher type. payment 1, receipt 2, contra 3 or journal 4 or system 5 
                        }
                        vDetail.OCode = OCode;
                        if (PFMemberID[i] != null && PFMemberID[i] != "")
                        {
                            vDetail.PFMemberID = Convert.ToInt32(PFMemberID[i]);
                        }
                        vDetail.PFLoanID = PFLoanID[i];
                        vDetail.PFAdditionalInformation = PFAdditionalInfo;
                        context.acc_VoucherDetail.Add(vDetail);
                        //Edited By Suman
                        context.SaveChanges();
                        atLeastOneEntryFound = true;
                    }
                }
                #endregion
            }
            else if (VoucherID > 0)
            {
                #region Update existing voucher
                acc_VoucherEntry vEntry = context.acc_VoucherEntry.Find(VoucherID);
                if (vEntry == null)
                {
                    messageToReturn = "The voucher which id " + VoucherID + " not found!";
                    return false;
                }
                vEntry.TransactionDate = TransactionDate;
                vEntry.EditDate = EditDate;
                vEntry.EditUser = EditUser;
                vEntry.Narration = Narration;
                vEntry.RestrictDelete = true;
                VoucherNumber = vEntry.VNumber;

                int vID = VoucherID;// because in annonymous method i can not use ref parameter
                var delvDetail = context.acc_VoucherDetail.Where(w => w.VoucherID == vID).ToList();
                delvDetail.ForEach(f => context.acc_VoucherDetail.Remove(f));

                acc_VoucherDetail vDetail;
                for (int i = 0; i < LedgerID.Count; i++)
                {
                    if (LedgerID[i] != Guid.Empty)
                    {
                        vDetail = new acc_VoucherDetail();

                        vDetail.Credit = Credit[i];
                        vDetail.Debit = Debit[i];
                        vDetail.VoucherDetailID = Guid.NewGuid();
                        vDetail.LedgerID = LedgerID[i];
                        vDetail.ChequeNumber = ChequeNumber[i] + "";
                        vDetail.VoucherID = vEntry.VoucherID;
                        vDetail.EditDate = EditDate;
                        vDetail.EditUser = EditUser;
                        vDetail.TransactionDate = TransactionDate;
                        vDetail.VTypeID = VTypeID;

                        if (PFMemberID[i] != null && PFMemberID[i] != "")
                        {
                            vDetail.PFMemberID = Convert.ToInt32(PFMemberID[i]);
                        }
                        vDetail.PFLoanID = PFLoanID[i];
                        vDetail.PFAdditionalInformation = PFAdditionalInfo;
                        context.acc_VoucherDetail.Add(vDetail);
                        //Edited by me
                        context.SaveChanges();
                        atLeastOneEntryFound = true;
                    }
                }
                #endregion
            }
            //try
            //{
            if (atLeastOneEntryFound)
            {
                //        unitOfWork.Save();
                messageToReturn = "System voucher generate";
                return true;
            }
            else
            {
                messageToReturn = "No data found to save!";
                return false;
            }
            //}
            //catch (Exception x)
            //{
            //    message = "Error: "+x.Message+"Inner exception : "+x.InnerException+"";
            //    return false;
            //}
        }
        public bool SingleEntryVoucherById(int EmpID, int VTypeID, DateTime TransactionDate, ref int VoucherID, string Narration, IList<Guid> LedgerId, IList<decimal> Debit, string EditUserName, Guid EditUserID, string PFMemberID, string PFAdditionalInfo, string PFMonth, string PFYear, int? processID, string PFLoanID, int OCode, string actionName)
        {
            decimal totalDebit = Debit.Where((t, i) => LedgerId[i] != Guid.Empty).Sum();
            string voucherNumber = "";
            Guid editUser = EditUserID;
            DateTime editDate = DateTime.Now;

            if (VoucherID == 0)
            {
                #region New Voucher Entry
                acc_VoucherEntry vEntry = new acc_VoucherEntry();
                vEntry.EmpID = EmpID;
                vEntry.TransactionDate = TransactionDate;
                vEntry.VNumber = "SV-" + GetMaxVoucherTypeID(VTypeID).ToString().PadLeft(8, '0');
                vEntry.VoucherID = GetMaxVoucherID();
                VoucherID = vEntry.VoucherID;
                vEntry.VoucherName = "";
                vEntry.VTypeID = VTypeID;
                vEntry.EditDate = editDate;
                vEntry.EditUser = editUser;
                vEntry.Narration = Narration;
                vEntry.RestrictDelete = true;
                vEntry.OCode = OCode;
                vEntry.ActionName = actionName;
                vEntry.PFMonth = string.IsNullOrEmpty(PFMonth) ? "" : PFMonth.PadLeft(2, '0');
                vEntry.PFYear = PFYear;
                vEntry.ProfitDistributionProcessID = processID ?? null;
                context.acc_VoucherEntry.Add(vEntry);

                acc_VoucherDetail vDetail;
                for (int i = 0; i < LedgerId.Count; i++)
                {
                    if (LedgerId[i] != Guid.Empty)
                    {
                        vDetail = new acc_VoucherDetail();
                        vDetail.Credit = 0;
                        vDetail.Debit = Debit[i];
                        vDetail.VoucherDetailID = Guid.NewGuid();
                        vDetail.LedgerID = LedgerId[i];
                        vDetail.VoucherID = vEntry.VoucherID;
                        vDetail.EditDate = editDate;
                        vDetail.EditUser = editUser;
                        vDetail.TransactionDate = TransactionDate;
                        vDetail.VTypeID = VTypeID;
                        vDetail.OCode = OCode;
                        if (!string.IsNullOrEmpty(PFMemberID))
                        {
                            vDetail.PFMemberID = Convert.ToInt32(PFMemberID);
                        }
                        vDetail.PFLoanID = PFLoanID;
                        vDetail.PFAdditionalInformation = PFAdditionalInfo;
                        context.acc_VoucherDetail.Add(vDetail);
                        context.SaveChanges();
                    }
                }
                #endregion
            }
            else if (VoucherID > 0)
            {
                #region Update existing voucher
                acc_VoucherEntry vEntry = context.acc_VoucherEntry.Find(VoucherID);
                if (vEntry == null)
                {
                    return false;
                }
                vEntry.TransactionDate = TransactionDate;
                vEntry.EditDate = editDate;
                vEntry.EditUser = editUser;
                vEntry.Narration = Narration;
                vEntry.RestrictDelete = true;

                int vID = VoucherID;
                var delvDetail = context.acc_VoucherDetail.Where(w => w.VoucherID == vID).ToList();
                delvDetail.ForEach(f => context.acc_VoucherDetail.Remove(f));

                acc_VoucherDetail vDetail;
                for (int i = 0; i < LedgerId.Count; i++)
                {
                    if (LedgerId[i] != Guid.Empty)
                    {
                        vDetail = new acc_VoucherDetail();

                        vDetail.Credit = 0;
                        vDetail.Debit = Debit[i];
                        vDetail.VoucherDetailID = Guid.NewGuid();
                        vDetail.LedgerID = LedgerId[i];
                        vDetail.VoucherID = vEntry.VoucherID;
                        vDetail.EditDate = editDate;
                        vDetail.EditUser = editUser;
                        vDetail.TransactionDate = TransactionDate;
                        vDetail.VTypeID = VTypeID;
                        if (PFMemberID != null && PFMemberID != "")
                        {
                            vDetail.PFMemberID = Convert.ToInt32(PFMemberID);
                        }
                        vDetail.PFLoanID = PFLoanID;
                        vDetail.PFAdditionalInformation = PFAdditionalInfo;
                        context.acc_VoucherDetail.Add(vDetail);
                        context.SaveChanges();
                    }
                }
                #endregion
            }
            return true;
        }

        #endregion

    }
}

