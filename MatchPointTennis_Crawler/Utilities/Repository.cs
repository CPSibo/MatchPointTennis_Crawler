using MatchPointTennis_Crawler.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MatchPointTennis_Crawler
{
    public class Repository
    {
        private TennisContext _entities = new TennisContext();

        public TennisContext Context
        {
            get { return _entities; }
            set { _entities = value; }
        }

        public IQueryable<T> GetAll<T>(Expression<Func<T, bool>> predicate = null)
            where T : class
        {
            IQueryable<T> query = null;

            if (predicate == null)
            {
                query = Context.Set<T>();
            }
            else
            {
                query = Context.Set<T>().Where(predicate);
            }
            
            return query;
        }

        public T Get<T>(Expression<Func<T, bool>> predicate)
            where T : class
        {
            var item = Context.Set<T>().FirstOrDefault(predicate);

            Dispose();

            return item;
        }

        public bool Exists<T>(params object[] keys)
            where T : class
        {
            var result = (Context.Set<T>().Find(keys) != null);

            Dispose();

            return result;
        }

        public Repository Add<T>(T entity)
            where T : class
        {
            Context.Set<T>().Add(entity);

            return this;
        }

        public Repository Delete<T>(T entity)
            where T : class
        {
            Context.Set<T>().Remove(entity);

            return this;
        }

        public Repository Edit<T>(T entity)
            where T : class
        {
            Context.Entry(entity).State = EntityState.Modified;

            return this;
        }

        public void Save()
        {
            Context.SaveChanges();

            Dispose();
        }

        public void Save<T>(T entity)
            where T : class
        {
            if (Context.Entry(entity).State == EntityState.Unchanged)
            {
                Context.Entry(entity).State = EntityState.Modified;
            }

            Context.SaveChanges();

            Dispose();
        }

        public void Dispose()
        {
            Context.Dispose();
        }
    }
}
