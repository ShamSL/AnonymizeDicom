using AnonymizeDicom.Anonymization;

namespace AnonymizeDicom.Tests;

public class DateOffsetGeneratorTests
{
    [Fact]
    public void NextOffset_IsWithinInclusiveRange()
    {
        var generator = new DateOffsetGenerator(10, 20);

        for (var i = 0; i < 10_000; i++)
        {
            var offset = generator.NextOffset();
            Assert.InRange(offset, 10, 20);
        }
    }

    [Fact]
    public void NextOffset_IsThreadSafe()
    {
        var generator = new DateOffsetGenerator(1, 5000);

        var offsets = Enumerable.Range(0, 10_000)
            .AsParallel()
            .Select(_ => generator.NextOffset())
            .ToList();

        Assert.All(offsets, offset => Assert.InRange(offset, 1, 5000));
    }
}
