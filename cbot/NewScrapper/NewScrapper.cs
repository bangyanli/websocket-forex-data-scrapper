using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Xml.Linq;
using cAlgo.API;
using cAlgo.API.Collections;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using Newtonsoft.Json;
using WebSocketSharp;
using WebSocket = WebSocketSharp.WebSocket;

namespace cAlgo.Robots
{
    [Robot(AccessRights = AccessRights.FullAccess)]
    public class NewsScrapper : Robot
    {

        //private static WebSocketClientOptions _webSocketClientOptions = new WebSocketClientOptions 
        //{
        //    KeepAliveInterval = new TimeSpan(0, 1, 30),
        //    UseDefaultCredentials = true,
        //};
        //private WebSocketClient _webSocketClient = new WebSocketClient(_webSocketClientOptions);
        //private readonly Uri _targetUri = new Uri("ws://amazingnews.com:8000");

        private WebSocket _webSocketClient;
        public List<ForexNews> _news {get; set;}

        protected override void OnStart()
        {
            //_webSocketClient = new WebSocket("ws://localhost:5163/ws");
            //var result = System.Diagnostics.Debugger.Launch();
            //
            //if (result is false)
            //{
            //    Print("Debugger launch failed");
            //}
            Print($"cBot start TimeFrame{Bars.TimeFrame}");
            //_webSocketClient.Connect();
            //_webSocketClient.OnMessage += NewsReceived;
            //Print($"is Alive: {_webSocketClient.IsAlive}");
            //_webSocketClient.Send("hello");
            //
            //System.Threading.Thread.Sleep(1000);
            //var bars = MarketData.GetSeries(Symbol.Name, TimeFrame.Minute); //change period/timeframe from ctrader ui
            var responseToGet = Http.Get($"http://localhost:5163/testReadCsv?pair={Symbol.Name}");

            if (responseToGet.IsSuccessful)
            {
                _news = JsonConvert.DeserializeObject<List<ForexNews>>(responseToGet.Body);
                Print(_news[0].Currency);
                //var result = PlaceLimitOrder(TradeType.Sell, tradedSymbol.SymbolName, 10000, tradedSymbol.ConversionRate + 0.15);

            }

            PendingOrders.Filled += PendingOrders_Filled;

        }

        /// <summary>
        /// remove other pending orders for this symbol when order filled
        /// </summary>
        /// <param name="obj"></param>
        private void PendingOrders_Filled(PendingOrderFilledEventArgs obj)
        {
            foreach (var order in PendingOrders)
            {
                if (order.SymbolName.Equals(Symbol.Name))
                {
                    CancelPendingOrderAsync(order);
                }
            }
        }

        protected override void OnTick()
        {
            //_webSocketClient.Send("hello");
            var curTime = Server.Time.ToLocalTime();

            //add 2 minutse margin before the event
            var curTimeWithMargin = curTime.AddMinutes(2);
            var isMatch = _news.Any(x => x.EventTime == curTimeWithMargin);
            if (isMatch) //TODO add position count == 0 in condition
            {
                foreach (var order in PendingOrders)
                {
                    if (order.SymbolName.Equals(Symbol.Name))
                    {
                        CancelPendingOrder(order);
                    }
                }
                var highPrice = CalculateRecentHigh();
                var lowPrice = CalculateRecentLow();
                MakePendingOrders(highPrice, lowPrice);

            }

            
            Print($"current tick time:{curTime.ToLocalTime()}, isMatch {isMatch}");
        }
        
        private void NewsReceived(object sender, MessageEventArgs e)
        {
            Print("message received");
            Print(e.Data);
        }

        protected override void OnStop()
        {
            Print("cBot stopped");
        }

        private void MakePendingOrders(double high, double low)
        {
            if(PendingOrders.Count == 0)
            {
                PlaceStopOrderAsync(TradeType.Buy, Symbol.Name, 12000, high, "", 25, 50);
                PlaceStopOrderAsync(TradeType.Sell, Symbol.Name, 12000, low, "", 25, 50);
                Print($"PendingOrder saved with buy price {high}, sell price {low}");
            }
        }
        private double CalculateRecentHigh()
        {
            // Get the index for 30 minutes ago
            int index = Bars.ClosePrices.Count - 1 - 30;

            // Ensure the index is not out of range
            if (index < 0) index = 0;

            // Get the highest price in the last 30 minutes
            //double high = Bars.ClosePrices[index];

            var high = Bars.ClosePrices.Reverse().Take(30).Max();
            Print("The highest price in the last 30 minutes was " + Bars.TimeFrame);
            return high;
        }

        private double CalculateRecentLow()
        {
            // Get the index for 30 minutes ago
            int index = Bars.ClosePrices.Count - 1 - 30;

            // Ensure the index is not out of range
            if (index < 0) index = 0;

            // Get the highest price in the last 30 minutes
            //double low = Bars.ClosePrices.Minimum(index);
            var low = Bars.ClosePrices.Reverse().Take(30).Min();
            Print("The lowest price in the last 30 minutes was " + low);

            return low;
        }

        public class ForexNews
        {
            public DateTime EventTime { get; set; }
            public string Currency { get; set; }
            public string News { get; set; }
            public string Impact { get; set; }
        }



    }
}