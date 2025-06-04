using Impinj.OctaneSdk;

namespace Service.Services.HttpService.Interfaces
{
    public interface IHttpClientQueue
    {
        int Count { get; }
        void Enqueue(Tag tag);
        public Task DequeueAsync();
    }
}