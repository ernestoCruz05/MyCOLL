using Microsoft.AspNetCore.Hosting;

namespace MyCOLL.Services
{
    public class ImageUploadService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly string _uploadsFolder;

        public ImageUploadService(IWebHostEnvironment environment)
        {
            _environment = environment;
            _uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
            
            // Ensure uploads folder exists
            Directory.CreateDirectory(_uploadsFolder);
        }

        /// <summary>
        /// Saves an image file and returns the relative URL path
        /// </summary>
        public async Task<string> SaveImageAsync(Stream imageStream, string fileName, string subfolder = "")
        {
            // Create subfolder if specified
            var targetFolder = string.IsNullOrEmpty(subfolder) 
                ? _uploadsFolder 
                : Path.Combine(_uploadsFolder, subfolder);
            
            Directory.CreateDirectory(targetFolder);

            // Generate unique filename to avoid conflicts
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(targetFolder, uniqueFileName);

            // Save file
            await using var fileStream = new FileStream(filePath, FileMode.Create);
            await imageStream.CopyToAsync(fileStream);

            // Return relative URL path (for use in <img src="">)
            var relativePath = string.IsNullOrEmpty(subfolder)
                ? $"/uploads/{uniqueFileName}"
                : $"/uploads/{subfolder}/{uniqueFileName}";

            return relativePath;
        }

        /// <summary>
        /// Deletes an image file given its URL path
        /// </summary>
        public void DeleteImage(string? imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl)) return;

            try
            {
                // Convert URL path to file system path
                var relativePath = imageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
                var fullPath = Path.Combine(_environment.WebRootPath, relativePath);

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
            }
            catch
            {
                // Silently ignore deletion errors
            }
        }

        /// <summary>
        /// Validates if a file is a valid image
        /// </summary>
        public static bool IsValidImage(string contentType, long size, long maxSizeBytes = 2 * 1024 * 1024)
        {
            var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
            return allowedTypes.Contains(contentType.ToLower()) && size <= maxSizeBytes;
        }
    }
}
