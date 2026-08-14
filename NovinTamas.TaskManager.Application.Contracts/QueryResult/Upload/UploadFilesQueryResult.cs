namespace NovinTamas.TaskManager.Application.Contracts.QueryResult.Upload
{
    public class UploadFilesQueryResult
    {
        public Dictionary<string, string> UploadedUrls { get; set; } = new();
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
