using NUnit.Framework;
using System.Reactive.Linq;

namespace AlgorithmicTrading
{
    [TestFixture]
    public class MovingAveragesTests
    {
        [Test]
        public void ShouldFoo()
        {
            var prices = new[] { 1F, 2F }.ToObservable();
            var result = MovingAverages.MovingAverage(1, prices).Wait();
            Assert.AreEqual((1F + 2F) / 2, result);
        }
    }
}
