using Core.Interfaces.IRepositories;
using Core.Models;
using Infrastructure.Data;

namespace Infrastructure.Repositories
{
    public class CartRepository : Repository<Cart>, ICartRepository
    {
        public CartRepository(AppDbContext context) : base(context)
        {
        }
    }
}
