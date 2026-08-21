using DnD.API.Dominio.Entidades;
using DnD.API.Dominio.Repositorios;
using DnD.API.Infraestrutura.Dados;
using Microsoft.EntityFrameworkCore;

namespace DnD.API.Infraestrutura.Repositorios;

public class MensagemRepositorio : IMensagemRepositorio
{
    private readonly AppDbContext _db;

    public MensagemRepositorio(AppDbContext db) => _db = db;

    public async Task AdicionarAsync(Mensagem mensagem) =>
        await _db.Mensagens.AddAsync(mensagem);

    public async Task<IEnumerable<Mensagem>> ObterConversaAsync(int compradorId) =>
        await _db.Mensagens
            .Where(m =>
                (m.TipoRemetente == "Buyer" && m.RemetenteId == compradorId) ||
                (m.TipoDestinatario == "Buyer" && m.DestinatarioId == compradorId))
            .OrderBy(m => m.CriadoEm)
            .ToListAsync();

    public async Task<IEnumerable<ResumoConversaProjecao>> ObterResumoConversasAsync()
    {
        // Agregação (MAX/COUNT) roda no banco, não na aplicação — evita carregar a
        // tabela inteira de mensagens em memória a cada abertura da lista de conversas.
        var resumos = await _db.Mensagens
            .Select(m => new
            {
                CompradorId = m.TipoRemetente == "Buyer" ? m.RemetenteId : m.DestinatarioId,
                m.CriadoEm,
                m.Lida,
                m.TipoRemetente
            })
            .GroupBy(x => x.CompradorId)
            .Select(g => new
            {
                CompradorId = g.Key,
                UltimaMensagemEm = g.Max(x => x.CriadoEm),
                NaoLidas = g.Sum(x => x.TipoRemetente == "Buyer" && !x.Lida ? 1 : 0)
            })
            .ToListAsync();

        if (resumos.Count == 0)
            return [];

        // Busca só as mensagens que efetivamente são "a última" de cada conversa
        // (poucas linhas), em vez da tabela toda.
        var datas = resumos.Select(r => r.UltimaMensagemEm).ToList();
        var candidatas = await _db.Mensagens
            .Where(m => datas.Contains(m.CriadoEm))
            .ToListAsync();

        return resumos.Select(r =>
        {
            var ultima = candidatas.FirstOrDefault(m =>
                m.CriadoEm == r.UltimaMensagemEm &&
                (m.TipoRemetente == "Buyer" ? m.RemetenteId : m.DestinatarioId) == r.CompradorId);

            return new ResumoConversaProjecao(r.CompradorId, ultima?.Conteudo ?? "", r.UltimaMensagemEm, r.NaoLidas);
        });
    }

    public async Task<Dictionary<int, Vendedor>> ObterVendedoresPorIdsAsync(IEnumerable<int> ids) =>
        await _db.Vendedores
            .Where(v => ids.Contains(v.Id))
            .ToDictionaryAsync(v => v.Id);

    public async Task<Dictionary<int, Comprador>> ObterCompradoresPorIdsAsync(IEnumerable<int> ids) =>
        await _db.Compradores
            .Where(c => ids.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id);

    public Task SalvarAlteracoesAsync() =>
        _db.SaveChangesAsync();
}
