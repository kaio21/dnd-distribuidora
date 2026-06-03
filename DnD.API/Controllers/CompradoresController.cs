using DnD.API.Aplicacao.DTOs;
using DnD.API.Dominio;
using DnD.API.Dominio.Repositorios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DnD.API.Controllers;

[ApiController]
[Route("api/buyers")]
[Authorize]
public class CompradoresController : ControllerBase
{
    private readonly ICompradorRepositorio _compradorRepo;

    public CompradoresController(ICompradorRepositorio compradorRepo) =>
        _compradorRepo = compradorRepo;

    [HttpGet("me")]
    public async Task<IActionResult> Perfil()
    {
        var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var comprador = await _compradorRepo.ObterPorIdAsync(id);
        if (comprador is null) return NotFound();

        return Ok(MapearPerfil(comprador));
    }

    [HttpPut("me")]
    public async Task<IActionResult> Atualizar([FromBody] AtualizarCompradorDto dto)
    {
        var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var comprador = await _compradorRepo.ObterPorIdAsync(id);
        if (comprador is null) return NotFound();

        comprador.Atualizar(dto.Nome, dto.NomeLoja, dto.Telefone, dto.Endereco);
        await _compradorRepo.SalvarAlteracoesAsync();

        return Ok(MapearPerfil(comprador));
    }

    [HttpPatch("me/password")]
    public async Task<IActionResult> AlterarSenha([FromBody] AlterarSenhaDto dto)
    {
        var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var comprador = await _compradorRepo.ObterPorIdAsync(id);
        if (comprador is null) return NotFound();

        if (!comprador.ValidarSenha(dto.SenhaAtual))
            throw new CredenciaisInvalidasException("Senha atual incorreta.");

        if (dto.NovaSenha.Length < 6)
            throw new DomainException("A nova senha deve ter pelo menos 6 caracteres.");

        comprador.AlterarSenha(BCrypt.Net.BCrypt.HashPassword(dto.NovaSenha));
        await _compradorRepo.SalvarAlteracoesAsync();

        return Ok(new { message = "Senha alterada com sucesso." });
    }

    private static object MapearPerfil(DnD.API.Dominio.Entidades.Comprador c) => new
    {
        id = c.Id, name = c.Nome, storeName = c.NomeLoja,
        cpf = c.CPF, cnpj = c.CNPJ, phone = c.Telefone,
        address = c.Endereco, email = c.Email
    };
}
