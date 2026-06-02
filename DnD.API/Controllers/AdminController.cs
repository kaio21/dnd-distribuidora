using DnD.API.Aplicacao.DTOs;
using DnD.API.Aplicacao.Servicos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DnD.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize]
public class AdminController : ControllerBase
{
    private readonly AdminServico _servico;

    public AdminController(AdminServico servico) => _servico = servico;

    [HttpGet("sellers")]
    public async Task<IActionResult> ListarVendedores()
    {
        if (!EhAdmin()) return Forbid();
        return Ok(await _servico.ListarVendedoresAsync());
    }

    [HttpPost("sellers")]
    public async Task<IActionResult> CriarVendedor([FromBody] CriarVendedorDto dto)
    {
        if (!EhAdmin()) return Forbid();
        return Ok(await _servico.CriarVendedorAsync(dto));
    }

    [HttpPatch("sellers/{id}/revoke")]
    public async Task<IActionResult> Revogar(int id)
    {
        if (!EhAdmin()) return Forbid();
        var resultado = await _servico.RevogarAsync(id);
        return resultado is null ? NotFound() : Ok(resultado);
    }

    [HttpPatch("sellers/{id}/approve")]
    public async Task<IActionResult> Aprovar(int id)
    {
        if (!EhAdmin()) return Forbid();
        var resultado = await _servico.AprovarAsync(id);
        return resultado is null ? NotFound() : Ok(resultado);
    }

    private bool EhAdmin() =>
        User.FindFirstValue(ClaimTypes.Role) == "Seller" &&
        User.FindFirstValue("SellerRole") == "Admin";
}
