using aoc_2025_p2;

namespace aoc_2025_p2_test
{
    public class ProgramTest
    {
        [Fact]
        public void InvalidSamples()
        {
            Assert.True(Program.IsInvalidId(11));
            Assert.True(Program.IsInvalidId(22));
            Assert.True(Program.IsInvalidId(99));
            Assert.True(Program.IsInvalidId(1010));
            Assert.True(Program.IsInvalidId(1188511885));
            Assert.True(Program.IsInvalidId(222222));
            Assert.True(Program.IsInvalidId(446446));
            Assert.True(Program.IsInvalidId(38593859));
        }

        [Fact]
        public void ValidSamples()
        {
            Assert.False(Program.IsInvalidId(10));
            Assert.False(Program.IsInvalidId(21));
            Assert.False(Program.IsInvalidId(95));
            Assert.False(Program.IsInvalidId(100));
            Assert.False(Program.IsInvalidId(1000));
            Assert.False(Program.IsInvalidId(1012));
            Assert.False(Program.IsInvalidId(222220));
            Assert.False(Program.IsInvalidId(1698522));
            Assert.False(Program.IsInvalidId(38593856));
            Assert.False(Program.IsInvalidId(2121212124));
        }
    }
}
