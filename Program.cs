
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Xml;
using Newtonsoft.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;

namespace C_Client
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Log.Logger = SetupLogger();          

            string host = $@"localhost";
            int port = 3000;

            try
            {
                List<Packet> packets = ABXRepository.GetPackets(host, port);

                List<int> missingSequence = ABXRepository.GetMissingSequence(packets);

                List<Packet> missingPackets = ABXRepository.GetMissingPackets(missingSequence, host, port);

                packets.AddRange(missingPackets);

                List<Packet> data = packets.OrderBy(packet => packet.Sequence).ToList();

                File.WriteAllText("output.json", JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented));

                Console.WriteLine("output.json Saved to: " + Path.GetFullPath("output.json"));
                Log.Information("output.json Saved to: " + Path.GetFullPath("output.json"));
            }
            catch(Exception ex)
            {
                var message = ex switch
                {
                    SocketException => "SocketException",
                    IOException => "IOException",
                    JsonSerializationException => "JsonSerializationException",
                    ArgumentException => "ArgumentException",
                    _ => "Exceptional"
                };

                Log.Error($"{message} {ex.Message}");
            }            
            finally
            {
                Log.Information("Code Execution Completed");
                Log.CloseAndFlush();
            }           
        
        }
        static void LogPackets(List<Packet> packets)
        {
            foreach (Packet packet in packets)
            {
                Console.WriteLine($"{packet.Symbol} {packet.Indicator} {packet.Quantity} {packet.Price} {packet.Sequence}");
            }

        }
        static Logger SetupLogger()
        {
            return new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();
        }
    }
}
