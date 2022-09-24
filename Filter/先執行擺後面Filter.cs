using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace TryMinimalAPIs.Filter
{
    public class 先執行擺後面Filter : IEndpointFilter
    {
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            WeatherForecast.step = "filter 2";
            context.HttpContext.Response.Headers.TryAdd("step2--before", WeatherForecast.step);

            var result = await next(context); //<--await next放在前面時，Invoke會先等待前次或controller執行完才繼續。

            if (context.HttpContext.Request.Query.Any())
            {
                //這裡無論如何都不會出現，因為一定有人先add過了
                bool isSuccess = context.HttpContext.Response.Headers.TryAdd("CodeMessage1", "this is filter 2 message!");
                if (isSuccess == false)
                {
                    //這裡無論如何都不會出現，因為都被蓋掉了，知道為什麼嗎？
                    //因為Filter1(await before) -> Filter2(await before) -> Filter3(await before) -> controller -> Filter3(await after) -> Filter2(await after) -> Filter1(await after)
                    context.HttpContext.Response.Headers["CodeMessage1"] = "this is filter 2 message!(modified)";
                }
            }

            context.HttpContext.Response.Headers.TryAdd("step2-after", WeatherForecast.step);
            return result;
        }
    }
}
