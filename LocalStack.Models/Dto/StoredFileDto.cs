namespace LocalStack.Models.Dto
{
    public sealed class StoredFileDto
    {
        public required Stream Content { get; init; }
        public required string ContentType { get; init; }
        public long ContentLength { get; init; }
    }
}
