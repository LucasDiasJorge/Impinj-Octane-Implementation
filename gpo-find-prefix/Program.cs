using Impinj.OctaneSdk;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Program
{
    class Program
    {
        static ConcurrentDictionary<string, int> tagCounts = new ConcurrentDictionary<string, int>();
        static ImpinjReader reader = new ImpinjReader();
        static bool running = true;
        static string configuredPrefix = "E280"; // Prefixo configurável

        static void Main(string[] args)
        {
            string readerHostname = "10.0.1.122"; // Substitua pelo IP do seu leitor

            try
            {
                Console.WriteLine("Conectando ao leitor...");
                reader.Connect(readerHostname);

                Console.WriteLine("Configurando o leitor...");
                Settings settings = reader.QueryDefaultSettings();
                settings.Report.Mode = ReportMode.Individual;
                settings.Antennas.DisableAll();
                settings.Antennas.GetAntenna(1).IsEnabled = true;
                settings.Antennas.GetAntenna(1).TxPowerInDbm = 30.0;
                settings.Antennas.GetAntenna(3).IsEnabled = true;
                settings.Antennas.GetAntenna(3).TxPowerInDbm = 30.0;

                reader.TagsReported += OnTagsReported;
                reader.ApplySettings(settings);
                reader.Start();

                Console.WriteLine("Pressione ENTER para exibir os resultados e encerrar...");
                Console.ReadLine();
                running = false;

                reader.Stop();
                reader.Disconnect();

                Console.WriteLine("Resultado da contagem de etiquetas:");
                foreach (var entry in tagCounts.OrderBy(kv => kv.Key))
                {
                    Console.WriteLine($"Prefixo {entry.Key}: {entry.Value} etiquetas");
                }
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
                }
            }
        }
    }
}
