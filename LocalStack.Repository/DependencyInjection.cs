using Microsoft.Extensions.DependencyInjection;
using LocalStack.Repository.Implementations;
using LocalStack.Repository.Interfaces;

namespace LocalStack.Repository
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddSingleton<IItemRepository, ItemRepository>();
            return services;
        }
    }
}
