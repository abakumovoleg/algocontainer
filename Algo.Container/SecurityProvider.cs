using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reactive.Linq;
using Algo.Abstracts.Interfaces;
using Algo.Abstracts.Models;

namespace Algo.Container
{
    public class SecurityProvider : ISecurityProvider
    { 
        private readonly ConcurrentDictionary<string, Security> _securities 
            = new ConcurrentDictionary<string, Security>();

        public SecurityProvider(IConnector connector)
        {
            connector.NewSecurity.Subscribe(x => _securities[x.Class + "." + x.Code] = x);

            connector.ConnectionChanged
                .Where(x => x.ConnectionState == ConnectionState.Connected &&
                            x.ConnectionType == ConnectionType.Transaction)
                .Subscribe(x => connector.RequestSecurities());

            if(connector.GetConnections().Any(x => x.ConnectionType == ConnectionType.Transaction && x.ConnectionState == ConnectionState.Connected))
                connector.RequestSecurities();
        }

        public Security Get(string code, string exchange)
        {
            return _securities.TryGetValue(exchange + "." + code, out var security) 
                ? security 
                : null;
        }
    }
}