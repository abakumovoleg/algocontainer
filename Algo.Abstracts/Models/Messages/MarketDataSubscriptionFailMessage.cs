namespace Algo.Abstracts.Models.Messages
{
    public class MarketDataSubscriptionFailMessage : FailMessage
    {
        public MarketDataSubscriptionMessage MarketDataSubscription { get; set; }

        public override string ToString()
        {
            return $"{Error} {MarketDataSubscription}";
        }
    }
}