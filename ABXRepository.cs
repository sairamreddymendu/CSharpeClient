using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace C_Client
{
    public static class ABXRepository
    {
        public static List<Packet> GetPackets(string host, int port)
        {
            List<Packet> packets = new List<Packet>();
            using (var client = new TcpClient(host, port))
            {
                using (var stream = client.GetStream())
                {

                    stream.Write(new byte[] { 1, 0 }, 0, 2);

                    var buffer = new byte[17];
                    int b;
                    while ((b = stream.Read(buffer, 0, 17)) > 0)
                    {
                        packets.Add(ParsePacket(buffer));
                    }

                }
            }
            return packets;
        }

        public static Packet ParsePacket(byte[] buffer)
        {
            string symbol = Encoding.ASCII.GetString(buffer, 0, 4);
            string indicator = Encoding.ASCII.GetString(buffer, 4, 1);
            int quantity = BitConverter.ToInt32(buffer.Skip(5).Take(4).Reverse().ToArray(), 0);
            int price = BitConverter.ToInt32(buffer.Skip(9).Take(4).Reverse().ToArray(), 0);
            int sequence = BitConverter.ToInt32(buffer.Skip(13).Take(4).Reverse().ToArray(), 0);

            var packet = new Packet()
            {
                Symbol = symbol,
                Indicator = indicator,
                Quantity = quantity,
                Price = price,
                Sequence = sequence
            };
            if(!packet.IsValid())
            {
                Log.Warning($"Invalid or corrupted packet received for sequence : {sequence}");
            }
            return packet;
        }

        public static List<int> GetMissingSequence(List<Packet> packets)
        {
            var sequences = packets.Select(p => p.Sequence);
            int max = sequences.Max();
            return Enumerable.Range(1, max).Except(sequences).ToList();
        }

        public static List<Packet> GetMissingPackets(List<int> sequences, string host, int port)
        {
            List<Packet> packets = new List<Packet>();
            using (var client = new TcpClient(host, port))
            {
                using (var stream = client.GetStream())
                {
                    foreach (var sequence in sequences)
                    {
                        byte[] buffer = new byte[17];
                        stream.Write(new byte[] { 2, (byte)sequence }, 0, 2);
                        stream.Read(buffer, 0, 17);
                        packets.Add(ParsePacket(buffer));
                    }

                }
            }
            return packets;

        }

    }
}
