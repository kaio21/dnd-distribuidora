using DnD.API.Dominio.Entidades;

namespace DnD.API.Dominio.Repositorios;

public interface IMensagemRepositorio
{
    Task AdicionarAsync(Mensagem mensagem);
    Task<IEnumerable<Mensagem>> ObterConversaAsync(int compradorId);
    Task<IEnumerable<Mensagem>> ObterTodasAsync();
    Task<IEnumerable<Comprador>> ObterCompradoresComMensagensAsync();
    Task<Dictionary<int, Vendedor>> ObterVendedoresPorIdsAsync(IEnumerable<int> ids);
    Task<Dictionary<int, Comprador>> ObterCompradoresPorIdsAsync(IEnumerable<int> ids);
    Task SalvarAlteracoesAsync();
}
