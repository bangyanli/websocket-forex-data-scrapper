namespace websocket_forex_data_scrapper
{

    public class NewsComparer : IEqualityComparer<ForexNews>
    {
        public bool Equals(ForexNews one, ForexNews two)
        {
            var date1 = one.EventTime;
            var date2 = two.EventTime;
            date1.Equals(date2);
            // Adjust according to your requirements.
            return StringComparer.InvariantCultureIgnoreCase.Equals(one.Currency, two.Currency) && date1.Equals(date2);
        }

        public int GetHashCode(ForexNews item)
        {
            var itemStr = $"{item.Currency}{item.EventTime.ToUniversalTime}";
            return StringComparer.InvariantCultureIgnoreCase.GetHashCode(itemStr);
        }
    }
}
