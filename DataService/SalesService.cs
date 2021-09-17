using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Common;
using DataRepo;
using System.Windows;
using Cordners.Api;
using Cordners.Model;
using System.Linq;

namespace DataService
{
    public class SalesService
    {
        private readonly SkuRepository _skuRepository;
        public SalesService()
        {
            _skuRepository = new SkuRepository();
        }

        public DataSet RetrieveAllSkuData()
        {
            try
            {
                return _skuRepository.RetrieveQuery(SqlQueries.AllSKUData);
            }
            catch (Exception e)
            {
                new LogWriter(e.Message);
                new LogWriter(e.StackTrace);
                MessageBox.Show(e.Message);
            }
            return new DataSet();
        }

        public decimal GetEuroPrice()
        {
            try
            {
                var dr = _skuRepository.RetrieveQuery(SqlQueries.FetchEUROPrice);
                var euro_price = Convert.ToDecimal(dr.Tables[0].Rows[0]["Price"].ToString());
                return euro_price;
            }
            catch (Exception e)
            {
                new LogWriter(e.Message);
                new LogWriter(e.StackTrace);
                MessageBox.Show(e.Message);
                throw e;
            }            
        }

        public SpecialPrice BuildSpecialPriceObj(string sku, string sell, int storeId, string start, string end, decimal adjustmentPrice, int adjustmentPercentage)
        {
            var salesService = new SalesService();
            var price = salesService.GenerateSalesPrice(adjustmentPrice, adjustmentPercentage, sell);

            return new SpecialPrice()
            {
                sku = sku,
                store_id = storeId,
                price = price,
                price_from = start,
                price_to = end
            };
        }

        public List<SpecialPrice> DeleteSKU(string sku, int store)
        {
            var prices = new List<SpecialPrice>();
            var doesExist = new OnSaleRepository().GetBySku(sku, store);
            if (doesExist == null)
                return new List<SpecialPrice>();
            for (int i = 1; i <= 13; i++)
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
                prices.Add(new SpecialPrice()
                {
                    price = Convert.ToDecimal(doesExist.Price),
                    price_from = doesExist.Start.ToString(),
                    price_to = doesExist.End.ToString(),
                    sku = (doesExist.Sku + size).ToString(),
                    store_id = Convert.ToInt32(doesExist.StoreId)
                });                    
            }
            return prices;
        }


        public void GenerateCSVAsync(string startDate, string endDate, int stamp, List<DataRow> dataRows, decimal adjustmentPrice = 0, int adjustmentPercentage = 0, decimal euro = 0)
        {
            try
            {
                var csv = new StringBuilder();                
                var specailsOrders = new List<SpecailOrders>();
                decimal actualPrice = 0;

                foreach (DataRow item in dataRows)
                {
                    BuildStockObject(ref specailsOrders, item);
                }
                var count = 0;
                foreach (var o in specailsOrders)
                {
                    if (count == (specailsOrders.Count - 1))
                        break;
                    
                    actualPrice = 0;
                    var newLine = "";

                    if (euro != 0)
                    {
                        actualPrice = GenerateSalesPrice(adjustmentPrice, adjustmentPercentage, GenerateEuroPrice(Convert.ToDecimal(o.Sell), euro));
                        newLine = $"{"\"" + (o).Ref + "\""},{"\"" + Convert.ToInt16(actualPrice) + "\""},{"\"" + (Convert.ToDateTime(startDate).ToString("yyyy/MM/dd") ?? "") + "\""},{"\"" + (Convert.ToDateTime(endDate).ToString("yyyy/MM/dd") ?? "") + "\""},{"\" \""},{"\" \""},{"\" \""},{"\"" + GenerateEuroPrice(Convert.ToDecimal(o.RRP), euro).ToString() + "\""}";                      
                    }
                    else
                    {
                        actualPrice = GenerateSalesPrice(adjustmentPrice, adjustmentPercentage, o.Sell);
                        newLine = $"{"\"" + (o).Ref + "\""},{"\"" + Convert.ToInt16(actualPrice) + "\""},{"\"" + (Convert.ToDateTime(startDate).ToString("yyyy/MM/dd") ?? "") + "\""},{"\"" + (Convert.ToDateTime(endDate).ToString("yyyy/MM/dd") ?? "") + "\""},{"\" \""},{"\" \""},{"\" \""},{"\"" + o.RRP + "\""}";                        
                    }
                    
                    csv.AppendLine(newLine);
                    count++;
                }
                if (euro != 0)
                {
                    File.AppendAllText(ConfigurationManager.AppSettings["SalesPriceOutput"] + stamp + "-euro.csv", csv.ToString());
                }
                else
                {
                    File.AppendAllText(ConfigurationManager.AppSettings["SalesPriceOutput"] + stamp + ".csv", csv.ToString());
                }
                var repo = new OnSaleRepository();

                //TODO Delete here
                if (euro != 0)
                {
                    var onSale = new OnSale()
                    {
                        StoreId = 2,
                        Sku = specailsOrders.First().Ref.Substring(0, 9),
                        Price = Convert.ToDecimal(actualPrice.ToString()).ToString(),
                        Start = startDate,
                        End = endDate
                    };
                    new MagentoSpecialPrice().DeleteSpecialPrice(specailsOrders.First().Ref.Substring(0, 9), 2);
                    repo.Insert(onSale);
                }
                else
                {
                    var onSale = new OnSale()
                    {
                        StoreId = 1,
                        Sku = specailsOrders.First().Ref.Substring(0, 9),
                        Price = Convert.ToDecimal(actualPrice.ToString()).ToString(),
                        Start = startDate,
                        End = endDate
                    };
                    new MagentoSpecialPrice().DeleteSpecialPrice(specailsOrders.First().Ref.Substring(0, 9), 1);
                    repo.Insert(onSale);
                }                
                repo.Save();
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        public string GenerateEuroPrice(decimal gbp, decimal conversion_rate)
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
            return rounding? Math.Round(euros).ToString() : euros.ToString();
        }

        public decimal GenerateSalesPrice(decimal adjustmentPrice, int adjustmentPercentage, string sell)
        {
            var sellPrice = Convert.ToDouble(sell);
            decimal actualPrice;
            if (adjustmentPercentage != 0)
            {
                actualPrice = Convert.ToDecimal(string.Format("{0:F2}", Convert.ToDecimal(sell) - (Convert.ToDecimal(Convert.ToDecimal(adjustmentPercentage) / 100) * Convert.ToDecimal(sell))));
            }
            else
            {
                actualPrice = adjustmentPrice > 0 ? Convert.ToDecimal(Math.Ceiling(Convert.ToDecimal(sell) - adjustmentPrice)) : Convert.ToDecimal(Math.Ceiling(sellPrice));
            }

            var decimalPart = actualPrice - Math.Truncate(actualPrice);

            if (((decimalPart * 100) < 50) && (decimalPart != 0))
            {
                actualPrice += 1.00m;
            }

            return Math.Round(actualPrice);
        }

        public List<SpecailOrders> GetCordners()
        {
            var data = ConfigurationManager.AppSettings["AllBranch"] == "False" ? _skuRepository.RetrieveQuery(SqlQueries.GetItemList) : _skuRepository.RetrieveQuery(SqlQueries.GetItemListAllBranches);
            var skuRecords = new List<SpecailOrders>();
            var counter = 0;
            foreach (DataRow dr in data.Tables[0].Rows)
            {
                counter++;
                this.DoJob(ref skuRecords, dr);
            }

            return skuRecords;
        }

        public List<SpecailOrders> GetSaleStock(Dictionary<string,object> suppliers, Dictionary<string, object> categorys, Dictionary<string, object> seasons, Dictionary<string, object> styles, Dictionary<string, object> stockTypes, Dictionary<string, object> colours)
        {
            var queryBuilder = new StringBuilder();
            var stockList = new List<SpecailOrders>();
            queryBuilder.Append(SqlQueries.CoreStockQuery);

            var isFirst = true;

            if (suppliers != null && suppliers.Count != 0)
            {
                queryBuilder.Append(" AND(");
                foreach (var supplier in suppliers)
                {
                    if (isFirst)
                    {
                        queryBuilder.Append(" MasterSupplier = '" + supplier.Value + "' ");
                        isFirst = false;
                    }
                    else
                    {
                        queryBuilder.Append(" OR MasterSupplier = '" + supplier.Value + "' ");
                    }
                    
                }
                queryBuilder.Append(") ");
            }
            if (seasons != null && seasons.Count != 0)
            {
                isFirst = true;
                queryBuilder.Append(" AND(");
                foreach (var season in seasons)
                {
                    if (isFirst)
                    {
                        queryBuilder.Append(" User1 = '" + season.Value + "' ");
                        isFirst = false;
                    }
                    else
                    {
                        queryBuilder.Append(" OR User1 = '" + season.Value + "' ");
                    }

                }
                queryBuilder.Append(") ");
            }
            if (categorys != null && categorys.Count != 0)
            {
                isFirst = true;
                queryBuilder.Append(" AND(");
                foreach (var category in categorys)
                {
                    if (isFirst)
                    {
                        queryBuilder.Append(" MasterSubDept = '" + category.Value + "' ");
                        isFirst = false;
                    }
                    else
                    {
                        queryBuilder.Append(" OR MasterSubDept = '" + category.Value + "' ");
                    }

                }
                queryBuilder.Append(") ");
            }
            if (stockTypes != null && stockTypes.Count != 0)
            {
                isFirst = true;
                queryBuilder.Append(" AND(");
                foreach (var stockType in stockTypes)
                {
                    if (isFirst)
                    {
                        queryBuilder.Append(" MasterStocktype = '" + stockType.Value + "' ");
                        isFirst = false;
                    }
                    else
                    {
                        queryBuilder.Append(" OR MasterStocktype = '" + stockType.Value + "' ");
                    }

                }
                queryBuilder.Append(") ");
            }
            if (styles != null && styles.Count != 0)
            {
                isFirst = true;
                queryBuilder.Append(" AND(");
                foreach (var style in styles)
                {
                    if (isFirst)
                    {
                        queryBuilder.Append(" STYPE = '" + style.Value + "' ");
                        isFirst = false;
                    }
                    else
                    {
                        queryBuilder.Append(" OR STYPE = '" + style.Value + "' ");
                    }

                }
                queryBuilder.Append(") ");
            }
            if (colours != null && colours.Count != 0)
            {
                isFirst = true;
                queryBuilder.Append(" AND(");
                foreach (var colour in colours)
                {
                    if (isFirst)
                    {
                        queryBuilder.Append(" Colour.MasterColour = '" + colour.Value + "' ");
                        isFirst = false;
                    }
                    else
                    {
                        queryBuilder.Append(" OR Colour.MasterColour = '" + colour.Value + "' ");
                    }

                }
                queryBuilder.Append(") ");
            }

            queryBuilder.Append(SqlQueries.CoreStockQueryOrder);

            var result = _skuRepository.RetrieveQuery(queryBuilder.ToString());

            foreach (DataRow row in result.Tables[0].Rows)
            {
                DoJob(ref stockList, row);
            }

            return stockList;
        }

        private void DoJob(ref List<SpecailOrders> skuRecords, DataRow dr)
        {
            try
            {

                //Check if season isn't before 2015
                var season = dr["User1"].ToString();
                if (season.Contains("S") || season.Contains("W"))
                {
                    var year = season.Substring(1, 2);
                    if (Convert.ToInt32(year) <= 15)
                        return;
                }


                skuRecords.Add(new SpecailOrders()
                {
                    Ref = dr["Ref"].ToString(),
                    Sell = dr["Sell"].ToString(),
                    MasterSupplier = dr["MasterSupplier"].ToString(),
                    Color = dr["MasterColour"].ToString(),
                    Style = dr["STYPE"].ToString(),
                    StockType = dr["MasterStocktype"].ToString(),                    
                    Season = dr["User1"].ToString(),
                    Name = dr["Short"].ToString(),
                    SupRef = dr["SUPPREF"].ToString(),
                    RRP = dr["BASESELL"].ToString(),
                    Category = dr["MasterSubDept"].ToString(),
                    NEWSTYLE = dr["NEWSTYLE"].ToString()
                });
                        
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private void BuildStockObject(ref List<SpecailOrders> skuRecords, DataRow dr)
        {
            try
            {
                for (var i = 1; i < 14; i++)
                {
                   var size = i < 10 ? dr["S0" + i].ToString() : dr["S" + i].ToString();

                    if (!string.IsNullOrEmpty(size))
                    {
                        var append = (1000 + i).ToString();
                        var newStyle = dr["NewStyle"] + append.Substring(1, 3);
                            

                        if (!string.IsNullOrEmpty(size))
                        {
                            skuRecords.Add(new SpecailOrders()
                            {
                                Ref = newStyle,
                                Sell = dr["Sell"].ToString(),
                                RRP = dr["BASESELL"].ToString()
                            });
                        }
                    }
                }

                if (!string.IsNullOrEmpty(dr["NewStyle"].ToString()))
                {
                    skuRecords.Add(new SpecailOrders()
                    {
                        Ref = dr["NewStyle"].ToString(),
                        Sell = dr["Sell"].ToString(),  
                        RRP = dr["BASESELL"].ToString()
                    });
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
