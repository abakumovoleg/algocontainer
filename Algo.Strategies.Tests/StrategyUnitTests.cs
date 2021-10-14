using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using Algo.Abstracts.Models;
using Algo.BackTesting;
using Algo.Strategies.Execution; 
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;

namespace Algo.Strategies.Tests
{
    [TestClass]
    public class StrategiesUnitTests
    {
        private readonly Portfolio _portfolio = new Portfolio();
        private readonly Security _security = new Security("class", "code");

        private readonly Security _securityLotSize1000 = new Security("class", "code")
        {
            LotSize = 1000
        };

        public static IEnumerable<object[]> GetData()
        {
            var hard = Directory.GetDirectories("TestCases\\HS").Select(x => new object[] {x, "hard"}).ToArray();
            var soft = Directory.GetDirectories("TestCases\\SS").Select(x => new object[] { x, "soft" }).ToArray();
            var gentle = Directory.GetDirectories("TestCases\\GS").Select(x => new object[] { x, "gentle" }).ToArray();

            return hard.Concat(soft).Concat(gentle).ToArray();
        }

        //[TestMethod]
        public async Task TestOne()
        {
            await Test("TestCases\\GS\\TimeInForceExpired", "gentle");
        }

        [TestMethod]
        [DynamicData(nameof(GetData), DynamicDataSourceType.Method)]
        public async Task Test(string testName, string strategyName)
        {
            var mdFilePath = Path.Combine(testName, "md.json");
            var ordersFilePath = Path.Combine(testName, "expected.json");
            var paramsFilePath = Path.Combine(testName, "params.json");

            var pContent = File.ReadAllText(paramsFilePath);
            

            var mdp = new SimpleTestDataProvider(new FileReader());

            var connector = new TradingSystemEmulator(mdp, mdp);

            var context = new StrategyContext(connector, mdp, new EventLoopScheduler(x=>
            {
                Debug.WriteLine("Creating Thread");

                var t = new Thread(x) {Name = "Test"};
                return t;
            }), NullLogger.Instance);

            IStrategy strategy = null;
            if (strategyName == "hard")
            {
                var p = Newtonsoft.Json.JsonConvert.DeserializeObject<HardStrategyParameters>(pContent);
                strategy = new Execution.HardExecution.Strategy(context,
                    new Execution.HardExecution.StrategyParameters(_security, _portfolio, p.Direction, p.Volume, p.PriceStep, p.MaxOrderCount,
                        "bot"));
            }
            else if(strategyName == "soft")
            {
                var p = Newtonsoft.Json.JsonConvert.DeserializeObject<SoftStrategyParameters>(pContent);
                strategy = new Execution.SoftExecution.Strategy(context,
                    new Execution.SoftExecution.StrategyParameters(_security, _portfolio, TimeSpan.FromSeconds(p.TimeInForce),  p.Direction, p.Volume,  "bot"));
            }
            else if (strategyName == "gentle")
            {
                var p = Newtonsoft.Json.JsonConvert.DeserializeObject<GentleStrategyParameters>(pContent);
                strategy = new Execution.GentleExecution.Strategy(context,
                    new Execution.GentleExecution.StrategyParameters(_security, _portfolio,
                        TimeSpan.FromSeconds(p.TimeInForce), TimeSpan.FromSeconds(p.WaitTime), p.Direction, p.Volume,
                        "bot"));
            }
            else
                throw new Exception();

            connector.Connect();

            var cts = new CancellationTokenSource(5000);

            var taskM = mdp.Start(mdFilePath, _security);

            await Task.Delay(500);

            var taskS = strategy.Start(cts.Token);

            await Task.WhenAll(taskM, taskS);

            var content = File.ReadAllText(ordersFilePath);

            var expected = Newtonsoft.Json.JsonConvert.DeserializeObject<Expected>(content);

            var orders = strategy.Orders.OrderBy(x => x.Time).ToArray();

            Debug.WriteLine(strategy.Log);

            Assert.AreEqual(expected.Orders.Length, orders.Length);
            Assert.AreEqual(expected.State, strategy.State);

            for (var i = 0; i < orders.Length; i++)
            {
                var o = orders[i];
                var e = expected.Orders[i];

                Assert.AreEqual(e.Volume,  o.Volume);
                Assert.AreEqual(e.Direction, o.Direction);
                Assert.AreEqual(e.Price, o.Price);
            }
        }
    }
}
