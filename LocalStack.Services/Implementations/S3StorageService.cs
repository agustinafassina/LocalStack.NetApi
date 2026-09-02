using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using LocalStack.Services.Options;
using LocalStack.Models.Dto;
using LocalStack.Services.Interfaces;

namespace LocalStack.Services.Implementations
{
    public class S3StorageService : IStorageService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly LocalStackOptions _options;
        private readonly ILogger<S3StorageService> _logger;

        public S3StorageService(
            IAmazonS3 s3Client,
            IOptions<LocalStackOptions> options,
            ILogger<S3StorageService> logger)
        {
            _s3Client = s3Client;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<string> UploadAsync(Stream content, string key, string? contentType = null, CancellationToken cancellationToken = default)
        {
            string normalizedKey = NormalizeKey(key);

            PutObjectRequest request = new()
            {
                BucketName = _options.BucketName,
                Key = normalizedKey,
                InputStream = content,
                ContentType = contentType ?? "application/octet-stream"
            };

            await _s3Client.PutObjectAsync(request, cancellationToken);
            _logger.LogInformation("Uploaded object to S3: {Key} in bucket {Bucket}", normalizedKey, _options.BucketName);
            return normalizedKey;
        }

        public async Task<StoredFileDto?> GetAsync(string key, CancellationToken cancellationToken = default)
        {
            string normalizedKey = NormalizeKey(key);

            try
            {
                using GetObjectResponse response = await _s3Client.GetObjectAsync(new GetObjectRequest
                {
                    BucketName = _options.BucketName,
                    Key = normalizedKey
                }, cancellationToken);

                MemoryStream memoryStream = new();
                await response.ResponseStream.CopyToAsync(memoryStream, cancellationToken);
                memoryStream.Position = 0;

                _logger.LogInformation("Retrieved object from S3: {Key}", normalizedKey);
                return new StoredFileDto
                {
                    Content = memoryStream,
                    ContentType = response.Headers.ContentType ?? "application/octet-stream",
                    ContentLength = response.ContentLength
                };
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Object not found in S3: {Key}", normalizedKey);
                return null;
            }
        }

        public async Task<FileListResultDto> ListKeysAsync(CancellationToken cancellationToken = default)
        {
            List<string> keys = new();
            string? continuationToken = null;

            do
            {
                ListObjectsV2Response response = await _s3Client.ListObjectsV2Async(new ListObjectsV2Request
                {
                    BucketName = _options.BucketName,
                    ContinuationToken = continuationToken
                }, cancellationToken);

                if (response.S3Objects != null)
                    keys.AddRange(response.S3Objects.Select(o => o.Key));

                continuationToken = response.IsTruncated ? response.NextContinuationToken : null;
            }
            while (continuationToken != null);

            _logger.LogInformation("Listed {Count} keys from bucket {Bucket}", keys.Count, _options.BucketName);
            return new FileListResultDto { Keys = keys };
        }

        public async Task DeleteAsync(string key, CancellationToken cancellationToken = default)
        {
            string normalizedKey = NormalizeKey(key);

            if (!await ExistsAsync(normalizedKey, cancellationToken))
                throw new KeyNotFoundException($"Object with key '{normalizedKey}' not found.");

            await _s3Client.DeleteObjectAsync(new()
            {
                BucketName = _options.BucketName,
                Key = normalizedKey
            }, cancellationToken);

            _logger.LogInformation("Deleted object from S3: {Key}", normalizedKey);
        }

        private async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken)
        {
            try
            {
                await _s3Client.GetObjectMetadataAsync(new()
                {
                    BucketName = _options.BucketName,
                    Key = key
                }, cancellationToken);
                return true;
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return false;
            }
        }

        private static string NormalizeKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be empty.", nameof(key));

            string normalized = key.Trim().Replace('\\', '/').TrimStart('/');
            if (normalized.Contains("..", StringComparison.Ordinal))
                throw new ArgumentException("Key cannot contain '..'.", nameof(key));

            return normalized;
        }
    }
}
