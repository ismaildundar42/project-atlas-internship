namespace DeUygulamaVitrini.Domain.Enums;

/// <summary>
/// Proje entegrasyonunun türünü belirtir.
/// </summary>
public enum IntegrationType
{
    RestApi = 1,
    Database = 2,
    FileTransfer = 3,
    MessageQueue = 4,
    ExternalService = 5,
    Other = 99
}
