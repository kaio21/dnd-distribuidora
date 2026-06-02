using DnD.API.Aplicacao.DTOs;
using DnD.API.Aplicacao.Servicos;
using Microsoft.AspNetCore.Mvc;

namespace DnD.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AutenticacaoController : ControllerBase
{
    private readonly AutenticacaoServico _servico;

    public AutenticacaoController(AutenticacaoServico servico) => _servico = servico;

    [HttpPost("seller/login")]
    public async Task<IActionResult> LoginVendedor([FromBody] LoginVendedorDto dto) =>
        Ok(await _servico.LoginVendedorAsync(dto));

    [HttpPost("buyer/register")]
    public async Task<IActionResult> RegistrarComprador([FromBody] RegistrarCompradorDto dto) =>
        Ok(await _servico.RegistrarCompradorAsync(dto));

    [HttpPost("buyer/login")]
    public async Task<IActionResult> LoginComprador([FromBody] LoginCompradorDto dto) =>
        Ok(await _servico.LoginCompradorAsync(dto));
}
