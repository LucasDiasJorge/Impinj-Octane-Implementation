using Impinj.OctaneSdk; // Namespace principal do SDK
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DesignPattern;  // Adicionando a diretiva 'using' correta

namespace Program
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string readerHostname = "10.0.1.122"; // Substitua pelo IP do seu leitor

            // Lista de EPCs alvos
            List<string> targetEpcs = new List<string>
            {
                "CC00 0000 0002 2024 0001 5011",
                "E280 1190 A503 0068 DEE2 4611",
                "E280 1190 A503 0063 9EF8 11E2"
            };

            ImpinjReader reader = new ImpinjReader();

            try
            {
                Console.WriteLine("Conectando ao leitor...");
                reader.Connect(readerHostname);

                Console.WriteLine("Configurando o leitor...");
                Settings settings = reader.QueryDefaultSettings();

                settings.Report.Mode = ReportMode.Individual;
                settings.Report.IncludeAntennaPortNumber = true;
                settings.Report.IncludeFirstSeenTime = true;
                settings.Antennas.DisableAll();
                settings.Session = 1;
                settings.Antennas.GetAntenna(1).IsEnabled = true;
                settings.Antennas.GetAntenna(1).TxPowerInDbm = 30.0;
                settings.Antennas.GetAntenna(3).IsEnabled = true;
                settings.Antennas.GetAntenna(3).TxPowerInDbm = 30.0;

                reader.TagsReported += (sender, report) => OnTagsReported(sender, report, targetEpcs, reader);
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

        private static async void OnTagsReported(object sender, TagReport report, List<string> targetEpcs, ImpinjReader reader)
        {
            foreach (Tag tag in report)
            {
                Console.WriteLine($"EPC: {tag.Epc}");
                Console.WriteLine($"Antena: {tag.AntennaPortNumber}");
                Console.WriteLine($"Primeira leitura: {tag.FirstSeenTime}");
                Console.WriteLine("----");

                if (targetEpcs.Contains(tag.Epc.ToString()))
                {
                    Console.WriteLine("EPC encontrado na lista de alvos. Ativando GPO 1 para HIGH.");

                    Singleton singleton = Singleton.GetInstance();

                    if (!singleton.IsGpoActive)
                    {
                        try
                        {
                            await singleton.GPOEvent(reader);
                        }
                        catch (OctaneSdkException ex)
                        {
                            Console.WriteLine($"Erro ao ativar GPO 1: {ex.Message}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("GPO 1 já está ativo. Não fazendo nada.");
                    }
                }
            }
        }
    }
}

namespace DesignPattern
{
    class Singleton
    {
        private Singleton() { }

        private static Singleton? _instance;
        private static readonly object _lock = new object();

        public bool IsGpoActive { get; set; } = false;

        public static Singleton GetInstance()
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new Singleton();
                    }
                }
            }
            return _instance;
        }

        public async Task GPOEvent(ImpinjReader reader)
        {
            if (!IsGpoActive)
            {
                reader.SetGpo(1, true);
                Console.WriteLine("GPO 1 ativado para HIGH.");
                IsGpoActive = true;
                await Task.Delay(2000);
                reader.SetGpo(1, false);
                Console.WriteLine("GPO 1 desativado para LOW.");
                IsGpoActive = false;
            }
        }
    }
}
