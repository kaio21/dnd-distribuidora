using DnD.API.Aplicacao.DTOs;
using DnD.API.Dominio.Entidades;
using DnD.API.Dominio.Repositorios;
using DnD.API.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace DnD.API.Aplicacao.Servicos;

public class MensagemServico
{
    private readonly IMensagemRepositorio _mensagemRepo;
    private readonly IHubContext<ChatHub> _hub;

    public MensagemServico(IMensagemRepositorio mensagemRepo, IHubContext<ChatHub> hub)
    {
        _mensagemRepo = mensagemRepo;
        _hub = hub;
    }

    public async Task<RespostaMensagemDto> EnviarAsync(EnviarMensagemDto dto, int remetenteId, string tipoRemetente, string nomeRemetente)
    {
        var mensagem = Mensagem.Criar(remetenteId, tipoRemetente, dto.DestinatarioId, dto.TipoDestinatario, dto.Conteudo);

        await _mensagemRepo.AdicionarAsync(mensagem);
        await _mensagemRepo.SalvarAlteracoesAsync();

        var resposta = new RespostaMensagemDto(
            mensagem.Id, remetenteId, tipoRemetente, nomeRemetente,
            dto.DestinatarioId, dto.TipoDestinatario, dto.Conteudo,
            false, mensagem.CriadoEm);

        var compradorId = tipoRemetente == "Buyer" ? remetenteId : dto.DestinatarioId;
        await _hub.Clients.Group($"chat-{compradorId}").SendAsync("ReceiveMessage", resposta);

        return resposta;
    }

    public async Task<IEnumerable<RespostaMensagemDto>> ObterConversaAsync(int compradorId, int usuarioId, string role)
    {
        var mensagens = (await _mensagemRepo.ObterConversaAsync(compradorId)).ToList();

        if (role == "Seller")
        {
            var naolidas = mensagens.Where(m => m.TipoRemetente == "Buyer" && !m.Lida).ToList();
            naolidas.ForEach(m => m.MarcarComoLida());
            if (naolidas.Any()) await _mensagemRepo.SalvarAlteracoesAsync();
        }

        var todosIds = mensagens.Select(m => m.RemetenteId)
            .Concat(mensagens.Select(m => m.DestinatarioId))
            .Distinct();

        var vendedoresIds = mensagens.Where(m => m.TipoRemetente == "Seller").Select(m => m.RemetenteId)
            .Concat(mensagens.Where(m => m.TipoDestinatario == "Seller").Select(m => m.DestinatarioId))
            .Distinct();

        var compradorIds = mensagens.Where(m => m.TipoRemetente == "Buyer").Select(m => m.RemetenteId)
            .Concat(mensagens.Where(m => m.TipoDestinatario == "Buyer").Select(m => m.DestinatarioId))
            .Distinct();

        var vendedores  = await _mensagemRepo.ObterVendedoresPorIdsAsync(vendedoresIds);
        var compradores = await _mensagemRepo.ObterCompradoresPorIdsAsync(compradorIds);

        return mensagens.Select(m => new RespostaMensagemDto(
            m.Id, m.RemetenteId, m.TipoRemetente,
            m.TipoRemetente == "Buyer"
                ? (compradores.TryGetValue(m.RemetenteId, out var c) ? c.Nome : "Comprador")
                : (vendedores.TryGetValue(m.RemetenteId, out var v) ? v.Nome : "Vendedor"),
            m.DestinatarioId, m.TipoDestinatario, m.Conteudo, m.Lida, m.CriadoEm));
    }

    public async Task<IEnumerable<ConversaDto>> ListarConversasAsync()
    {
        var resumos = (await _mensagemRepo.ObterResumoConversasAsync()).ToList();
        var compradores = await _mensagemRepo.ObterCompradoresPorIdsAsync(resumos.Select(r => r.CompradorId));

        return resumos
            .Where(r => compradores.ContainsKey(r.CompradorId))
            .Select(r =>
            {
                var comprador = compradores[r.CompradorId];
                return new ConversaDto(
                    comprador.Id, comprador.Nome, comprador.NomeLoja, r.NaoLidas,
                    r.UltimoConteudo, r.UltimaMensagemEm);
            })
            .OrderByDescending(c => c.UltimaMensagemEm);
    }
}
