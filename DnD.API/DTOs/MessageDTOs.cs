namespace DnD.API.DTOs;

public record SendMessageDto(
    int ReceiverId,
    string ReceiverType,
    string Content
);

public record MessageResponseDto(
    int Id,
    int SenderId,
    string SenderType,
    string SenderName,
    int ReceiverId,
    string ReceiverType,
    string Content,
    bool IsRead,
    DateTime CreatedAt
);

public record ConversationDto(
    int BuyerId,
    string BuyerName,
    string BuyerStoreName,
    int UnreadCount,
    string LastMessage,
    DateTime LastMessageAt
);
