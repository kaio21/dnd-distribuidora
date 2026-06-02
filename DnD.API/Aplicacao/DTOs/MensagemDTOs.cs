namespace DnD.API.Aplicacao.DTOs;

public record EnviarMensagemDto(
    int DestinatarioId,
    string TipoDestinatario,
    string Conteudo
);

public record RespostaMensagemDto(
    int Id,
    int RemetenteId,
    string TipoRemetente,
    string NomeRemetente,
    int DestinatarioId,
    string TipoDestinatario,
    string Conteudo,
    bool Lida,
    DateTime CriadoEm
);

public record ConversaDto(
    int CompradorId,
    string NomeComprador,
    string NomeLoja,
    int NaoLidas,
    string UltimaMensagem,
    DateTime UltimaMensagemEm
);
