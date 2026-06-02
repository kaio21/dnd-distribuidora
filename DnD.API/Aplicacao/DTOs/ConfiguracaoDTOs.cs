using System.Text.Json.Serialization;

namespace DnD.API.Aplicacao.DTOs;

public record ConfiguracaoLojaDto(
    [property: JsonPropertyName("openTime")]       string HoraAbertura,
    [property: JsonPropertyName("closeTime")]      string HoraFechamento,
    [property: JsonPropertyName("isOpen")]         bool Aberta,
    [property: JsonPropertyName("whatsAppNumber")] string NumeroWhatsApp
);
