using Service.Services.HttpService;
using Service.Services.HttpService.Interfaces;

namespace Service.Services.TagReporterService;

public class TagReporterService : ITagReporterService
{
    
    private IHttpClientQueue _httpClientQueue;

    public TagReporterService(IHttpClientQueue httpClientQueue)
    {
        _httpClientQueue = httpClientQueue;
    }

    public async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("Initializing TagReporterService");
        
        Task.Run( async () =>
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (!_httpClientQueue.IsEmpty)
                {
                    _httpClientQueue.DequeueAsync();
                    await Task.Delay(300000);
                }
                else
                {
                    await Task.Delay(3000);
                }
                
            }
            
        }, stoppingToken);
    }

}