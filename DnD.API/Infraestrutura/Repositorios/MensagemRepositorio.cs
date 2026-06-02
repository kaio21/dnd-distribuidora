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

    public async Task<IEnumerable<Mensagem>> ObterTodasAsync() =>
        await _db.Mensagens
            .Include(m => m.Comprador)
            .ToListAsync();

    public async Task<IEnumerable<Comprador>> ObterCompradoresComMensagensAsync()
    {
        var mensagens = await _db.Mensagens.ToListAsync();

        var compradorIds = mensagens
            .Select(m => m.TipoRemetente == "Buyer" ? m.RemetenteId : m.DestinatarioId)
            .Distinct();

        return await _db.Compradores
            .Where(c => compradorIds.Contains(c.Id))
            .ToListAsync();
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
