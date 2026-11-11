namespace SociosApp.MercadoPagoExtension.Services.Interfaces;

public interface IMpConfigurationManager
{
    string MercadoPagoUri { get; }
    string NotificationUrl { get; }
    string StoreExternalId { get; }
    string StoreId { get; }
    string Token { get; }
    string SponsorId { get; }
    string UserId { get; }


    //string GetConnectionString(string connectionName);

    //IConfigurationSection GetConfigurationSection(string Key);
}