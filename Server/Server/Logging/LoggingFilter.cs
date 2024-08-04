using Microsoft.AspNetCore.Mvc.Filters;

namespace Server.Logging
{
    public class LoggingFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            Console.WriteLine($"Executing {context.HttpContext.Request.Method} {context.HttpContext.Request.Path}");
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            Console.WriteLine($"Executed {context.HttpContext.Request.Method} {context.HttpContext.Request.Path}");
        }
    }
}
