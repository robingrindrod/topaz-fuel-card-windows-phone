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
        private const string PricesCacheFileName = "prices.dat";
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

            Stream pricesStream = GetPricesStream(result, request);
            if (pricesStream != null)
            {
                ParsePrices(pricesStream);
                pricesStream.Close();
            }
        }

        private void ParsePrices(Stream pricesStream)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(PricesHolder));
            PricesHolder holder = (PricesHolder)serializer.Deserialize(pricesStream);
            foreach (Price price in holder.Prices)
            {
                prices[price.Fuel] = price;
            }

            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                PropertyChanged(this, new PropertyChangedEventArgs("DieselPrice"));
                PropertyChanged(this, new PropertyChangedEventArgs("PetrolPrice"));
            });
        }

        private static Stream GetPricesStream(IAsyncResult result, HttpWebRequest request)
        {
            Stream pricesStream = new MemoryStream();

            IsolatedStorageFile isolatedStorage = IsolatedStorageFile.GetUserStoreForApplication();

            try
            {
                // Copy resopnse to memory
                HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(result);
                Stream resultStream = response.GetResponseStream();
                resultStream.CopyTo(pricesStream);
                pricesStream.Position = 0;
                resultStream.Close();

                // Copy memory to cache
                IsolatedStorageFileStream pricesCacheStream = isolatedStorage.OpenFile(PricesCacheFileName, FileMode.Create, FileAccess.Write);
                pricesStream.CopyTo(pricesCacheStream);
                pricesStream.Position = 0;
                pricesCacheStream.Close();
            }
            catch (WebException)
            {
                // If there is a problem retreiving the prices from the Internet, load the prices from the cache
                pricesStream.Close();
                if (isolatedStorage.FileExists(PricesCacheFileName))
                {
                    pricesStream = isolatedStorage.OpenFile(PricesCacheFileName, FileMode.Open, FileAccess.Read);
                }
                else
                {
                    pricesStream = null;
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        MessageBox.Show("Unable to retrieve prices.");
                    });

                }
            }
            return pricesStream;
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
