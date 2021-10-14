using Algo.Abstracts.Models;
using Algo.Abstracts.Models.Messages;

namespace Algo.Abstracts.Interfaces
{
    public interface IStorage<TEntity, TKey>
    {
        void Add(TEntity order, TKey id);
        TEntity Get(TKey id);
    }

    public interface IConnectorStorage
    {
        IStorage<Order,string> Orders { get; }
        IStorage<Trade, string> Trades { get; }
        IStorage<Security, string> Securities { get; }
        IStorage<Message, string> Messages { get; }

        IStorage<Portfolio, string> Portfolios { get; }
    }

}