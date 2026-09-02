using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using LocalStack.Api.HealthChecks;
using LocalStack.Services.Options;

namespace LocalStack.Api.Configurations
{
    public static class S3ConfigurationExtensions
    {
        public static IServiceCollection AddS3Storage(this IServiceCollection services, IConfiguration configuration)
        {
            var options = configuration.GetSection(LocalStackOptions.SectionName).Get<LocalStackOptions>() ?? new LocalStackOptions();
            services.Configure<LocalStackOptions>(configuration.GetSection(LocalStackOptions.SectionName));

            if (options.IsEnabled)
            {
                var region = Amazon.RegionEndpoint.GetBySystemName(options.Region);
                var credentials = new BasicAWSCredentials(options.AccessKey, options.SecretKey);
                services.AddSingleton<IAmazonS3>(_ => new AmazonS3Client(credentials, new AmazonS3Config
                {
                    ServiceURL = options.ServiceUrl,
                    ForcePathStyle = true,
                    AuthenticationRegion = region.SystemName
                }));
            }
            else
            {
                services.AddDefaultAWSOptions(configuration.GetAWSOptions());
                services.AddAWSService<IAmazonS3>();
            }

            services.AddHealthChecks()
                .AddCheck<S3HealthCheck>("s3", tags: ["ready"]);

            return services;
        }

        public static async Task EnsureS3BucketExistsAsync(this WebApplication app, LocalStackOptions options)
        {
            if (!options.IsEnabled)
                return;

            using var scope = app.Services.CreateScope();
            var s3 = scope.ServiceProvider.GetRequiredService<IAmazonS3>();

            try
            {
                await s3.PutBucketAsync(new PutBucketRequest { BucketName = options.BucketName });
                app.Logger.LogInformation("Bucket {Bucket} ready (LocalStack)", options.BucketName);
            }
            catch (AmazonS3Exception ex) when (ex.ErrorCode is "BucketAlreadyOwnedByYou" or "BucketAlreadyExists")
            {
                app.Logger.LogInformation("Bucket {Bucket} already exists (LocalStack)", options.BucketName);
            }
        }
    }
}
