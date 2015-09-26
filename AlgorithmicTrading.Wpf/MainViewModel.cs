using System;
using System.Linq;
using System.Reactive.Linq;
using ReactiveUI;
using System.Collections.ObjectModel;
using Abt.Controls.SciChart;
using Abt.Controls.SciChart.Model.DataSeries;
using Abt.Controls.SciChart.Visuals.RenderableSeries;
using System.Collections.Generic;

namespace AlgorithmicTrading.Wpf
{
    public class MainViewModel : ReactiveObject
    {
        public ObservableCollection<IChartSeriesViewModel> ChartSeries { get; set; } = new ObservableCollection<IChartSeriesViewModel>();

        string symbolTextBox;
        public string SymbolTextBox
        {
            get { return symbolTextBox; }
            set { this.RaiseAndSetIfChanged(ref symbolTextBox, value); }
        }

        IndexRange xVisibleRange;
        public IndexRange XVisibleRange
        {
            get { return xVisibleRange; }
            set
            {
                if (Equals(xVisibleRange, value)) return;
                this.RaiseAndSetIfChanged(ref xVisibleRange, value);
            }
        }

        DoubleRange yVisibleRange;
        public DoubleRange YVisibleRange
        {
            get { return yVisibleRange; }
            set
            {
                if (Equals(yVisibleRange, value)) return;
                this.RaiseAndSetIfChanged(ref yVisibleRange, value);
            }
        }

        public MainViewModel()
        {
            var quotes = from symbol in this.WhenAnyValue(x => x.SymbolTextBox).Throttle(TimeSpan.FromSeconds(1))
                         where !string.IsNullOrEmpty(symbol)
                         from quote in YahooDataProvider.Historic(symbol: symbol, start: DateTime.Now.AddDays(-30), end: DateTime.Now, period: YahooDataProvider.Period.Daily)  // TODO: should we do a join instead?
                         where quote != null
                         select quote;

            var seriesDic = new Dictionary<string, XyDataSeries<DateTime, double>>();

            quotes.SubscribeOn(RxApp.TaskpoolScheduler)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Do(Console.WriteLine)
                .Subscribe(quote =>
                {
                    var series = AppendToSeries(quote, seriesDic);
                    SetVisibleRanges(quote, series);
                });
        }

        void SetVisibleRanges(YahooDataProvider.HistoricalQuote quote, XyDataSeries<DateTime, double> series)
        {
            if (XVisibleRange != null && series.Count > XVisibleRange.Max)
            {
                XVisibleRange = new IndexRange(XVisibleRange.Min + 1, XVisibleRange.Max + 1);
            }

            if (YVisibleRange != null && quote.Price > YVisibleRange.Max)
            {
                YVisibleRange = new DoubleRange(YVisibleRange.Min, quote.Price + 10);
            }

            if (YVisibleRange != null && quote.Price < YVisibleRange.Min)
            {
                YVisibleRange = new DoubleRange(quote.Price - 10, YVisibleRange.Max);
            }
        }

        XyDataSeries<DateTime, double> AppendToSeries(YahooDataProvider.HistoricalQuote quote, Dictionary<string, XyDataSeries<DateTime, double>> seriesDic)
        {
            XyDataSeries<DateTime, double> series;

            if (seriesDic.ContainsKey(quote.Symbol))
            {
                series = seriesDic[quote.Symbol];
            }
            else
            {
                series = new XyDataSeries<DateTime, double>();
                series.SeriesName = quote.Symbol;
                ChartSeries.Add(new ChartSeriesViewModel(series, new FastLineRenderableSeries()));
                seriesDic[quote.Symbol] = series;
            }

            series.Append(quote.Date, quote.Price);
            return series;
        }
    }
}
