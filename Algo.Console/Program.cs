using System;
using System.Reactive.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading;
using System.Threading.Tasks;
using Algo.Abstracts.Interfaces;
using Algo.Abstracts.Models;
using Algo.Abstracts.Models.Messages;
using Algo.Container.Connectors.Bcs.FIX;
using Algo.Container.Serilog;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QuickFix;
using Serilog;

namespace Algo.Console
{
    class Program
    {
         
        static void Main()
        { 
            var settings = new SessionSettings("FixConfig.txt");

            var host = Host.CreateDefaultBuilder().UseSerilog((context, configuration) =>
                {
                    foreach (var sessionId in settings.GetSessions())
                    {
                        configuration.AddFixLoggingToFiles(sessionId.SenderCompID, sessionId.TargetCompID);
                    }
                    
                })
                .Build();

            var lf = host.Services.GetRequiredService<ILoggerFactory>();

            /*
            var fixBcs = (from x in XDocument.Parse(File.ReadAllText("FIXBCS.xml")).Root.XPathSelectElements("//components//component") select x).ToArray();
            var fix44 = (from x in XDocument.Parse(File.ReadAllText("FIX44.xml")).Root.XPathSelectElements("//components//component") select x).ToArray();
            var add = fix44.Where(x => fixBcs.All(z => z.Attribute("name").Value != x.Attribute("name").Value)).ToArray();
            var resultStatus = string.Join(Environment.NewLine, add.Select(x=>x.ToString()));
            */


            IConnector connector = new Connector("FixConfig.txt", lf);

      
            connector.ConnectionChanged.Subscribe(System.Console.WriteLine);

            connector.MarketDataSubscriptionFailed.Subscribe(System.Console.WriteLine);
            connector.RegisterOrderFailed.Subscribe(System.Console.WriteLine);
            connector.OrderChanged.Subscribe(System.Console.WriteLine);
            connector.TradeChanged.Subscribe(System.Console.WriteLine);
            connector.MarketDepthChanged.Subscribe(System.Console.WriteLine);

            connector.Connect();
             
            connector.WaitForConnect();

            connector.NewSecurity
                .Subscribe(x=>
                {
                    if (x.Code.Contains("USD000UTSTOM"))
                    {
                        connector.RegisterOrder(new Order
                        {
                            OrderType = OrderType.Limit,
                            Portfolio = new Portfolio
                            {
                                ClientCode = "30_10008",
                                Name = "MB1002600091"
                            },
                            Price = 76,
                            Security = x,
                            Direction = Direction.Buy,
                            TimeInForce = TimeInForce.PutInQueue,
                            Volume = 1000
                        });
                    }

                 

                    if (x.Code.Contains("USDRUB"))
                    {
                        var security = x;
                        connector.SubscribeMarketData(new MarketDataSubscriptionMessage
                        {
                            MarketDataType = MarketDataType.Trades,
                            Security = security
                        });
                    }
                });

            connector.RequestSecurities();

            System.Console.ReadLine();


            //connector.SubscribeTrades(new Security {Code = "USD000UTSTOM"});
            

           
            

            connector.SubscribeMarketData(new MarketDataSubscriptionMessage
            {
                Security = new Security("USD/RUB_B_T1", "QME_FXBCS"),
                MarketDataType = MarketDataType.MarketDepth
            });

            connector.SubscribeMarketData(new MarketDataSubscriptionMessage
            {
                Security = new Security("USD/RUB_B_T1", "QME_FXBCS"),
                MarketDataType = MarketDataType.MarketDepth
            });


            connector.RegisterOrder(new Order
            {
                
            });


            /*
            var strategy = new Algo.Strategies.Fixing.StrategyController(connector);
            var p = new StrategyParameters
            {
                Security = new Security {Code = "USD000UTSTOM"},
                Portfolio = new Portfolio {ClientCode = "30_10008", Name = "MB1002600091"},
                Direction = Direction.Buy,
                MinRate = 0,
                MaxRate = 100,
                ThinkInterval = TimeSpan.FromSeconds(5),
                OrderSize = 1000,
                Volume = 5000
            };

            strategy.Run(p); 
            */

            /*
            connector.MarketDepthChanged.Subscribe(x =>
            {
                System.Console.WriteLine(x);
            });


            connector.SubscribeMarketData(new Security
            {
                Code = "USD000UTSTOM"
            }); 

                
            connector.RegisterOrder(new Order
            {
                OrderType = OrderType.Limit,
                Portfolio = new Portfolio
                {
                    ClientCode = "30_10008",
                    Name = "MB1002600091"
                },
                Price = 76,
                Security = new Security
                {
                    Code = "USD000UTSTOM"
                },
                Direction = Direction.Buy,
                TimeInForce = TimeInForce.PutInQueue,
                Volume = 1000
            }); 
            */


            System.Console.ReadLine();

            
            
        }
 
    }
}
