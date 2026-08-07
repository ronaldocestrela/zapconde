namespace Modules.WhatsApp.Domain.Enums;

/// <summary>
/// Tipos de mensagens recebidas ou enviadas via WhatsApp.
/// </summary>
public enum WhatsAppMessageType
{
    Text = 1,
    Image = 2,
    Audio = 3,
    Document = 4,
    Location = 5,
    Contact = 6,
    Sticker = 7,
    Unknown = 99
}
