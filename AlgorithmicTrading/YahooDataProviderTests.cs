using NUnit.Framework;
using RestSharp;
using System;
using System.Linq;
using System.Reactive.Linq;

namespace AlgorithmicTrading
{
    [TestFixture]
    public class YahooDataProviderTests
    {
        [Test]
        public void ShouldGetQuotesFromYahoo()
        {
            var result = YahooDataProvider.TryQueryYahoo("aapl", "MSFT ");

            Assert.AreEqual("aapl", result[0].Symbol);
            Assert.AreEqual("MSFT", result[1].Symbol);
        }

        [Test]
        public async void ShouldGetLiveFeedFromYahoo()
        {
            var result = await YahooDataProvider
                .LiveFeed("aapl", "MSFT")
                .Do(Console.WriteLine)
                .Take(30);

            //Assert.AreEqual("aapl", result[0].Symbol);
            //Assert.AreEqual("MSFT", result[1].Symbol);
        }

        [Test]
        public void Should()
        {
            var client = new RestClient("http://finance.yahoo.com");
            var request = new RestRequest("d/quotes.csv");// ?s=bac+bp+c+msft+aapl&f=sl1
            request.AddQueryParameter("s", "bac+bp+c+msft+aapl");
            request.AddQueryParameter("f", "sl1");
            var result = client.Execute(request);
        }
    }

    
}
