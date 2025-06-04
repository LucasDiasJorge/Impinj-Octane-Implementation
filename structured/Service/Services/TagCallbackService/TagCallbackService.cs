using Impinj.OctaneSdk;
using Service.Services.FilterService;
using Service.Services.HttpService;

namespace Service.Services.TagCallbackService;

public class TagCallbackService
{
    
    private readonly FilterDictionary _filters;
    private readonly HttpClientQueue _httpClientQueue;

    public TagCallbackService(FilterDictionary filters, HttpClientQueue httpClientQueue)
    {
        _filters = filters;
        _httpClientQueue = httpClientQueue;
    }

    public void OnTagsReported(object sender, TagReport report)
    {
        foreach (Tag tag in report)
        {
            if (_filters.ShouldReportTag(tag.Epc.ToString()))
            {
                _httpClientQueue.Enqueue(tag);
                // Enqueue
            }
        }
    }
}