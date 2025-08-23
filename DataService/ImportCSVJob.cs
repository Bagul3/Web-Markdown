using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Common;
using DataRepo;
using DataService;

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
        private static decimal conversion_rate;
        private static decimal usd_conversion_rate;
        private static decimal aud_conversion_rate;
        private DataSet eanCodes;

        public ImportCsvJob(string excelConnection)
        {
            this._logger = new LogWriter();
            this._mapper = new ExcelMapper();
            descriptions = _mapper.MapToDescriptions(excelConnection);
            REMTable = new SkuRepository().RetrieveQuery(SqlQuery.FetchREM);
            data = Query(SqlQuery.ImportProductsAllQuery);
            conversion_rate = new SalesService().GetEuroPrice();
            usd_conversion_rate = new SalesService().GetUSDPrice();
            aud_conversion_rate = new SalesService().GetAUDPrice();
            eanCodes = Query(SqlQuery.GetAllEanCodes);
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
                        var descriptionObject = FetchDescription(descriptions, reff);
                        if (descriptionObject == null)
                        {
                            errors.Add(new Error()
                            {
                                RefNumber = dr["NewStyle"].ToString(),
                                ErrorMessage = "No description found!"
                            });
                            break;
                        }
                        var groupSkus2 = sizeSkuValue(dr["NewStyle"].ToString(), dr["MINSIZE"].ToString());
                        parentSKU = dr["NewStyle"].ToString();

                        if (!string.IsNullOrEmpty(dr["NewStyle"].ToString()))
                        {
                            var newLine = ParentImportProduct(parentSKU, reff, dr, t2TreFs, descriptionObject, groupSkus2);
                            csvLines.AppendLine(newLine);
                        }
                        for (var i = Convert.ToInt16(dr["MINSIZE"]); i <= Convert.ToInt16(dr["MAXSIZE"]); i++)
                        {
                            groupSkus2 = sizeSkuValue(dr["NewStyle"].ToString(), i);
                            actualStock = calculateStock(dr, i);
                            string size = calculateSize(dr, i);
                            string eanCode = calculateEANCode(groupSkus2);
                            string newLine = null;
                            sizes.Add(dr["SIZERANGE"].ToString() + size);
                            newLine = BuildChildImportProduct(groupSkus2, dr, reff, actualStock, size, t2TreFs, eanCode, parentSKU, descriptionObject);
                            csvLines.AppendLine(newLine);
                            actualStock = "0";
                        }

                    }
                    _logger.LogWrite("Finished for: " + refff);
                }
                return csvLines;
            }
            catch (Exception ex)
            {
                errors.Add(new Error()
                {                     
                    RefNumber = reffCode,
                    ErrorMessage = $"An error occurred trying to generate, double check excel sheet and description"
                });
                new LogWriter().LogWrite($"Parent SKU: {reffCode}");
                new LogWriter().LogWrite($"Full SKU: {refff}");
                new LogWriter().LogWrite(ex.Message);
                new LogWriter().LogWrite(ex.StackTrace);
            }
            return new StringBuilder();
        }

        private List<string> commonSKUs(IEnumerable<string> t2TreFs, string sku)
        {
            return t2TreFs.Where(x => x.Contains(sku)).ToList();
        }

        private static string calculateStock(DataRow dr, short i)
        {
            string actualStock;
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

            if (actualStock == "" || actualStock == null)
                actualStock = "0";
            return actualStock;
        }

        private string calculateEANCode(string groupSkus2)
        {
            var eanDataset = eanCodes.Tables[0].Select($"T2T_CODE = {groupSkus2}");
            string eanCode = null;
            if (eanDataset.Length != 0)
            {
                eanCode = eanDataset[0]["EAN_CODE"].ToString();
            }

            return eanCode;
        }

        private static string calculateSize(DataRow dr, short i)
        {
            var size = "";
            size = i < 10 ? dr["S0" + i].ToString() : dr["S" + i].ToString();
            if (size.Contains("½"))
                size = size.Replace("½", ".5");
            return size;
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

        private static string ParentImportProduct(string groupSkus, string reff, DataRow dr, IEnumerable<string> t2TreFs, Descriptions descriptions, String group2)
        {
            if (descriptions == null)
                return null;

            return new Model.ImportProducts()
                                          .Setattribut_set("Default")
                                          .SetStore("admin")
                                          .SetWebsites()
                                          .Settype("configurable")
                                          .Setsku(groupSkus?.TrimEnd())
                                          .SethasOption("1")
                                          .SetName(descriptions.Descriptio.TrimEnd() + " in " + dr["MasterColour"].ToString().Trim())
                                          .SetpageLayout("No layout updates.")
                                          .SetoptionsContainer("Product Info Column")
                                          .Setprice(dr["BASESELL"].ToString().Trim())
                                          .Setweight("0.01")
                                          .Setstatus("Enabled")
                                          .Setvisibility(null)
                                          .SetshortDescription(BuildShortDescription(descriptions))
                                          .Setgty("")
                                          .SetproductName(descriptions.Descriptio.Trim())
                                          .Setcolor(dr["MasterColour"].ToString().Trim())
                                          .SetsizeRange(null)
                                          .SettaxClass("Taxable Goods")
                                          .SettaxCode(dr["VAT"].ToString())
                                          .SetconfigurableAttribute("size=Size")
                                          .SetManufactoring(dr["MasterSupplier"].ToString())
                                          .Setcategory(dr)
                                          .SetsubCategory(dr["STYPE"].ToString())
                                          .Setseason("")
                                          .SetstockType(dr["MasterStocktype"].ToString())
                                          .Setimage(t2TreFs.Where(x => x.Contains(dr["NewStyle"].ToString())).FirstOrDefault() + ".jpg")
                                          .SetsmallImage(t2TreFs.Where(x => x.Contains(dr["NewStyle"].ToString())).FirstOrDefault() + ".jpg")
                                          .Setthumbnail(t2TreFs.Where(x => x.Contains(dr["NewStyle"].ToString())).FirstOrDefault() + ".jpg")
                                          .Setgallery(t2TreFs, dr["NewStyle"].ToString())
                                          .Setcondition("new")
                                          .Setinfocare("")
                                          .Setsizeguide("")
                                          .Setrrp(dr["SELL"].ToString())
                                          .Seturl_key((descriptions.Descriptio.Trim() + " in " + dr["MasterColour"].ToString().Trim()).Replace(" ", "-").Replace("'", "").ToLower() + "-" + groupSkus)
                                          .Seturl_path((descriptions.Descriptio.Trim() + " in " + dr["MasterColour"].ToString().Trim()).Replace(" ", "-").Replace("'", "").ToLower() + ".html")
                                          .Setean(null)
                                          .Setrem1(GetREMValue(dr["REM2"].ToString()))
                                          .Setrem2(GetREMValue(dr["REM"].ToString()))
                                          .Setmodel(Regex.Replace(dr["SUPPREF"].ToString(), @"\t|\n|\r", ""))
                                          .SetsuSKU(GetSUSKU(dr["NewStyle"].ToString(), t2TreFs))
                                          .SetDescription(Regex.Replace(descriptions.Description, @"\t|\n|\r", ""))
                                          .SetSType("")
                                          .SetUDef("")
                                          .SetParentSku("")
                                          .Seteuro_special_price(GenerateExchangePrice(Convert.ToDecimal(dr["BASESELL"].ToString().Trim()), conversion_rate))
                                          .Setusd_special_price(GenerateExchangePrice(Convert.ToDecimal(dr["BASESELL"].ToString().Trim()), usd_conversion_rate))
                                          .Setaud_special_price(GenerateExchangePrice(Convert.ToDecimal(dr["BASESELL"].ToString().Trim()), aud_conversion_rate))
                                          .Setcreation_date(false)
                                          .Setnew_to_date(false)
                                          .Setnew_from_date(false)
                                          .ToString();
        }
        private static string BuildChildImportProduct(string groupSkus2, 
            DataRow dr, 
            string reff,
            string actualStock, 
            string size, 
            IEnumerable<string> t2TreFs, 
            string eanCode,
            string parentSku,
            Descriptions descriptions)
        {
            if (descriptions == null)
                return null;

            var shortDescription = BuildShortDescription(descriptions);

            return new Model.ImportProducts()
                                          .Setattribut_set("Default")
                                          .SetStore("admin")
                                          .SetWebsites()
                                          .Settype("simple")
                                          .Setsku(groupSkus2?.TrimEnd())
                                          .SethasOption("1")
                                          .SetName(dr["MasterSupplier"] + " " + descriptions.Descriptio.Trim() + " in " + dr["MasterColour"])
                                          .SetpageLayout("No layout updates.")
                                          .SetoptionsContainer("Product Info Column")
                                          .Setprice(dr["BASESELL"].ToString().Trim())
                                          .Setweight("0.01")
                                          .Setstatus("Enabled")
                                          .Setvisibility("Not Visible Individually")
                                          .SetshortDescription(shortDescription?.Trim())
                                          .Setgty(actualStock)
                                          .SetproductName(descriptions.Descriptio?.Trim())
                                          .Setcolor(dr["MasterColour"].ToString().TrimEnd())
                                          .SetsizeRange(dr["SIZERANGE"].ToString() + size)
                                          .SettaxClass("Taxable Goods")
                                          .SettaxCode(dr["VAT"].ToString())
                                          .SetconfigurableAttribute("")
                                          .SetManufactoring(dr["MasterSupplier"].ToString())
                                          .Setcategory(null)
                                          .SetsubCategory(null)
                                          .Setseason("")
                                          .SetstockType(dr["MasterStocktype"].ToString())
                                          .Setimage(t2TreFs.Where(x => x.Contains(dr["NewStyle"].ToString())).FirstOrDefault() + ".jpg")
                                          .SetsmallImage(t2TreFs.Where(x => x.Contains(dr["NewStyle"].ToString())).FirstOrDefault() + ".jpg")
                                          .Setthumbnail(t2TreFs.Where(x => x.Contains(dr["NewStyle"].ToString())).FirstOrDefault() + ".jpg")
                                          .Setgallery(null, "")
                                          .Setcondition("new")
                                          .Setinfocare("")
                                          .Setsizeguide("")
                                          .Setrrp(dr["SELL"].ToString())
                                          .Seturl_key((dr["MasterSupplier"] + " " + descriptions.Descriptio.Trim() + " in " + dr["MasterColour"]).Replace(" ", "-").ToLower() + groupSkus2)
                                          .Seturl_path((dr["MasterSupplier"] + " " + descriptions.Descriptio.Trim() + " in " + dr["MasterColour"]).Replace(" ", "-").ToLower() + ".html")
                                          .Setean(eanCode)
                                          .Setrem1(GetREMValue(dr["REM2"].ToString()))
                                          .Setrem2(GetREMValue(dr["REM"].ToString()))
                                          .Setmodel(Regex.Replace(dr["SUPPREF"].ToString(), @"\t|\n|\r", ""))
                                          .SetsuSKU(GetSUSKU(dr["NewStyle"].ToString(), t2TreFs))
                                          .SetDescription(Regex.Replace(descriptions.Description, @"\t|\n|\r", ""))
                                          .SetParentSku(parentSku)
                                          .SetUDef(String.IsNullOrEmpty(dr["MasterSubDept"].ToString()) ? "" : dr["MasterSubDept"].ToString())
                                          .SetSType(String.IsNullOrEmpty(dr["MasterDept"].ToString()) ? "" : dr["MasterDept"].ToString())   
                                          .Seteuro_special_price(GenerateExchangePrice(Convert.ToDecimal(dr["BASESELL"].ToString().Trim()), conversion_rate))
                                          .Setusd_special_price(GenerateExchangePrice(Convert.ToDecimal(dr["BASESELL"].ToString().Trim()), usd_conversion_rate))
                                          .Setaud_special_price(GenerateExchangePrice(Convert.ToDecimal(dr["BASESELL"].ToString().Trim()), aud_conversion_rate))
                                          .Setcreation_date(true)
                                          .Setnew_to_date(true)
                                          .Setnew_from_date(true)
                                          .ToString();
        }

        private string sizeSkuValue(string sku, string minSize)
        {
            if (minSize.Length == 1)
                return sku + "00" + minSize;
            else if (minSize.Length == 2)
                return sku + "0" + minSize;
            return sku + minSize;
        }

        private string sizeSkuValue(string sku, int i)
        {
            if (i <10)
                return sku + "00" + i;
            else
                return sku + "0" + i;
        }

        public static string GenerateExchangePrice(decimal gbp, decimal conversion_rate)
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

        private static string GetREMValue(string rem)
        {
            if (!string.IsNullOrEmpty(rem))
            {
                var remresult = REMTable.Tables[0].Select("Id = '" + rem + "'").FirstOrDefault();
                if (remresult != null)
                {
                    return remresult["NAME"].ToString();
                }
            }
            return "";
        }
        private static string GetSUSKU(string refff, IEnumerable<string> t2TreFs)
        {
            var matchingItems = t2TreFs.Where(x => x.Contains(refff)).ToList(); ;
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

            var actualDesc = descriptions.Where(x => x.T2TRef == reff).LastOrDefault();
            return actualDesc;

            //Descriptions actualDesc = null;
            //foreach (var description in descriptions)
            //{
            //    new LogWriter().LogWrite(description.ToString());
            //    if (description.T2TRef.ToString() != "")
            //    {
            //        if (description.T2TRef.ToString().ToCharArray()[0] != '0')
            //        {
            //            var desc = description.T2TRef;
            //            while (desc.ToString().ToCharArray().Count() != 6)
            //            {
            //                desc = "0" + desc;
            //            }
            //            if (desc.ToString() == reff)
            //            {
            //                actualDesc = description;
            //            }
            //        }
            //        else
            //        {
            //            if ($"{description.T2TRef}" == reff)
            //            {
            //                actualDesc = description;
            //            }
            //        }
            //    }

            //}
            //return actualDesc;
        }
    }
}