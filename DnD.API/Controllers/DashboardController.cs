using DnD.API.Aplicacao.Servicos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DnD.API.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize(Roles = "Seller")]
public class DashboardController : ControllerBase
{
    private readonly DashboardServico _servico;

    public DashboardController(DashboardServico servico) => _servico = servico;

    [HttpGet]
    public async Task<IActionResult> ObterDashboard() =>
        Ok(await _servico.ObterDashboardAsync());

    [HttpGet("sales")]
    public async Task<IActionResult> ObterVendas(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to) =>
        Ok(await _servico.ObterVendasAsync(from, to));
}
