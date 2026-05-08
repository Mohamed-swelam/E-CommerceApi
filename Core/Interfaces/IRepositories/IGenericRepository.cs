using System.Linq.Expressions;

namespace Core.Interfaces.IRepositories
{
    public interface IGenericRepository<T> where T : class
    {
        IQueryable<T> GetAll(Expression<Func<T, bool>>? expression = null, string? includeProperties = null);
        Task<T?> GetAsync(Expression<Func<T, bool>> expression, string? includeProperties = null);

        Task<T?> GetByIdAsync(int id);
        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);
    }
}
