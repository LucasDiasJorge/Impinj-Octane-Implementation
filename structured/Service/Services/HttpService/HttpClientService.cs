using System.Text;
using System.Text.Json;
using Impinj.OctaneSdk;
using Microsoft.Extensions.Configuration;

namespace Service.Services.HttpService;

public class HttpClientService
{
    
    private readonly string url;
    
    public HttpClientService(string url)
    {
        this.url = url;
    }

    public async Task<bool> SendTagToApiAsync(Tag tag)
    {
        try
        {
            using HttpClient client = new();

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "<BEARER TOKEN>");

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

            var response = await client.PostAsync(url, content);
            string responseBody = await response.Content.ReadAsStringAsync(); // Obtém o corpo da resposta

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"✔ Etiqueta {tag.Epc} reportada com sucesso!");
                return true;
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
        return false;

    }
    
}