using Microsoft.AspNetCore.Http;

namespace Core.Interfaces.Helpers
{
    public interface IImageHelper
    {
        Task<string> SaveImageAsync(IFormFile image, string folder);
        void DeleteImage(string imageName, string folder);
    }
}
