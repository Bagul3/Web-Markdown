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
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
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

        public List<string> OnlineSKUs()
        {
            var fileList = new SkuService().GetCSV("https://www.cordners.co.uk/exportcsv/");
            string[] tempStr;
            var splitted = new List<string>();
            tempStr = fileList.Split('\t');
            var skus = new List<string>();
            foreach (var item in tempStr)
            {
                if (!string.IsNullOrEmpty(item))
                {
                    if (item.Contains('\n') && item.Split('\n')[0].Length > 6)
                    {
                        var sku = item.Split('\n')[0].Substring(0, 6);
                        if (!skus.Contains(sku))
                        {
                            splitted.Add(sku);
                            skus.Add(sku);
                        }
                    }

                }
            }
            return skus;
        }

        public List<string> OnlineSKUSizes()
        {
            var fileList = new SkuService().GetCSV("https://www.cordners.co.uk/exportcsv/");
            string[] tempStr;
            var splitted = new List<string>();
            tempStr = fileList.Split('\t');
            var skus = new List<string>();
            foreach (var item in tempStr)
            {
                if (!string.IsNullOrEmpty(item))
                {
                    if (item.Contains('\n') && item.Split('\n')[0].Length > 8)
                    {
                        var sku = item.Split('\n')[0].Substring(0, 9);
                        if (!skus.Contains(sku))
                        {
                            splitted.Add(sku);
                            skus.Add(sku);
                        }
                    }

                }
            }
            return skus;
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

        public List<SpecailOrders> GetOnlineSKuValues(List<string> SKU)
        {
            var mongeto = new List<SpecailOrders>();
            var data = _skuRepository.RetrieveQueryAsync(SqlQueries.StockQueryALL).Result;
            for (var i = 0; i < SKU.Count; i++)
            {
                var rows = data.Tables[0].Select($"REF = {SKU[i]}");
                foreach (DataRow reff in rows)
                {
                    this.BuildMongetoObj(ref mongeto, reff);
                }
            }
            
            return mongeto;
        }

        public List<SizeRanges> GetOnlineSKuValuesWithSizes(List<string> SKU)
        {
            var mongeto = new List<SizeRanges>();
            var data = _skuRepository.RetrieveQueryAsync(SqlQueries.StockQueryALL).Result;
            for (var i = 0; i < SKU.Count; i++)
            {
                var rows = data.Tables[0].Select($"REF = {SKU[i]}");
                foreach (DataRow reff in rows)
                {
                    this.BuildSize(ref mongeto, reff);
                }
            }

            return mongeto;
        }

        public List<SizeRanges> GetOnlineSkuSize(List<string> SKU)
        {
            var mongeto = new List<SizeRanges>();
            var data = _skuRepository.RetrieveQueryAsync(SqlQueries.StockQueryALL).Result;
            for (var i = 0; i < SKU.Count; i++)
            {
                var rows = data.Tables[0].Select($"REF = {SKU[i]}");
                foreach (DataRow reff in rows)
                {
                    this.BuildSizeOnline(ref mongeto, SKU[i], reff);
                }
                
            }

            return mongeto;
        }

        public DataSet GetAllCordnersStock()
        {
            return _skuRepository.RetrieveQuery(SqlQueries.AllSkusQuery);
        }        

        public async Task<List<SpecailOrders>> GetStock()
        {
            var data = await _skuRepository.RetrieveQueryAsync(SqlQueries.StockQueryALL);
            var mongeto = new List<SpecailOrders>();
            for (var i = 0; i < data.Tables[0].Rows.Count; i++)
            {
                this.BuildMongetoObj(ref mongeto, data.Tables[0].Rows[i]);
            }
            return mongeto;
        }

        public async Task<List<SizeRanges>> GetStockWithSize()
        {
            var data = await _skuRepository.RetrieveQueryAsync(SqlQueries.StockQueryALL);
            var mongeto = new List<SizeRanges>();
            for (var i = 0; i < data.Tables[0].Rows.Count; i++)
            {
                this.BuildSize(ref mongeto, data.Tables[0].Rows[i]);
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

        public async Task<List<SizeRanges>> GetMissingSizes(List<SizeRanges> onlineStock, List<SizeRanges> stock)
        {
            return await Task.Run(() =>
            {
                return stock.Where(p => onlineStock.All(p2 => p2.SKUSize != p.SKUSize)).ToList();
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
                    NEWSTYLE = dr["NEWSTYLE"].ToString(),
                    Ref = dr["REF"].ToString(),
                    Sell = dr["Sell"].ToString(),
                    MasterSupplier = dr["MasterSupplier"].ToString(),
                    Color = dr["MasterColour"].ToString(),
                    Style = dr["STYPE"].ToString(),
                    StockType = dr["MasterStocktype"].ToString(),
                    Category = dr["MasterSubDept"].ToString(),
                    Season = dr["User1"].ToString(),
                    SupRef = dr["SUPPREF"].ToString(),
                    Desc = dr["DESC"].ToString(),
                    Name = dr["SHORT"].ToString(),
                    Qty = TotalStock(dr).ToString()
                }
               );
        }

        private void BuildSize(ref List<SizeRanges> sizeRanges, DataRow dr)
        {
            for (int i = 1; i < 14; i++)
            {
                if (!string.IsNullOrEmpty(dr["QTY" + i].ToString()))
                {
                    sizeRanges.Add(new SizeRanges()
                    {
                        NEWSTYLE = dr["NEWSTYLE"].ToString(),
                        Season = dr["User1"].ToString(),
                        SKUSize = dr["NEWSTYLE"].ToString() + (i < 10 ? dr["S0" + i].ToString() : dr["S" + i].ToString())
                    });
                }
            }
        }

        private void BuildSizeOnline(ref List<SizeRanges> sizeRanges, string sku, DataRow dr)
        {
            sizeRanges.Add(new SizeRanges()
            {
                NEWSTYLE = sku.Substring(0,6),
                Season = dr["User1"].ToString(),
                SKUSize = sku
            });
        }

        private int TotalStock(DataRow dr)
        {
            try
            {
                var actualStock = "";
                var totalStock = 0;

                for (var i = 1; i < 14; i++)
                {
                    if (!string.IsNullOrEmpty(dr["QTY" + i].ToString()))
                    {
                        if (dr["QTY" + i].ToString() != "")
                        {
                            if (Convert.ToInt32(dr["QTY" + i]) > 0)
                            {
                                if (dr["LY" + i].ToString() == "0" ||
                                    string.IsNullOrEmpty(dr["LY" + i].ToString()))
                                {
                                    actualStock = dr["QTY" + i].ToString();
                                }
                                else
                                {
                                    actualStock =
                                        (Convert.ToInt32(dr["QTY" + i]) - Convert.ToInt32(dr["LY" + i]))
                                        .ToString();
                                }

                                totalStock += Convert.ToInt32(actualStock);
                            }
                        }
                    }
                }
                return totalStock;
            }
            catch(Exception e)
            {
                throw e;
            }
        }
    }
}
