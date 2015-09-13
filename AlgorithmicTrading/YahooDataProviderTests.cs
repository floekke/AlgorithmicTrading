using NUnit.Framework;
using System;
using System.Linq;
using System.Reactive.Linq;

namespace AlgorithmicTrading
{
    [TestFixture]
    public class YahooDataProviderTests
    {
        [Test]
        public async void ShouldGetLiveFeedFromYahoo()
        {
            var result = await YahooDataProvider
                .LiveFeed("aapl", "MSFT")
                .Do(Console.WriteLine)
                .Take(1);

            Assert.AreEqual("aapl", result.Symbol);
        }
    }
}
