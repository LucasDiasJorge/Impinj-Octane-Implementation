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
    private readonly IHttpClientQueueService _httpClientQueueService;

    // Use interfaces in constructor
    public TagCallbackService(IFilterDictionary filterDictionary, IHttpClientQueueService httpClientQueueService)
    {
        _filterDictionary = filterDictionary;
        _httpClientQueueService = httpClientQueueService;
    }

    public void OnTagsReported(object sender, TagReport report)
    {
        foreach (Tag tag in report)
        {
            if (_filterDictionary.ShouldReportTag(tag.Epc.ToString()))
            {
                _httpClientQueueService.Enqueue(tag);
            }
        }
    }
}