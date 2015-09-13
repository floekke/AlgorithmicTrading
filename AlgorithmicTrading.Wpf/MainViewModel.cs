using System;
using System.Linq;
using System.Reactive.Linq;
using ReactiveUI;
using System.Collections.ObjectModel;
using Abt.Controls.SciChart;
using Abt.Controls.SciChart.Model.DataSeries;
using Abt.Controls.SciChart.Visuals.RenderableSeries;
using System.Windows.Media;

namespace AlgorithmicTrading.Wpf
{
    public class MainViewModel : ReactiveObject
    {
        string symbolTextBox;
        public string SymbolTextBox
        {
            get { return symbolTextBox; }
            set { this.RaiseAndSetIfChanged(ref symbolTextBox, value); }
        }

        readonly ObservableAsPropertyHelper<IObservable<YahooDataProvider.Quote>> ticks;
        public IObservable<YahooDataProvider.Quote> Ticks
        {
            get { return ticks.Value; }
        }

        public MainViewModel()
        {
            this.WhenAnyValue(x => x.SymbolTextBox)
                .Throttle(TimeSpan.FromSeconds(1))
                .Where(x => string.IsNullOrEmpty(x) == false)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x =>
                {
                    var newSeries = new XyDataSeries<DateTime, double> { SeriesName = x };
                    ChartSeries.Add(new ChartSeriesViewModel(newSeries, new FastLineRenderableSeries { SeriesColor = Color.FromRgb(11, 29, 63) }));

                    YahooDataProvider.LiveFeed(x)
                    .SubscribeOn(RxApp.TaskpoolScheduler)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(q =>
                    {
                        newSeries.Append(DateTime.Now, q.Ask);
                    });
                });

        }

        public ObservableCollection<IChartSeriesViewModel> ChartSeries { get; set; } = new ObservableCollection<IChartSeriesViewModel>();

        // Sci chart uses this!
        public int FontSize { get; set; } = 14;
    }
}
