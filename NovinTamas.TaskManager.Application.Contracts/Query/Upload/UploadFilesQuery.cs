using NovinTamas.Framework.Application;

namespace NovinTamas.TaskManager.Application.Contracts.Query.Upload
{
    public class UploadFilesQuery : IQuery
    {
        public List<FileToUpload> Files { get; set; } = new();
    }

    public class FileToUpload
    {
        public string Key { get; set; } = string.Empty;
        public byte[] Bytes { get; set; } = Array.Empty<byte>();
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
    }
}
