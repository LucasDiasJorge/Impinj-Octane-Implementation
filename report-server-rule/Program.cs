using Impinj.OctaneSdk;
using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

class Program
{
    // Dicionário para controlar o tempo da última leitura de cada etiqueta
    private static readonly ConcurrentDictionary<string, DateTime> LastReadTimes = new();

    // Tempo de filtro (configurável, em segundos)
    private static readonly int FilterTimeInSeconds = 5;

    // URL da API REST para enviar os dados
    private static readonly string ApiUrl = "http://localhost:8892/api/v2/reader/read/serial";

    private static readonly string BearerToken = "<TOKEN>";
    static async Task Main(string[] args)
    {
        string readerHostname = "10.0.1.122"; // Substitua pelo IP ou hostname do leitor
        ImpinjReader reader = new ImpinjReader();

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

            // Callback para processar etiquetas lidas
            reader.TagsReported += OnTagsReported;

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

    private static void OnTagsReported(object sender, TagReport report)
    {
        foreach (Tag tag in report)
        {
            // Verifica se o EPC foi lido recentemente
            if (ShouldReportTag(tag.Epc.ToString()))
            {
                // Dispara a tarefa de envio para a API REST
                _ = SendTagToApiAsync(tag);
            }
        }
    }

    private static bool ShouldReportTag(string epc)
    {
        var now = DateTime.UtcNow;

        // Tenta obter o timestamp da última leitura
        if (LastReadTimes.TryGetValue(epc, out var lastReadTime))
        {
            // Se a etiqueta foi lida recentemente, não reporta
            if ((now - lastReadTime).TotalSeconds < FilterTimeInSeconds)
            {
                return false;
            }
        }

        // Atualiza o tempo da última leitura
        LastReadTimes[epc] = now;
        return true;
    }

    private static async Task SendTagToApiAsync(Tag tag)
    {
        try
        {
            using HttpClient client = new();

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", BearerToken);

            var tagData = new
            {
                Epc = tag.Epc.ToString(),
                Antenna = tag.AntennaPortNumber,
                Timestamp = tag.FirstSeenTime.Utc,
                Rssi = tag.PeakRssiInDbm
            };

            Console.WriteLine(tagData);

            var json = JsonSerializer.Serialize(tagData, new JsonSerializerOptions { WriteIndented = true });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            Console.WriteLine($"Enviando dados para API: {json}");

            var response = await client.PostAsync(ApiUrl, content);
            string responseBody = await response.Content.ReadAsStringAsync(); // Obtém o corpo da resposta

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"✔ Etiqueta {tag.Epc} reportada com sucesso!");
            }
            else
            {
                Console.WriteLine($"❌ Erro ao reportar {tag.Epc}");
                Console.WriteLine($"Status Code: {response.StatusCode} ({(int)response.StatusCode})");
                Console.WriteLine($"Response Headers: {response.Headers}");
                Console.WriteLine($"Response Body: {responseBody}");
            }
        }
        catch (HttpRequestException httpEx)
        {
            Console.WriteLine($"❗ Erro HTTP: {httpEx.Message}");
        }
        catch (TaskCanceledException tcEx)
        {
            Console.WriteLine($"⏳ Timeout ou requisição cancelada: {tcEx.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠ Erro inesperado ao enviar dados para a API: {ex.Message}");
        }
    }

}
