using System.Collections.Concurrent;

namespace Service.Services.FilterService;

public class FilterDictionary
{
    private static readonly ConcurrentDictionary<string, DateTime> LastReadTimes = new();

    private readonly int _filterTimeInSeconds;

    public FilterDictionary(int filterTimeInSeconds)
    {
        _filterTimeInSeconds = filterTimeInSeconds;
    }

    public bool ShouldReportTag(string epc)
    {
        var now = DateTime.UtcNow;

        if (LastReadTimes.TryGetValue(epc, out var lastReadTime))
        {
            if ((now - lastReadTime).TotalSeconds < _filterTimeInSeconds)
            {
                return false;
            }
        }

        LastReadTimes[epc] = now;
        return true;
    }
}