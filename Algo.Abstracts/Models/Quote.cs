namespace Algo.Abstracts.Models
{
    public class Quote
    {
        public Security Security { get; set; }

        public decimal Price { get; set; }

        public decimal Volume { get; set; }

        public QuoteType QuoteType { get; set; }

        public override string ToString()
        {
            return $"{Security} {Price} {Volume} {QuoteType}";
        }
    }
}