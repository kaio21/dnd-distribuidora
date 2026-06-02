namespace DnD.API.Aplicacao.Servicos;

public interface IArmazenamentoServico
{
    Task<string?> EnviarAsync(IFormFile arquivo);
    Task RemoverAsync(string? urlImagem);
}
