using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Timers;

namespace websocket_forex_data_scrapper
{
    public class BackgroundSocketProcessor
    {
        public WebSocket _webSocket { get; set; }
        private WebSocketMessageType _messageType = WebSocketMessageType.Text;
        private string _newsJson;

        public BackgroundSocketProcessor()
        {
            var reader = new ForexNewsReader();
            var news = reader.ReadNews();
            _newsJson = JsonSerializer.Serialize(_newsJson);
        }
        public async Task Echo(WebSocket webSocket, TaskCompletionSource<int> taskCompletionSource)
        {
            try
            {


                   //await Task.Delay(1000);//1 second
                    var buffer = new byte[1024 * 4];
                    var receiveResult = await webSocket.ReceiveAsync(
                        new ArraySegment<byte>(buffer), CancellationToken.None);
                    
                    var response = Encoding.UTF8.GetBytes(_newsJson);
                    await _webSocket.SendAsync(
                        new ArraySegment<byte>(response),
                        _messageType,
                        true,
                        CancellationToken.None);
                    Console.WriteLine(_newsJson);

                    var timer = new System.Timers.Timer(1000); // Create a timer with a 1-second interval
                    timer.Elapsed += OnTimerElapsed;
                    timer.AutoReset = true;
                    timer.Enabled = true;

                //await webSocket.CloseAsync(
                //    receiveResult.CloseStatus.Value,
                //    receiveResult.CloseStatusDescription,
                //    CancellationToken.None);

            }
            catch (Exception ex)
            {
                taskCompletionSource.SetException(ex);
            }

        }

        private async void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if(_webSocket.State == WebSocketState.Open)
            {
                var heartBeat = "hi";
                var response = Encoding.UTF8.GetBytes(heartBeat);
                await _webSocket.SendAsync(
                    new ArraySegment<byte>(response),
                    _messageType,
                    true,
                    CancellationToken.None);
            }

        }
    }
}
