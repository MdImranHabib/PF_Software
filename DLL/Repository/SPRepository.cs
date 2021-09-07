using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLL.Repository
{
    public class SPRepository : IDisposable
    {
        private PFTMEntities context;
        public SPRepository(PFTMEntities context)
        {
            context = this.context;
        }


        //public IEnumerable<sp> sp_GetVoucherEntryDD(int? voucherID)
        //{
        //    var v = context.sp_GetVoucherEntryDD(voucherID);
        //    return sp_GetVoucherEntryDD_Result;
        //}
        //========================================

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
    }
}
