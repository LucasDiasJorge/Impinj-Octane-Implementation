using Impinj.OctaneSdk;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace RFIDReader
{
    class Program
    {
        static ConcurrentDictionary<string, bool> uniqueTags = new ConcurrentDictionary<string, bool>();
        static ImpinjReader reader = new ImpinjReader();
        static bool running = true;
        static string configuredPrefix;

        static void Main(string[] args)
        {
            Console.Write("Digite o prefixo das etiquetas a serem filtradas: ");
            configuredPrefix = Console.ReadLine()?.Trim() ?? "";

            Console.Write("Digite o IP do leitor: ");
            string readerHostname = Console.ReadLine()?.Trim() ?? "10.0.1.122";
            
            try
            {
                Console.WriteLine("Conectando ao leitor...");
                reader.Connect(readerHostname);

                Console.WriteLine("Configurando o leitor...");
                Settings settings = reader.QueryDefaultSettings();
                settings.Report.Mode = ReportMode.Individual;
                settings.Antennas.DisableAll();
                settings.Antennas.GetAntenna(1).IsEnabled = true;
                settings.Antennas.GetAntenna(1).TxPowerInDbm = 22.0;
                settings.Antennas.GetAntenna(3).IsEnabled = true;
                settings.Antennas.GetAntenna(3).TxPowerInDbm = 22.0;

                reader.TagsReported += OnTagsReported;
                reader.ApplySettings(settings);
                reader.Start();

                Thread displayThread = new Thread(DisplayTagCount);
                displayThread.Start();

                Console.WriteLine("Pressione ENTER para exibir os resultados finais e encerrar...");
                Console.ReadLine();
                running = false;

                reader.Stop();
                reader.Disconnect();
                
                Console.WriteLine("Resultado da contagem de etiquetas únicas:");
                Console.WriteLine($"Total de etiquetas únicas com prefixo {configuredPrefix}: {uniqueTags.Count}");
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
                string epc = tag.Epc.ToString();
                if (epc.StartsWith(configuredPrefix))
                {
                    uniqueTags.TryAdd(epc, true);
                }
            }
        }

        private static void DisplayTagCount()
        {
            while (running)
            {
                Console.WriteLine($"Total de etiquetas únicas com prefixo {configuredPrefix}: {uniqueTags.Count}");
                Thread.Sleep(5000);
            }
        }
    }
}
