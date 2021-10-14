namespace Algo.Abstracts.Models.Messages
{
    public class CancelOrderMessage : Message
    {
        public CancelOrderMessage(Order order)
        {
            Order = order;
        }
        public Order Order { get; }

        public override string ToString()
        {
            return $"{Order}";
        }
    }
}