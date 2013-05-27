using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Topaz_Fuel_Card
{
    class Prices
    {
        private Decimal petrolPrice;
        private Decimal dieselPrice;

        public static Prices RetrievePrices()
        {
            Prices prices = new Prices();
            prices.petrolPrice = 160;
            prices.dieselPrice = 150;
            return prices;
        }

        public String DieselPrice
        {
            get
            {
                return dieselPrice.ToString("F2") + " c";
            }
        }

        public String PetrolPrice
        {
            get
            {
                return petrolPrice.ToString("F2") + " c";
            }
        }
    }
}
