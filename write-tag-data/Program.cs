using Impinj.OctaneSdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ImpinjWriteTags
{
    internal class Program
    {

        private readonly static ImpinjReader reader = new ImpinjReader();
        private static Dictionary<string, TagEncode> TagsEncode = new Dictionary<string, TagEncode>();
        private readonly static int TimeoutOp = 5;
        private static DateTime StartedOp = DateTime.Now;

        static void Main(string[] args)
        {
            // Criamos uma relação de EPCs a serem gravados
            TagsEncode.Add("E280110520007541DE0008B4", new TagEncode("303500000000000001010101", "E280110520007541DE0008B4", "303500000000000000000011"));

            // TagsEncode.Add(TID to find, new TagEncode(CurrentEPC, TID, new EPC))

            Connect("10.0.1.122");
            ApplySettings();
            Start();

            Console.WriteLine("Coloque os tags na antena do leitor....");

            while (TagsEncode.Count(x => x.Value.Encoded) != TagsEncode.Count())
            {
                Thread.Sleep(1000);

                if (StartedOp.AddSeconds(TimeoutOp) < DateTime.Now) break;
            }

            Console.WriteLine("Operação concluída!");
            Console.WriteLine();
            Console.WriteLine(string.Format("{0} tags escritos, {1} erros", TagsEncode.Count(x => x.Value.Encoded), TagsEncode.Count(x => !x.Value.Encoded)));
            Console.WriteLine();

            foreach (KeyValuePair<string, TagEncode> entry in TagsEncode)
            {
                Console.WriteLine(string.Format("TID: {0}, Epc Atual: {1} => Novo EPC {2}, Codificado: {3}", entry.Key, entry.Value.CurrentEpc, entry.Value.NewEpc, entry.Value.Encoded ? "Sim" : "Não"));
            }

            Console.WriteLine();
            Console.WriteLine("Pressione qualquer tecla para encerrar...");

            Stop();
            Disconnect();
        }

        static void Connect(string host)
        {
            try
            {
                reader.Connect(host);
                reader.TagsReported += TagsReported;
                reader.TagOpComplete += TagOpComplete;
            }
            catch (OctaneSdkException e)
            {
                Console.WriteLine("Octane SDK exception: {0}", e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception : {0}", e.Message);
            }
        }

        static void Disconnect()
        {
            if (reader.IsConnected)
                reader.Disconnect();
        }

        static void ApplySettings()
        {
            try
            {
                Settings settings = reader.QueryDefaultSettings();
                settings.Report.IncludeFastId = true;
                settings.Antennas.DisableAll();
                settings.Antennas.GetAntenna(4).IsEnabled = true;
                settings.Antennas.GetAntenna(4).TxPowerInDbm = 20;
                settings.Antennas.GetAntenna(4).RxSensitivityInDbm = -70;
                reader.ApplySettings(settings);
            }
            catch (OctaneSdkException e)
            {
                Console.WriteLine("Octane SDK exception: {0}", e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception : {0}", e.Message);
            }
        }

        static void Start()
        {
            try
            {
                reader.Start();
            }
            catch (OctaneSdkException e)
            {
                Console.WriteLine("Octane SDK exception: {0}", e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception : {0}", e.Message);
            }
        }

        static void Stop()
        {
            reader.Stop();
        }

        static void AddTagToEncode(TagEncode tagEncode)
        {
            try
            {
                // Criando uma nova sequencia de operação
                TagOpSequence seq = new TagOpSequence();

                // Definindo o tag de destino com base no TID
                seq.TargetTag.MemoryBank = MemoryBank.Tid;
                seq.TargetTag.BitPointer = 0;
                seq.TargetTag.Data = tagEncode.Tid;

                // Criando uma operação de escrita do banco de memória EPC
                TagWriteOp writeEpc = new TagWriteOp();
                writeEpc.Id = 1;
                writeEpc.MemoryBank = MemoryBank.Epc;
                writeEpc.Data = TagData.FromHexString(tagEncode.NewEpc);
                writeEpc.WordPointer = WordPointers.Epc;
                seq.Ops.Add(writeEpc);

                reader.AddOpSequence(seq);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        static void TagsReported(ImpinjReader reader, TagReport report)
        {
            foreach (Tag tag in report)
            {
                Console.WriteLine(string.Format("EPC: {0}, TID: {1} ", tag.Epc.ToHexString(), tag.Tid.ToHexString()));

                if (TagsEncode.ContainsKey(tag.Tid.ToHexString()))
                {
                    TagEncode tagEnconde = TagsEncode[tag.Tid.ToHexString()];

                    if (!tagEnconde.Encoded)
                        AddTagToEncode(tagEnconde);
                }
                if (StartedOp.AddSeconds(TimeoutOp) < DateTime.Now) break;
            }
        }

        static void TagOpComplete(ImpinjReader reader, TagOpReport results)
        {
            foreach (TagOpResult result in results)
            {
                if (result is TagWriteOpResult)
                {// Operação de escrita de tag concluída                    
                    TagWriteOpResult writeResult = result as TagWriteOpResult;

                    if (writeResult.OpId == 1)
                    {
                        string tid = writeResult.Tag.Tid.ToHexString();

                        TagEncode tagEncode;

                        if (TagsEncode.TryGetValue(tid, out tagEncode) && !tagEncode.Encoded)
                        {
                            if (writeResult.Result == WriteResultStatus.Success)
                                TagsEncode[tid].Encoded = true;
                        }
                    }
                }
            }
        }
    }
}