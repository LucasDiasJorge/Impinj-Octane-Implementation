using Impinj.OctaneSdk; // Namespace principal do SDK
using System;

class Program
{
    static void Main(string[] args)
    {
        // IP ou hostname do leitor R700
        string readerHostname = "10.0.1.122"; // Substitua pelo IP ou nome do seu leitor

        // Define o EPC alvo (configurável)
        string targetEpc = "3035 0000 0000 0000 0000 0011"; // Substitua pelo EPC desejado

        // Cria uma instância do leitor
        ImpinjReader reader = new ImpinjReader();

        try
        {
            // Conecta ao leitor
            Console.WriteLine("Conectando ao leitor...");
            reader.Connect(readerHostname);

            // Configura o leitor
            Console.WriteLine("Configurando o leitor...");
            Settings settings = reader.QueryDefaultSettings();

            // Configuração básica:
            settings.Report.Mode = ReportMode.Individual; // Relata cada etiqueta individualmente
            settings.Report.IncludeAntennaPortNumber = true; // Inclui o número da antena no relatório
            settings.Report.IncludeFirstSeenTime = true; // Inclui o timestamp no relatório

            settings.Antennas.DisableAll();
            settings.Session = 1; // Define a sessão do protocolo EPC Gen2
            settings.Antennas.GetAntenna(4).IsEnabled = true; // Ativa a antena 4
            settings.Antennas.GetAntenna(4).TxPowerInDbm = 14.0; // Define a potência de transmissão (em dBm)

            // Define o evento de callback para quando etiquetas forem lidas
            reader.TagsReported += (sender, report) => OnTagsReported(sender, report, targetEpc, reader);

            // Aplica as configurações no leitor
            reader.ApplySettings(settings);

            // Inicia o processo de leitura
            Console.WriteLine("Iniciando leitura...");
            reader.Start();

            Console.WriteLine("Pressione ENTER para parar a leitura...");
            Console.ReadLine();

            // Para a leitura e desconecta do leitor
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

    // Callback para processar etiquetas lidas
    private static void OnTagsReported(object sender, TagReport report, string targetEpc, ImpinjReader reader)
    {
        foreach (Tag tag in report)
        {
            Console.WriteLine($"EPC: {tag.Epc}");
            Console.WriteLine($"Antena: {tag.AntennaPortNumber}");
            Console.WriteLine($"Primeira leitura: {tag.FirstSeenTime}");
            Console.WriteLine("----");

            // Verifica se o EPC corresponde ao valor configurado
            if (tag.Epc.ToString() == targetEpc)
            {
                Console.WriteLine("EPC corresponde ao alvo. Ativando GPO 1 para HIGH.");
                
                // Ativa o GPO 1 (General Purpose Output 1) para HIGH
                try
                {
                    // GPO 1 é controlado com o método SetGpo
                    reader.SetGpo(1, true); // true para HIGH
                    Console.WriteLine("GPO 1 ativado para HIGH.");
                }
                catch (OctaneSdkException ex)
                {
                    Console.WriteLine($"Erro ao ativar GPO 1: {ex.Message}");
                }finally{
                    reader.SetGpo(1,false);
                }
            }
        }
    }
}
