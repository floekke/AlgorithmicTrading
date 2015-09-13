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
            var quotes = from symbol in this.WhenAnyValue(x => x.SymbolTextBox)
                         where !string.IsNullOrEmpty(symbol)
                         from quote in YahooDataProvider.LiveFeed(symbol)  // TODO: should we do a join instead?
                         where quote != null
                         select quote;

            var newSeries = new XyDataSeries<DateTime, double> { SeriesName = "Data" };
            ChartSeries.Add(new ChartSeriesViewModel(newSeries, new FastLineRenderableSeries { SeriesColor = Color.FromRgb(11, 29, 63) }));

            quotes.SubscribeOn(RxApp.TaskpoolScheduler)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(quote => newSeries.Append(DateTime.Now, quote.Ask));
        }

        public ObservableCollection<IChartSeriesViewModel> ChartSeries { get; set; } = new ObservableCollection<IChartSeriesViewModel>();

        // Sci chart uses this!
        public int FontSize { get; set; } = 14;
    }
}
