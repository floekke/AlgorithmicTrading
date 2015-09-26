using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using YSQ.core.Historical;
using YSQ.core.Quotes;

namespace AlgorithmicTrading
{
    public class YahooDataProvider
    {
        public static IObservable<Quote> Live(params string[] symbols) =>
            new YahooDataProvider().NewLive(symbols);

        public static IObservable<Historical> Historic(string symbol, DateTime start, DateTime end, Period period) =>
            new YahooDataProvider().NewHistoric(symbol, start, end, period);

        IObservable<Historical> NewHistoric(string symbol, DateTime start, DateTime end, Period period)
        {
            return Observable.Create<Historical>(observer =>
            {
                // TODO: webexception

                new HistoricalPriceService()
                .Get(symbol, start, end, (YSQ.core.Historical.Period)period)
                .Reverse()
                .Select(x => new Historical { Date = x.Date, Price = (double)x.Price, Symbol = symbol })
                .ToList()
                .ForEach(x => observer.OnNext(x));

                return Disposable.Empty;
            })
            .Publish()
            .RefCount();
        }

        IObservable<Quote> NewLive(params string[] symbols)
        {
            if (symbols.Any(x => string.IsNullOrWhiteSpace(x))) throw new ArgumentNullException(nameof(symbols));

            return Observable
                .Interval(TimeSpan.FromSeconds(1))
                .Select(x => new QuoteService().Quote(symbols))
                .Select(x => ParametersToReturn(x))
                .SelectMany(x => x)
                .Select<dynamic, Quote>(x => TryParseQuote(x))
                .Where(x => x != null)
                .Retry();
        }

        Quote TryParseQuote(dynamic x)
        {
            try
            {
                return new Quote
                {
                    Symbol = ((string)x.Symbol).TrimEnd('"').TrimStart('"'),
                    Volume = long.Parse(x.Volume),
                    Ask = float.Parse(x.Ask),
                    Bid = float.Parse(x.Bid),
                    LatestTradePrice = float.Parse(x.LatestTradePrice)
                };
            }
            catch (FormatException) { }

            return null;
        }

        IEnumerable<dynamic> ParametersToReturn(IFindQuotes x) => x.Return(
            QuoteReturnParameter.Symbol,
            QuoteReturnParameter.Ask,
            QuoteReturnParameter.Bid,
            QuoteReturnParameter.Volume,
            QuoteReturnParameter.LatestTradePrice,
            QuoteReturnParameter.LatestTradeTime);

        public enum Period
        {
            Daily = 0,
            Weekly = 1,
            Monthly = 2
        }


        public class Historical
        {
            public string Symbol;
            public double Price;
            public DateTime Date;

            public override string ToString() => $"Symbol {Symbol} Price {Price} Date {Date}";
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

            public override string ToString() =>
                $"Symbol {Symbol} Ask {Ask} Bid {Bid} Open {Open} Volume {Volume} LatestTradeTime {LatestTradeTime} LatestTradePrice {LatestTradePrice}";
        }
    }
}
