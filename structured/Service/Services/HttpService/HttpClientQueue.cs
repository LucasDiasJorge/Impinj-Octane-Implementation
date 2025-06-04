using System.Collections.Concurrent;
using Impinj.OctaneSdk;
using Service.Services.HttpService.Interfaces;

namespace Service.Services.HttpService;

public class HttpClientQueue : IHttpClientQueue
{
    private readonly ConcurrentQueue<Tag> _tagsToSend = new();
    private readonly int _maxSize;
    private readonly HttpClientService _httpClientService;

    public int Count => _tagsToSend.Count;

    public HttpClientQueue(int maxSize, HttpClientService httpClientService)
    {
        _maxSize = maxSize;
        _httpClientService = httpClientService;
        Console.WriteLine($"✅ HttpClientQueue initialized with max size: {_maxSize}");
    }
    
    public bool IsEmpty => _tagsToSend.Count == 0;

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

    public async Task DequeueAsync()
    {
        if (_tagsToSend.IsEmpty)
        {
            Console.WriteLine("⚠ Queue is empty. No tag to dequeue.");
            return;
        }

        if (_tagsToSend.TryDequeue(out Tag tag))
        {
            Console.WriteLine($"🔄 Attempting to send tag {tag.Epc} to API...");

            try
            {
                bool success = await _httpClientService.SendTagToApiAsync(tag);

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

}