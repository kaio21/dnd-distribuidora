using System.ComponentModel.DataAnnotations.Schema;
using DnD.API.Dominio;

namespace DnD.API.Dominio.Entidades;

[Table("Sellers")]
public class Vendedor
{
    public int Id { get; private set; }

    [Column("Name")]
    public string Nome { get; private set; } = string.Empty;

    public string? CPF { get; private set; }

    public string Email { get; private set; } = string.Empty;

    [Column("PasswordHash")]
    public string SenhaHash { get; private set; } = string.Empty;

    [Column("Role")]
    public string Perfil { get; private set; } = "Vendedor";

    [Column("IsApproved")]
    public bool Aprovado { get; private set; } = true;

    [Column("CreatedAt")]
    public DateTime CriadoEm { get; private set; } = DateTime.UtcNow;

    private Vendedor() { }

    public static Vendedor Criar(string nome, string? cpf, string email, string senhaHash, string perfil, DateTime? criadoEm = null) =>
        new()
        {
            Nome = nome,
            CPF = cpf,
            Email = email,
            SenhaHash = senhaHash,
            Perfil = perfil,
            Aprovado = true,
            CriadoEm = criadoEm ?? DateTime.UtcNow
        };

    public void Atualizar(string nome) => Nome = nome;

    public void AlterarSenha(string novaSenhaHash) => SenhaHash = novaSenhaHash;

    public bool ValidarSenha(string senha) => BCrypt.Net.BCrypt.Verify(senha, SenhaHash);

    public void Aprovar() => Aprovado = true;

    public void Revogar()
    {
        if (Perfil == "Admin")
            throw new DomainException("Não é possível revogar o administrador.");
        Aprovado = false;
    }
}
