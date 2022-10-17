using Microsoft.AspNetCore.Mvc;

namespace AppConverter.API.Extensions
{
    public static class ControllerExtensions
    {
        /// <summary>
        /// Такой же как <see cref="ControllerBase.BadRequest()"/>, но ещё записывает лог с возникшим исключением.
        /// </summary>
        /// <param name="controller">Контроллер.</param>
        /// <param name="exception">Возникшее исключение.</param>
        /// <param name="logger">Логер.</param>
        /// <returns>Создаёт <see cref="BadRequestResult"/> в качестве ответа.</returns>
        public static ActionResult BadRequestWithLog(this Controller controller, Exception exception, NLog.ILogger logger)
        {
            logger.Error(exception, exception.Message);
            return controller.BadRequest(exception.Message);
        }
    }
}
