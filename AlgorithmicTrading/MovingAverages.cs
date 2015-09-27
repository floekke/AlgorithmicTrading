using System;
using System.Reactive.Linq;
using System.Linq;

namespace AlgorithmicTrading
{
    public static class MovingAverages
    {
        // TODO: better name for days?
        public static IObservable<float> MovingAverage(int days, IObservable<float> prices)
        {
            return prices
                .Buffer(days, 1)
                .Where(x => x.Count == days)
                .Select(x => x.Average());
        }
    }
}
