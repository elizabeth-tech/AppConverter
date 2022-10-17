using AppConverter.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using NLog;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using System.Reflection;

namespace AppConverter.Services
{
    public class ConverterService : IConverterService
    {
        private readonly string storage = "clients_files";
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Конвертация файл из HTML в PDF
        /// </summary>
        /// <param name="memoryStream">Временное хранилище байтов файла</param>
        /// <param name="guid">Уникальный ID (название файла) по которому клиент будет получать файл</param>
        public async Task ConvertHtmlToPdfAsync(MemoryStream memoryStream, Guid guid)
        {
            try
            {
                var bytes = memoryStream.ToArray();
                string html = System.Text.Encoding.UTF8.GetString(bytes);

                var browserFetcher = new BrowserFetcher();
                await browserFetcher.DownloadAsync();
                var browser = await Puppeteer.LaunchAsync(new LaunchOptions());
                var page = await browser.NewPageAsync();
                await page.EmulateMediaTypeAsync(MediaType.Screen);
                await page.SetContentAsync(html);

                // Сохранение файла в PDF
                await SavePdfAsync(page, guid);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            finally
            {
                memoryStream.Dispose();
            }
        }

        /// <summary>
        /// Сохранение файла в PDF
        /// </summary>
        private async Task SavePdfAsync(IPage page, Guid guid)
        {
            string uniqueFileName = $"{guid}.pdf";
            string executableLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string path = Path.Combine(executableLocation, storage);
            Directory.CreateDirectory(path);
            string file = Path.Combine(path, uniqueFileName);
            await page.PdfAsync(file);
        }

        /// <summary>
        /// Генерация Guid на основе времени
        /// </summary>
        public Guid GenerateTimeBasedGuid()
        {
            var bytes = BitConverter.GetBytes(DateTime.Now.Ticks);
            Array.Resize(ref bytes, 16);
            return new Guid(bytes);
        }

        /// <summary>
        /// Проверяет, существует ли файл в папке
        /// </summary>
        /// <param name="guid">ID файла</param>
        public bool IsFileExisting(Guid guid)
        {
            string executableLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string path = Path.Combine(executableLocation, storage, guid + ".pdf");
            return File.Exists(path);
        }

        /// <summary>
        /// Возвращает файл
        /// </summary>
        public async Task<FileContentResult> DownloadFileAsync(ControllerBase controller, Guid guid)
        {
            string executableLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string path = Path.Combine(executableLocation, storage, guid + ".pdf");
            FileInfo fileInfo = new FileInfo(path);
            if (fileInfo.Exists)
            {
                var bytes = await File.ReadAllBytesAsync(path);
                return controller.File(bytes, "application/pdf", fileInfo.Name);
            }
            else
                throw new Exception($"Файла с guid {guid} не существует");
        }
    }
}
