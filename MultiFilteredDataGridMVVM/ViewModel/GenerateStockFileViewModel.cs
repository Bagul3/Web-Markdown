using Common;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MultiFilteredDataGridMVVM.WpfElements;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MultiFilteredDataGridMVVM.ViewModel
{
    public class GenerateStockFileViewModel : ViewModelBase
    {

        private bool _btnGenerateSingleFile;
        private bool _btnGenerateMupliteFile;

        private BackgroundWorker worker;
        private double _progressValue;
        private bool _btnCancel;
        private string _status;
        private readonly LogWriter _logger;
        private List<string> doneList;

        public GenerateStockFileViewModel()
        {
            InitializeCommands();
            _btnCancel = false;
            this.worker = new BackgroundWorker();
            this.worker.WorkerReportsProgress = true;
            this.worker.DoWork += this.ExecuteJob;
            this.worker.ProgressChanged += this.ProgressChanged;
            _logger = new LogWriter();
            doneList = new List<string>();
        }

        public bool BtnGenerateSingleFile
        {
            get { return _btnGenerateSingleFile; }
            set
            {
                if (_btnGenerateSingleFile == value)
                    return;
                _btnGenerateSingleFile = value;
                RaisePropertyChanged("BtnGenerateSingleFile");
            }
        }

        public bool BtnGenerateMupliteFile
        {
            get { return _btnGenerateMupliteFile; }
            set
            {
                if (_btnGenerateMupliteFile == value)
                    return;
                _btnGenerateMupliteFile = value;
                RaisePropertyChanged("BtnGenerateMupliteFile");
            }
        }

        public double ProgressValue
        {
            get { return _progressValue; }
            set
            {
                _progressValue = value;
                RaisePropertyChanged("ProgressValue");
            }
        }

        public bool BtnCancel
        {
            get { return _btnCancel; }
            set
            {
                if (_btnCancel == value)
                    return;
                _btnCancel = value;
                RaisePropertyChanged("BtnCancel");
            }
        }

        public string Status
        {
            get { return _status; }
            set { _status = value;
                RaisePropertyChanged("Status");
            }
            
        }

        public ICommand SingleFileCommand
        {
            get;
            private set;
        }

        public ICommand MulitpleFileCommand
        {
            get;
            private set;
        }

        private void InitializeCommands()
        {
            SingleFileCommand = new CommandHandler(() =>
            {
                this.worker = new BackgroundWorker();
                this.worker.WorkerReportsProgress = true;
                this.worker.DoWork += this.ExecuteJob;
                this.worker.ProgressChanged += this.ProgressChanged;
                this.worker.RunWorkerAsync();
            }, () =>
            {
                return !this.worker.IsBusy;
            });

            MulitpleFileCommand = new CommandHandler(() =>
            {
                this.worker = new BackgroundWorker();
                this.worker.WorkerReportsProgress = true;
                this.worker.DoWork += this.ExecuteMupliteFilesJob;
                this.worker.ProgressChanged += this.ProgressChanged;
                this.worker.RunWorkerAsync();
            }, () =>
            {
                return !this.worker.IsBusy;
            });
        }

        public void ExecuteJob(object sender, DoWorkEventArgs e)
        {
            try
            {
                Status = " Generating";
                BtnGenerateMupliteFile = false;
                BtnGenerateSingleFile = false;
                MessageBox.Show("Generating Stock job, you will be notified once the file is generated.");
                worker.ReportProgress(1);
                var csv = new StringBuilder();
                this.DoCleanup();
                var headers = $"{"sku"},{"qty"},{"is_in_stock"},{"sort_date"},{"ean"},{"price"},{"REM"},{"REM2"},{"season"}";
                csv.AppendLine(headers);                
                Status += "\n Getting SKUs from online";
                var t2TreFs = RetrieveStockFromOnline();                
                Status += "\n Gathering EAN Codes";
                var eanDataset = Connection(null, StockSqlQueries.GetEanCodes);
                worker.ReportProgress(10);
                Status += "\n Injecting SKUs";
                for (var i = 0; i < t2TreFs.Count; i++)
                {
                    Status = "\n Injecting SKU: " + t2TreFs[i] + " for processing";
                    InsertIntoDescriptions(t2TreFs[i]);
                }

                t2TreFs = null;

                worker.ReportProgress(90);
                Status += "\n Building the stock";
                var rows = this.Connection(null, StockSqlQueries.StockQuery);

                foreach (DataRow reff in rows.Tables[0].Rows)
                {
                    csv.Append(DoJob(reff, eanDataset));
                }

                eanDataset = null;
                rows = null;
                worker.ReportProgress(100);

                File.AppendAllText(System.Configuration.ConfigurationManager.AppSettings["StockFilePath"], csv.ToString());
                MessageBox.Show("Stock file created.");
                csv = null;
                Status += "\n Done";
                worker.ReportProgress(0);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                MessageBox.Show(ex.StackTrace);
            }  
            finally
            {
                BtnGenerateMupliteFile = true;
                BtnGenerateSingleFile = true;
            }
        }

        public void ExecuteMupliteFilesJob(object sender, DoWorkEventArgs e)
        {
            try
            {
                BtnGenerateMupliteFile = false;
                BtnGenerateSingleFile = false;
                MessageBox.Show("Generating Stock job with Muplite files, you will be notified once the file is generated.");
                Status = "\n Generating";
                worker.ReportProgress(1);
                var csv = new StringBuilder();
                this.DoCleanup();
                var headers = $"{"sku"},{"qty"},{"is_in_stock"},{"sort_date"},{"ean"},{"price"},{"REM"},{"REM2"},{"season"}";
                csv.AppendLine(headers);
                Status += "\n Getting SKUs from online file";
                var t2TreFs = RetrieveStockFromOnline();

                Status += "\n Gathering EAN Codes";
                var eanDataset = Connection(null, StockSqlQueries.GetEanCodes);
                worker.ReportProgress(10);
                Status += "\n Injecting SKUs for processing";
                for (var j = 0; j < t2TreFs.Count; j++)
                {
                    Status = "\n Injecting SKU: " + t2TreFs[j] + " for processing";
                    InsertIntoDescriptions(t2TreFs[j]);
                }
                worker.ReportProgress(90);
                Status += "\n Building the stock";
                var rows = this.Connection(null, StockSqlQueries.StockQuery);

                var i = 0;
                int card = 0;
                foreach (DataRow reff in rows.Tables[0].Rows)
                {
                    Random rnd = new Random();
                    card = rnd.Next(520);
                    csv.Append(this.DoJob(reff, eanDataset));
                    i++;
                    if (i == Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["Split_Delimiter"]))
                    {
                        File.AppendAllText(System.Configuration.ConfigurationManager.AppSettings["Split_OutputPath"] + card + ".csv", csv.ToString());
                        Status += "\n Stock file: " + System.Configuration.ConfigurationManager.AppSettings["Split_OutputPath"] + card + ".csv Generated.";
                        csv = new StringBuilder();
                        csv.AppendLine(headers);
                        i = 0;
                    }
                }

                File.AppendAllText(System.Configuration.ConfigurationManager.AppSettings["Split_OutputPath"] + card + ".csv", csv.ToString());
                csv = new StringBuilder();
                eanDataset = null;
                rows = null;
                worker.ReportProgress(100);                
                MessageBox.Show("Stock file created on");
                Status += "\n Done.";
                worker.ReportProgress(0);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                BtnGenerateMupliteFile = true;
                BtnGenerateSingleFile = true;
            }
        }

        private void InsertIntoDescriptions(string sku)
        {
            var doesSKUExist = Connection(sku, StockSqlQueries.DoesSKUExist);
            if (doesSKUExist.Tables[0].Rows.Count == 0)
            {
                Console.WriteLine("Adding new SKU to DESC Table: " + sku);
                Connection(sku, StockSqlQueries.InsertSKU);
                Connection(sku, StockSqlQueries.InsertNewProduct);
            }
        }

        private string GetCSV(string url)
        {
            var req = (HttpWebRequest)WebRequest.Create(url);
            var resp = (HttpWebResponse)req.GetResponse();

            var sr = new StreamReader(resp.GetResponseStream());
            var results = sr.ReadToEnd();
            sr.Close();

            return results;
        }

        private void DeleteExistingDd()
        {
            Console.WriteLine("Creating new DESC database");
            if (File.Exists(System.Configuration.ConfigurationManager.AppSettings["AccessConnectionString"] + "DESC.dbf"))
            {
                File.Delete(System.Configuration.ConfigurationManager.AppSettings["AccessConnectionString"] + "DESC.dbf");
            }
        }

        private void CreateDbfFile()
        {
            try
            {
                using (var connection = new OleDbConnection(System.Configuration.ConfigurationManager.AppSettings["AccessConnectionString"]))
                {
                    connection.Open();
                    var command = connection.CreateCommand();

                    command.CommandText = "create table DESC(INDIVIDUAL (ID int primary key, SKU char(100))";
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Stack trace: " + e.StackTrace);
                Console.WriteLine("Message: " + e.Message);
                Console.Read();
            }

        }

        private List<string> RetrieveStockFromOnline()
        {
            var fileList = GetCSV("https://www.cordners.co.uk/exportcsv/");
            string[] tempStr;
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
                            skus.Add(sku);
                        }
                    }
                }
            }

            return skus;
        }

        private string DoJob(DataRow dr, DataSet dt)
        {
            try
            {
                if (dr == null)
                    return "";
                var csv = new StringBuilder();
                var actualStock = "0";
                var inStockFlag = false;
                var groupSkus = "";
                var empty = "";

                var isStock = 0;
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

                                isStock = actualStock == "0" ? 0 : 1;

                                inStockFlag = true;
                            }
                            else
                            {
                                isStock = 0;
                            }
                            var append = (1000 + i).ToString();
                            groupSkus = dr["NewStyle"].ToString();
                            var groupSkus2 = dr["NewStyle"] + append.Substring(1, 3);

                            var eanRow = dt.Tables[0].Select("T2T_CODE = '" + groupSkus2 + "'").FirstOrDefault();
                            var eanCode = "";
                            if (eanRow != null)
                            {
                                eanCode = eanRow["EAN_CODE"].ToString();
                            }

                            var year = IncreaseYearIfCurrentSeason(dr["USER1"].ToString(), Convert.ToDateTime(dr["LASTDELV"])).ToString("yyyy/MM/dd");

                            var newLine = $"{"\"" + groupSkus2 + "\""},{"\"" + actualStock + "\""},{"\"" + isStock + "\""},{"\"" + year + "\""},{"\"" + RemoveLineEndings(eanCode) + "\""},{"\"" + dr["SELL"] + "\""},{"\"" + dr["REM"] + "\""},{"\"" + dr["REM2"] + "\""},{"\"" + dr["USER1"] + "\""}";
                            csv.AppendLine(newLine);

                        }
                        actualStock = "0";
                    }
                }
                doneList.Add(dr["NewStyle"].ToString());


                isStock = inStockFlag ? 1 : 0;
                if (!string.IsNullOrEmpty(dr["NewStyle"].ToString()))
                {
                    var year = IncreaseYearIfCurrentSeason(dr["USER1"].ToString(), Convert.ToDateTime(dr["LASTDELV"])).ToString("yyyy/MM/dd");
                    var newLine2 = $"{"\"" + groupSkus + "\""},{"\"" + actualStock + "\""},{"\"" + isStock + "\""},{"\"" + year + "\""},{"\"" + empty + "\""},{"\"" + dr["SELL"] + "\""},{"\"" + dr["REM"] + "\""},{"\"" + dr["REM2"] + "\""},{"\"" + dr["USER1"] + "\""}";
                    csv.AppendLine(newLine2);
                }

                return csv.ToString();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                _logger.LogWrite(e.Message + e.StackTrace);
                throw;
            }
            finally
            {
                dt = null;
                dr = null;
                doneList = new List<string>();
            }
        }

        private void UpdateStockValues(string sku, string todaysStock)
        {

            var dataset = Connection(null, StockSqlQueries.GetSalesValues);
        }

        private DateTime IncreaseYearIfCurrentSeason(string season, DateTime date)
        {
            //if (season.Length != 3 || !season.ToLower().Contains('s') || !season.ToLower().Contains('w'))
            //    return date;

            //var year = DateTime.Now.Year.ToString().Split(new string[] { "20" }, StringSplitOptions.None)[1];
            //var seasonYear = season.ToLower().Split('s')[1];
            //if (year == seasonYear)
            //    return date.AddYears(1);
            
            if (season.ToLower() == System.Configuration.ConfigurationManager.AppSettings["Season"].ToLower())
            {
                return date.AddYears(1);
            }
            return date;
        }

        private void DoCleanup()
        {
            Console.WriteLine($"The Clean Job thread started successfully.");
            Console.WriteLine("Clean up: removing exisiting stock.csv");
            if (File.Exists(System.Configuration.ConfigurationManager.AppSettings["StockFilePath"]))
            {
                File.Delete(System.Configuration.ConfigurationManager.AppSettings["StockFilePath"]);
            }
        }

        public DataSet Connection(string reff, string query)
        {
            var dataset = new DataSet();
            using (var connectionHandler = new OleDbConnection(System.Configuration.ConfigurationManager.AppSettings["AccessConnectionString"]))
            {
                connectionHandler.OpenAsync();
                var myAccessCommand = new OleDbCommand(query, connectionHandler);
                if (reff != null)
                {
                    myAccessCommand.Parameters.AddWithValue("?", reff);
                }

                var myDataAdapter = new OleDbDataAdapter(myAccessCommand);
                myDataAdapter.Fill(dataset);
            }
            return dataset;
        }

        public DataSet Connection(string reff, string lastmonth, string lastweek, string yesertday, string query)
        {
            var dataset = new DataSet();
            using (var connectionHandler = new OleDbConnection(System.Configuration.ConfigurationManager.AppSettings["AccessConnectionString"]))
            {
                connectionHandler.OpenAsync();
                var myAccessCommand = new OleDbCommand(query, connectionHandler);
                if (reff != null)
                {
                    myAccessCommand.Parameters.AddWithValue("?", lastmonth);
                    myAccessCommand.Parameters.AddWithValue("?", lastweek);
                    myAccessCommand.Parameters.AddWithValue("?", yesertday);
                    myAccessCommand.Parameters.AddWithValue("?", reff);
                }

                var myDataAdapter = new OleDbDataAdapter(myAccessCommand);
                myDataAdapter.Fill(dataset);
            }
            return dataset;
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

        private void ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.ProgressValue = e.ProgressPercentage;
        }       
    }

}
