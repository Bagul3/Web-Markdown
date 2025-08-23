using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Common;
using Common.Model;
using Cordners.Model;
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

        public List<SpecialPrice> GetItemsOnSale()
        {
            var data = new OnSaleRepository().GetAll().ToList();
            return ParseSalesData(data);
        }

        private List<SpecialPrice> ParseSalesData(List<OnSale> data)
        {
            var specialPrice = new List<SpecialPrice>();
            foreach (OnSale onSale in data)
            {
                if (Convert.ToDateTime(onSale.End) < DateTime.Now)
                {
                    var repo = new OnSaleRepository();
                    repo.Delete(onSale);
                    repo.Save();
                }
                else
                {
                    specialPrice.Add(new SpecialPrice()
                    {
                        price = Convert.ToDecimal(onSale.Price),
                        price_from = onSale.Start,
                        price_to = onSale.End,
                        sku = onSale.Sku,
                        store_id = Convert.ToInt32(onSale.StoreId)
                    });
                }
            }
            return specialPrice.Distinct().ToList();
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

        public List<string> OnlineSKUsAll()
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
                    if (item.Contains('\n') && item.Split('\n')[0].Length > 11)
                    {
                        var sku = item.Split('\n')[0].Substring(0, 12);
                        splitted.Add(sku);
                        skus.Add(sku);
                    }
                    else if (item.Contains('\n') && item.Split('\n')[0].Length > 8)
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

        public List<string> OnlineSKUWithColour()
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
                    if (item.Contains('\n') && item.Split('\n')[0].Length > 9)
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

        public List<SpecailOrders> GetOnlineSKuValuesWithColour(List<string> SKU)
        {
            var mongeto = new List<SpecailOrders>();
            var data = _skuRepository.RetrieveQueryAsync(SqlQueries.StockQueryALL).Result;
            for (var i = 0; i < SKU.Count; i++)
            {
                var rows = data.Tables[0].Select($"NEWSTYLE = {SKU[i]}");
                foreach (DataRow reff in rows)
                {
                    this.BuildMongetoObj(ref mongeto, reff);
                }
            }

            return mongeto;
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

        public List<SpecailOrders> GetOnlineSKuValuesWithColourAndSizeNoConfigs(List<string> SKU)
        {
            var mongeto = new List<SpecailOrders>();
            var processList = new List<String>();
            var data = _skuRepository.RetrieveQueryAsync(SqlQueries.StockQueryALL).Result;
            for ( var i = 0; i < SKU.Count; i++)
            {
                var rows = data.Tables[0].Select($"NEWSTYLE = {SKU[i].Substring(0,9)}");
                foreach (DataRow reff in rows)
                {
                    if (!processList.Contains(SKU[i].Substring(0, 9)))
                    {
                        processList.Add(SKU[i].Substring(0, 9));
                        this.GenerateSimples(ref mongeto, reff, SKU[i]);
                    }
                }
            }

            return mongeto;
        }


        public DataSet GetAllCordnersStock()
        {
            return _skuRepository.RetrieveQuery(SqlQueries.AllSkusQuery);
        }


        public DataSet GetImportProductsAllQuery()
        {
            return _skuRepository.RetrieveQuery(SqlQuery.ImportProductsAllQuery);
        }


        public List<Sales> GetSales(DataSet CordnersStock, List<SpecialPrice> specialPrices)
        {
            var sales = new List<Sales>();
            var euro = new SkuRepository().RetrieveQuery(SqlQueries.FetchEUROPrice);
            foreach(var specialPrice in specialPrices)
            {
                var stocks = CordnersStock.Tables[0].Select($"NEWSTYLE = {specialPrice.sku}");
                
                foreach(DataRow dr in stocks)
                {
                    string rrp;
                    if (specialPrice.store_id == 2)
                        rrp = GenerateEuroPrice(Convert.ToDecimal(dr["SELL"].ToString()), Convert.ToDecimal(euro.Tables[0].Rows[0]["PRICE"]));
                    else
                        rrp = dr["SELL"].ToString();

                    sales.Add(new Sales()
                    {
                        MasterSupplier = dr["MasterSupplier"].ToString().Trim(),
                        Color = dr["MasterColour"].ToString(),
                        Style = dr["STYPE"].ToString(),
                        StockType = dr["MasterStocktype"].ToString(),
                        Category = dr["MasterSubDept"].ToString(),
                        Season = dr["User1"].ToString(),
                        Name = dr["SHORT"].ToString(),                       
                        Sales_Price = specialPrice.price,
                        Price_From = specialPrice.price_from.Trim(),
                        Price_To = specialPrice.price_to.Trim(),
                        SKU = specialPrice.sku.Trim(),
                        RRP = rrp,
                        Discount = (int)Math.Floor((Convert.ToDecimal(rrp) -specialPrice.price) / Convert.ToDecimal(rrp)*100),
                        Store = Convert.ToInt16(specialPrice.store_id) == 1 ? "UK" : "Ireland"
                    });
                }                
            }
            return sales;
        }

        private string GenerateEuroPrice(decimal gbp, decimal conversion_rate)
        {
            var rounding = true;
            var euros = gbp * conversion_rate;
            var decimalPart = euros - Math.Truncate(euros);
            if ((decimalPart * 100) < 50)
            {
                if (gbp < 20)
                {
                    var additional = 0.5m - decimalPart;
                    euros += additional;
                    rounding = false;
                }
                else
                    euros++;

            }
            return rounding ? Math.Round(euros).ToString() : euros.ToString();
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
                return stock.Where(p => onlineStock.All(p2 => p2.SKU != p.SKU)).ToList();
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
                    SKU = dr["NEWSTYLE"].ToString(),
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

        private void BuildMongetoObjFullSKU(ref List<SpecailOrders> skuRecords, DataRow dr)
        {
            skuRecords.Add(
                new SpecailOrders()
                {
                    SKU = dr["NEWSTYLE"].ToString(),
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

        private void BuildMongetoObjSimple(ref List<SpecailOrders> skuRecords, DataRow dr, string sku)
        {
            skuRecords.Add(
                new SpecailOrders()
                {
                    SKU = sku,
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

        public void GenerateSimples(ref List<SpecailOrders> skuRecords, DataRow dr, string sku)
        {
            try
            {
                var actualStock = "0";

                for (var i = Convert.ToInt32(dr["MINSIZE"]); i <= Convert.ToInt32(dr["MAXSIZE"]); i++)
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
                            }

                            skuRecords.Add(
                                new SpecailOrders()
                                {
                                    SKU = dr["NEWSTYLE"] + size(i),
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
                                    Qty = actualStock
                                }
                               );
                            actualStock = "0";
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private string size(int i)
        {
            if (i < 10)
                return "00" + i;
            return "0" + i;
        }

    }
}
