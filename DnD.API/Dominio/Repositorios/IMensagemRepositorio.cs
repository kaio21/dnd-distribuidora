using DnD.API.Dominio.Entidades;

namespace DnD.API.Dominio.Repositorios;

public interface IMensagemRepositorio
{
    Task AdicionarAsync(Mensagem mensagem);
    Task<IEnumerable<Mensagem>> ObterConversaAsync(int compradorId);
    Task<IEnumerable<ResumoConversaProjecao>> ObterResumoConversasAsync();
    Task<Dictionary<int, Vendedor>> ObterVendedoresPorIdsAsync(IEnumerable<int> ids);
    Task<Dictionary<int, Comprador>> ObterCompradoresPorIdsAsync(IEnumerable<int> ids);
    Task SalvarAlteracoesAsync();
}

public record ResumoConversaProjecao(int CompradorId, string UltimoConteudo, DateTime UltimaMensagemEm, int NaoLidas);
