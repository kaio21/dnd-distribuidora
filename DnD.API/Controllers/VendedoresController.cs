using DnD.API.Aplicacao.DTOs;
using DnD.API.Dominio;
using DnD.API.Dominio.Repositorios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DnD.API.Controllers;

[ApiController]
[Route("api/sellers")]
public class VendedoresController : ControllerBase
{
    private readonly IVendedorRepositorio _vendedorRepo;

    public VendedoresController(IVendedorRepositorio vendedorRepo) =>
        _vendedorRepo = vendedorRepo;

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ListarAtivos()
    {
        var vendedores = await _vendedorRepo.ListarAprovadosAsync();
        return Ok(vendedores.Select(v => new { v.Id, Name = v.Nome }));
    }

    [HttpGet("me")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> Perfil()
    {
        var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var vendedor = await _vendedorRepo.ObterPorIdAsync(id);
        if (vendedor is null) return NotFound();

        return Ok(new { id = vendedor.Id, name = vendedor.Nome, email = vendedor.Email, role = vendedor.Perfil });
    }

    [HttpPut("me")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> Atualizar([FromBody] AtualizarVendedorDto dto)
    {
        var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var vendedor = await _vendedorRepo.ObterPorIdAsync(id);
        if (vendedor is null) return NotFound();

        vendedor.Atualizar(dto.Nome);
        await _vendedorRepo.SalvarAlteracoesAsync();

        return Ok(new { id = vendedor.Id, name = vendedor.Nome, email = vendedor.Email, role = vendedor.Perfil });
    }

    [HttpPatch("me/password")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> AlterarSenha([FromBody] AlterarSenhaDto dto)
    {
        var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var vendedor = await _vendedorRepo.ObterPorIdAsync(id);
        if (vendedor is null) return NotFound();

        if (!vendedor.ValidarSenha(dto.SenhaAtual))
            throw new CredenciaisInvalidasException("Senha atual incorreta.");

        if (dto.NovaSenha.Length < 6)
            throw new DomainException("A nova senha deve ter pelo menos 6 caracteres.");

        vendedor.AlterarSenha(BCrypt.Net.BCrypt.HashPassword(dto.NovaSenha));
        await _vendedorRepo.SalvarAlteracoesAsync();

        return Ok(new { message = "Senha alterada com sucesso." });
    }
}
