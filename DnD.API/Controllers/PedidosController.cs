using DnD.API.Aplicacao.DTOs;
using DnD.API.Aplicacao.Servicos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DnD.API.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public class PedidosController : ControllerBase
{
    private readonly PedidoServico _servico;

    public PedidosController(PedidoServico servico) => _servico = servico;

    [HttpPost]
    [Authorize(Roles = "Buyer")]
    public async Task<IActionResult> Criar([FromBody] CriarPedidoDto dto)
    {
        var compradorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var resultado = await _servico.CriarAsync(dto, compradorId);
        return CreatedAtAction(nameof(ObterPorId), new { id = resultado.Id }, resultado);
    }

    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] string? status,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var role = User.FindFirstValue(ClaimTypes.Role);
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var perfilVendedor = User.FindFirstValue("SellerRole");

        return Ok(await _servico.ListarAsync(role, userId, perfilVendedor, status, from, to, page, pageSize));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObterPorId(int id)
    {
        var dto = await _servico.ObterPorIdAsync(id);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> AtualizarStatus(int id, [FromBody] AtualizarStatusPedidoDto dto) =>
        Ok(await _servico.AtualizarStatusAsync(id, dto.Status));

    [HttpPatch("{id}/seller")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> AtribuirVendedor(int id, [FromBody] AtribuirVendedorDto dto)
    {
        var resultado = await _servico.AtribuirVendedorAsync(id, dto);
        return resultado is null ? NotFound() : Ok(resultado);
    }
}
