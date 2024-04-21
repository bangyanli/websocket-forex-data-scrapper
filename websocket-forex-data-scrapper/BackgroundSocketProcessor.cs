using System;
using System.Net.WebSockets;
using System.Text;
using System.Timers;

namespace websocket_forex_data_scrapper
{
    public class BackgroundSocketProcessor
    {
        public WebSocket _webSocket { get; set; }
        private WebSocketMessageType _messageType = WebSocketMessageType.Text;

        public async Task Echo(WebSocket webSocket, TaskCompletionSource<int> taskCompletionSource)
        {
            try
            {

                    var timer = new System.Timers.Timer(1000); // Create a timer with a 1-second interval
                    timer.Elapsed += OnTimerElapsed;
                    timer.AutoReset = true;
                    timer.Enabled = true;
                    //await Task.Delay(1000);//1 second
                    //receiveResult = await webSocket.ReceiveAsync(
                    //    new ArraySegment<byte>(buffer), CancellationToken.None);
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
                string str = $"Hello world {e.SignalTime}";
                var buffer = Encoding.UTF8.GetBytes(str);
                await _webSocket.SendAsync(
                    new ArraySegment<byte>(buffer),
                    _messageType,
                    true,
                    CancellationToken.None);
                Console.WriteLine("Timer ticked at " + e.SignalTime);
            }

        }
    }
}
