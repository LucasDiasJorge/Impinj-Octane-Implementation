using Impinj.OctaneSdk;
using Service.Services.HttpService.Interfaces;

namespace Service.Services.HttpService;

public class HttpClientQueue : IHttpClientQueue
{
    private readonly Queue<Tag> _tagsToSend = new();
    private readonly int _maxSize;
    private readonly HttpClientService _httpClientService;

    public int Count => _tagsToSend.Count;

    public HttpClientQueue(int maxSize, HttpClientService httpClientService)
    {
        _maxSize = maxSize;
        _httpClientService = httpClientService;
        Console.WriteLine($"✅ HttpClientQueue initialized with max size: {_maxSize}");
    }

    public void Enqueue(Tag tag)
    {
        if (_tagsToSend.Count >= _maxSize)
        {
            Console.WriteLine($"⚠ Queue is full. Cannot enqueue tag: {tag.Epc}");
            return;
        }
        
        _tagsToSend.Enqueue(tag);
        Console.WriteLine($"➕ Enqueued tag: {tag.Epc} | Queue size: {_tagsToSend.Count}");
    }

    public void Dequeue()
    {
        if (_tagsToSend.Count == 0)
        {
            Console.WriteLine("⚠ Queue is empty. No tag to dequeue.");
            return;
        }

        Tag tag = _tagsToSend.Dequeue();
        Console.WriteLine($"🔄 Attempting to send tag {tag.Epc} to API...");

        try
        {
            bool success = _httpClientService.SendTagToApiAsync(tag).Result;
            
            if (success)
            {
                Console.WriteLine($"✔ Successfully dequeued and reported tag: {tag.Epc} | Remaining queue size: {_tagsToSend.Count}");
            }
            else
            {
                Console.WriteLine($"❌ API reporting failed for tag: {tag.Epc}. Re-enqueuing...");
                _tagsToSend.Enqueue(tag);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠ Unexpected error when sending tag {tag.Epc}: {ex.Message}");
            _tagsToSend.Enqueue(tag); // Re-enqueue failed tag
        }
    }
}