using Core.Interfaces.IRepositories;
using Core.Models;
using Infrastructure.Data;

namespace Infrastructure.Repositories
{
    public class OrderRepository : Repository<Order>, IOrderRepository
    {
        public OrderRepository(AppDbContext context) : base(context)
        {
        }
    }
}
