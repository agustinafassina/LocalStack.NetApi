namespace LocalStack.Models.Dto
{
    public sealed class FileUploadResultDto
    {
        public required string Key { get; init; }
        public required string FileName { get; init; }
        public long Size { get; init; }
        public string? ContentType { get; init; }
    }
}
