using Core.Interfaces.IRepositories;
using Core.Models;
using Infrastructure.Data;
namespace Infrastructure.Repositories
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        public CategoryRepository(AppDbContext context) : base(context)
        {
        }
    }
}