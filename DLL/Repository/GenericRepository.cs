﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.Entity;
using System.Linq.Expressions;

namespace DLL.Repository
{
    public class GenericRepository<TEntity> where TEntity : class
    {
        internal PFTMEntities context;
        internal DbSet<TEntity> dbSet;
        private PayRollAccess.PREntities pRContext;

        public GenericRepository(PFTMEntities context)
        {
            this.context = context;
            this.dbSet = context.Set<TEntity>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericRepository{TEntity}"/> class.
        /// </summary>
        /// <param name="pRContext">The p r context.</param>
        /// <CreatedBy>Kamrul</CreatedBy>
        /// <DateOfCreation>2019-04-15</DateOfCreation>
        public GenericRepository(PayRollAccess.PREntities pRContext)
        {
            this.pRContext = pRContext;
            this.dbSet = context.Set<TEntity>();
        }
        public virtual bool IsExist(Expression<Func<TEntity, bool>> filter = null)
        {
            IQueryable<TEntity> query = dbSet;
            return query.Where(filter).Any();
        }

        public virtual IEnumerable<TEntity> Get(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            string includeProperties = "")
        {
            IQueryable<TEntity> query = dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            foreach (var includeProperty in includeProperties.Split
                (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            if (orderBy != null)
            {
                return orderBy(query);
            }
            else
            {
                return query;
            }
        }

        public virtual TEntity GetByID(object id)
        {
            return dbSet.Find(id);
        }

        public virtual void Insert(TEntity entity)
        {
            dbSet.Add(entity);
        }

        public virtual void Delete(object id)
        {
            TEntity entityToDelete = dbSet.Find(id);
            Delete(entityToDelete);
        }

        public virtual void Delete(TEntity entityToDelete)
        {
            if (context.Entry(entityToDelete).State == EntityState.Detached)
            {
                dbSet.Attach(entityToDelete);
            }
            dbSet.Remove(entityToDelete);
        }

        public virtual void Update(TEntity entityToUpdate)
        {
            try
            {
                if (context.Entry(entityToUpdate).State == EntityState.Detached)
                {
                    dbSet.Attach(entityToUpdate);
                }

                context.Entry(entityToUpdate).State = EntityState.Modified;
            }
            catch (Exception ex)
            {
                
                throw ex;
            }
        }

        public virtual int GetRowCount(string s)
        {
                return context.Database.SqlQuery<int>(s).First();
        }

        
    }
}