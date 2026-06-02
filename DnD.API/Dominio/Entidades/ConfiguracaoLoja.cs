using System.ComponentModel.DataAnnotations.Schema;

namespace DnD.API.Dominio.Entidades;

[Table("StoreSettings")]
public class ConfiguracaoLoja
{
    public int Id { get; private set; }

    [Column("OpenTime")]
    public string HoraAbertura { get; private set; } = "08:00";

    [Column("CloseTime")]
    public string HoraFechamento { get; private set; } = "18:00";

    [Column("IsOpen")]
    public bool Aberta { get; private set; } = true;

    [Column("WhatsAppNumber")]
    public string NumeroWhatsApp { get; private set; } = string.Empty;

    private ConfiguracaoLoja() { }

    public static ConfiguracaoLoja Criar() => new();

    public void Atualizar(string horaAbertura, string horaFechamento, bool aberta, string? numeroWhatsApp)
    {
        HoraAbertura = horaAbertura;
        HoraFechamento = horaFechamento;
        Aberta = aberta;
        if (numeroWhatsApp != null)
            NumeroWhatsApp = numeroWhatsApp;
    }
}
