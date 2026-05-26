using System.Net.Http.Headers;

namespace DnD.API.Services;

public class SupabaseStorageService
{
    private readonly IHttpClientFactory _factory;
    private readonly string _url;
    private readonly string _key;
    private const string Bucket = "products";

    public SupabaseStorageService(IHttpClientFactory factory, IConfiguration config)
    {
        _factory = factory;
        _url = config["Supabase:Url"]!;
        _key = config["Supabase:ServiceKey"]!;
    }

    public async Task<string> UploadAsync(IFormFile file)
    {
        var ext = Path.GetExtension(file.FileName).ToLower();
        var fileName = $"{Guid.NewGuid()}{ext}";

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _key);

        using var content = new StreamContent(file.OpenReadStream());
        content.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

        var response = await client.PostAsync(
            $"{_url}/storage/v1/object/{Bucket}/{fileName}", content);

        response.EnsureSuccessStatusCode();

        return $"{_url}/storage/v1/object/public/{Bucket}/{fileName}";
    }
}
