using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualBasic;
using QuickFix;
using QuickFix.FIXBCS;
using QuickFix.Transport;

namespace Fix.Hub
{
    public class FixHubServer
    {
        private SocketInitiator _socketInitiator;
        private ThreadedSocketAcceptor _socketAcceptor;

        public MessageHub MessageHub { get; private set; } = new MessageHub();

        public void Start()
        {
            var serverSettings = new SessionSettings("FixServerConfig.txt");

            var serverStoreFactory = new FileStoreFactory(serverSettings); 

            var clientSettings = new SessionSettings("FixClientConfig.txt");

            var clientStoreFactory = new FileStoreFactory(clientSettings);

            MessageHub = new MessageHub();

            var session = clientSettings.GetSessions().First();

            _socketAcceptor = new ThreadedSocketAcceptor(new FixServer(new MessageProxy(MessageHub, session)),
                serverStoreFactory, serverSettings);

            _socketInitiator = new SocketInitiator(new FixClient(new MessageRouter(MessageHub)), clientStoreFactory,
                clientSettings);

            _socketInitiator.Start();
            _socketAcceptor.Start();
        }

        public void Stop()
        {
            _socketInitiator.Stop();
            _socketAcceptor.Stop();
        }
    } 
}
