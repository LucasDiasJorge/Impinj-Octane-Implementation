using System.Globalization;
using Impinj.OctaneSdk;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Service.Reader;
using Service.Reader.Config;
using Service.Reader.Config.Interfaces;
using Service.Services.FilterService;
using Service.Services.FilterService.Interfaces;
using Service.Services.HttpService;
using Service.Services.HttpService.Interfaces;
using Service.Services.TagCallbackService;
using Service.Services.TagCallbackService.Interfaces;
using Service.Services.TagReporterService;

class Program
{
    static async Task Main(string[] args)
    {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        var cts = new CancellationTokenSource();
        
        // Load configuration from appsettings.json
        IConfiguration configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        // Setup Dependency Injection
        var serviceProvider = new ServiceCollection()
            .AddSingleton<IConfiguration>(configuration)
            .AddSingleton<IFilterDictionary, FilterDictionary>(sp =>
                new FilterDictionary(int.Parse(sp.GetRequiredService<IConfiguration>()["Filter:timestamp"] ?? "5")))
            .AddSingleton<IHttpClientQueue, HttpClientQueue>(sp =>
                new HttpClientQueue(500, 
                    new HttpClientService(
                        sp.GetRequiredService<IConfiguration>()["HttpClient:url"] ?? 
                        throw new InvalidOperationException("HttpClient URL not configured"))))
            .AddSingleton<ITagCallbackService, TagCallbackService>()
            .AddSingleton<ITagReporterService, TagReporterService>()
            .BuildServiceProvider();

        ImpinjReader reader = new ImpinjReader();

        var readerServiceProvider = new ServiceCollection()
            .AddSingleton(reader)
            .AddSingleton<IReaderConfiguration, ReaderConfiguration>()
            .BuildServiceProvider();
        
        var backgroudTask = serviceProvider.GetRequiredService<ITagReporterService>().ExecuteAsync(cts.Token);
        
        try
        {
            Console.WriteLine("Conectando ao leitor...");
            reader.Connect(configuration["Reader:hostname"] ?? "127.0.0.1");

            Console.WriteLine("Configurando o leitor...");

            // Resolve through interface (recommended)
            var tagCallbackService = serviceProvider.GetRequiredService<ITagCallbackService>();
            reader.TagsReported += tagCallbackService.OnTagsReported;

            reader.ApplySettings(readerServiceProvider.GetRequiredService<IReaderConfiguration>().SetConfig());
            Console.WriteLine("Iniciando leitura...");
            reader.Start();

            Console.WriteLine("Pressione ENTER para parar a leitura...");
            Console.ReadLine();

            cts.Dispose();
            
            Console.WriteLine("Parando a leitura e desconectando...");
            reader.Stop();
            reader.Disconnect();
        }
        catch (OctaneSdkException ex)
        {
            Console.WriteLine($"Erro no SDK: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro geral: {ex.Message}");
        }
    }
}