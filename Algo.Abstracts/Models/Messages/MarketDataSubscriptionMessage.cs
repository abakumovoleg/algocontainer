namespace Algo.Abstracts.Models.Messages
{
    public class MarketDataSubscriptionMessage : Message
    {
        public Security Security { get; set; }

        public MarketDataType MarketDataType { get; set; }

        public MarketDataSubscriptionType MarketDataSubscriptionType { get; set; }

        public MarketDataSubscriptionQuoteType MarketDataSubscriptionQuoteType { get; set; }

        public override string ToString()
        {
            return $"{Security} {MarketDataType}";
        }
    }
}