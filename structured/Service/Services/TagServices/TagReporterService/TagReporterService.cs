using Service.Helper;
using Service.Services.HttpService;
using Service.Services.HttpService.Interfaces;

namespace Service.Services.TagReporterService;

public class TagReporterService : ITagReporterService
{
    
    private IHttpClientQueueService _httpClientQueueService;

    private static int DELAY = 5000;

    public TagReporterService(IHttpClientQueueService httpClientQueueService)
    {
        _httpClientQueueService = httpClientQueueService;
    }

    private async Task AwaitDelay()
    {
        await Task.Delay(DELAY);
    }

    public async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("Initializing TagReporterService");
        
        Task.Run( async () =>
        {
            while (!stoppingToken.IsCancellationRequested)
            {

                bool isNetworkAvailable = await NetworkHelper.IsNetworkAvailableAsync();
                
                if (!isNetworkAvailable)
                {
                    if (!_httpClientQueueService.IsEmpty)
                    {
                        await _httpClientQueueService.DequeueAsync();
                    }
                    else
                    {
                        await AwaitDelay();
                    }
                }
                else
                {
                    await AwaitDelay();
                }
                                
            }
            
        }, stoppingToken);
    }

}