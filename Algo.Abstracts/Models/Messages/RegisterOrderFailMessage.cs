namespace Algo.Abstracts.Models.Messages
{
    public class RegisterOrderFailMessage : FailMessage
    {
        public Order Order { get; set; }

        public override string ToString()
        {
            return $"{Error} {Order}";
        }
    }
}