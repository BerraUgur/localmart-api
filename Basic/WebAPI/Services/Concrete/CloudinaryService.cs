using System.Text.RegularExpressions;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using WebAPI.Services.Abstract;

namespace WebAPI.Services.Concrete;

public class CloudinaryService : ICloudinaryService
{
    private readonly Cloudinary _cloudinary;
    private readonly ILogger<CloudinaryService> _logger;

    public CloudinaryService(IConfiguration configuration, ILogger<CloudinaryService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        var cloudName = configuration["Cloudinary:CloudName"];
        var apiKey = configuration["Cloudinary:ApiKey"];
        var apiSecret = configuration["Cloudinary:ApiSecret"];

        if (string.IsNullOrEmpty(cloudName) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
        {
            throw new InvalidOperationException("Cloudinary configuration is incomplete.");
        }

        var account = new Account(cloudName, apiKey, apiSecret);
        _cloudinary = new Cloudinary(account);
    }

    public async Task<string> UploadImageAsync(string base64Image)
    {
        try
        {
            if (string.IsNullOrEmpty(base64Image))
            {
                return null;
            }

            // Remove data URI prefix if present
            var imageData = Regex.Replace(base64Image, "^data:image/[a-zA-Z]+;base64,", string.Empty);
            var bytes = Convert.FromBase64String(imageData);

            using var stream = new MemoryStream(bytes);
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription($"{Guid.NewGuid()}.jpg", stream),
                Folder = "localmart"
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return uploadResult.SecureUrl.ToString();
            }

            _logger.LogError("Cloudinary upload failed: {Error}", uploadResult.Error?.Message);
            throw new Exception($"Image upload failed: {uploadResult.Error?.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading image to Cloudinary");
            throw;
        }
    }

    public async Task<bool> DeleteImageAsync(string imageUrl)
    {
        try
        {
            if (string.IsNullOrEmpty(imageUrl))
            {
                return false;
            }

            // Extract public ID from URL
            var uri = new Uri(imageUrl);
            var segments = uri.AbsolutePath.Split('/');
            var publicIdWithExtension = string.Join("/", segments.Skip(segments.Length - 2));
            var publicId = publicIdWithExtension.Substring(0, publicIdWithExtension.LastIndexOf('.'));

            var deletionParams = new DeletionParams(publicId);
            var result = await _cloudinary.DestroyAsync(deletionParams);

            return result.Result == "ok";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting image from Cloudinary: {ImageUrl}", imageUrl);
            return false;
        }
    }
}
