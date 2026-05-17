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

            string webRootPath = !string.IsNullOrEmpty(_webHostEnvironment.WebRootPath)
                ? _webHostEnvironment.WebRootPath
                : Path.Combine(_webHostEnvironment.ContentRootPath, "wwwroot");
            Console.WriteLine($"[ImageHelper] WebRootPath = '{_webHostEnvironment.WebRootPath}'");
            Console.WriteLine($"[ImageHelper] Saving to: {webRootPath}");

            string folderPath = Path.Combine(webRootPath, folder);

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string fullPath = Path.Combine(folderPath, imageName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            return imageName;
        }

        public void DeleteImage(string imagePath, string folder)
        {
            string cleanImageName = Path.GetFileName(imagePath);

            string rootPath = _webHostEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

            string fullPath = Path.Combine(rootPath, folder, cleanImageName);
            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }
    }
}
