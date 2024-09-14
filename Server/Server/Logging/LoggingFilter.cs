using Microsoft.AspNetCore.Mvc.Filters;

namespace Server.Logging
{
    public class LoggingFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            var method = context.HttpContext.Request.Method;
            var path = context.HttpContext.Request.Path;
            Console.WriteLine($"Executing {method} @ {path}");
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            var method = context.HttpContext.Request.Method;
            var path = context.HttpContext.Request.Path;
            Console.WriteLine($"Executing {method} @ {path}");
        }
    }
}
