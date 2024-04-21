using System.Data;
using System.Text.Json.Serialization;
using websocket_forex_data_scrapper;

var builder = WebApplication.CreateSlimBuilder(args);


//add service

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


app.Run();
