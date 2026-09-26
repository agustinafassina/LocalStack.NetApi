using LocalStack.Models.Dto;

namespace LocalStack.Services.Interfaces
{
    public interface IStorageService
    {
        Task<string> UploadAsync(Stream content, string key, string? contentType = null, CancellationToken cancellationToken = default);
        Task<StoredFileDto?> GetAsync(string key, CancellationToken cancellationToken = default);
        Task<FileListResultDto> ListKeysAsync(CancellationToken cancellationToken = default);
        Task DeleteAsync(string key, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
        Task<FileMetadataDto?> GetMetadataAsync(string key, CancellationToken cancellationToken = default);
        Task<PresignedUrlDto> GetPresignedUrlAsync(string key, string verb = "GET", int expiresInMinutes = 15, CancellationToken cancellationToken = default);
    }
}
