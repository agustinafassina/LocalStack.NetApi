using LocalStack.Api.Mappers;

namespace LocalStack.Api.Configurations
{
    public static class AutoMapperConfiguration
    {
        public static void AddMappers(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(ContractMapping));
        }
    }
}
