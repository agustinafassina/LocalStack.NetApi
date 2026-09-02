namespace LocalStack.Services.Options
{
    public sealed class LocalStackOptions
    {
        public const string SectionName = "LocalStack";
        public string? ServiceUrl { get; set; }
        public string BucketName { get; set; } = "local-bucket";
        public string Region { get; set; } = "us-east-1";
        public string AccessKey { get; set; } = "test";
        public string SecretKey { get; set; } = "test";
        public long MaxUploadBytes { get; set; } = 10 * 1024 * 1024;
        public bool IsEnabled => !string.IsNullOrWhiteSpace(ServiceUrl);
    }
}
