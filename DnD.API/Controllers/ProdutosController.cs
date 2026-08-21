using DnD.API.Aplicacao.DTOs;
using DnD.API.Aplicacao.Servicos;
using DnD.API.Dominio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DnD.API.Controllers;

[ApiController]
[Route("api/products")]
public class ProdutosController : ControllerBase
{
    private readonly ProdutoServico _servico;

    public ProdutosController(ProdutoServico servico) => _servico = servico;

    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] bool? activeOnly,
        [FromQuery] string? search,
        [FromQuery] string? category,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 24) =>
        Ok(await _servico.ListarPaginadoAsync(activeOnly, search, category, page, pageSize));

    [HttpGet("{id}")]
    public async Task<IActionResult> ObterPorId(int id)
    {
        var dto = await _servico.ObterPorIdAsync(id);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> Criar([FromForm] CriarProdutoDto dto, IFormFile? image)
    {
        var resultado = await _servico.CriarAsync(dto, image);
        return CreatedAtAction(nameof(ObterPorId), new { id = resultado.Id }, resultado);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> Atualizar(int id, [FromForm] AtualizarProdutoDto dto, IFormFile? image)
    {
        var resultado = await _servico.AtualizarAsync(id, dto, image);
        return resultado is null ? NotFound() : Ok(resultado);
    }

    [HttpPatch("{id}/toggle")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> AlternarAtividade(int id)
    {
        var ativo = await _servico.AlternarAtividadeAsync(id);
        return ativo is null ? NotFound() : Ok(new { isActive = ativo });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> Remover(int id)
    {
        var removido = await _servico.RemoverAsync(id);
        return removido ? NoContent() : NotFound();
    }
}
