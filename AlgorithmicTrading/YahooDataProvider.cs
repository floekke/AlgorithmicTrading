using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using YSQ.core.Historical;
using YSQ.core.Quotes;

namespace AlgorithmicTrading
{
    public class YahooDataProvider
    {
        public static IObservable<Quote> LiveFeed(params string[] symbols) =>
            new YahooDataProvider().NewLiveFeed(symbols);

        IObservable<Quote> NewLiveFeed(params string[] symbols)
        {
            if(symbols.Any(x => string.IsNullOrWhiteSpace(x))) throw new ArgumentNullException(nameof(symbols));

            return Observable
                .Interval(TimeSpan.FromSeconds(1))
                .Select(x => new QuoteService().Quote(symbols))
                .Select(x => ParametersToReturn(x))
                .SelectMany(x => x)
                .Select<dynamic, Quote>(x => ParseQuote(x));
        }

        Quote ParseQuote(dynamic x) => new Quote
        {
            Symbol = ((string)x.Symbol).TrimEnd('"').TrimStart('"'),
            Volume = long.Parse(x.Volume),
            Ask = float.Parse(x.Ask),
            Bid = float.Parse(x.Bid),
            LatestTradePrice = float.Parse(x.LatestTradePrice)
        };

        IEnumerable<dynamic> ParametersToReturn(IFindQuotes x) => x.Return(
            QuoteReturnParameter.Symbol,
            QuoteReturnParameter.Ask,
            QuoteReturnParameter.Bid,
            QuoteReturnParameter.Volume,
            QuoteReturnParameter.LatestTradePrice,
            QuoteReturnParameter.LatestTradeTime);

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
    }
}
