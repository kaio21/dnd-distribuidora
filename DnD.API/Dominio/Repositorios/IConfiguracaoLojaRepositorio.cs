using DnD.API.Dominio.Entidades;

namespace DnD.API.Dominio.Repositorios;

public interface IConfiguracaoLojaRepositorio
{
    Task<ConfiguracaoLoja?> ObterAsync();
    Task AdicionarAsync(ConfiguracaoLoja configuracao);
    Task SalvarAlteracoesAsync();
}
