using DnD.API.Dominio.Repositorios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
}
