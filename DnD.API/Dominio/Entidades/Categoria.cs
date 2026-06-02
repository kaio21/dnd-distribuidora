using System.ComponentModel.DataAnnotations.Schema;

namespace DnD.API.Dominio.Entidades;

[Table("Categories")]
public class Categoria
{
    public int Id { get; private set; }

    [Column("Name")]
    public string Nome { get; private set; } = string.Empty;

    private Categoria() { }

    public static Categoria Criar(string nome) => new() { Nome = nome };
}
