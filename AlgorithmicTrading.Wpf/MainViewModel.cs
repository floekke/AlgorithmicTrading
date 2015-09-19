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
            //XVisibleRange = new IndexRange();

            var quotes = from symbol in this.WhenAnyValue(x => x.SymbolTextBox).Throttle(TimeSpan.FromSeconds(3))
                         where !string.IsNullOrEmpty(symbol)
                         from quote in YahooDataProvider.LiveFeed(symbol)  // TODO: should we do a join instead?
                         where quote != null
                         select quote;

            

            var series = new XyDataSeries<DateTime, double>();
            ChartSeries.Add(new ChartSeriesViewModel(series, new FastLineRenderableSeries()));

            quotes.SubscribeOn(RxApp.TaskpoolScheduler)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Do(Console.WriteLine)
                .Subscribe(quote =>
                {

                    if (XVisibleRange != null && XVisibleRange.Max > series.Count)
                    {
                        var existingRange = xVisibleRange;
                        var newRange = new IndexRange(existingRange.Min + 1, existingRange.Max + 1);
                        XVisibleRange = newRange;
                    }

                    series.Append(DateTime.Now, quote.Ask + random.Next(1, 5));
                });

        }

        public ObservableCollection<IChartSeriesViewModel> ChartSeries { get; set; } = new ObservableCollection<IChartSeriesViewModel>();

    }
}
