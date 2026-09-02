using Microsoft.Extensions.DependencyInjection;
using LocalStack.Services.Implementations;
using LocalStack.Services.Interfaces;

namespace LocalStack.Services
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddTransient<IItemService, ItemService>();
            services.AddTransient<IStorageService, S3StorageService>();
            return services;
        }
    }
}
