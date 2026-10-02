namespace AnonymizeDicom.Anonymization;

public class DateOffsetGenerator
{
    private readonly int _minDays;
    private readonly int _maxDays;

    public DateOffsetGenerator()
        : this(1, 36500)
    {
    }

    public DateOffsetGenerator(int minDays, int maxDays)
    {
        if (minDays < 0 || maxDays < minDays)
        {
            throw new ArgumentOutOfRangeException(nameof(maxDays), "offset range is invalid");
        }

        _minDays = minDays;
        _maxDays = maxDays;
    }

     public int NextOffset() => Random.Shared.Next(_minDays, _maxDays + 1);
}
