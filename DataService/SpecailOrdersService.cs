using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Common;
using DataRepo;

namespace DataService
{
    public class SpecailOrdersService
    {
        private readonly SkuRepository _skuRepository;

        public SpecailOrdersService()
        {
            _skuRepository = new SkuRepository();
        }

        public void GenerateCSVAsync(SpecailOrders specailOrders, string startDate, string endDate, int stamp, double adjustmentPrice = 0, int adjustmentPercentage = 0)
        {
            var csv = new StringBuilder();
           
            var items = _skuRepository.RetrieveQuery(specailOrders.Ref, SqlQueries.SingleSkuSmaller);
            var specailsOrders = new List<SpecailOrders>();

            foreach (DataRow item in items.Tables[0].Rows)
            {
                BuildStockObject(ref specailsOrders, item);
            }
            
            foreach (var o in specailsOrders)
            {
                var price = 0.0;
                var sellPrice = Convert.ToDouble(o.SellB);
                var actualPrice = 0.0;

                if (adjustmentPercentage != 0)
                {
                    actualPrice = adjustmentPercentage > 0
                        ? Convert.ToDouble(o.Sell) - (Convert.ToDouble(Convert.ToDouble(adjustmentPercentage) / 100) * Convert.ToDouble(o.Sell))
                        : Convert.ToDouble(o.Sell);
                }
                else
                {
                    actualPrice = adjustmentPrice > 0 ? Math.Ceiling(sellPrice - adjustmentPrice) : Math.Ceiling(sellPrice);
                }

                if (actualPrice.ToString().Split('.').Length > 2 && Convert.ToInt16(actualPrice.ToString().Split('.')[1]) > 0)
                {
                    var floored = Convert.ToInt16(sellPrice.ToString().Split('.')[1]);
                    double roundup = (100 - floored) / 100;
                    actualPrice += roundup;
                }
                
                var newLine =  $"{"\"" + (o).Ref + "\""},{"\"" + Math.Round(actualPrice) + "\""},{"\"" + (Convert.ToDateTime(startDate).ToString("yyyy/MM/dd") ?? "") + "\""},{"\"" + (Convert.ToDateTime(endDate).ToString("yyyy/MM/dd") ?? "") + "\""}";
                csv.AppendLine(newLine);
            }
                
            File.AppendAllText(System.Configuration.ConfigurationManager.AppSettings["SalesPriceOutput"] + stamp + ".csv", csv.ToString());
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

        public List<SpecailOrders> GetSaleStock(Dictionary<string,object> suppliers, Dictionary<string, object> categorys, Dictionary<string, object> seasons, Dictionary<string, object> styles, Dictionary<string, object> stockTypes)
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
                skuRecords.Add(new SpecailOrders()
                {
                    Ref = dr["Ref"].ToString(),
                    Sell = dr["Sell"].ToString(),
                    SellB = dr["SellB"].ToString(),
                    MasterSupplier = dr["MasterSupplier"].ToString(),
                    Color = dr["MasterColour"].ToString(),
                    Style = dr["STYPE"].ToString(),
                    StockType = dr["MasterStocktype"].ToString(),
                    Category = dr["MasterSubDept"].ToString(),
                    Season = dr["User1"].ToString()
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
                                SellB = dr["SellB"].ToString()
                            });
                        }
                    }
                }

                if (!string.IsNullOrEmpty(dr["NewStyle"].ToString()))
                {
                    skuRecords.Add(new SpecailOrders()
                    {
                        Ref = dr["NewStyle"].ToString(),
                        SellB = dr["SellB"].ToString(),
                        Sell = dr["Sell"].ToString()
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
