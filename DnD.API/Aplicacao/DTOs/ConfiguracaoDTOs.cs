namespace DnD.API.Aplicacao.DTOs;

public record ConfiguracaoLojaDto(
    string HoraAbertura,
    string HoraFechamento,
    bool Aberta,
    string NumeroWhatsApp
);
