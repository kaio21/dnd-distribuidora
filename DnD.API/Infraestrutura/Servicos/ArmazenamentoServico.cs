using DnD.API.Aplicacao.Servicos;
using System.Net.Http.Headers;

namespace DnD.API.Infraestrutura.Servicos;

public class ArmazenamentoServico : IArmazenamentoServico
{
    private readonly IHttpClientFactory _factory;
    private readonly string _url;
    private readonly string _chave;
    private const string Bucket = "products";

    public ArmazenamentoServico(IHttpClientFactory factory, IConfiguration config)
    {
        _factory = factory;
        _url  = config["Supabase:Url"]!;
        _chave = config["Supabase:ServiceKey"]!;
    }

    public async Task<string?> EnviarAsync(IFormFile arquivo)
    {
        if (string.IsNullOrEmpty(_url) || string.IsNullOrEmpty(_chave))
            return null;

        var extensao = Path.GetExtension(arquivo.FileName).ToLower();
        var nomeArquivo = $"{Guid.NewGuid()}{extensao}";

        var cliente = _factory.CreateClient();
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _chave);

        using var conteudo = new StreamContent(arquivo.OpenReadStream());
        conteudo.Headers.ContentType = new MediaTypeHeaderValue(arquivo.ContentType);

        var resposta = await cliente.PostAsync(
            $"{_url}/storage/v1/object/{Bucket}/{nomeArquivo}", conteudo);

        resposta.EnsureSuccessStatusCode();

        return $"{_url}/storage/v1/object/public/{Bucket}/{nomeArquivo}";
    }

    public async Task RemoverAsync(string? urlImagem)
    {
        if (string.IsNullOrEmpty(_url) || string.IsNullOrEmpty(_chave) || string.IsNullOrEmpty(urlImagem))
            return;

        var marcador = $"/storage/v1/object/public/{Bucket}/";
        var idx = urlImagem.IndexOf(marcador, StringComparison.OrdinalIgnoreCase);
        if (idx < 0) return;

        var nomeArquivo = urlImagem[(idx + marcador.Length)..];
        if (string.IsNullOrEmpty(nomeArquivo)) return;

        var cliente = _factory.CreateClient();
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _chave);

        try
        {
            await cliente.DeleteAsync($"{_url}/storage/v1/object/{Bucket}/{nomeArquivo}");
        }
        catch
        {
            // falha ao remover imagem antiga não deve interromper a operação principal
        }
    }
}
