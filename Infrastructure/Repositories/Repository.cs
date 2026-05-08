using Core.Interfaces.IRepositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Repositories
{
    public class Repository<T> : IGenericRepository<T> where T : class
    {
        private readonly AppDbContext context;
        private readonly DbSet<T> dbSet;

        public Repository(AppDbContext context)
        {
            this.context = context;
            this.dbSet = context.Set<T>();
        }

        public async Task AddAsync(T entity)
        {
            await dbSet.AddAsync(entity);
        }

        public void Delete(T entity)
        {
            dbSet.Remove(entity);
        }

        public IQueryable<T> GetAll(Expression<Func<T, bool>>? expression = null, string? includeProperties = null)
        {
            IQueryable<T> query = context.Set<T>();
            if (expression != null)
            {
                query = query.Where(expression);
            }
            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (var property in includeProperties
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                {
                    query = query.Include(property);
                }
            }
            return query;
        }

        public async Task<T?> GetAsync(Expression<Func<T, bool>> expression, string? includeProperties = null)
        {
            IQueryable<T> query = context.Set<T>();
            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (var property in includeProperties
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                {
                    query = query.Include(property);
                }
            }
            return await query.FirstOrDefaultAsync(expression);
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            return await dbSet.FindAsync(id);
        }

        public void Update(T entity)
        {
            dbSet.Update(entity);
        }
    }
}
