using System;
using System.Reactive;
using System.Reactive.Linq;
using OxyPlot;
using OxyPlot.Series;
using ReactiveUI;

namespace AlgorithmicTrading.Wpf
{
    public class MainViewModel : ReactiveObject
    {
        public ReactiveCommand<YahooDataProvider.Quote> SubscribeToSymbolCommmand { get; private set; }

        public PlotModel Plot { get; private set; }

        string symbolTextBox;
        public string SymbolTextBox
        {
            get { return symbolTextBox; }
            set { this.RaiseAndSetIfChanged(ref symbolTextBox, value); }
        }


        public MainViewModel()
        {
            ExamplePlot();

            var canSubscribeToSymbol = this.WhenAny(vm => vm.SymbolTextBox,
                   s => 
                   !string.IsNullOrWhiteSpace(s.Value));

            SubscribeToSymbolCommmand =
            ReactiveCommand.CreateAsyncObservable(canSubscribeToSymbol, x => YahooDataProvider.LiveFeed((string)x));

            SubscribeToSymbolCommmand
                .Subscribe(x =>
                {
                    Console.WriteLine(x);
                });
        }

        private void ExamplePlot()
        {
            Plot = new PlotModel { Title = "Example 1" };
            Plot.Series.Add(new FunctionSeries(Math.Cos, 0, 10, 0.1, "cos(x)"));
        }



    }
}
