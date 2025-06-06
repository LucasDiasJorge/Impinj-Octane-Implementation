﻿using System.Text;
using System.Text.Json;
using Impinj.OctaneSdk;
using Microsoft.Extensions.Configuration;
using Service.Services.HttpService.Interfaces;

namespace Service.Services.HttpService;

public class HttpClientService : IHttpClientService
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

            client.DefaultRequestHeaders.Add("X-API-KEY", "application/json");
            
            var requestData = new
            {
                data = new[]
                {
                    new
                    {
                        epc = tag.Epc.ToString(),
                        antenna = tag.AntennaPortNumber,
                        timestamp = tag.FirstSeenTime.Utc / 1000000,
                    }
                }
            };

            //Console.WriteLine(tagData);

            var json = JsonSerializer.Serialize(requestData, new JsonSerializerOptions { WriteIndented = true });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            //Console.WriteLine($"Sent to API: {json}");

            var response = await client.PostAsync(url, content);
            string responseBody = await response.Content.ReadAsStringAsync(); // Obtém o corpo da resposta

            if (response.IsSuccessStatusCode)
            {
                //Console.WriteLine($"✔ Tag {tag.Epc} reported successfully!");
                return true;
            }
            else
            {
                Console.WriteLine($"❌ Error while report {tag.Epc}");
                Console.WriteLine($"Status Code: {response.StatusCode} ({(int)response.StatusCode})");
                Console.WriteLine($"Response Headers: {response.Headers}");
                Console.WriteLine($"Response Body: {responseBody}");
                Console.WriteLine($"Posting Body: {json}");
            }
        }
        catch (HttpRequestException httpEx)
        {
            Console.WriteLine($"❗ Error HTTP: {httpEx.Message}");
        }
        catch (TaskCanceledException tcEx)
        {
            Console.WriteLine($"⏳ Timeout or request was cancelled: {tcEx.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠ Non mapped error during request: {ex.Message}");
        }
        return false;

    }
    
}