using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Impinj.OctaneSdk;

namespace ImpinjInventorySystem
{
    public class TagInventory
    {
        // Configurações do leitor
        private static readonly string ReaderHostname = "192.168.1.100"; // Substitua pelo IP do seu leitor
        
        // Instância do leitor
        private ImpinjReader _reader;
        
        // Dicionário para armazenar tags únicas com contagem de leituras
        private readonly Dictionary<string, TagReport> _tagInventory = new Dictionary<string, TagReport>();
        
        // Caminho para exportação
        private readonly string _exportPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "RFID_Inventory.csv");

        public void Run()
        {
            try
            {
                InitializeReader();
                ConfigureReader();
                StartInventory();
                
                Console.WriteLine("Pressione Enter para parar o inventário e exportar os dados...");
                Console.ReadLine();
                
                StopInventory();
                ExportToCsv();
                
                Console.WriteLine($"Inventário concluído. Dados exportados para: {_exportPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro: {ex.Message}");
            }
        }

        private void InitializeReader()
        {
            // Cria uma instância do leitor
            _reader = new ImpinjReader();
            
            // Conecta ao leitor
            Console.WriteLine($"Conectando ao leitor {ReaderHostname}...");
            _reader.Connect(ReaderHostname);
            
            // Registra handlers de eventos
            _reader.TagsReported += OnTagsReported;
            _reader.Stopped += OnStopped;
        }

        private void ConfigureReader()
        {
            // Obtém as configurações padrão
            Settings settings = _reader.QueryDefaultSettings();
            
            // Configura o modo de leitura
            settings.Report.Mode = ReportMode.Individual;
            settings.Report.IncludeAntennaPortNumber = true;
            settings.Report.IncludeFirstSeenTime = true;
            settings.Report.IncludeLastSeenTime = true;
            settings.Report.IncludeSeenCount = true;
            
            // Configura as antenas
            settings.Antennas.DisableAll();
            settings.Antennas.GetAntenna(1).IsEnabled = true;
            settings.Antennas.GetAntenna(1).TxPowerInDbm = 30.0; // Ajuste conforme necessário
            settings.Antennas.GetAntenna(1).RxSensitivityInDbm = -70;
            
            // Configura a busca por tags
            settings.SearchMode = SearchMode.DualTarget;
            settings.Session = 2;
            
            // Aplica as configurações
            _reader.ApplySettings(settings);
        }

        private void StartInventory()
        {
            Console.WriteLine("Iniciando inventário...");
            _reader.Start();
        }

        private void StopInventory()
        {
            Console.WriteLine("Parando inventário...");
            _reader.Stop();
        }

        private void OnTagsReported(ImpinjReader sender, TagReport report)
        {
            foreach (Tag tag in report)
            {
                // Filtra tags com EPC muito curto (possivelmente ruído)
                if (tag.Epc.Length < 6) continue;
                
                // Atualiza o inventário
                lock (_tagInventory)
                {
                    if (_tagInventory.ContainsKey(tag.Epc))
                    {
                        // Atualiza a tag existente
                        var existingTag = _tagInventory[tag.Epc];
                        existingTag.LastSeenTime = tag.LastSeenTime;
                        existingTag.SeenCount += tag.SeenCount;
                    }
                    else
                    {
                        // Adiciona nova tag
                        _tagInventory[tag.Epc] = tag;
                        Console.WriteLine($"Nova tag detectada: EPC: {tag.Epc} Antena: {tag.AntennaPortNumber}");
                    }
                }
            }
        }

        private void OnStopped(ImpinjReader sender, StoppedEventArgs e)
        {
            Console.WriteLine("Inventário parado.");
        }

        private void ExportToCsv()
        {
            try
            {
                Console.WriteLine($"Exportando {_tagInventory.Count} tags para CSV...");
                
                using (var writer = new StreamWriter(_exportPath))
                {
                    // Cabeçalho
                    writer.WriteLine("EPC,Antena,Primeira Leitura,Última Leitura,Total Leituras");
                    
                    // Ordena por total de leituras (decrescente)
                    var sortedTags = _tagInventory.Values.OrderByDescending(t => t.SeenCount);
                    
                    foreach (var tag in sortedTags)
                    {
                        writer.WriteLine(
                            $"{tag.Epc}," +
                            $"{tag.AntennaPortNumber}," +
                            $"{tag.FirstSeenTime:yyyy-MM-dd HH:mm:ss.fff}," +
                            $"{tag.LastSeenTime:yyyy-MM-dd HH:mm:ss.fff}," +
                            $"{tag.SeenCount}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao exportar para CSV: {ex.Message}");
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var inventory = new TagInventory();
            inventory.Run();
        }
    }
}