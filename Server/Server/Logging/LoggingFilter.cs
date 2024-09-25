using Microsoft.AspNetCore.Mvc.Filters;
using static System.Net.Mime.MediaTypeNames;
using System.Text;
using Azure.Core;

namespace Server.Logging
{
    public class LoggingFilter : IActionFilter
    {
        public async void OnActionExecuting(ActionExecutingContext context)
        {
            var method = context.HttpContext.Request.Method;
            var path = context.HttpContext.Request.Path;
            var statusCode = context.HttpContext.Response.StatusCode;
            Console.WriteLine($"Executing {method} @ {path} with code {statusCode}");
            var request = context.HttpContext.Request;
            try
            {
                var bodyContent = await new StreamReader(request.Body).ReadToEndAsync();
                Console.WriteLine(bodyContent);
            }
            finally
            {
                request.Body.Position = 0;
            }

        }

        public async void OnActionExecuted(ActionExecutedContext context)
        {
            var method = context.HttpContext.Request.Method;
            var path = context.HttpContext.Request.Path;
            var statusCode = context.HttpContext.Response.StatusCode;
            Console.WriteLine($"Executing {method} @ {path} with code {statusCode}");

            var request = context.HttpContext.Request;
            try
            {
                var bodyContent = await new StreamReader(request.Body).ReadToEndAsync();
                Console.WriteLine(bodyContent);
            }
            finally
            {
                request.Body.Position = 0;
            }
        }
    }
}
