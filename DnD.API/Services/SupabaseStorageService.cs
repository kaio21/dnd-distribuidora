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

    public async Task<string?> UploadAsync(IFormFile file)
    {
        if (string.IsNullOrEmpty(_url) || string.IsNullOrEmpty(_key))
            return null;

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

    public async Task DeleteAsync(string? imageUrl)
    {
        if (string.IsNullOrEmpty(_url) || string.IsNullOrEmpty(_key) || string.IsNullOrEmpty(imageUrl))
            return;

        // só apaga arquivos que pertencem ao nosso bucket
        var marker = $"/storage/v1/object/public/{Bucket}/";
        var idx = imageUrl.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (idx < 0) return;

        var fileName = imageUrl[(idx + marker.Length)..];
        if (string.IsNullOrEmpty(fileName)) return;

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _key);

        try
        {
            await client.DeleteAsync($"{_url}/storage/v1/object/{Bucket}/{fileName}");
        }
        catch
        {
            // falha ao remover arquivo antigo não deve quebrar a operação principal
        }
    }
}
