using InstantSyncBackend.Application.Interfaces.IServices;
using InstantSyncBackend.Persistence.Services;
using Microsoft.Extensions.DependencyInjection;

namespace InstantSyncBackend.Persistence.ServiceCollections;

public static class ServiceRegistration
{
    public static void AddServices(this IServiceCollection services)
    {
        services.AddScoped<IAccountService, AccountService>();
    }
}