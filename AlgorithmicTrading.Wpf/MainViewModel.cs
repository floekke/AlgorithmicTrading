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

        DateTime start;
        public DateTime Start
        {
            get { return start; }
            set
            {
                if (Equals(start, value)) return;
                this.RaiseAndSetIfChanged(ref start, value);
            }
        }

        DateTime end;
        public DateTime End
        {
            get { return end; }
            set
            {
                if (Equals(end, value)) return;
                this.RaiseAndSetIfChanged(ref end, value);
            }
        }

        public MainViewModel()
        {
            Start = DateTime.Now.AddYears(-1); 
            End = DateTime.Now;

            var input = (from symbol in this.WhenAnyValue(x => x.SymbolTextBox).Throttle(TimeSpan.FromSeconds(1))
                         where !string.IsNullOrEmpty(symbol)
                         from start in this.WhenAnyValue(x => x.Start)
                         from end in this.WhenAnyValue(x => x.End)
                             // TODO: produce smaller intervals from long interval! Datetime vs. timespan
                         select new { Symbol = symbol, Start = start, End = end });

            input
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => 
                ChartSeries.Clear());

            // Maybe use combine latest!
            var quotes = from i in input
                         from quote in YahooDataProvider.Historic(symbol: i.Symbol, start: i.Start, end: i.End, period: YahooDataProvider.Period.Daily).SubscribeOn(RxApp.TaskpoolScheduler) // 
                         where quote != null
                         select quote;

            var seriesDic = new Dictionary<string, XyDataSeries<DateTime, double>>();

            quotes
                .SubscribeOn(RxApp.TaskpoolScheduler)
                .Do(Console.WriteLine)
                .ObserveOn(RxApp.MainThreadScheduler)
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
