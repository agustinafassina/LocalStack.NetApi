namespace LocalStack.Models.Dto
{
    public sealed class PresignedUrlDto
    {
        public required string Key { get; init; }
        public required string Url { get; init; }
        public required string Verb { get; init; }
        public DateTime ExpiresAt { get; init; }
    }
}
