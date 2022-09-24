using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Channels;
using TryMinimalAPIs.Background;
using TryMinimalAPIs.Filter;
using TryMinimalAPIs.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var maxQueueSize = 1024;
builder.Services.AddSingleton<Channel<ReadOnlyMemory<byte>>>((_) =>
                     Channel.CreateBounded<ReadOnlyMemory<byte>>(maxQueueSize));
builder.Services.AddHostedService<�I����C>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};
//https://www.youtube.com/watch?v=TwlaTWuMMmQ
//https://learn.microsoft.com/zh-tw/aspnet/core/release-notes/aspnetcore-7.0?view=aspnetcore-7.0#filters-in-minimal-api-apps
//https://blog.csdn.net/sD7O95O/article/details/126457649

//1. �U�عL�o���Ϊk
app.MapGet("/weatherforecast", () =>
{
    WeatherForecast.step = "controller";

    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.AddEndpointFilter<�������\�e��Filter>() //
.AddEndpointFilter<������Filter>()
.AddEndpointFilter<�������\�᭱Filter>()
.AddEndpointFilter(async (context, next) => {
    //����add
    var result = await next(context);
    if (context.HttpContext.Request.Query.Any())
    {
        context.HttpContext.Response.Headers.TryAdd("CodeMessage2", "this is filter inline message!");
    }

    return result;
});

//2. �W���ɮ� IFormFile or IFormFileCollection 
app.MapPost("/upload", async (IFormFile file) =>
{
    var buffer = new MemoryStream();
    await file.CopyToAsync(buffer);
    return new { fileLength = buffer.ToArray().Length };
});

//3. �}�C�䴩 (�R�ѡG�o��²�檺�F��H�e���䴩???)
app.MapGet("/array", (string[] str) =>
{
    //�쪩�d�Ҷö�@�w�|�X���A�ҥH�ڧﱼ�F
    StringBuilder sb = new StringBuilder();
    int i = 0;
    str.ToList().ForEach(item => {
        sb.Append($" name{++i}={item} ,");
    });

    return sb.ToString();
    
});
//��L�٦� (StringValues strs)

//4.�ݩʪ���(���`)
app.MapPost("/todos1", async (TodoRequest todo) =>
{
    return todo;
});

//4.�ݩʪ���[�JAsParameters(�`�N�e�X�˦�)
app.MapPost("/todos2", async ([AsParameters] TodoRequest todo) =>
{
    return todo;
});

//5.�w��S�wapi�ۭq���������Ψ�L
app.MapGet("/cutomer-desc1", () => "Hellow sucker!") //<--inline Lambda message.
    .WithOpenApi(operation =>
    {
        operation.Summary = "OH Hi!";
        operation.Description = "Don't look me.";
        return operation; 
    });

app.MapGet("/cutomer-desc2", () => "Hellow sucker!")
    .WithOpenApi(operation =>
    {
        operation.Summary = "OH Hi!";
        operation.Description = ApiDescModel.GetContent();
        return operation;
    });

//�T����ܦbapi doc�W
app.MapGet("/skipme", () => "Skipping Swagger.")
                    .ExcludeFromDescription();

//6. Route Groups
var api = app.MapGroup("/api");
api.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.AddEndpointFilter(async (context, next) => {
    //����add
    var result = await next(context);
    if (context.HttpContext.Request.Query.Any())
    {
        context.HttpContext.Response.Headers.TryAdd("CodeMessage2", "this is filter 2 message!");
    }

    return result;
});

//7.��C��Ʀ���
api.MapPost("/register", async (HttpRequest req, Stream body,
                                 Channel<ReadOnlyMemory<byte>> queue) =>
{
    //�o��G�N�屼�d�Ҫ��\�h���ҡA�u�O���n�]��BackgroundService�������A�аѷ�
    //https://learn.microsoft.com/zh-tw/aspnet/core/fundamentals/minimal-apis?view=aspnetcore-7.0#bind-the-request-body-as-a-stream-or-pipereader

    var readSize = (int)(req.ContentLength ?? 0);

    var buffer = new byte[readSize];

    var read = await body.ReadAtLeastAsync(buffer, readSize, throwOnEndOfStream: false);

    if (queue.Writer.TryWrite(buffer.AsMemory(0..read)))
    {
        return Results.Accepted();
    }

    return Results.StatusCode(StatusCodes.Status429TooManyRequests);
});

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

    public static string step = "none";

    public string Step => step;
}
