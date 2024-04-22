using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using System.Data;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using websocket_forex_data_scrapper;
using static System.Runtime.InteropServices.JavaScript.JSType;

var builder = WebApplication.CreateSlimBuilder(args);

var services = builder.Services;



var app = builder.Build();


var webSocketOptions = new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromMinutes(2)
};

app.UseWebSockets(webSocketOptions);


app.Use(async (context, next) =>
{
    if (context.Request.Path == "/ws")
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            using var webSocket = await context.WebSockets.AcceptWebSocketAsync();

            var processor = new BackgroundSocketProcessor() { _webSocket = webSocket};
            var socketFinishedTcs = new TaskCompletionSource<int>();

            //scrappd data
            await processor.Echo(webSocket, socketFinishedTcs);

            await socketFinishedTcs.Task;
        }
        else
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }
    else
    {
        await next(context);
    }

});

app.MapGet("/testReadCsv", (HttpRequest request) => {
    var page = request.Query["pair"];

    var reader = new ForexNewsReader();
    var news = new List<ForexNews>();
    if(page.ToString() != "")
    {
        news = reader.ReadNews(page.ToString());
    }
    else
    {
        news = reader.ReadNews();

    }
    var newsJson = JsonConvert.SerializeObject(news);

    return newsJson;
});




app.Run();

