using System.ComponentModel.DataAnnotations.Schema;

namespace DnD.API.Dominio.Entidades;

[Table("Messages")]
public class Mensagem
{
    public int Id { get; private set; }

    [Column("SenderId")]
    public int RemetenteId { get; private set; }

    [Column("SenderType")]
    public string TipoRemetente { get; private set; } = string.Empty;

    [Column("ReceiverId")]
    public int DestinatarioId { get; private set; }

    [Column("ReceiverType")]
    public string TipoDestinatario { get; private set; } = string.Empty;

    [Column("Content")]
    public string Conteudo { get; private set; } = string.Empty;

    [Column("IsRead")]
    public bool Lida { get; private set; } = false;

    [Column("CreatedAt")]
    public DateTime CriadoEm { get; private set; } = DateTime.UtcNow;

    [Column("BuyerId")]
    public int? CompradorId { get; private set; }

    public Comprador? Comprador { get; private set; }

    private Mensagem() { }

    public static Mensagem Criar(int remetenteId, string tipoRemetente, int destinatarioId, string tipoDestinatario, string conteudo) =>
        new()
        {
            RemetenteId = remetenteId,
            TipoRemetente = tipoRemetente,
            DestinatarioId = destinatarioId,
            TipoDestinatario = tipoDestinatario,
            Conteudo = conteudo,
            CriadoEm = DateTime.UtcNow
        };

    public void MarcarComoLida() => Lida = true;

    internal void DefinirCompradorIdSeed(int compradorId) => CompradorId = compradorId;
    internal void DefinirDataSeed(DateTime data) => CriadoEm = data;
}
