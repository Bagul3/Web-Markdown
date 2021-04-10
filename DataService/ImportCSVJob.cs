using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Common;
using DataRepo;

namespace ImportProducts.Services
{
    public class ImportCsvJob
    {
        private readonly LogWriter _logger;
        private readonly ExcelMapper _mapper;
        private readonly List<Descriptions> descriptions;
        private static DataSet REMTable;
        private string reffCode = "";
        private readonly DataSet data;
        private static List<string> sizes = new List<string>();

        public ImportCsvJob(string excelConnection)
        {
            this._logger = new LogWriter();
            this._mapper = new ExcelMapper();
            descriptions = _mapper.MapToDescriptions(excelConnection);
            REMTable = new SkuRepository().RetrieveQuery(SqlQuery.FetchREM);
            data = Query(SqlQuery.ImportProductsAllQuery);
        }

        public StringBuilder DoJob(string refff, IEnumerable<string> t2TreFs, ref ObservableCollection<Error> errors)
        {
            try
            {
                var csvLines = new StringBuilder();
                _logger.LogWrite("Generating stock.csv: This will take a few minutes, please wait....");

                var reff = refff.Substring(0, 6);
                reffCode = reff;               

                var actualStock = "0";
                var parentSKU = "";

                sizes = new List<string>();

                var skudata = data.Tables[0].Select($"REF = {reff}");

                foreach (DataRow dr in skudata)
                {
                    if (t2TreFs.Any(x => x.Contains(dr["NewStyle"].ToString())))
                    {
                        parentSKU = dr["NewStyle"].ToString();
                        var imageLists = t2TreFs.First(stringToCheck => stringToCheck.Contains(dr["NewStyle"].ToString()));
                        if (!string.IsNullOrEmpty(dr["NewStyle"].ToString()))
                        {
                            var newLine = ParentImportProduct(parentSKU, descriptions, reff, dr, imageLists, t2TreFs);
                            if (newLine != null)
                            {
                                csvLines.AppendLine(newLine);
                            }
                            else
                            {
                                errors.Add(new Error()
                                {
                                    RefNumber = reff
                                });
                                break;
                            }
                        }
                        for (var i = 1; i < 14; i++)
                        { 
                            if (!string.IsNullOrEmpty(dr["QTY" + i].ToString()))
                            {
                                if (string.IsNullOrEmpty(dr["LY" + i].ToString()))
                                {
                                    actualStock = dr["QTY" + i].ToString();
                                }
                                else
                                {
                                    actualStock =
                                        (Convert.ToInt32(dr["QTY" + i]) - Convert.ToInt32(dr["LY" + i]))
                                        .ToString();
                                }

                                var append = (1000 + i).ToString();
                                var groupSkus2 = dr["NewStyle"] + append.Substring(1, 3);

                                var shortDescription = BuildShortDescription(FetchDescription(descriptions, reff));
                                var descripto = FetchDescription(descriptions, reff)?.Descriptio;

                                var size = "";
                                size = i < 10 ? dr["S0" + i].ToString() : dr["S" + i].ToString();

                                if (size.Contains("½"))
                                    size = size.Replace("½", ".5");

                                var eanDataset = Query(groupSkus2, SqlQuery.GetEanCodes);
                                string eanCode = null;
                                if (eanDataset.Tables[0].Rows.Count != 0)
                                {
                                    eanCode = eanDataset.Tables[0].Rows[0]["EAN_CODE"].ToString();
                                }

                                string newLine = null;
                                sizes.Add(dr["SIZERANGE"].ToString() + size);
                                if (Convert.ToInt32(actualStock) != 0)
                                {
                                    newLine = BuildChildImportProduct(groupSkus2, dr, descriptions, reff, shortDescription, actualStock, descripto, size, imageLists, t2TreFs, eanCode, parentSKU);
                                }

                                if (newLine != null)
                                {
                                    csvLines.AppendLine(newLine);
                                }
                                else if (Convert.ToInt32(actualStock) != 0)
                                {
                                    errors.Add(new Error()
                                    {
                                        RefNumber = reff,
                                        ErrorMessage = "No description found!"
                                    });
                                    break;
                                }

                                actualStock = "0";
                            }

                        }                        
                    }
                }

                _logger.LogWrite("Finished for: " + refff);
                return csvLines;
            }
            catch(Exception ex)
            {
                errors.Add(new Error()
                {
                    RefNumber = reffCode,
                    ErrorMessage = $"An error occurred trying to generate, double check execel sheet and description"
                });
                new LogWriter().LogWrite($"Parent SKU: {reffCode}");
                new LogWriter().LogWrite($"Full SKU: {refff}");
                new LogWriter().LogWrite(ex.Message);
                new LogWriter().LogWrite(ex.StackTrace);
            }
            return new StringBuilder();
        }

        private static DataSet Query(string param, string query)
        {
            var data = new DataSet();
            using (var connectionHandler = new OleDbConnection(System.Configuration.ConfigurationManager.AppSettings["AccessConnectionString"]))
            {
                connectionHandler.Open();
                var myAccessCommand = new OleDbCommand(query, connectionHandler);
                myAccessCommand.Parameters.AddWithValue("?", param);
                var myDataAdapter = new OleDbDataAdapter(myAccessCommand);
                myDataAdapter.Fill(data);
            }
            return data;
        }

        private static DataSet Query(string query)
        {
            var data = new DataSet();
            using (var connectionHandler = new OleDbConnection(System.Configuration.ConfigurationManager.AppSettings["AccessConnectionString"]))
            {
                connectionHandler.Open();
                var myAccessCommand = new OleDbCommand(query, connectionHandler);
                var myDataAdapter = new OleDbDataAdapter(myAccessCommand);
                myDataAdapter.Fill(data);
            }
            return data;
        }

        public async Task<DataSet> RetrieveQueryAsync(string param, string query)
        {
            return await Task.Run(() =>
            {
                var data = new DataSet();
                using (var connectionHandler = new OleDbConnection(System.Configuration.ConfigurationManager.AppSettings["AccessConnectionString"]))
                {
                    connectionHandler.Open();
                    var myAccessCommand = new OleDbCommand(query, connectionHandler);
                    myAccessCommand.Parameters.AddWithValue("?", param);
                    var myDataAdapter = new OleDbDataAdapter(myAccessCommand);
                    myDataAdapter.Fill(data);
                }
                return data;
            });
        }

        private static string ParentImportProduct(string groupSkus, List<Descriptions> descriptions, string reff, DataRow dr, string reffColour, IEnumerable<string> t2TreFs)
        {
            var result = FetchDescription(descriptions, reff);

            if (result == null)
                return null;

            var description = result.Description?.TrimEnd();

            if (string.IsNullOrEmpty(description))
                return null;

            return new Model.ImportProducts()
                                          .Setattribut_set("Default")
                                          .SetStore("admin")
                                          .SetWebsites()
                                          .Settype("configurable")
                                          .Setsku(groupSkus?.TrimEnd())
                                          .SethasOption("1")
                                          .SetName(result.Descriptio + " in " + dr["MasterColour"])
                                          .SetpageLayout("No layout updates.")
                                          .SetoptionsContainer("Product Info Column")
                                          .Setprice(dr["BASESELL"].ToString().TrimEnd())
                                          .Setweight("0.01")
                                          .Setstatus("Enabled")
                                          .Setvisibility(null)
                                          .SetshortDescription(BuildShortDescription(result))
                                          .Setgty("0")
                                          .SetproductName(result.Descriptio)
                                          .Setcolor(dr["MasterColour"].ToString().TrimEnd())
                                          .SetsizeRange(null)
                                          .SettaxClass("Taxable Goods")
                                          .SetconfigurableAttribute("size=Size")
                                          .SetManufactoring(dr["MasterSupplier"].ToString())
                                          .Setcategory(dr)
                                          .SetsubCategory(dr["STYPE"].ToString())
                                          .Setseason("")
                                          .SetstockType(dr["MasterStocktype"].ToString())
                                          .Setimage(reffColour + ".jpg")
                                          .SetsmallImage(reffColour + ".jpg")
                                          .Setthumbnail(reffColour + ".jpg")
                                          .Setgallery(t2TreFs, groupSkus.Substring(0, 9))
                                          .Setcondition("new")
                                          .Setinfocare("")
                                          .Setsizeguide("")
                                          .Setrrp(dr["SELL"].ToString())
                                          .Seturl_key((result.Descriptio + " in " + dr["MasterColour"]).Replace(" ", "-").Replace("'", "").ToLower() + "-" + groupSkus)
                                          .Seturl_path((result.Descriptio + " in " + dr["MasterColour"]).Replace(" ", "-").Replace("'", "").ToLower() + ".html")
                                          .Setean(null)
                                          .Setrem1(GetREMValue(dr["REM2"].ToString()))
                                          .Setrem2(GetREMValue(dr["REM"].ToString()))
                                          .Setmodel(dr["SUPPREF"].ToString())
                                          .SetsuSKU(GetSUSKU(reff, t2TreFs))
                                          .SetDescription(Regex.Replace(description, @"\t|\n|\r", ""))
                                          .SetParentSku("")
                                          .ToString();
        }

        private static string BuildChildImportProduct(string groupSkus2, DataRow dr, List<Descriptions> descriptions, string reff,
            string short_description, string actualStock, string descripto, string size, string reffColour, IEnumerable<string> t2TreFs, string eanCode, string parentSku)
        {
            var result = FetchDescription(descriptions, reff);
            if (result == null)
                return null;

            var description = result?.Description;
            if (string.IsNullOrEmpty(description))
                return null;
            else
                description = description.TrimEnd();

            return new Model.ImportProducts()
                                          .Setattribut_set("Default")
                                          .SetStore("admin")
                                          .SetWebsites()
                                          .Settype("simple")
                                          .Setsku(groupSkus2?.TrimEnd())
                                          .SethasOption("1")
                                          .SetName(dr["MasterSupplier"] + " " + result.Descriptio + " in " + dr["MasterColour"])
                                          .SetpageLayout("No layout updates.")
                                          .SetoptionsContainer("Product Info Column")
                                          .Setprice(dr["BASESELL"].ToString().TrimEnd())
                                          .Setweight("0.01")
                                          .Setstatus("Enabled")
                                          .Setvisibility("Not Visible Individually")
                                          .SetshortDescription(short_description?.TrimEnd())
                                          .Setgty(actualStock)
                                          .SetproductName(descripto?.TrimEnd())
                                          .Setcolor(dr["MasterColour"].ToString().TrimEnd())
                                          .SetsizeRange(dr["SIZERANGE"].ToString() + size)
                                          .SettaxClass("Taxable Goods")
                                          .SetconfigurableAttribute("")
                                          .SetManufactoring(dr["MasterSupplier"].ToString())
                                          .Setcategory(null)
                                          .SetsubCategory(null)
                                          .Setseason("")
                                          .SetstockType(dr["MasterStocktype"].ToString())
                                          .Setimage(reffColour + ".jpg")
                                          .SetsmallImage(reffColour + ".jpg")
                                          .Setthumbnail(reffColour + ".jpg")
                                          .Setgallery(t2TreFs, groupSkus2.Substring(0, 9))
                                          .Setcondition("new")
                                          .Setinfocare("")
                                          .Setsizeguide("")
                                          .Setrrp(dr["SELL"].ToString())
                                          .Seturl_key((dr["MasterSupplier"] + " " + result.Descriptio + " in " + dr["MasterColour"]).Replace(" ", "-").ToLower() + groupSkus2)
                                          .Seturl_path((dr["MasterSupplier"] + " " + result.Descriptio + " in " + dr["MasterColour"]).Replace(" ", "-").ToLower() + ".html")
                                          .Setean(eanCode)
                                          .Setrem1(GetREMValue(dr["REM2"].ToString()))
                                          .Setrem2(GetREMValue(dr["REM"].ToString()))
                                          .Setmodel(dr["SUPPREF"].ToString())
                                          .SetsuSKU(GetSUSKU(reff, t2TreFs))
                                          .SetDescription(Regex.Replace(description, @"\t|\n|\r", ""))
                                          .SetParentSku(parentSku)
                                          .ToString();
        }

        private static string GetREMValue(string rem)
        {
            if (!string.IsNullOrEmpty(rem))
            {
                var remresult = REMTable.Tables[0].Select("PROPERTY = '" + rem + "'").FirstOrDefault();
                if (remresult != null)
                {
                    return remresult["NAME"].ToString();
                }
            }
            return "";
        }

        private static string GetSUSKU(string refff, IEnumerable<string> t2TreFs)
        {
            var matchingItems = t2TreFs.Select(x => x).Where(y => y.Any(z => y.Contains(refff))).ToList();
            if (matchingItems.Count == 0)
                return "";
            return String.Join(",", matchingItems);
        }    

        public static string RemoveLineEndings(string value)
        {
            if (String.IsNullOrEmpty(value))
            {
                return value;
            }

            string lineSeparator = ((char)0x2028).ToString();
            string paragraphSeparator = ((char)0x2029).ToString();

            return value.Replace("\r\n", string.Empty)
                .Replace("\n", string.Empty)
                .Replace("\r", string.Empty)
                .Replace(lineSeparator, string.Empty)
                .Replace(paragraphSeparator, string.Empty);
        }       

        public void DoCleanup(string ImagePath)
        {
            var dateFromFolder = ImagePath.Split('\\');
            for (var i = 0; i < 100; i++)
            {
                if (File.Exists(System.Configuration.ConfigurationManager.AppSettings["ImportProductsOutput"] + " " + dateFromFolder[dateFromFolder.Length - 1].Trim() + "" + i + ".csv"))
                {
                    File.Delete(System.Configuration.ConfigurationManager.AppSettings["ImportProductsOutput"] + " " + dateFromFolder[dateFromFolder.Length - 1].Trim() + "" + i + ".csv");
                }
            }
        }

        private static string BuildShortDescription(Descriptions description)
        {
            if (description == null)
                return "<ul></ul>";

            var bullet1 = string.IsNullOrEmpty(description.Bullet1) ? "" : "<li>" + Regex.Replace(description.Bullet1, @"\t|\n|\r", "") + "</li>";
            var bullet2 = string.IsNullOrEmpty(description.Bullet2) ? "" : "<li>" + Regex.Replace(description.Bullet2, @"\t|\n|\r", "") + "</li>";
            var bullet3 = string.IsNullOrEmpty(description.Bullet3) ? "" : "<li>" + Regex.Replace(description.Bullet3, @"\t|\n|\r", "") + "</li>";
            var bullet4 = string.IsNullOrEmpty(description.Bullet4) ? "" : "<li>" + Regex.Replace(description.Bullet4, @"\t|\n|\r", "") + "</li>";
            var bullet5 = string.IsNullOrEmpty(description.Bullet5) ? "" : "<li>" + Regex.Replace(description.Bullet5, @"\t|\n|\r", "") + "</li>";
            var bullet6 = string.IsNullOrEmpty(description.Bullet6) ? "" : "<li>" + Regex.Replace(description.Bullet6, @"\t|\n|\r", "") + "</li>";
            var bullet7 = string.IsNullOrEmpty(description.Bullet7) ? "" : "<li>" + Regex.Replace(description.Bullet7, @"\t|\n|\r", "") + "</li>";
            return "<ul>" + bullet1 + bullet2 + bullet3 + bullet4 + bullet5 + bullet6 + bullet7 + "</ul>";
        }

        //For Conor make sure you add a 0 as somme reason it gets dropped for 
        private static Descriptions FetchDescription(List<Descriptions> descriptions, string reff)
        {
            foreach(var description in descriptions)
            {
                if(description.T2TRef.ToString().ToCharArray()[0] != '0')
                {
                    if ($"0{description.T2TRef}" == reff)
                    {
                        return description;
                    }
                }
                else
                {
                    if ($"{description.T2TRef}" == reff)
                    {
                        return description;
                    }
                }                
            }
            return null;
        }
    }
}