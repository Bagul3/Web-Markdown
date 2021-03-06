﻿using System;
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

        public ImportCsvJob(string excelConnection)
        {
            this._logger = new LogWriter();
            this._mapper = new ExcelMapper();
            descriptions = _mapper.MapToDescriptions(excelConnection);
            REMTable = new SkuRepository().RetrieveQuery(SqlQuery.FetchREM);
        }

        public StringBuilder DoJob(string refff, IEnumerable<string> t2TreFs, ref ObservableCollection<Error> errors)
        {
            try
            {
                var csvLines = new StringBuilder();
                _logger.LogWrite("Generating stock.csv: This will take a few minutes, please wait....");

                var reff = refff.Substring(0, 6);
                reffCode = reff;
                var data = Query(reff, SqlQuery.ImportProductsQuery);

                var actualStock = "0";
                var inStockFlag = false;
                var groupSkus = "";

                foreach (DataRow dr in data.Tables[0].Rows)
                {
                    _logger.LogWrite("Working....");
                    var simpleSkusList = new List<string>();
                    if (t2TreFs.Any(x => x.Contains(dr["NewStyle"].ToString())))
                    {
                        var imageLists = t2TreFs.First(stringToCheck => stringToCheck.Contains(dr["NewStyle"].ToString()));
                        var isStock = 0;
                        for (var i = 1; i < 14; i++)
                        {
                            Console.WriteLine(dr["QTY" + i].ToString());

                            if (!string.IsNullOrEmpty(dr["QTY" + i].ToString()))
                            {
                                if (Convert.ToInt32(dr["QTY" + i]) > 0)
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

                                    isStock = 1;
                                    inStockFlag = true;
                                }
                                else
                                {
                                    isStock = 0;
                                }

                                var append = (1000 + i).ToString();
                                groupSkus = dr["NewStyle"].ToString();
                                var groupSkus2 = dr["NewStyle"] + append.Substring(1, 3);

                                var shortDescription = BuildShortDescription(FetchDescription(descriptions, reff));
                                var descripto = FetchDescription(descriptions, reff)?.Descriptio;

                                var size = "";
                                size = i < 10 ? dr["S0" + i].ToString() : dr["S" + i].ToString();

                                if (size.Contains("½"))
                                    size = size.Replace("½", ".5");

                                simpleSkusList.Add(groupSkus2);
                                var eanDataset = Query(groupSkus2, SqlQuery.GetEanCodes);
                                string eanCode = null;
                                if (eanDataset.Tables[0].Rows.Count != 0)
                                {
                                    eanCode = eanDataset.Tables[0].Rows[0]["EAN_CODE"].ToString();
                                }

                                string newLine = null;
                                if (Convert.ToInt32(actualStock) != 0)
                                {
                                    newLine = BuildChildImportProduct(groupSkus2, dr, descriptions, reff, shortDescription, actualStock, descripto, size, isStock, imageLists, t2TreFs, eanCode);
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

                        isStock = inStockFlag ? 1 : 0;
                        if (!string.IsNullOrEmpty(dr["NewStyle"].ToString()))
                        {
                            var newLine = ParentImportProduct(groupSkus, descriptions, reff, dr, simpleSkusList,
                                isStock, imageLists, t2TreFs);
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
                        inStockFlag = false;
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

        private static string ParentImportProduct(string groupSkus, List<Descriptions> descriptions, string reff, DataRow dr, List<string> simpleSkusList,
            int isStock, string reffColour, IEnumerable<string> t2TreFs)
        {
            // descriptions.Where(x => x.T2TRef.ToString() == reff).Select(y => y.Description).FirstOrDefault()?.TrimEnd()
            var result = FetchDescription(descriptions, reff);

            if (result == null)
                return null;

            var description = result.Description?.TrimEnd();

            if (string.IsNullOrEmpty(description))
                return null;

            var unquotedName = result.Descriptio + " in " + dr["MasterColour"];
            description = Regex.Replace(description, @"\t|\n|\r", "");
            description = "\"" + description + "\"";
            var store = "\"admin\"";
            var websites = Websites()?.TrimEnd();
            var attribut_set = "\"Default\"";
            var type = "\"configurable\"";
            var sku = "\"" + groupSkus?.TrimEnd() + "\"";
            var hasOption = "\"1\"";
            var name = "\"" + unquotedName + "\"";
            var pageLayout = "\"No layout updates.\"";
            var optionsContainer = "\"Product Info Column\"";
            var price = "\"" + dr["BASESELL"].ToString().TrimEnd() + "\"";
            var weight = "\"0.01\"";
            var status = "\"Enabled\"";
            var visibility = Visibility()?.TrimEnd();
            var shortDescription = "\"" + BuildShortDescription(result) + "\"";
            var gty = "\"0\"";
            var productName = "\"" + result.Descriptio + "\"";
            var color = "\"" + dr["MasterColour"].ToString().TrimEnd() + "\"";
            var sizeRange = "\"\"";
            var vat = dr["VAT"].ToString() == "A" ? "TAX" : "None";
            var taxClass = "\"" + vat + "\"";
            var configurableAttribute = "\"size\"";
            var simpleSku = BuildSimpleSku(simpleSkusList, reff);
            var manufactor = "\"" + dr["MasterSupplier"] + "\"";
            var isInStock = "\"" + isStock + "\"";
            var cat = Category(dr, out string subCat);
            var subCategory = "\"" + subCat + "\"";
            var category = "\"" + cat + "\"";
            var season = "\"\"";
            var stockType = "\"" + dr["MasterStocktype"] + "\"";
            var image = "\"+/" + reffColour + ".jpg\"";
            var smallImage = "\"/" + reffColour + ".jpg\"";
            var thumbnail = "\"/" + reffColour + ".jpg\"";
            var gallery = "\"" + BuildGalleryImages(t2TreFs, groupSkus.Substring(0, 9)) + "\"";
            var condition = "\"new\"";
            var ean = "\"\"";
            var model = "\"" + dr["SUPPREF"] + "\"";
            var infocare = "\"" + "row-product-featured-shoe-care" + "\"";
            var sizeguide = "\"" + "product_tab_size_guide" + "\"";
            var rrp = "\"" + dr["SELL"] + "\"";
            var url_key = "\"" + unquotedName.Replace(" ", "-").ToLower() + "\"";
            var url_path = "\"" + unquotedName.Replace(" ", "-").ToLower() + ".html" + "\"";
            var rem1 = "\"" + GetREMValue(dr["REM2"].ToString()) + "\"";
            var rem2 = "\"" + GetREMValue(dr["REM"].ToString()) + "\"";
            var suSKU = "\"" + GetSUSKU(reff, t2TreFs) + "\"";
            var newLine = $"{store}," +
                          $"{websites},{attribut_set},{type},{sku},{hasOption},{name.TrimEnd()},{pageLayout},{optionsContainer},{price},{weight},{status}," +
                          $"{visibility}," +
                          $"{shortDescription},{gty},{productName},{color}," +
                          $"{sizeRange},{taxClass},{configurableAttribute},{simpleSku},{manufactor},{isInStock}," +
                          $"{category},{subCategory},{season},{stockType},{image},{smallImage},{thumbnail},{gallery},{condition},{ean}," +
                          $"{description},{model},{infocare},{sizeguide},{rrp},{url_key},{url_path},{rem1},{rem2},{suSKU}";
            return newLine;
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
            

        private static string Category(DataRow dr, out string subCategory)
        {
            var category = "Brands/" + dr["MasterSupplier"] + ";;";
            subCategory = "";
            // Ladies bags
            if (dr["MasterStocktype"].ToString() == "UNISEX")
            {
                category = category + "UNISEX" + "/Shop By Brand/" +
                       dr["MasterSupplier"];
                return category;
            }
            if (dr["STYPE"].ToString() == "BAG" && dr["MasterStocktype"].ToString() == "LADIES")
            {
                category = category + "BAGS & ACCESSORIES" + "/Shop By Department" + "/" + "LADIES BAGS" + ";;";
                category = category + "BAGS & ACCESSORIES" + "/Shop By Brand" + "/" + dr["MasterSupplier"].ToString();
                return category;
            }

            if (dr["STYPE"].ToString() == "BAG")
            {
                category = category + "BAGS & ACCESSORIES" + "/Shop By Department" + "/" + "BACKPACKS" + ";;";
                category = category + "BAGS & ACCESSORIES" + "/Shop By Brand" + "/" + dr["MasterSupplier"].ToString();
                return category;
            }
            
            if(dr["STYPE"].ToString() == "PEN")
            {
                category = category + "BAGS & ACCESSORIES" + "/Shop By Department" + "/" + "PENCIL CASES" + ";;";
                category = category + "BAGS & ACCESSORIES" + "/Shop By Brand" + "/" + dr["MasterSupplier"].ToString();
                return category;
            }

            // FIX LATER
            

            //Bags and Accessories and Shoecare
            if (dr["MasterStocktype"].ToString() == "SUNDRIES")
            {
                switch (dr["STYPE"].ToString())
                {
                    case "ACC":
                        subCategory = "ACCESSORIES";
                        break;
                    case "C&C":
                        subCategory = "CLEAN & CARE";
                        break;
                    case "C&P":
                        subCategory = "CREAMS & POLISH";
                        break;
                    case "I&S":
                        subCategory = "INSOLES & SUPPORT";
                        break;
                    case "LAC":
                        subCategory = "LACES";
                        break;
                }
                category = category + "BAGS & ACCESSORIES" + "/Shop By Department" + "/" + "SHOE CARE" + ";;";
                category = category + "BAGS & ACCESSORIES" + "/Shop By Brand" + "/" + dr["MasterSupplier"].ToString();
                return category;
            }

            // Old category Formatting

            category += dr["MasterStocktype"] + "/Shop By Department/" + dr["MasterDept"] + ";;";

            if (dr["MasterSubDept"] != "ANY" || dr["MasterSubDept"] != "")
            {
                category = category + dr["MasterStocktype"] + "/Shop By Department/" +
                           dr["MasterDept"] + "/" + dr["MasterSubDept"] + "::1::1::0;;";
            }

            category = category + dr["MasterStocktype"] + "/Shop By Brand/" +
                       dr["MasterSupplier"] + ";;";
            category = category + dr["MasterStocktype"] + "/Shop By Brand/" +
                       dr["MasterSupplier"] + "/" + dr["MasterDept"] + "::1::1::0";

            if (dr["MasterSubDept"] != "ANY" || dr["MasterSubDept"] != "")
            {
                category = category + ";;" + dr["MasterStocktype"] + "/Shop By Brand/" +
                           dr["MasterSupplier"] + "/" + dr["MasterDept"] +
                           "/" + dr["MasterSubDept"] + "::1::1::0";
            }

            if (dr["USER1"].ToString().Contains("B"))
            {
                category += ";;BACK TO SCHOOL/" + dr["MasterStocktype"];
            }

            return category;
        }

        private static string BuildChildImportProduct(string groupSkus2, DataRow dr, List<Descriptions> descriptions, string reff,
            string short_description, string actualStock, string descripto, string size, int isStock, string reffColour, IEnumerable<string> t2TreFs, string eanCode)
        {
            //descriptions.Where(x => x.T2TRef.ToString() == reff).Select(y => y.Description).FirstOrDefault()?.TrimEnd()
            var result = FetchDescription(descriptions, reff);

            if (result == null)
                return null;

            var description = result?.Description;
            if (string.IsNullOrEmpty(description))
                return null;
            else
                description = description.TrimEnd();

            var unquotedName = dr["MasterSupplier"] + " " + result.Descriptio + " in " + dr["MasterColour"];            
            description = Regex.Replace(description, @"\t|\n|\r", "");
            description = "\"" + description + "\"";
            const string store = "\"admin\"";
            var websites = Websites()?.TrimEnd();
            const string attribut_set = "\"Default\"";
            const string type = "\"simple\"";
            var sku = "\"" + groupSkus2?.TrimEnd() + "\"";
            const string hasOption = "\"1\"";
            var name = "\"" + unquotedName + "\"";
            const string pageLayout = "\"No layout updates.\"";
            const string optionsContainer = "\"Product Info Column\"";
            var price = "\"" + dr["BASESELL"].ToString().TrimEnd() + "\"";
            const string weight = "\"0.01\"";
            const string status = "\"Enabled\"";
            const string visibility = "\"Not Visible Individually\"";
            var shortDescription = "\"" + short_description?.TrimEnd() + "\"";
            var gty = "\"" + actualStock + "\"";
            var productName = "\"" + descripto?.TrimEnd() + "\"";
            var color = "\"" + dr["MasterColour"].ToString().TrimEnd() + "\"";

            var sizerange = dr["SIZERANGE"].ToString() + size;

            if (new[] { "A", "Q", "S", "R"}.Any(c => sizerange.ToUpper().Contains(c)))
            {
                try
                {
                    if (sizerange.Contains('F'))
                    {
                        
                    }
                    else
                    {
                        sizerange = sizerange.Remove(0, 1);
                    }
                    
                }
                catch (Exception e)
                {
                    new LogWriter().LogWrite(e.StackTrace);
                }
            }

            var sizeRange = "\"" + sizerange + "\"";
            var vat = dr["VAT"].ToString() == "A" ? "TAX" : "None";
            var taxClass = "\"" + vat + "\"";
            const string configurableAttribute = "\"\"";
            const string simpleSku = "\"\"";
            var manufactor = "\"" + dr["MasterSupplier"] + "\"";
            var isInStock = "\"" + isStock + "\"";
            const string subCategory = "\"\"";
            const string category = "\"\"";
            const string season = "\"\"";
            var stockType = "\"" + dr["MasterStocktype"] + "\"";
            var image = "\"+/" + reffColour + ".jpg\"";
            var smallImage = "\"/" + reffColour + ".jpg\"";
            var thumbnail = "\"/" + reffColour + ".jpg\"";
            var gallery = "\"" + BuildGalleryImages(t2TreFs, groupSkus2.Substring(0, 9)) + "\"";
            const string condition = "\"new\"";
            var infocare = "\"" + "row-product-featured-shoe-care" + "\"";
            var sizeguide = "\"" + "product_tab_size_guide" + "\"";
            var rrp = "\"" + dr["SELL"] + "\"";
            var url_key = "\"" + unquotedName.Replace(" ", "-").ToLower() + "\"";
            var url_path = "\"" + unquotedName.Replace(" ", "-").ToLower() + ".html" + "\"";
            var escapedEanCode = "\"\"";
            var rem1 = "\"" + GetREMValue(dr["REM2"].ToString()) + "\"";
            var rem2 = "\"" + GetREMValue(dr["REM"].ToString()) + "\"";
            if (!string.IsNullOrEmpty(eanCode))
            {
                escapedEanCode = "\"" + RemoveLineEndings(eanCode.Trim().Replace(",", "")) + "\"";
            }

            var ean = escapedEanCode;

            var model = "\"" + dr["SUPPREF"] + "\"";
            var suSKU = "\"" +  GetSUSKU(reff, t2TreFs) + "\"";
            var newLine = $"{store}," +
                          $"{websites},{attribut_set},{type},{sku},{hasOption},{name.TrimEnd()},{pageLayout},{optionsContainer},{price},{weight},{status},{visibility}," +
                          $"{shortDescription},{gty},{productName},{color}," +
                          $"{sizeRange},{taxClass},{configurableAttribute},{simpleSku},{manufactor},{isInStock}," +
                          $"{category},{subCategory},{season},{stockType},{image},{smallImage},{thumbnail},{gallery},{condition},{ean}," +
                          $"{description},{model},{infocare},{sizeguide},{rrp},{url_key},{url_path},{rem1},{rem2},{suSKU}";
            return newLine;
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

        private static string BuildGalleryImages(IEnumerable<string> t2TreFs, string reff)
        {
            var images = t2TreFs.Where(t2TRef => t2TRef.Contains(reff)).ToList();
            return images.Aggregate("", (current, image) => current + ("/" + image + ".jpg;"));
        }

        private static string BuildSimpleSku(IEnumerable<string> t2TreFs, string reff)
        {
            var output = t2TreFs.Where(t2TReff => t2TReff.Contains(reff)).Aggregate("\"", (current, t2TReff) => current + t2TReff + ",");
            return output.Remove(output.Length - 1) + "\"";
        }

        private static string Websites()
        {
            var output = "\"";
            var fields = new string[] { "admin", "base" };
            foreach (var field in fields)
            {
                output += field + ",";
            }
            return output.Remove(output.Length - 1) + "\"";
        }

        private static string Visibility()
        {
            var fields = new[] { "Catalog", "Search" };
            var output = fields.Aggregate("\"", (current, field) => current + (field + ","));
            return output.Remove(output.Length - 1) + "\"";
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