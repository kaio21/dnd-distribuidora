using System.Text.Json.Serialization;

namespace DnD.API.Aplicacao.DTOs;

public record EnviarMensagemDto(
    [property: JsonPropertyName("receiverId")]   int DestinatarioId,
    [property: JsonPropertyName("receiverType")] string TipoDestinatario,
    [property: JsonPropertyName("content")]      string Conteudo
);

public record RespostaMensagemDto(
    [property: JsonPropertyName("id")]           int Id,
    [property: JsonPropertyName("senderId")]     int RemetenteId,
    [property: JsonPropertyName("senderType")]   string TipoRemetente,
    [property: JsonPropertyName("senderName")]   string NomeRemetente,
    [property: JsonPropertyName("receiverId")]   int DestinatarioId,
    [property: JsonPropertyName("receiverType")] string TipoDestinatario,
    [property: JsonPropertyName("content")]      string Conteudo,
    [property: JsonPropertyName("isRead")]       bool Lida,
    [property: JsonPropertyName("createdAt")]    DateTime CriadoEm
);

public record ConversaDto(
    [property: JsonPropertyName("buyerId")]       int CompradorId,
    [property: JsonPropertyName("buyerName")]     string NomeComprador,
    [property: JsonPropertyName("buyerStoreName")] string NomeLoja,
    [property: JsonPropertyName("unreadCount")]   int NaoLidas,
    [property: JsonPropertyName("lastMessage")]   string UltimaMensagem,
    [property: JsonPropertyName("lastMessageAt")] DateTime UltimaMensagemEm
);
