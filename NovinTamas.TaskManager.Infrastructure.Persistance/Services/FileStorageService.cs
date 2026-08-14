using Microsoft.AspNetCore.Hosting;
using NovinTamas.TaskManager.Application.Contracts.Contracts;

namespace NovinTamas.TaskManager.Infrastructure.Persistance.Services
{
    public class FileStorageService : IFileStorageService
    {
        private const string RelativePath = "uploads/tasks";

        private readonly IWebHostEnvironment _environment;

        public FileStorageService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string> UploadAsync(byte[] bytes, string fileName, string contentType)
        {
            var uploadPath = Path.Combine(GetWebRoot(), RelativePath);

            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            var newFileName = $"{Guid.NewGuid()}{Path.GetExtension(fileName)}";
            await File.WriteAllBytesAsync(Path.Combine(uploadPath, newFileName), bytes);

            return $"/{RelativePath}/{newFileName}";
        }

        public Task DeleteAsync(string path)
        {
            var fullPath = Path.Combine(GetWebRoot(), path.TrimStart('/'));

            if (File.Exists(fullPath))
                File.Delete(fullPath);

            return Task.CompletedTask;
        }

        private string GetWebRoot()
        {
            var webRoot = string.IsNullOrWhiteSpace(_environment.WebRootPath)
                ? Path.Combine(_environment.ContentRootPath, "wwwroot")
                : _environment.WebRootPath;

            if (!Directory.Exists(webRoot))
                Directory.CreateDirectory(webRoot);

            return webRoot;
        }
    }
}
