using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Common
{
    public class ExcelMapper
    {

        public List<Descriptions> MapToDescriptions(string excelConnection)
        {
            var dvEmp = new DataView();
            var descriptions = new List<Descriptions>();
            _logger.LogWrite("Getting refs from description file");
            try
            {
                //System.Configuration.ConfigurationManager.AppSettings["ExcelConnectionString"]
                //using (var connectionHandler = new OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source="+ excelConnection + ";Extended Properties='Excel 12.0;IMEX=1;'"))
                //{
                //    connectionHandler.Open();
                //    var adp = new OleDbDataAdapter("SELECT * FROM [Sheet1$A:R]", connectionHandler);

                //    var dsXls = new DataSet();
                //    adp.Fill(dsXls);
                //    dvEmp = new DataView(dsXls.Tables[0]);
                //}

                using (var stream = File.Open(excelConnection, FileMode.Open, FileAccess.Read))
                {
                    IExcelDataReader reader;

                    // Create Reader - old until 3.4+
                    ////var file = new FileInfo(originalFileName);
                    ////if (file.Extension.Equals(".xls"))
                    ////    reader = ExcelDataReader.ExcelReaderFactory.CreateBinaryReader(stream);
                    ////else if (file.Extension.Equals(".xlsx"))
                    ////    reader = ExcelDataReader.ExcelReaderFactory.CreateOpenXmlReader(stream);
                    ////else
                    ////    throw new Exception("Invalid FileName");
                    // Or in 3.4+ you can only call this:
                    reader = ExcelReaderFactory.CreateReader(stream);

                    ExcelDataSetConfiguration conf = new ExcelDataSetConfiguration
                    {
                        ConfigureDataTable = _ => new ExcelDataTableConfiguration
                        {
                            UseHeaderRow = true
                        }
                    };

                    var dataSet = reader.AsDataSet(conf);

                    // Now you can get data from each sheet by its index or its "name"
                    var dataTable = dataSet.Tables[0];

                    for (var i = 0; i < dataTable.Rows.Count; i++)
                    {
                        var descrip = new Descriptions()
                        {
                            T2TRef = (from DataRow row in dataTable.Rows select row["T2TREF"] != DBNull.Value ? row["T2TREF"].ToString() : "").ElementAt(i),
                            Descriptio = (from DataRow row in dataTable.Rows select row["TITLE"] != DBNull.Value ? row["TITLE"].ToString() : "").ElementAt(i).ToString(),
                            Description = Regex.Replace((from DataRow row in dataTable.Rows select row["DESCRIPTION"] != DBNull.Value ? row["DESCRIPTION"].ToString() : "").ElementAt(i).ToString(), @"\t|\n|\r|’|™|®|é", ""),
                            Bullet1 = (from DataRow row in dataTable.Rows select row["BULLET 1"] != DBNull.Value ? row["BULLET 1"].ToString() : "").ElementAt(i).ToString(),
                            Bullet2 = (from DataRow row in dataTable.Rows select row["BULLET 2"] != DBNull.Value ? row["BULLET 2"].ToString() : "").ElementAt(i).ToString(),
                            Bullet3 = (from DataRow row in dataTable.Rows select row["BULLET 3"] != DBNull.Value ? row["BULLET 3"].ToString() : "").ElementAt(i).ToString(),
                            Bullet4 = (from DataRow row in dataTable.Rows select row["BULLET 4"] != DBNull.Value ? row["BULLET 4"].ToString() : "").ElementAt(i).ToString(),
                            Bullet5 = (from DataRow row in dataTable.Rows select row["BULLET 5"] != DBNull.Value ? row["BULLET 5"].ToString() : "").ElementAt(i).ToString(),
                            Bullet6 = (from DataRow row in dataTable.Rows select row["BULLET 6"] != DBNull.Value ? row["BULLET 6"].ToString() : "").ElementAt(i).ToString(),
                            Bullet7 = (from DataRow row in dataTable.Rows select row["BULLET 7"] != DBNull.Value ? row["BULLET 7"].ToString() : "").ElementAt(i).ToString()

                        };

                        descriptions.Add(descrip);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                _logger.LogWrite("Error occured getting refs from description file: " + e);
            }


            return descriptions;
        }

        private LogWriter _logger = new LogWriter();
    }
}
