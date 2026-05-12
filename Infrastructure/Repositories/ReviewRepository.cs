using Core.Interfaces.IRepositories;
using Core.Models;
using Infrastructure.Data;
namespace Infrastructure.Repositories
{
    public class ReviewRepository : Repository<Review>, IReviewRepository
    {
        public ReviewRepository(AppDbContext context) : base(context)
        {
        }
    }
}