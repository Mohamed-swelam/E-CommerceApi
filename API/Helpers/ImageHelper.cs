using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Core.Interfaces.Helpers;

namespace API.Helpers
{
    public class ImageHelper : IImageHelper
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ImageHelper(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }
        public async Task<string> SaveImageAsync(IFormFile image, string folder)
        {
            string imageName = Guid.NewGuid() + Path.GetExtension(image.FileName);
            string folderPath = Path.Combine(_webHostEnvironment.WebRootPath, folder);
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
            string fullPath = Path.Combine(folderPath, imageName);
            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }
            return imageName;
        }
        public void DeleteImage(string imageName, string folder)
        {
            string fullPath = Path.Combine(_webHostEnvironment.WebRootPath, folder, imageName);
            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }
    }
}
