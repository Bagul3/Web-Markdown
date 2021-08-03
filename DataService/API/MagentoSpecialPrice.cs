using Common;
using Cordners.Model;
using DataService.Helper;
using DataService.Models;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Cordners.Api
{
    public class MagentoSpecialPrice
    {
        private const string URL = "https://cordners.co.uk/";

        public bool UpdateSpecialPrice(SpecialPrice[] prices)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                //HttpClient client = new HttpClient(new LoggingHandler(new HttpClientHandler()));
                HttpClient client = new HttpClient();
                SpecialPriceAPIRequest requestContent = new SpecialPriceAPIRequest();
                requestContent.prices = prices;
                //Prod  m4beidhfgv0bizp6g5v6qso6uj7uqid1
                //Staging dmgye318kdfqqohcb3wf9uv8cumtvkcz
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "m4beidhfgv0bizp6g5v6qso6uj7uqid1");
                client.BaseAddress = new Uri(URL);
                var response = client.PostAsJsonAsync("/rest/default/V1/products/special-price-delete", requestContent).Result;
                if (response.IsSuccessStatusCode)
                {
                    response = client.PostAsJsonAsync("rest/default/V1/products/special-price", requestContent).Result;
                    if (response.IsSuccessStatusCode)
                        return true;
                }

                new LogWriter().LogWrite(response.ReasonPhrase);
                new LogWriter().LogWrite(response.Content.ReadAsStringAsync().Result);
                new LogWriter().LogWrite(response.StatusCode.ToString());
            }
            catch(Exception ex)
            {
                new LogWriter().LogWrite(ex.Message);
                new LogWriter().LogWrite(ex.StackTrace);
            }            
            return false;
        }
    }
}
