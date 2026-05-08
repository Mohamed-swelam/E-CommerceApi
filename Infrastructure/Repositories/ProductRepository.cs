using Core.Interfaces.IRepositories;
using Core.Models;
using Infrastructure.Data;

namespace Infrastructure.Repositories
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        public ProductRepository(AppDbContext context) : base(context)
        {
        }
    }
}
