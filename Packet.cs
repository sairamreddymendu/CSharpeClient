using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C_Client
{
    public class Packet
    {
        public string Symbol { get; set; }
        public string Indicator { get; set; }
        public int Quantity { get; set; }
        public int Price { get; set; }
        public int Sequence { get; set; }

        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Symbol)
                && Symbol.All(char.IsLetterOrDigit)
                && Symbol.Length == 4
                && (Indicator == "B" || Indicator == "S")
                && Quantity > 0
                && Price >= 0
                && Sequence >= 0;
        }
    }
}
