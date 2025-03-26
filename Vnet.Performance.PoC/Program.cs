using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient();

var app = builder.Build();

app.MapGet("/compare", async (IHttpClientFactory httpClientFactory) =>
{
    var httpClient = httpClientFactory.CreateClient();

    var publicUrl = "https://public-api.azurewebsites.net/heavy-process";
    var privateUrl = "http://10.0.0.5/heavy-process"; // IP privada o nombre DNS interno en VNET

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
});

app.Run();
