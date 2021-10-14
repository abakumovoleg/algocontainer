using System.Collections.Generic;

namespace Algo.BackTesting
{
    public interface IFileReader
    {
        IEnumerable<MarketAction> ReadFile(string filePath);
    }
}