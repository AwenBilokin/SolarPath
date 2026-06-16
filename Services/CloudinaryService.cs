using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace SolarPath.Web.Services;

public interface ICloudinaryService
{
    Task<string?> UploadImageAsync(IFormFile file, string folder = "routes");
    Task DeleteImageAsync(string imageUrl);
}

public class CloudinaryService : ICloudinaryService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryService(IConfiguration config)
    {
        var account = new Account(
            config["Cloudinary:CloudName"],
            config["Cloudinary:ApiKey"],
            config["Cloudinary:ApiSecret"]);
        _cloudinary = new Cloudinary(account) { Api = { Secure = true } };
    }

    public async Task<string?> UploadImageAsync(IFormFile file, string folder = "routes")
    {
        if (file == null || file.Length == 0) return null;

        await using var stream = file.OpenReadStream();
        var uploadParams = new ImageUploadParams
        {
            File           = new FileDescription(file.FileName, stream),
            Folder         = $"solarpath/{folder}",
            Transformation = new Transformation().Width(1200).Height(800).Crop("fill").Quality("auto").FetchFormat("auto")
        };

        var result = await _cloudinary.UploadAsync(uploadParams);
        return result.SecureUrl?.ToString();
    }

    public async Task DeleteImageAsync(string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl) || !imageUrl.Contains("cloudinary")) return;

        // Витягуємо public_id з URL
        var uri      = new Uri(imageUrl);
        var segments = uri.AbsolutePath.Split('/');
        var upload   = Array.IndexOf(segments, "upload");
        if (upload < 0) return;

        // Пропускаємо версію (v1234...) якщо є
        var start   = upload + 1;
        if (start < segments.Length && segments[start].StartsWith("v")) start++;
        var publicId = string.Join("/", segments[start..]).Replace(".jpg","").Replace(".png","").Replace(".webp","");

        await _cloudinary.DestroyAsync(new DeletionParams(publicId));
    }
}
