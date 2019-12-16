using System;
using System.Linq;
using System.Linq.Expressions;

namespace YouTubeListManager.DataContracts.Repository
{
    public interface IRepository<TEntity> where TEntity : class
    {
        IQueryable<TEntity> GetAll();
        IQueryable<TEntity> FindBy(Expression<Func<TEntity, bool>> predicate);
        TEntity Create();
        void Delete(TEntity entity);
        void Insert(TEntity entity);
    }
}