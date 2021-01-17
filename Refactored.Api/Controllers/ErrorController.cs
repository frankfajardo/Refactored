using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Refactored.Api.Controllers
{
    // Based on https://docs.microsoft.com/en-us/aspnet/core/web-api/handle-errors?view=aspnetcore-5.0
    // Additional info: https://andrewlock.net/creating-a-custom-error-handler-middleware-function/

    [ApiExplorerSettings(IgnoreApi = true)] // This is to exclude this controller from our Swagger.
    [ApiController]
    public class ErrorController : Controller
    {

        [Route("/error")]
        public IActionResult Error() => Problem();

        [Route("/error-local-development")]
        public IActionResult ErrorLocalDevelopment(
        [FromServices] IWebHostEnvironment webHostEnvironment)
        {
            if (webHostEnvironment.EnvironmentName != "Development")
            {
                throw new InvalidOperationException(
                    "This shouldn't be invoked in non-development environments.");
            }

            var context = HttpContext.Features.Get<IExceptionHandlerFeature>();

            return Problem(
                detail: $"{context.Error.GetType().FullName} received {context.Error.StackTrace}",
                title: context.Error.Message);
        }
    }
}
