using Impinj.OctaneSdk;

namespace Service.Services.HttpService.Interfaces
{
    public interface IHttpClientQueueService
    {
        int Count { get; }
        bool IsEmpty { get; }
        void Enqueue(Tag tag);
        public Task DequeueAsync();
    }
}