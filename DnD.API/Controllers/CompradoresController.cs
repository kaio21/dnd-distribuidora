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

        return Ok(new
        {
            id = comprador.Id, name = comprador.Nome, storeName = comprador.NomeLoja,
            cpf = comprador.CPF, cnpj = comprador.CNPJ, phone = comprador.Telefone,
            address = comprador.Endereco, email = comprador.Email
        });
    }
}
