using Impinj.OctaneSdk;

namespace Service.Services.TagCallbackService.Interfaces
{
    public interface ITagCallbackService
    {
        void OnTagsReported(object sender, TagReport report);
    }
}