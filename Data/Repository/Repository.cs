using System;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using Data;

namespace YouTubeListManager.Data.Repository
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class, new()
    {
        protected DbSet<TEntity> set;

        public Repository(YouTubeListManagerContext context)
        {
            Context = context;
        }

        public DbSet<TEntity> Set => set ?? (set = Context.Set<TEntity>());

        protected YouTubeListManagerContext Context { get; }

        public IQueryable<TEntity> GetAll()
        {
            IQueryable<TEntity> entities = Set;
            return entities;
        }

        public IQueryable<TEntity> FindBy(Expression<Func<TEntity, bool>> predicate)
        {
            var entities = Set.Where(predicate);
            return entities;
        }

        public virtual TEntity Create()
        {
            return Set.Create();
        }

        public virtual void Delete(TEntity entity)
        {
            Set.Remove(entity);
        }

        public virtual void Insert(TEntity entity)
        {
            Set.Add(entity);
        }
    }
}