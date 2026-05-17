using Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Interfaces.IRepositories
{
    public interface IBannerRepository : IGenericRepository<Banner>
    {
        Task<IEnumerable<Banner>> GetActiveBannersAsync();
    }
}
