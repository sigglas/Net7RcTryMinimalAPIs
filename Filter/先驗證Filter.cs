namespace TryMinimalAPIs.Filter
{
    public class 先驗證Filter : IEndpointFilter
    {
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            WeatherForecast.step = "filter 1";
            context.HttpContext.Response.Headers.TryAdd("step1--before", WeatherForecast.step);

            if (context.HttpContext.Request.Query.Any(a => a.Key == "a"))
            {
                return Results.BadRequest(new { msg = $"禁止出現a" });
            }
            else
            {
                bool isSuccess = context.HttpContext.Response.Headers.TryAdd("CodeMessage1", "this is filter 1 message!");//驗證通過且沒帶任何參數的，會出現這行
                if (isSuccess == false)
                {
                    //永遠不會執行到
                    context.HttpContext.Response.Headers["CodeMessage1"] = "this is filter 1 message!(modified)";
                }
            }

            context.HttpContext.Response.Headers.TryAdd("step1-after", WeatherForecast.step);
            return await next(context); //<--await next放在後面時，Invoke會先執行完才輪到下一個。
        }
    }
}
