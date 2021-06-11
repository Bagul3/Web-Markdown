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

        public void GenerateCSVAsync(SpecailOrders specailOrders, string startDate, string endDate, int stamp, List<DataRow> dataRows, decimal adjustmentPrice = 0, int adjustmentPercentage = 0)
        {
            try
            {
                var csv = new StringBuilder();
                //var items = _skuRepository.RetrieveQuery(specailOrders.Ref, SqlQueries.SingleSkuSmaller);
                var specailsOrders = new List<SpecailOrders>();

                foreach (DataRow item in dataRows)
                {
                    BuildStockObject(ref specailsOrders, item);
                }
                var count = 0;
                foreach (var o in specailsOrders)
                {
                    if (count == (specailsOrders.Count-1))
                        break;
                    var sellPrice = Convert.ToDouble(o.Sell);
                    var actualPrice = 0.0m;
                    if (adjustmentPercentage != 0)
                    {                        
                        actualPrice = Convert.ToDecimal(string.Format("{0:F2}", Convert.ToDecimal(o.Sell) - (Convert.ToDecimal(Convert.ToDecimal(adjustmentPercentage) / 100) * Convert.ToDecimal(o.Sell))));                        
                    }
                    else
                    {
                        actualPrice = adjustmentPrice > 0 ? Convert.ToDecimal(Math.Ceiling(Convert.ToDecimal(o.Sell) - adjustmentPrice)) : Convert.ToDecimal(Math.Ceiling(sellPrice));
                    }
                    
                    if (actualPrice.ToString().Contains(".") && Convert.ToInt16(actualPrice.ToString().Split('.')[1]) > 0 && 
                        Convert.ToInt16(actualPrice.ToString().Split('.')[1]) <= 49)
                    {
                        actualPrice++;
                    }

                    var newLine = $"{"\"" + (o).Ref + "\""},{"\"" + Convert.ToInt16(actualPrice) + "\""},{"\"" + "\""},{"\"" + (Convert.ToDateTime(startDate).ToString("yyyy/MM/dd") ?? "") + "\""},{"\"" + "\""},{"\"" + (Convert.ToDateTime(endDate).ToString("yyyy/MM/dd") ?? "") + "\""},{"\"" + "\""},{"\"" + (o).RRP + "\""}";
                    csv.AppendLine(newLine);
                    count++;
                }

                File.AppendAllText(System.Configuration.ConfigurationManager.AppSettings["SalesPriceOutput"] + stamp + ".csv", csv.ToString());
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message);
            }
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
