using System.Collections.Generic;
using System.IO;

namespace Algo.BackTesting
{
    public class FileReader : IFileReader
    {
        public IEnumerable<MarketAction> ReadFile(string filePath)
        {
            var content = File.ReadAllText(filePath);

            var result = Newtonsoft.Json.JsonConvert.DeserializeObject<MarketAction[]>(content);

            return result;
        }
    }
}