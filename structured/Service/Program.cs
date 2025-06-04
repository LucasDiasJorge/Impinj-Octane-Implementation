using Impinj.OctaneSdk;
using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;
using Service.Services.FilterService;
using Service.Services.HttpService;
using Service.Services.TagCallbackService;

class Program
{
    
    static async Task Main(string[] args)
    {
        string readerHostname = "10.0.1.122"; // Substitua pelo IP ou hostname do leitor
        ImpinjReader reader = new ImpinjReader();

        IConfiguration configuration = new ConfigurationBuilder().Build();
        
        FilterDictionary filter = new FilterDictionary(int.Parse(configuration["Filter:timestamp"] ?? throw new InvalidOperationException()));
        HttpClientQueue queue = new HttpClientQueue(500, new HttpClientService(configuration["HttpClient:url"] ?? throw new InvalidOperationException()));
        TagCallbackService tagCallbackService = new TagCallbackService(filter, queue);

        try
        {
            Console.WriteLine("Conectando ao leitor...");
            reader.Connect(readerHostname);

            Console.WriteLine("Configurando o leitor...");
            Settings settings = reader.QueryDefaultSettings();
            settings.Report.Mode = ReportMode.Individual; // Relatar etiquetas individualmente
            settings.Report.IncludeFirstSeenTime = true; // Incluir o timestamp da primeira leitura
            settings.Antennas.GetAntenna(1).IsEnabled = true; // Ativar a antena 1
            settings.Antennas.GetAntenna(1).TxPowerInDbm = 24.0; // Potência de transmissão

            reader.TagsReported += tagCallbackService.OnTagsReported;

            reader.ApplySettings(settings);
            Console.WriteLine("Iniciando leitura...");
            reader.Start();

            Console.WriteLine("Pressione ENTER para parar a leitura...");
            Console.ReadLine();

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
