using AppConverter.API.Extensions;
using AppConverter.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using NLog;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace AppConverter.API.Controllers
{
    [ApiController]
    [Route("api/convert")]
    [Produces("application/json")]
    public class ConverterController : Controller
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConverterService _converterService;

        private static readonly NLog.ILogger _logger = LogManager.GetCurrentClassLogger();

        public ConverterController(IConverterService converterService, IServiceScopeFactory scopeFactory)
        {
            _converterService = converterService;
            _scopeFactory = scopeFactory;
        }

        /// <summary>
        /// Загружает файл на сервер и конвертирует HTML в PDF
        /// </summary>
        [HttpPost("html-pdf")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(Guid))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public IActionResult ConvertHtmlToPdf([Required] IFormFile uploadedHtmlFile)
        {
            try
            {
                byte[] bytes;
                using (var binaryReader = new BinaryReader(uploadedHtmlFile.OpenReadStream()))
                    bytes = binaryReader.ReadBytes((int)uploadedHtmlFile.Length);
                MemoryStream memoryStream = new MemoryStream(bytes);

                // Создаем guid для клиента, чтобы он потом мог забрать файл
                var clientGuid = _converterService.GenerateTimeBasedGuid();

                // Отправляем конвертацию файла в отдельный поток, чтобы сразу вернуть код 200 клиенту
                var task = Task.Run(async () =>
                {
                    using var scope = _scopeFactory.CreateScope();
                    var scopedService = scope.ServiceProvider
                    .GetServices<IConverterService>()
                    .FirstOrDefault();

                    await scopedService.ConvertHtmlToPdfAsync(memoryStream, clientGuid);
                });
                task.ContinueWith(t => this.BadRequestWithLog(t.Exception, _logger), TaskContinuationOptions.OnlyOnFaulted);

                return Ok(clientGuid);
            }
            catch (Exception ex)
            {
                return this.BadRequestWithLog(ex, _logger);
            }
        }

        /// <summary>
        /// Проверяет наличие сконвертированного файла
        /// </summary>
        [HttpHead("check-file")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public IActionResult IsFileExist([Required] Guid guid)
        {
            try
            {
                var isExist = _converterService.IsFileExisting(guid);
                return isExist ? Ok() : NotFound();
            }
            catch (Exception ex)
            {
                return this.BadRequestWithLog(ex, _logger);
            }
        }

        /// <summary>
        /// Скачивает сконвертированный файл
        /// </summary>
        [HttpGet("download-file")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(FileContentResult))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetFile([Required] Guid guid)
        {
            try
            {
                var file = await _converterService.DownloadFileAsync(this, guid);
                return file;
            }
            catch (Exception ex)
            {
                return this.BadRequestWithLog(ex, _logger);
            }
        }
    }
}
