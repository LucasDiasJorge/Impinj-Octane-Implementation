using Impinj.OctaneSdk;

namespace Service.Services.HttpService.Interfaces
{
    public interface IHttpClientService
    {
        Task<bool> SendTagToApiAsync(Tag tag);
    }
}