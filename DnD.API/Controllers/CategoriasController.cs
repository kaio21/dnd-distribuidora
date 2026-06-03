using DnD.API.Aplicacao.Servicos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DnD.API.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriasController : ControllerBase
{
    private readonly CategoriaServico _servico;

    public CategoriasController(CategoriaServico servico) => _servico = servico;

    [HttpGet]
    public async Task<IActionResult> Listar() =>
        Ok(await _servico.ListarAsync());

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Criar([FromBody] CriarCategoriaRequisicao req)
    {
        if (User.FindFirstValue(ClaimTypes.Role) != "Seller")
            return Forbid();

        return Ok(await _servico.CriarAsync(req.Name));
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Remover(int id)
    {
        if (User.FindFirstValue(ClaimTypes.Role) != "Seller")
            return Forbid();

        var removido = await _servico.RemoverAsync(id);
        return removido ? NoContent() : NotFound();
    }
}

public record CriarCategoriaRequisicao(string Name);
