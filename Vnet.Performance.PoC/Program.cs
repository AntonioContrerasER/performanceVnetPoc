using System.Diagnostics;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API Benchmark",
        Version = "v1",
        Description = "Compara latencia entre APIs pública y privada"
    });
});

builder.Services.AddHttpClient();

var app = builder.Build();


app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Benchmark v1");
});


app.MapGet("/compare", async (IHttpClientFactory httpClientFactory) =>
{
    var httpClient = httpClientFactory.CreateClient();

    var publicUrl = "https://public-api.azurewebsites.net/heavy-process";
    var privateUrl = "http://10.0.0.5/heavy-process"; // IP privada o DNS interno

    async Task<object> PingUrl(string url)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var response = await httpClient.GetAsync(url);
            sw.Stop();

            return new
            {
                url,
                statusCode = (int)response.StatusCode,
                timeInMs = sw.ElapsedMilliseconds
            };
        }
        catch (Exception ex)
        {
            sw.Stop();
            return new
            {
                url,
                error = ex.Message,
                timeInMs = sw.ElapsedMilliseconds
            };
        }
    }

    var publicResult = await PingUrl(publicUrl);
    var privateResult = await PingUrl(privateUrl);

    return Results.Ok(new
    {
        publicApi = publicResult,
        privateApi = privateResult
    });
})
.WithName("CompareApis")
.WithOpenApi();

app.Run();
