using Impinj.OctaneSdk;
using Service.Services.FilterService;
using Service.Services.FilterService.Interfaces;
using Service.Services.HttpService;
using Service.Services.HttpService.Interfaces;
using Service.Services.TagCallbackService.Interfaces;

namespace Service.Services.TagCallbackService;

public class TagCallbackService : ITagCallbackService
{
    
    private readonly IFilterDictionary _filterDictionary;
    private readonly IHttpClientQueue _httpClientQueue;

    // Use interfaces in constructor
    public TagCallbackService(IFilterDictionary filterDictionary, IHttpClientQueue httpClientQueue)
    {
        _filterDictionary = filterDictionary;
        _httpClientQueue = httpClientQueue;
    }

    public void OnTagsReported(object sender, TagReport report)
    {
        foreach (Tag tag in report)
        {
            if (_filterDictionary.ShouldReportTag(tag.Epc.ToString()))
            {
                _httpClientQueue.Enqueue(tag);
            }
        }
    }
}