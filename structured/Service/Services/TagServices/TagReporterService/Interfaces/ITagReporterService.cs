namespace Service.Services.TagReporterService;

public interface ITagReporterService
{
    public Task ExecuteAsync(CancellationToken stoppingToken);

}