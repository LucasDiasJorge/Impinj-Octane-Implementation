using Impinj.OctaneSdk;
using Service.Services.FilterService;

namespace Service.Services.TagCallbackService;

public class TagCallbackService
{
    
    private readonly FilterDictionary _filters;

    public TagCallbackService(FilterDictionary filters)
    {
        _filters = filters;
    }

    public void OnTagsReported(object sender, TagReport report)
    {
        foreach (Tag tag in report)
        {
            if (_filters.ShouldReportTag(tag.Epc.ToString()))
            {
                // Enqueue
            }
        }
    }
}