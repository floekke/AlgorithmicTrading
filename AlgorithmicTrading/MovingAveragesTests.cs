using Microsoft.FSharp.Core;
using NUnit.Framework;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace AlgorithmicTrading
{
    [TestFixture]
    public class MovingAveragesTests
    {
        [Test]
        public void ShouldNotProduceAverage()
        {
            var prices = new[] { 1F }.ToObservable();
            var result = prices.MovingAverage(10);
            Assert.AreEqual(0, result.ToEnumerable().Count());
        }

        [Test]
        public void ShouldProduceOneAverage()
        {
            var prices = new[] { 1F, 2F, 3F }.ToObservable();
            var result = prices.MovingAverage(3);
            Assert.AreEqual(1, result.ToEnumerable().Count());
        }

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
            AssertLastAvg(days: 3, prices: new[] { 1F, 2F, 3F, 6F, 1F, 10F, }, lastAvg: (6F + 1F + 10F) / 3);
        }

        void AssertLastAvg(int days, float[] prices, float lastAvg)
        {
            Assert.AreEqual(lastAvg, prices.ToObservable().MovingAverage(days).Wait());
        }
    }
}
