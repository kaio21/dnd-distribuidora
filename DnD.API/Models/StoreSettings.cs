namespace DnD.API.Models;

public class StoreSettings
{
    public int Id { get; set; }
    public string OpenTime { get; set; } = "08:00";
    public string CloseTime { get; set; } = "18:00";
    public bool IsOpen { get; set; } = true;
    public string WhatsAppNumber { get; set; } = string.Empty;
}
