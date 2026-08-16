using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CourseHub.Controllers
{
    public class ErrorController : Controller
    {
        private readonly ILogger<ErrorController> _logger;

        public ErrorController(ILogger<ErrorController> logger)
        {
            _logger = logger;
        }

        [Route("/Error")]
        public IActionResult Error()
        {
            var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerFeature>();

            var exception = exceptionFeature?.Error;

            if (exception != null)
            {
                _logger.LogError(exception, "Unhandled exception occurred.");
            }

            return View();
        }

        [Route("/Error/StatusCode")]
        public IActionResult StatusCode(int statusCode)
        {
            _logger.LogWarning("HTTP Status Code {StatusCode} occurred for {Path}", statusCode, HttpContext.Request.Path);

            ViewBag.StatusCode = statusCode;

            return View();
        }
    }
}