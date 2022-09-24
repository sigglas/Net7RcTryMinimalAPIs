using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace TryMinimalAPIs.Filter
{
    public class 先執行擺前面Filter : IEndpointFilter
    {
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            WeatherForecast.step = "filter 0";
            context.HttpContext.Response.Headers.TryAdd("step0--before", WeatherForecast.step);

            var result = await next(context); //<--await next放在前面時，Invoke會先等待前次或controller執行完才繼續。

            if (context.HttpContext.Request.Query.Any())
            {
                
                bool isSuccess = context.HttpContext.Response.Headers.TryAdd("CodeMessage1", "this is filter 0 message!");//有帶參數的情況，如果"先驗證Filter"報告驗證失敗，那只會顯示此行
                if (isSuccess == false)
                {
                    //有帶參數的情況，如果所有流程完整結束，那只會顯示此行
                    context.HttpContext.Response.Headers["CodeMessage1"] = "this is filter 0 message!(modified)";
                }
            }

            context.HttpContext.Response.Headers.TryAdd("step0-after", WeatherForecast.step);
            return result;
        }
    }
}
