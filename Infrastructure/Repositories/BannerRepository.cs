using Core.Interfaces.IRepositories;
using Core.Models;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class BannerRepository : Repository<Banner>, IBannerRepository
    {
        private readonly AppDbContext _context;
        
        public BannerRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Banner>> GetActiveBannersAsync()
        {
            return await _context.Banners
                .Include(b => b.Category)
                .Where(b => b.IsActive)
                .ToListAsync();
        }
    }
}
