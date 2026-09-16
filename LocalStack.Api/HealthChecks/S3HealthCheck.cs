using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using LocalStack.Services.Options;

namespace LocalStack.Api.HealthChecks
{
    public sealed class S3HealthCheck : IHealthCheck
    {
        private readonly IAmazonS3 _s3Client;
        private readonly LocalStackOptions _options;

        public S3HealthCheck(IAmazonS3 s3Client, IOptions<LocalStackOptions> options)
        {
            _s3Client = s3Client;
            _options = options.Value;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                await _s3Client.GetBucketLocationAsync(new GetBucketLocationRequest
                {
                    BucketName = _options.BucketName
                }, cancellationToken);

                return HealthCheckResult.Healthy($"Bucket '{_options.BucketName}' is reachable.");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy($"S3 bucket '{_options.BucketName}' is not reachable.", ex);
            }
        }
    }
}
