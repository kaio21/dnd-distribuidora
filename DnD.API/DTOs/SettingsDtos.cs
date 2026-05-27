namespace DnD.API.DTOs;

public record StoreSettingsDto(string OpenTime, string CloseTime, bool IsOpen, string WhatsAppNumber);
