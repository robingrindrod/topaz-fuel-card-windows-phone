using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Windows;
using System.Xml.Linq;
using System.Xml.Serialization;
using Topaz_Fuel_Card.Model;

namespace Topaz_Fuel_Card.ViewModel
{
    public class TopazViewModel : INotifyPropertyChanged
    {
        private const string PricesUrl = "http://topazfuelcard.azurewebsites.net/get-prices.php";

        IDictionary<String, Price> prices = new Dictionary<String, Price>();

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
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(result);
                Stream resultStream = response.GetResponseStream();

                XmlSerializer serializer = new XmlSerializer(typeof(PricesHolder));
                PricesHolder holder = (PricesHolder)serializer.Deserialize(resultStream);
                foreach(Price price in holder.Prices)
                {
                    prices.Add(price.Fuel, price);
                }

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("DieselPrice"));
                    PropertyChanged(this, new PropertyChangedEventArgs("PetrolPrice"));
                });
            }
            catch (WebException)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    // TODO Extract as an event
                    MessageBox.Show("Unable to retreive prices. Please check your Internet connectivity.");
                });
            }

        }

        public Price DieselPrice
        {
            get
            {
                Price dieselPrice;
                prices.TryGetValue("Diesel", out dieselPrice);
                return dieselPrice;
            }
        }

        public Price PetrolPrice
        {
            get
            {
                Price petrolPrice;
                prices.TryGetValue("Petrol", out petrolPrice);
                return petrolPrice;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
