using Algo.Abstracts.Models;

namespace Algo.Abstracts.Interfaces
{
    public interface ISecurityProvider
    {
        Security Get(string code, string exchange);
    }
}