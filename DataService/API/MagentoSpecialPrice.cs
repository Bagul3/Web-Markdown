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
using System.Threading.Tasks;

namespace Cordners.Api
{
    public class MagentoSpecialPrice
    {
        private const string URL = "https://cordners.co.uk/";

        //Prod  pj1c7qo2ce6301lassshrdnyme0ffl69
        //Staging dmgye318kdfqqohcb3wf9uv8cumtvkcz
        private SkuRepository skuRepository;
        private string APIKEY = "pj1c7qo2ce6301lassshrdnyme0ffl69";
        public MagentoSpecialPrice()
        {
            skuRepository = new SkuRepository();
        }

        public async Task<bool> UpdateSpecialPrice(SpecialPrice[] prices)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                //HttpClient client = new HttpClient(new LoggingHandler(new HttpClientHandler()));
                HttpClient client = new HttpClient();
                
                
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", APIKEY);
                client.BaseAddress = new Uri(URL);

                SpecialPriceAPIRequest requestContent = new SpecialPriceAPIRequest();
                requestContent.prices = prices;
                new LogWriter().LogWrite("rest/default/V1/products/special-price");
                new LogWriter().LogWrite(Newtonsoft.Json.JsonConvert.SerializeObject(requestContent));
                var response = await client.PostAsJsonAsync("rest/default/V1/products/special-price", requestContent);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                new LogWriter().LogWrite("rest/default/V1/products/special-price");


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

        public async Task<bool> DeleteSpecialPrice(string sku, int storeId)
        {
            try
            {
                var repo = new OnSaleRepository();
                var onSale = repo.GetBySku(sku, storeId);
                if (onSale == null)
                    return true;
                onSale.End = onSale.End.Trim();
                onSale.Start = onSale.Start.Trim();
                onSale.Sku = onSale.Sku.Trim();
                onSale.Price = onSale.Price.Trim();
                
                var prices = MapToSpecialPrice(onSale);
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                //HttpClient client = new HttpClient(new LoggingHandler(new HttpClientHandler()));
                HttpClient client = new HttpClient();
                SpecialPriceAPIRequest requestContent = new SpecialPriceAPIRequest();
                requestContent.prices = prices;
                new LogWriter().LogWrite("rest/default/V1/products/special-price-delete");
                new LogWriter().LogWrite(Newtonsoft.Json.JsonConvert.SerializeObject(requestContent));
                //Prod  pj1c7qo2ce6301lassshrdnyme0ffl69
                //Staging dmgye318kdfqqohcb3wf9uv8cumtvkcz
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", APIKEY);
                client.BaseAddress = new Uri(URL);

                var result = await client.PostAsJsonAsync("/rest/default/V1/products/special-price-delete", requestContent);
                repo.Delete(onSale);
                repo.Save();
                return true;
            }
            catch (Exception ex)
            {
                new LogWriter().LogWrite(ex.Message);
                new LogWriter().LogWrite(ex.StackTrace);
                return false;
            }

        }

        private SpecialPrice[] MapToSpecialPrice(OnSale onSale)
        {
            var specialPrice = new List<SpecialPrice>();
            for(int i = 1; i < 14;i++)
            {
                var size = "";
                if (i < 10)
                {
                    size += "00" + i;
                }
                else
                {
                    size += "0" + i;
                }

                specialPrice.Add(new SpecialPrice()
                {
                    price = Convert.ToDecimal(onSale.Price),
                    price_from = onSale.End,
                    price_to = onSale.Start,
                    sku = onSale.Sku + size,
                    store_id = onSale.StoreId.Value
                });
            }
            return specialPrice.ToArray();
        }
    }
}
