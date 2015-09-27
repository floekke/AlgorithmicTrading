using System.Windows;
using ReactiveUI;
using System.Reactive;
using System;
using System.Reactive.Linq;

namespace AlgorithmicTrading.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window // TODO: Maybe use IViewFor ?
    {
        public MainWindow()
        {
            InitializeComponent();


            // Use command or event instead?
            //this.WhenAnyValue(x => x.ViewModel.NewChart)
            //    .Where(x => x == true)
            //    .Subscribe(_ => NewChart());
        }

        //void NewChart()
        //{
        //    // Re-order dataseries indices after remove
        //    //for (int i = 0; i < PriceChart.RenderableSeries.Count; i++)
        //    //{
        //    //    PriceChart.RenderableSeries[i].DataSeries = i;
        //    //}

        //    PriceChart.RenderableSeries.Clear();

        //    PriceChart.InvalidateElement();
        //}

        MainViewModel ViewModel
        {
            get
            {
                return (MainViewModel)DataContext;
            }
        }
    }
}
