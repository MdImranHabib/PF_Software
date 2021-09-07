using System;
using DLL.Repository;

namespace DLL.PayRollAccess.Repository
{
    public class PRUnitOfWork : IDisposable
    {
        private PREntities pRContext = new PREntities();
        public void Save()
        {
            try
            {
                pRContext.SaveChanges();
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    pRContext.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        private GenericRepository<HRM_DEPARTMENTS> departmentRepository;
        public GenericRepository<HRM_DEPARTMENTS> DepartmentRepository
        {
            get {
                return departmentRepository ?? (departmentRepository = new GenericRepository<HRM_DEPARTMENTS>(pRContext));
            }
        }

        private GenericRepository<HRM_DESIGNATIONS> designationRepository;
        public GenericRepository<HRM_DESIGNATIONS> DesignationRepository
        {
            get
            {
                return designationRepository ?? (designationRepository = new GenericRepository<HRM_DESIGNATIONS>(pRContext));
            }
        }

        private GenericRepository<HRM_Emp_PF_Contribution> salaryRepositoty;
        public GenericRepository<HRM_Emp_PF_Contribution> SalaryRepositotyRepository
        {
            get
            {
                return salaryRepositoty ?? (salaryRepositoty = new GenericRepository<HRM_Emp_PF_Contribution>(pRContext));
            }
        }

        private GenericRepository<HRM_PersonalInformations> personalInformationRepository;
        public GenericRepository<HRM_PersonalInformations> PersonalInformationRepository
        {
            get
            {
                return personalInformationRepository ?? (personalInformationRepository = new GenericRepository<HRM_PersonalInformations>(pRContext));
            }
        }

        //======================custom repository====================//
        private CustomeRepository customeRepository;
        public CustomeRepository CustomeRepository
        {
            get
            {

                if (this.customeRepository == null)
                {
                    this.customeRepository = new CustomeRepository(pRContext);
                }
                return customeRepository;
            }
        }
        //====================end of custom repository==================//
    }
}
