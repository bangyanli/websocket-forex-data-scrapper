namespace websocket_forex_data_scrapper
{
    public class ForexNews
    {
        public DateTime EventTime { get; set; }
        public string Currency { get; set; }
        public string News { get; set; }
        public string Impact { get; set; }
        public string ComparePrev { get; set; }
    }
}
