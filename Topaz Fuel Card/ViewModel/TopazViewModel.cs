using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace Topaz_Fuel_Card
{
    public class TopazViewModel : INotifyPropertyChanged
    {
        private Price petrolPrice = new Price();
        private Price dieselPrice = new Price();
        private const string PricesUrl = "http://topazfuelcard.azurewebsites.net/get-prices.php";

        private class State
        {
            public HttpWebRequest Request { get; set; }
        }

        public void UpdatePrices()
        {
            UriBuilder uri = new UriBuilder(PricesUrl);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri.Uri);
            State state = new State()
            {
                Request = request,
            };
            request.BeginGetResponse(HandleResponse, state);
        }

        private void HandleResponse(IAsyncResult result)
        {
            State state = (State)result.AsyncState;
            HttpWebRequest request = state.Request;
            HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(result);
            Stream resultStream = response.GetResponseStream();

            // TODO Parse XML
            XDocument pricesXml = XDocument.Load(resultStream);
            IEnumerable<XElement> pricesXmlElements = pricesXml.Descendants("price");
            String dieselPrice = (from price in pricesXmlElements
                                  where price.Attribute("fuel").Value == "Diesel"
                                  select price.Value).Single<String>();
            String petrolPrice = (from price in pricesXmlElements
                                  where price.Attribute("fuel").Value == "Petrol"
                                  select price.Value).Single<String>();

            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                UpdateDieselPrice(Decimal.Parse(dieselPrice));
                UpdatePetrolPrice(Decimal.Parse(petrolPrice));
            });

        }

        private void UpdateDieselPrice(Decimal price)
        {
            dieselPrice.Value = price;
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("DieselPrice"));
            }
        }

        private void UpdatePetrolPrice(Decimal price)
        {
            petrolPrice.Value = price;
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("PetrolPrice"));
            }
        }

        public Price DieselPrice
        {
            get
            {
                return dieselPrice;
            }
        }

        public Price PetrolPrice
        {
            get
            {
                return petrolPrice;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
