using Common;
using Cordners.Model;
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
                //Staging j3yd488tm6ldgb1a7ranb5pu659zndj3
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "pj1c7qo2ce6301lassshrdnyme0ffl69");
                client.BaseAddress = new Uri(URL);
                var response = client.PostAsJsonAsync("rest/default/V1/products/special-price", requestContent).Result;
                if (response.IsSuccessStatusCode)
                    return true;                
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
