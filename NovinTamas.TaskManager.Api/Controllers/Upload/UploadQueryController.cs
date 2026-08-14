using Microsoft.AspNetCore.Mvc;
using NovinTamas.Framework.Api;
using NovinTamas.Framework.Application;
using NovinTamas.TaskManager.Application.Contracts.Query.Upload;
using NovinTamas.TaskManager.Application.Contracts.QueryResult.Upload;

namespace NovinTamas.TaskManager.Api.Controllers.Upload
{
    public class UploadQueryController : BaseQueryController
    {
        private const long MaxUploadFileSize = 10 * 1024 * 1024;
        private const long MaxUploadRequestSize = 50 * 1024 * 1024;

        public UploadQueryController(IQueryBus bus) : base(bus)
        {
        }

        [HttpPost("UploadFiles")]
        [RequestSizeLimit(MaxUploadRequestSize)]
        [RequestFormLimits(MultipartBodyLengthLimit = MaxUploadRequestSize)]
        public async Task<ActionResult<UploadFilesQueryResult>> UploadFiles([FromForm] List<IFormFile> files, [FromForm] List<string> keys)
        {
            if (files.Count != keys.Count)
                return BadRequest(new { ErrorCode = "ERR_400", ErrorMessage = "تعداد فایل‌ها و کلیدها برابر نیست." });

            var payload = new List<FileToUpload>();

            for (var i = 0; i < files.Count; i++)
            {
                if (files[i].Length > MaxUploadFileSize)
                    return BadRequest(new { ErrorCode = "ERR_400", ErrorMessage = $"حداکثر حجم هر فایل {MaxUploadFileSize / 1024 / 1024} مگابایت است." });

                using var memoryStream = new MemoryStream();
                await files[i].CopyToAsync(memoryStream);

                payload.Add(new FileToUpload
                {
                    Key = keys[i],
                    Bytes = memoryStream.ToArray(),
                    FileName = files[i].FileName,
                    ContentType = files[i].ContentType
                });
            }

            return Ok(await Bus.Dispatch<UploadFilesQuery, UploadFilesQueryResult>(new UploadFilesQuery { Files = payload }));
        }
    }
}
