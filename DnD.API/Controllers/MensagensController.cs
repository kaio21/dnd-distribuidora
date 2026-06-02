using DnD.API.Aplicacao.DTOs;
using DnD.API.Aplicacao.Servicos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DnD.API.Controllers;

[ApiController]
[Route("api/messages")]
[Authorize]
public class MensagensController : ControllerBase
{
    private readonly MensagemServico _servico;

    public MensagensController(MensagemServico servico) => _servico = servico;

    [HttpPost]
    public async Task<IActionResult> Enviar([FromBody] EnviarMensagemDto dto)
    {
        var remetenteId   = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var tipoRemetente = User.FindFirstValue(ClaimTypes.Role)!;
        var nomeRemetente = User.FindFirstValue(ClaimTypes.Name)!;

        var resultado = await _servico.EnviarAsync(dto, remetenteId, tipoRemetente, nomeRemetente);
        return Ok(resultado);
    }

    [HttpGet("conversation/{compradorId}")]
    public async Task<IActionResult> ObterConversa(int compradorId)
    {
        var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var role = User.FindFirstValue(ClaimTypes.Role)!;

        if (role == "Buyer" && usuarioId != compradorId)
            return Forbid();

        var resultado = await _servico.ObterConversaAsync(compradorId, usuarioId, role);
        return Ok(resultado);
    }

    [HttpGet("conversations")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> ListarConversas() =>
        Ok(await _servico.ListarConversasAsync());
}
