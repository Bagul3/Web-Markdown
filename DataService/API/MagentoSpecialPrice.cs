using Common;
using Common.Model;
using Cordners.Model;
using DataRepo;
using DataService.Helper;
using DataService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Cordners.Api
{
    public class MagentoSpecialPrice
    {
        private const string URL = "https://staging.cordners.co.uk/";

        private SkuRepository skuRepository;

        public MagentoSpecialPrice()
        {
            skuRepository = new SkuRepository();
        }

        public bool UpdateSpecialPrice(SpecialPrice[] prices)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                //HttpClient client = new HttpClient(new LoggingHandler(new HttpClientHandler()));
                HttpClient client = new HttpClient();
                
                //Prod  pj1c7qo2ce6301lassshrdnyme0ffl69
                //Staging dmgye318kdfqqohcb3wf9uv8cumtvkcz
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "dmgye318kdfqqohcb3wf9uv8cumtvkcz");
                client.BaseAddress = new Uri(URL);                

                if(prices.Length > 100)
                {
                    for(int i = 0; i < prices.Length; i++)
                    {
                        SpecialPriceAPIRequest requestContent = new SpecialPriceAPIRequest();
                        requestContent.prices = prices.Skip(i).Take((i+13)).ToArray();
                        client.PostAsJsonAsync("rest/default/V1/products/special-price", requestContent);
                        i = i + 13;
                    }                    
                }
                else
                {
                    SpecialPriceAPIRequest requestContent = new SpecialPriceAPIRequest();
                    requestContent.prices = prices;
                    client.PostAsJsonAsync("rest/default/V1/products/special-price", requestContent);
                }

                //if (response.IsSuccessStatusCode)
                //{
                //    return true;
                //}
                //new LogWriter().LogWrite("rest/default/V1/products/special-price");
                //new LogWriter().LogWrite(Newtonsoft.Json.JsonConvert.SerializeObject(requestContent));


                //new LogWriter().LogWrite(response.ReasonPhrase);
                //new LogWriter().LogWrite(response.Content.ReadAsStringAsync().Result);
                //new LogWriter().LogWrite(response.StatusCode.ToString());
                return true;
            }
            catch(Exception ex)
            {
                new LogWriter().LogWrite(ex.Message);
                new LogWriter().LogWrite(ex.StackTrace);
            }            
            return false;
        }

        public bool DeleteSpecialPrice(SpecialPrice[] prices)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                //HttpClient client = new HttpClient(new LoggingHandler(new HttpClientHandler()));
                HttpClient client = new HttpClient();
                SpecialPriceAPIRequest requestContent = new SpecialPriceAPIRequest();
                requestContent.prices = prices;
                //Prod  pj1c7qo2ce6301lassshrdnyme0ffl69
                //Staging dmgye318kdfqqohcb3wf9uv8cumtvkcz
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "dmgye318kdfqqohcb3wf9uv8cumtvkcz");
                client.BaseAddress = new Uri(URL);

                client.PostAsJsonAsync("/rest/default/V1/products/special-price-delete", requestContent);
                var skus = new List<DeleteSKU>();
                foreach (var price in prices)
                {
                    skus.Add(new DeleteSKU(){
                        Sku = price.sku.Substring(0, 9),
                        StoreId = price.store_id
                    });
                }
                skus = skus.Distinct().ToList();
                skuRepository.RetrieveQuery(skus.ToArray(), SqlQueries.DeleteSales);
                return true;
            }
            catch(Exception ex)
            {
                new LogWriter().LogWrite(ex.Message);
                new LogWriter().LogWrite(ex.StackTrace);
                return false;
            }
            
        }
    }
}
