using System.Collections.Concurrent;

namespace Service.Services.FilterService;

public class FilterDictionary
{
    private static readonly ConcurrentDictionary<string, DateTime> LastReadTimes = new();

    private static readonly int FilterTimeInSeconds = 5;

    public bool ShouldReportTag(string epc)
    {
        var now = DateTime.UtcNow;

        if (LastReadTimes.TryGetValue(epc, out var lastReadTime))
        {
            if ((now - lastReadTime).TotalSeconds < FilterTimeInSeconds)
            {
                return false;
            }
        }

        LastReadTimes[epc] = now;
        return true;
    }
}