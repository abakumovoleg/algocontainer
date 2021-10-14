using System;
using System.Collections.Generic;
using System.Linq;

namespace Algo.Abstracts.Models
{
    public class MarketDepth
    {
        public MarketDepth(Security security, Quote[] asks, Quote[] bids)
        {
            if (asks != null) Asks = asks;
            if (bids != null) Bids = bids;
            if (security != null) Security = security;
        }

        public Quote[] Asks { get; }
        public Quote[] Bids { get; }

        public DateTime? DateTime { get; set; }

        public Security Security { get; set; }

        public override string ToString()
        {
            var asks = string.Join(Environment.NewLine, (IEnumerable<Quote>)Asks);
            var bids = string.Join(Environment.NewLine, (IEnumerable<Quote>)Bids);

            return $"Asks:{Environment.NewLine}{asks}{Environment.NewLine}Bids{Environment.NewLine}{bids}";
        }
    }
}