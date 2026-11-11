using Microsoft.Extensions.Configuration;
using SociosApp.MercadoPagoExtension.Services.Interfaces;

namespace SociosApp.MercadoPagoExtension.Services;

public class MpConfigurationManager : IMpConfigurationManager
{
    private readonly IConfiguration _configuration;
    public MpConfigurationManager(IConfiguration configuration)
    {
        this._configuration = configuration;
    }

    public string MercadoPagoUri
        => _configuration["MercadoPago:Uri"] ?? throw new ArgumentNullException("AppSetting MercadoPago:Uri can not be null");

    public string NotificationUrl
        => _configuration["MercadoPago:NotificationUrl"] ?? throw new ArgumentNullException("AppSetting MercadoPago:NotificationUrl can not be null");

    public string StoreExternalId
        => _configuration["MercadoPago:StoreExternalId"] ?? throw new ArgumentNullException("AppSetting MercadoPago:StoreExternalId can not be null");

    public string StoreId
        => _configuration["MercadoPago:StoreId"] ?? throw new ArgumentNullException("AppSetting MercadoPago:StoreId can not be null");

    public string Token
        => _configuration["MercadoPago:Token"] ?? throw new ArgumentNullException("AppSetting MercadoPago:Token can not be null");

    public string SponsorId
      => _configuration["MercadoPago:SponsorId"] ?? throw new ArgumentNullException("AppSetting MercadoPago:SponsorId can not be null");

    public string UserId
        => _configuration["MercadoPago:UserId"] ?? throw new ArgumentNullException("AppSetting MercadoPago:UserId can not be null");


    //public string GetConnectionString(string connectionName)
    //    => this._configuration.GetConnectionString(connectionName);

    //public IConfigurationSection GetConfigurationSection(string key)
    //    => this._configuration.GetSection(key);
}
