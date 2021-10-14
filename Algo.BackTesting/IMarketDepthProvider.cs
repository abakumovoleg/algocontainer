using System;
using System.Net.NetworkInformation;
using System.Reactive;
using System.Reflection;
using Algo.Abstracts.Models;
using Newtonsoft.Json.Linq;

namespace Algo.BackTesting
{
    public interface IMarketDepthProvider
    {
        IObservable<MarketDepth> MarketDepthChanged { get; }
    }
}
