using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using YSQ.core.Historical;
using YSQ.core.Quotes;

namespace AlgorithmicTrading
{
    public class YahooDataProvider
    {
        public static IObservable<LiveQuote> Live(params string[] symbols) =>
            new YahooDataProvider().NewLive(symbols);

        public static IObservable<HistoricalQuote> Historic(string symbol, DateTime start, DateTime end, Period period) =>
            new YahooDataProvider().NewHistoric(symbol, start, end, period);

        IObservable<HistoricalQuote> NewHistoric(string symbol, DateTime start, DateTime end, Period period)
        {
            return Observable.Create<HistoricalQuote>(observer =>
            {
                new HistoricalPriceService()
                .Get(symbol, start, end, (YSQ.core.Historical.Period)period)
                .Reverse()
                .Select(x => new HistoricalQuote { Date = x.Date, Price = (float)x.Price, Symbol = symbol })
                .ToList()
                .ForEach(x => observer.OnNext(x));
                return Disposable.Empty;
            })
            .Catch((WebException e) =>  Observable.Empty<HistoricalQuote>())
            .Publish()
            .RefCount();
        }

        IObservable<LiveQuote> NewLive(params string[] symbols)
        {
            if (symbols.Any(x => string.IsNullOrWhiteSpace(x))) throw new ArgumentNullException(nameof(symbols));

            return Observable
                .Interval(TimeSpan.FromSeconds(1))
                .Select(x => new QuoteService().Quote(symbols))
                .Select(x => ParametersToReturn(x))
                .SelectMany(x => x)
                .Select<dynamic, LiveQuote>(x => TryParseQuote(x))
                .Where(x => x != null)
                .Retry();
        }

        LiveQuote TryParseQuote(dynamic x)
        {
            try
            {
                return new LiveQuote
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

        public abstract class Quote
        {
            public abstract float Price
            {
                get; set;
            }
        }

        public class HistoricalQuote : Quote
        {
            public string Symbol;
            public DateTime Date;

            public override float Price
            {
                get; set;
            }
                
            public override string ToString() => $"Symbol {Symbol} Price {Price} Date {Date}";
        }

        public class LiveQuote : Quote
        {
            public string Symbol;
            public float Ask;
            public float Bid;
            public float Open;
            public long Volume;
            public DateTimeOffset LatestTradeTime;
            public float LatestTradePrice;

            public override float Price
            {
                get
                {
                    throw new NotImplementedException();
                }

                set
                {
                    throw new NotImplementedException();
                }
            }

            public override string ToString() =>
                $"Symbol {Symbol} Ask {Ask} Bid {Bid} Open {Open} Volume {Volume} LatestTradeTime {LatestTradeTime} LatestTradePrice {LatestTradePrice}";
        }
    }
}
