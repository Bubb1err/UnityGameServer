using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace UnityGameServer.DataAccess.Repository
{
    public interface IRepository<T> where T : class
    {
        Task SaveAsync();
        IQueryable<T> GetAll(
            Expression<Func<T, bool>> predicate = null,
            Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null,
            bool track = false);
        void Create(T entity);
        Task CreateAsync(T entity);

        void Update(T entity);

        void Delete(T entity);
        Task<T> GetFirstOrDefaultAsync(
            Expression<Func<T, bool>> predicate = null,
            Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null,
            bool track = true);
    }
}
