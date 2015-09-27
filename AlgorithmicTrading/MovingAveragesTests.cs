using NUnit.Framework;
using System.Reactive.Linq;
using static AlgorithmicTrading.MovingAverages;

namespace AlgorithmicTrading
{
    [TestFixture]
    public class MovingAveragesTests
    {
        [Test]
        public void ShouldBeLastNumber()
        {
            AssertLastAvg(days: 1, prices: new[] { 1F, 2F }, lastAvg: 2F / 1);
        }

        [Test]
        public void ShouldBeAverageOfTwoLastNumbers()
        {
            AssertLastAvg(days: 2, prices: new[] { 1F, 2F, 3F }, lastAvg: (2F + 3F) / 2);
        }

        [Test]
        public void ShouldBeAverageOfThreeLastNumbers()
        {
            AssertLastAvg(days: 3, prices: new[] { 1F, 2F, 3F, 6F, 1F, 10F,  }, lastAvg: (6F + 1F + 10F) / 3);
        }

        void AssertLastAvg(int days, float[] prices, float lastAvg)
        {
            Assert.AreEqual(lastAvg, MovingAverage(days, prices.ToObservable()).Wait());
        }
    }
}
