using System;
using System.Linq;
using System.Reactive.Linq;
using ReactiveUI;
using System.Collections.ObjectModel;
using Abt.Controls.SciChart;
using Abt.Controls.SciChart.Model.DataSeries;
using Abt.Controls.SciChart.Visuals.RenderableSeries;
using System.Windows.Media;
using Abt.Controls.SciChart.Visuals.Axes;
using System.Collections.Generic;

namespace AlgorithmicTrading.Wpf
{
    // TODO: remove scíchart license from file

    public class MainViewModel : ReactiveObject
    {
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

        Random random = new Random();

        public MainViewModel()
        {
            XVisibleRange = new IndexRange(0, 100);

            var quotes = from symbol in this.WhenAnyValue(x => x.SymbolTextBox).Throttle(TimeSpan.FromSeconds(1))
                         where !string.IsNullOrEmpty(symbol)
                         from quote in YahooDataProvider.LiveFeed(symbol)  // TODO: should we do a join instead?
                         where quote != null
                         select new { Quote = quote, Symbol = symbol };

            var seriesDic = new Dictionary<string, XyDataSeries<DateTime, double>>();

            quotes.SubscribeOn(RxApp.TaskpoolScheduler)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Do(Console.WriteLine)
                .Subscribe(quoteAndSymbol =>
                {
                    XyDataSeries<DateTime, double> series;

                    if (seriesDic.ContainsKey(quoteAndSymbol.Symbol))
                    {
                        series = seriesDic[quoteAndSymbol.Symbol];
                    }
                    else
                    {
                        series = new XyDataSeries<DateTime, double>();
                        series.SeriesName = quoteAndSymbol.Symbol;
                        ChartSeries.Add(new ChartSeriesViewModel(series, new FastLineRenderableSeries()));
                        seriesDic[quoteAndSymbol.Symbol] = series;
                    }

                    series.Append(DateTime.Now, quoteAndSymbol.Quote.Ask);

                    if (XVisibleRange != null && series.Count > XVisibleRange.Max)
                    {
                        var existingRange = xVisibleRange;
                        var newRange = new IndexRange(existingRange.Min + 1, existingRange.Max + 1);
                        XVisibleRange = newRange;
                    }

                });

        }

        public ObservableCollection<IChartSeriesViewModel> ChartSeries { get; set; } = new ObservableCollection<IChartSeriesViewModel>();

    }
}
