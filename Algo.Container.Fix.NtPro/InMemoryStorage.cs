using Algo.Abstracts.Interfaces;
using Algo.Abstracts.Models;
using Algo.Abstracts.Models.Messages;

namespace Algo.Container.Fix.NtPro
{
    public class InMemoryStorage : IConnectorStorage
    {
        public IStorage<Order, string> Orders { get; } = new Storage<Order, string>();
        public IStorage<Trade, string> Trades { get; } = new Storage<Trade, string>();
        public IStorage<Security, string> Securities { get; } = new Storage<Security, string>();
        public IStorage<Message, string> Messages { get; } = new Storage<Message, string>();
        public IStorage<Portfolio, string> Portfolios { get; } = new Storage<Portfolio, string>();
    }
}