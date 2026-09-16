using LocalStack.Models.Dto;

namespace LocalStack.Services.Interfaces
{
    public interface IStorageService
    {
        Task<string> UploadAsync(Stream content, string key, string? contentType = null, CancellationToken cancellationToken = default);
        Task<StoredFileDto?> GetAsync(string key, CancellationToken cancellationToken = default);
        Task<FileListResultDto> ListKeysAsync(CancellationToken cancellationToken = default);
        Task DeleteAsync(string key, CancellationToken cancellationToken = default);
    }
}
