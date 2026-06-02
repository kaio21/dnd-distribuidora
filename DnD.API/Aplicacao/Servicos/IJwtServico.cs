namespace DnD.API.Aplicacao.Servicos;

public interface IJwtServico
{
    string GerarToken(int usuarioId, string email, string role, string nome, string? perfilVendedor = null);
}
