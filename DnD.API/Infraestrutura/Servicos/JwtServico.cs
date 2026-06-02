using DnD.API.Aplicacao.Servicos;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DnD.API.Infraestrutura.Servicos;

public class JwtServico : IJwtServico
{
    private readonly IConfiguration _config;

    public JwtServico(IConfiguration config) => _config = config;

    public string GerarToken(int usuarioId, string email, string role, string nome, string? perfilVendedor = null)
    {
        var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSettings:SecretKey"]!));
        var credenciais = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuarioId.ToString()),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Role, role),
            new(ClaimTypes.Name, nome)
        };

        if (perfilVendedor != null)
            claims.Add(new Claim("SellerRole", perfilVendedor));

        var token = new JwtSecurityToken(
            issuer: _config["JwtSettings:Issuer"],
            audience: _config["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(int.Parse(_config["JwtSettings:ExpirationHours"]!)),
            signingCredentials: credenciais);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
