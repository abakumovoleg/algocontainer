using System.Collections.Concurrent;
using Algo.Abstracts.Interfaces;

namespace Algo.Container.Fix.NtPro
{
    public class Storage<TValue, TKey> : IStorage<TValue, TKey>
    {
        private readonly ConcurrentDictionary<TKey, TValue> _items = new ConcurrentDictionary<TKey, TValue>();

        public void Add(TValue entity, TKey key)
        {
            _items[key] = entity;
        }

        public TValue Get(TKey key)
        {
            _items.TryGetValue(key, out var value);

            return value;
        }
    }
}