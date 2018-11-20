using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Common;
using DataRepo;

namespace DataService
{
    public class SkuService
    {
        public string GetCSV(string url)
        {
            var req = (HttpWebRequest)WebRequest.Create(url);
            var resp = (HttpWebResponse)req.GetResponse();

            var sr = new StreamReader(resp.GetResponseStream());
            var results = sr.ReadToEnd();
            sr.Close();

            return results;
        }

        private readonly SkuRepository _skuRepository;

        public SkuService()
        {
            _skuRepository = new SkuRepository();
        }

        public void DeleteDescriptions()
        {
            _skuRepository.RetrieveQuery(SqlQueries.DeleteSKUs);
        }

        public bool InsertIntoDescriptions(List<string> sku)
        {
            _skuRepository.RetrieveQuery(sku, SqlQueries.InsertSKU);
            return true;
        }

        public async Task<List<SpecailOrders>> GetOnlineStock()
        {
            var data = await _skuRepository.RetrieveQueryAsync(SqlQueries.OnlineStock);
            var mongeto = new List<SpecailOrders>();
            for (var i = 0; i < data.Tables[0].Rows.Count; i++)
            {
                this.BuildMongetoObj(ref mongeto, data.Tables[0].Rows[i]);
            }
            return mongeto;
        }

        public async Task<List<SpecailOrders>> GetStock()
        {
            var data = await _skuRepository.RetrieveQueryAsync(SqlQueries.StockQuery);
            var mongeto = new List<SpecailOrders>();
            for (var i = 0; i < data.Tables[0].Rows.Count; i++)
            {
                this.BuildMongetoObj(ref mongeto, data.Tables[0].Rows[i]);
            }
            return mongeto;
        }

        public List<SpecailOrders> GetStockSync()
        {
            var data = _skuRepository.RetrieveQuery(SqlQueries.StockQuery);
            var mongeto = new List<SpecailOrders>();
            for (var i = 0; i < data.Tables[0].Rows.Count; i++)
            {
                this.BuildMongetoObj(ref mongeto, data.Tables[0].Rows[i]);
            }
            return mongeto;
        }

        public List<SpecailOrders> GetOnlineStockSync()
        {
            var data = _skuRepository.RetrieveQuery(SqlQueries.OnlineStock);
            var mongeto = new List<SpecailOrders>();
            for (var i = 0; i < data.Tables[0].Rows.Count; i++)
            {
                this.BuildMongetoObj(ref mongeto, data.Tables[0].Rows[i]);
            }
            return mongeto;
        }

        public async Task<List<SpecailOrders>> GetMissingStock(List<SpecailOrders> onlineStock, List<SpecailOrders> stock)
        {
            return await Task.Run(() =>
            {
                return stock.Where(p => onlineStock.All(p2 => p2.Ref != p.Ref)).ToList();
            });
        }

        public List<SpecailOrders> GetMissingStockSync(List<SpecailOrders> onlineStock, List<SpecailOrders> stock)
        {
            return stock.Where(p => onlineStock.All(p2 => p2.Ref != p.Ref)).ToList();
        }

        private void BuildMongetoObj(ref List<SpecailOrders> skuRecords, DataRow dr)
        {
            skuRecords.Add(
                new SpecailOrders()
                {
                    Ref = dr["Ref"].ToString(),
                    Sell = dr["Sell"].ToString(),
                    SellB = dr["SellB"].ToString(),
                    MasterSupplier = dr["MasterSupplier"].ToString(),
                    Color = dr["MasterColour"].ToString(),
                    Style = dr["STYPE"].ToString(),
                    StockType = dr["MasterStocktype"].ToString(),
                    Category = dr["MasterSubDept"].ToString(),
                    Season = dr["User1"].ToString(),
                    SupRef = dr["SUPPREF"].ToString(),
                    Desc = dr["T2_HEAD.DESC"].ToString(),
                    Name = dr["SHORT"].ToString()
                }
                );
        }
    }
}
