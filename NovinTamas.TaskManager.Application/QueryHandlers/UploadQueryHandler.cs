using NovinTamas.Framework.Application;
using NovinTamas.TaskManager.Application.Contracts.Contracts;
using NovinTamas.TaskManager.Application.Contracts.Query.Upload;
using NovinTamas.TaskManager.Application.Contracts.QueryResult.Upload;

namespace NovinTamas.TaskManager.Application.QueryHandlers
{
    public class UploadQueryHandler : IQueryHandler<UploadFilesQuery, UploadFilesQueryResult>
    {
        private readonly IFileStorageService _fileStorage;

        public UploadQueryHandler(IFileStorageService fileStorage)
        {
            _fileStorage = fileStorage;
        }

        public async Task<UploadFilesQueryResult> Handle(UploadFilesQuery query)
        {
            var result = new UploadFilesQueryResult();
            var uploaded = new List<string>();

            try
            {
                foreach (var file in query.Files)
                {
                    var url = await _fileStorage.UploadAsync(file.Bytes, file.FileName, file.ContentType);
                    result.UploadedUrls[file.Key] = url;
                    uploaded.Add(url);
                }

                result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                // آپلود جزئی به معنی فایل یتیم روی دیسک است؛ همه‌ی موفق‌ها برگردانده می‌شوند
                foreach (var path in uploaded)
                    await _fileStorage.DeleteAsync(path);

                result.UploadedUrls.Clear();
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
            }

            return result;
        }
    }
}
