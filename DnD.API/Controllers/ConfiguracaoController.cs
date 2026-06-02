using DnD.API.Aplicacao.DTOs;
using DnD.API.Aplicacao.Servicos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DnD.API.Controllers;

[ApiController]
[Route("api/settings")]
public class ConfiguracaoController : ControllerBase
{
    private readonly ConfiguracaoLojaServico _servico;

    public ConfiguracaoController(ConfiguracaoLojaServico servico) => _servico = servico;

    [HttpGet]
    public async Task<IActionResult> Obter() =>
        Ok(await _servico.ObterAsync());

    [HttpPut]
    [Authorize]
    public async Task<IActionResult> Atualizar([FromBody] ConfiguracaoLojaDto dto)
    {
        if (User.FindFirstValue(ClaimTypes.Role) != "Seller")
            return Forbid();

        return Ok(await _servico.AtualizarAsync(dto));
    }
}
