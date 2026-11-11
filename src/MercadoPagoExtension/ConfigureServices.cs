using MercadoPagoExtension.Clients;
using SociosApp.MercadoPagoExtension.Clients.Interfaces;
using SociosApp.MercadoPagoExtension.Services;
using SociosApp.MercadoPagoExtension.Services.Interfaces;

namespace Microsoft.Extensions.DependencyInjection;

public static class ConfigureServices
{
    public static IServiceCollection AddMercadoPagoServices(this IServiceCollection services)
    {
        //Services
        services.AddSingleton<IMpConfigurationManager, MpConfigurationManager>();

        //Clients
        services.AddTransient<IOrderClient, OrdenClient>();


        return services;
    }
}
