namespace NovinTamas.TaskManager.Application.Contracts.Contracts
{
    public interface IFileStorageService
    {
        Task<string> UploadAsync(byte[] bytes, string fileName, string contentType);
        Task DeleteAsync(string path);
    }
}
