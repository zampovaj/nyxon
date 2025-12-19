namespace Nyxon.Core.Tests.Crypto
{
    public class RandomServiceTests
    {
        private readonly IRandomService _randomService = new RandomService();

        [Fact]
        public void GenerateRandomBytes_ReturnsCorrectLength()
        {
            var bytes = _randomService.GenerateRandomBytes(32);

            Assert.NotNull(bytes);
            Assert.Equal(32, bytes.Length);
        }

        [Fact]
        public void GenerateRandomBytes_ProducesDifferentValues()
        {
            var a = _randomService.GenerateRandomBytes(32);
            var b = _randomService.GenerateRandomBytes(32);

            Assert.NotEqual(a, b);
        }

        [Fact]
        public void GenerateRandomBytes_ThrowsOnInvalidLength()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                _randomService.GenerateRandomBytes(0));
        }
    }
}
