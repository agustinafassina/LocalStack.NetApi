using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using LocalStack.Models.Dto;
using LocalStack.Services.Interfaces;
using LocalStack.Services.Options;

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
            string? normalizedKey = NormalizeKey(key);

            PutObjectRequest request = new PutObjectRequest
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
            string? normalizedKey = NormalizeKey(key);

            try
            {
                using GetObjectResponse? response = await _s3Client.GetObjectAsync(new GetObjectRequest
                {
                    BucketName = _options.BucketName,
                    Key = normalizedKey
                }, cancellationToken);

                MemoryStream memoryStream = new MemoryStream();
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
            List<string> keys = new List<string>();
            string? continuationToken = null;

            do
            {
                ListObjectsV2Response? response = await _s3Client.ListObjectsV2Async(new ListObjectsV2Request
                {
                    BucketName = _options.BucketName,
                    ContinuationToken = continuationToken
                }, cancellationToken);

                if (response.S3Objects != null)
                    keys.AddRange(response.S3Objects.Select(o => o.Key));

                continuationToken = response.IsTruncated == true ? response.NextContinuationToken : null;
            }
            while (continuationToken != null);

            _logger.LogInformation("Listed {Count} keys from bucket {Bucket}", keys.Count, _options.BucketName);
            return new FileListResultDto { Keys = keys };
        }

        public async Task DeleteAsync(string key, CancellationToken cancellationToken = default)
        {
            string? normalizedKey = NormalizeKey(key);

            if (!await ExistsAsync(normalizedKey, cancellationToken))
                throw new KeyNotFoundException($"Object with key '{normalizedKey}' not found.");

            await _s3Client.DeleteObjectAsync(new DeleteObjectRequest
            {
                BucketName = _options.BucketName,
                Key = normalizedKey
            }, cancellationToken);

            _logger.LogInformation("Deleted object from S3: {Key}", normalizedKey);
        }

        public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
        {
            string? normalizedKey = NormalizeKey(key);

            try
            {
                await _s3Client.GetObjectMetadataAsync(new GetObjectMetadataRequest
                {
                    BucketName = _options.BucketName,
                    Key = normalizedKey
                }, cancellationToken);
                return true;
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return false;
            }
        }

        public async Task<FileMetadataDto?> GetMetadataAsync(string key, CancellationToken cancellationToken = default)
        {
            string? normalizedKey = NormalizeKey(key);

            try
            {
                GetObjectMetadataResponse? response = await _s3Client.GetObjectMetadataAsync(new GetObjectMetadataRequest
                {
                    BucketName = _options.BucketName,
                    Key = normalizedKey
                }, cancellationToken);

                return new FileMetadataDto
                {
                    Key = normalizedKey,
                    ContentType = response.Headers.ContentType ?? "application/octet-stream",
                    ContentLength = response.ContentLength,
                    LastModified = response.LastModified,
                    ETag = response.ETag
                };
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Metadata not found for S3 object: {Key}", normalizedKey);
                return null;
            }
        }

        public Task<PresignedUrlDto> GetPresignedUrlAsync(string key, string verb = "GET", int expiresInMinutes = 15, CancellationToken cancellationToken = default)
        {
            string? normalizedKey = NormalizeKey(key);
            HttpVerb httpVerb = ParseVerb(verb);
            DateTime expiresAt = DateTime.UtcNow.AddMinutes(Math.Clamp(expiresInMinutes, 1, 60));

            var request = new GetPreSignedUrlRequest
            {
                BucketName = _options.BucketName,
                Key = normalizedKey,
                Verb = httpVerb,
                Expires = expiresAt
            };

            string? url = _s3Client.GetPreSignedURL(request);
            _logger.LogInformation("Generated {Verb} presigned URL for {Key}, expires at {ExpiresAt}", httpVerb, normalizedKey, expiresAt);

            return Task.FromResult(new PresignedUrlDto
            {
                Key = normalizedKey,
                Url = url,
                Verb = httpVerb.ToString(),
                ExpiresAt = expiresAt
            });
        }

        private static HttpVerb ParseVerb(string verb)
        {
            return verb.Trim().ToUpperInvariant() switch
            {
                "GET" => HttpVerb.GET,
                "PUT" => HttpVerb.PUT,
                "DELETE" => HttpVerb.DELETE,
                "HEAD" => HttpVerb.HEAD,
                _ => throw new ArgumentException("Verb must be GET, PUT, DELETE or HEAD.", nameof(verb))
            };
        }

        private static string NormalizeKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be empty.", nameof(key));

            string? normalized = key.Trim().Replace('\\', '/').TrimStart('/');
            if (normalized.Contains("..", StringComparison.Ordinal))
                throw new ArgumentException("Key cannot contain '..'.", nameof(key));

            return normalized;
        }
    }
}
