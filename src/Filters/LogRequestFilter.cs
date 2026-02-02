using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using System.Text.Json;

namespace SW_PortalCliente_BeachPark.API.src.Filters
{
    public class LogRequestFilter : ActionFilterAttribute
    {
        private readonly ILogger<LogRequestFilter> _logger;
        private readonly IServiceBase _serviceBase;
        public LogRequestFilter(ILogger<LogRequestFilter> logger,
            IServiceBase serviceBase)
        {
            _logger = logger;
            _serviceBase = serviceBase;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            try
            {
                if (context.ActionArguments.Any())
                {
                    _serviceBase.RequestArguments = JsonSerializer.Serialize(context.ActionArguments);
                }

            }
            catch (Exception err)
            { }
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            int status = StatusCodes.Status204NoContent;
            //if (context.Result is ObjectResult objectResult)
            //{
            //    var data = objectResult.Value ?? objectResult;
            //    if (data != null)
            //    {
            //        _serviceBase.ResponseData = JsonSerializer.Serialize(data);
            //    }
            //    status = objectResult.StatusCode.GetValueOrDefault();
            //}

            if (context.Result is FileResult fileResultCodeResult)
            {
                status = StatusCodes.Status200OK;
                //var data = fileResultCodeResult;
                //if (data != null)
                //{
                //    _serviceBase.ResponseData = JsonSerializer.Serialize(data);
                //}
            }
            else if (context.Result is StatusCodeResult statusCodeResult)
            {
                status = statusCodeResult.StatusCode;
            }
            else if (context.Result is BadRequestResult badRequestResult)
            {
                status = badRequestResult.StatusCode;
            }
            else if (context.Result is FileNotFoundException)
            {
                status = 404;
            }
            else if (context.Result is OkObjectResult okObjectResult)
            {
                status = okObjectResult.StatusCode.GetValueOrDefault(200);
            }


            _serviceBase.AddLogAuditoriaMessageToQueue(context.HttpContext, status).GetAwaiter().GetResult();

        }

    }
}
