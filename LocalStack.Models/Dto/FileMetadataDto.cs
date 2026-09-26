namespace LocalStack.Models.Dto
{
    public sealed class FileMetadataDto
    {
        public required string Key { get; init; }
        public required string ContentType { get; init; }
        public long ContentLength { get; init; }
        public DateTime? LastModified { get; init; }
        public string? ETag { get; init; }
    }
}
