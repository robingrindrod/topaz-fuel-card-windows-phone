using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Topaz_Fuel_Card.Model
{
    [XmlRoot("price")]
    public class Price
    {
        [XmlText]
        public decimal Value { get; set; }

        [XmlAttribute("fuel")]
        public string Fuel { get; set; }

        public override string ToString()
        {
            return Value.ToString("F2") + " c";
        }
    }
}
