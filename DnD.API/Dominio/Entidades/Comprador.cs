using System.ComponentModel.DataAnnotations.Schema;

namespace DnD.API.Dominio.Entidades;

[Table("Buyers")]
public class Comprador
{
    public int Id { get; private set; }

    [Column("Name")]
    public string Nome { get; private set; } = string.Empty;

    [Column("StoreName")]
    public string NomeLoja { get; private set; } = string.Empty;

    public string? CPF { get; private set; }

    public string? CNPJ { get; private set; }

    public string Email { get; private set; } = string.Empty;

    [Column("PasswordHash")]
    public string SenhaHash { get; private set; } = string.Empty;

    [Column("Phone")]
    public string Telefone { get; private set; } = string.Empty;

    [Column("Address")]
    public string Endereco { get; private set; } = string.Empty;

    [Column("CreatedAt")]
    public DateTime CriadoEm { get; private set; } = DateTime.UtcNow;

    [Column("ReferredBySellerId")]
    public int? IndicadoPorVendedorId { get; private set; }

    public Vendedor? IndicadoPorVendedor { get; private set; }

    public ICollection<Pedido> Pedidos { get; private set; } = new List<Pedido>();
    public ICollection<Mensagem> Mensagens { get; private set; } = new List<Mensagem>();

    private Comprador() { }

    public static Comprador Criar(
        string nome, string nomeLoja, string email, string senhaHash,
        string telefone, string endereco, string? cpf = null,
        string? cnpj = null, int? indicadoPorVendedorId = null,
        DateTime? criadoEm = null) =>
        new()
        {
            Nome = nome,
            NomeLoja = nomeLoja,
            Email = email,
            SenhaHash = senhaHash,
            Telefone = telefone,
            Endereco = endereco,
            CPF = cpf,
            CNPJ = cnpj,
            IndicadoPorVendedorId = indicadoPorVendedorId,
            CriadoEm = criadoEm ?? DateTime.UtcNow
        };

    public void Atualizar(string nome, string nomeLoja, string telefone, string endereco)
    {
        Nome = nome;
        NomeLoja = nomeLoja;
        Telefone = telefone;
        Endereco = endereco;
    }

    public void AlterarSenha(string novaSenhaHash) => SenhaHash = novaSenhaHash;

    public bool ValidarSenha(string senha) => BCrypt.Net.BCrypt.Verify(senha, SenhaHash);
}
