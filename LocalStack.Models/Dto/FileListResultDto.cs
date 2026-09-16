namespace LocalStack.Models.Dto
{
    public sealed class FileListResultDto
    {
        public required IReadOnlyList<string> Keys { get; init; }
        public int Count => Keys.Count;
    }
}
