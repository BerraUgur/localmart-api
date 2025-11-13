namespace WebAPI.Services.Abstract;

public interface ICloudinaryService
{
    Task<string> UploadImageAsync(string base64Image);
    Task<bool> DeleteImageAsync(string imageUrl);
}
