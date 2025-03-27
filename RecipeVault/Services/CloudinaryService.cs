using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace RecipeVault.Services
{
    public class CloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService()
        {
            var account = new Account(
                Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME"),
                Environment.GetEnvironmentVariable("CLOUDINARY_API_KEY"),
                Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET")
            );

            if (string.IsNullOrEmpty(account.Cloud) ||
                string.IsNullOrEmpty(account.ApiKey) ||
                string.IsNullOrEmpty(account.ApiSecret))
            {
                throw new ArgumentException("Cloudinary credentials are missing or invalid.");
            }

            _cloudinary = new Cloudinary(account);
        }

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = "recipevault"
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            return uploadResult.SecureUrl.ToString();
        }
    }
}