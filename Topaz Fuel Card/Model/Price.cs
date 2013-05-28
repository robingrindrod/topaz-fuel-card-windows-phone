using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Topaz_Fuel_Card
{
    public class Price
    {
        public decimal Value { get; set; }

        public override string ToString()
        {
            return Value.ToString("F2") + " c";
        }
    }
}
