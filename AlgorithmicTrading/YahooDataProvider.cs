using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using YSQ.core.Quotes;

namespace AlgorithmicTrading
{
    static class YahooDataProvider
    {
        public static IObservable<Quote> LiveFeed(params string[] symbols)
        {
            var quoteService = new QuoteService();

            return Observable
                   .Interval(TimeSpan.FromSeconds(1))
                   .Select(x => quoteService.Quote(symbols))
                   .Select(x => x.Return(QuoteReturnParameter.Symbol,
                                         QuoteReturnParameter.Name,
                                         QuoteReturnParameter.Volume,
                                         QuoteReturnParameter.LatestTradePrice,
                                         QuoteReturnParameter.LatestTradeTime))
                   .SelectMany(x => x)
                   .Select(x => new Quote
                   {
                       Symbol = x.Symbol,
                       Volume = long.Parse(x.Volume),
                       Ask = float.Parse(x.Ask),
                       Bid = float.Parse(x.Bid),
                       LatestTradePrice = float.Parse(x.LatestTradePrice)
                   });
        }

        //public static IObservable<List<Quote>> LiveFeed(params string[] symbols)
        //{
        //    return Observable
        //        .Interval(TimeSpan.FromSeconds(1))
        //        .Select(x => TryQueryYahoo(symbols))
        //        .Publish()
        //        .RefCount();
        //}

        public static List<Quote> TryQueryYahoo(params string[] symbols)
        {
            var client = new RestClient("http://query.yahooapis.com");
            var request = QueryRequest(symbols);
            var result = client.Execute(request);
            return Deserialize(result);
        }

        static List<Quote> Deserialize(IRestResponse result)
        {
            var yahooResponse = JsonConvert.DeserializeObject<YahooResponse>(result.Content);
            return yahooResponse.Query.Results.Quote;
        }

        static RestRequest QueryRequest(string[] symbols)
        {
            var request = new RestRequest("v1/public/yql", Method.GET);
            string ss = EscapeSymbols(symbols);
            request.AddQueryParameter("q", $"select * from yahoo.finance.quotes where symbol in ({ss})");
            request.AddQueryParameter("format", "json");
            request.AddQueryParameter("env", "http://datatables.org/alltables.env");
            return request;
        }

        static string EscapeSymbols(string[] symbols)
        {
            return symbols.Aggregate(string.Empty, (acc, s) => acc + $", \"{s}\"").TrimStart(',');
        }

        public class Quote
        {
            public string Symbol;
            public float Ask;
            public float Bid;
            public float Open;
            public long Volume;
            public DateTimeOffset LatestTradeTime;
            public float LatestTradePrice;

            public override string ToString()
            {
                return $"Symbol {Symbol} Ask {Ask} Bid {Bid} Open {Open} Volume {Volume} LatestTradeTime {LatestTradeTime} LatestTradePrice {LatestTradePrice}";
            }
        }
        class YahooResponse { public Query Query; }
        class Results { public List<Quote> Quote; }
        class Query { public int Count; public Results Results; }
    }
}
