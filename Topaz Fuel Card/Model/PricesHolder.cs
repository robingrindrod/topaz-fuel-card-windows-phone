using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Topaz_Fuel_Card.Model
{
    [XmlRoot("prices")]
    public class PricesHolder
    {
        [XmlElement("price")]
        public List<Price> Prices { get; set; }
    }
}
