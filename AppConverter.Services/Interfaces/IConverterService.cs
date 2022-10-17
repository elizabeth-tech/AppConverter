using Microsoft.AspNetCore.Mvc;

namespace AppConverter.Services.Interfaces
{
    public interface IConverterService
    {
        Guid GenerateTimeBasedGuid();

        Task ConvertHtmlToPdfAsync(MemoryStream memoryStream, Guid guid);

        Task<FileContentResult> DownloadFileAsync(ControllerBase controller, Guid guid);

        bool IsFileExisting(Guid guid);
    }
}
