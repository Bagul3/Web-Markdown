using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using Common;
using DataRepo;
using DataService;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using ImportProducts.Services;
using MultiFilteredDataGridMVVM.WpfElements;
namespace MultiFilteredDataGridMVVM.ViewModel
{
    public class ImportProductViewModel : ViewModelBase
    {
        private ObservableCollection<Image> _fileNames;
        private ObservableCollection<Error> _errors;
        CancellationTokenSource _cancelToken;
        public static string ImagePath;
        private ImportCsvJob job;
        private StringBuilder _csv;
        private string _descriptionsPath;
        private ObservableCollection<Image> _image;
        private ObservableCollection<Error> _error;
        private string _text;
        private bool _btnCancel;
        private bool _btnLoadImages;
        private bool _btnGenerateImportCsv;
        private bool _btnLoadExcel;
        private double _progressValue;
        private BackgroundWorker worker;
        private static DataSet REMTable;
        private bool _isBusy;
        private string headers = $"{"sku"},{"store_view_codes"},{"websites"},{"attribut_set"},{"type"},{"has_options"},{"name"},{"page_layout"},{"options_container"},{"price"},{"weight"},{"status"},{"visibility"},{"short_description"},{"qty"},{"product_name"},{"color"}," +
        $"{"size"},{"tax_class_id"},{"configurable_attributes"},{"manufacturer"},{"categories"},{"sub_categories"},{"season"},{"stock_type"},{"image"},{"small_image"},{"thumbnail"},{"gallery"}," +
        $"{"condition"},{"ean"},{"description"},{"model"},{"infocare"},{"sizeguide"},{"RRP"},{"url_key"},{"url_path"},{"rem"},{"rem2"},{"susku"},{"parent_sku"},{"uDef2"},{"d_type"},{"euro_price"},{"usd_price"}";
        public ImportProductViewModel()
        {
            InitializeCommands();
            _fileNames = new ObservableCollection<Image>();
            _error = new ObservableCollection<Error>();
            LoadData();
            _btnCancel = false;
            _btnLoadImages = true;

            _csv = new StringBuilder();
            _errors = new ObservableCollection<Error>();
            ImageName = new ObservableCollection<Image>();
            this.worker = new BackgroundWorker();
            this.worker.WorkerReportsProgress = true;
            this.worker.DoWork += this.GenerateImportCsv;
            REMTable = new SkuRepository().RetrieveQuery(SqlQuery.FetchREM);
            this.worker.ProgressChanged += this.ProgressChanged;
        }
        private async Task LoadData()
        {
            await Task.Factory.StartNew(() =>
            {
                IsBusy = true;
            }).ContinueWith((task) =>
            {
                IsBusy = false;
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }
        private string _LoadExcelButton;
        public string LoadExcelButton
        {
            get { return _LoadExcelButton ?? (_LoadExcelButton = "Load Excel"); }
            set
            {
                _LoadExcelButton = value;
                RaisePropertyChanged("LoadExcelButton");
            }
        }
        public ObservableCollection<Image> ImageName
        {
            get { return _image; }
            set
            {
                if (_image == value)
                    return;
                _image = value;
                RaisePropertyChanged("ImageName");
            }
        }
        public bool IsBusy
        {
            get
            {
                return _isBusy;
            }
            set
            {
                _isBusy = value;
                RaisePropertyChanged("IsBusy");
            }
        }
        public ObservableCollection<Error> Errors
        {
            get { return _error; }
            set
            {
                if (_error == value)
                    return;
                _error = value;
                RaisePropertyChanged("Errors");
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
        public string TxtStatus
        {
            get { return _text; }
            set
            {
                if (_text == value)
                    return;
                _text = value;
                RaisePropertyChanged("TxtStatus");
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
        public bool BtnLoadImages
        {
            get { return _btnLoadImages; }
            set
            {
                if (_btnLoadImages == value)
                    return;
                _btnLoadImages = value;
                RaisePropertyChanged("BtnLoadImages");
            }
        }
        public bool BtnLoadExcelBtn
        {
            get { return _btnLoadExcel; }
            set
            {
                if (_btnLoadExcel == value)
                    return;
                _btnLoadExcel = value;
                RaisePropertyChanged("BtnLoadExcelBtn");
            }
        }
        public bool BtnGenerateImportCsv
        {
            get { return _btnGenerateImportCsv; }
            set
            {
                if (_btnGenerateImportCsv == value)
                    return;
                _btnGenerateImportCsv = value;
                RaisePropertyChanged("BtnLoadImages");
            }
        }
        public ICommand LoadImagesCommand
        {
            get;
            private set;
        }
        public ICommand LoadExcelCommand
        {
            get;
            private set;
        }
        public ICommand CancelCommand
        {
            get;
            private set;
        }
        public ICommand GenerateCommand
        {
            get;
            private set;
        }
        private void InitializeCommands()
        {
            LoadExcelCommand = new RelayCommand(LoadDescriptions, null);
            LoadImagesCommand = new RelayCommand(LoadImages, null);
            CancelCommand = new RelayCommand(Cancel, null);
            GenerateCommand = new CommandHandler(() =>
            {
                this.worker.RunWorkerAsync();
            }, () =>
            {
                return !this.worker.IsBusy;
            });
        }
        private async void LoadImages()
        {
            _cancelToken = new CancellationTokenSource();
            BtnLoadImages = false;
            BtnCancel = true;
            TxtStatus = "Loading.....";
            try
            {
                var folderBrowserDlg = new FolderBrowserDialog
                {
                    ShowNewFolderButton = true
                };
                var dlgResult = folderBrowserDlg.ShowDialog();
                ImagePath = folderBrowserDlg.SelectedPath;
                await LoadImagesAsync(_cancelToken.Token);
                _errors.Clear();
                TxtStatus = "Operation Completed";
            }
            catch (OperationCanceledException ex)
            {
                TxtStatus = "Operation cancelled" + ex.Message;
            }
            catch (Exception ex)
            {
                TxtStatus = "Operation cancelled" + ex.Message;
            }
            finally
            {
                _cancelToken.Dispose();
                BtnLoadImages = true;
                BtnCancel = false;
            }
        }
        private void LoadDescriptions()
        {
            try
            {
                var dlg = new Microsoft.Win32.OpenFileDialog
                {
                    DefaultExt = ".xls"
                };
                dlg.ShowDialog();
                _descriptionsPath = dlg.FileName;
                LoadExcelButton = "File: " + Path.GetFileName(_descriptionsPath);
            }
            catch (OperationCanceledException ex)
            {
                TxtStatus = "Operation cancelled" + ex.Message;
            }
            catch (Exception ex)
            {
                TxtStatus = "Operation cancelled" + ex.Message;
            }
            finally
            {
                BtnLoadImages = true;
                BtnCancel = false;
            }
        }
        private void Cancel()
        {
            _cancelToken.Cancel();
        }

        private async Task LoadImagesAsync(CancellationToken ct)
        {
            _fileNames = new ObservableCollection<Image>();
            await Task.Run(() =>
            {
                var images = Directory.GetFiles(ImagePath).Select(Path.GetFileName).ToArray();
                foreach (var file in images)
                {
                    if (file.Length > 10)
                        _fileNames.Add(new Image() { ImageName = file, T2TRef = file.Substring(0, 6), ColourCode = file.Substring(6, 3), SizeCode = file.Substring(9, 3) });
                }
                ImageName = _fileNames;
            });
        }
        private void GenerateImportCsv(object sender, DoWorkEventArgs e)
        {
            _csv = new StringBuilder();
            _cancelToken = new CancellationTokenSource();
            BtnLoadImages = false;
            BtnGenerateImportCsv = false;
            BtnCancel = true;
            var bodyContent = new StringBuilder();
            var checkNumber = "00000";
            var batchNumber = 0;
            var batchInc = 19;
            var recCount = 3;
            var first = true;
            var usedSkuNums = new List<string>();
            _errors = new ObservableCollection<Error>();
            try
            {
                TxtStatus = "Removing exisiting import product file...";
                job = new ImportCsvJob(_descriptionsPath);
                //job.DoCleanup(ImagePath);
                TxtStatus = "Generating import product csv file, please wait this can take several minutes....";
                //var t2tRefs = new ImageService().ReadImageDetails(ImagePath);


                var t2tRefsDATABASE = new SkuService().GetAllCordnersStock();
                var t2tRefsDataSet = t2tRefsDATABASE.Tables[0].Select($"USER1 = 'W21'");
                var t2tRefs = new SkuService().OnlineSKUWithColourAndSize();
                worker.ReportProgress(1);
                var contains = new List<string>();
                var doesNotContains = new List<string>();
                foreach (DataRow dr in t2tRefsDataSet)
                {
                    for (var i = Convert.ToInt16(dr["MINSIZE"]); i <= Convert.ToInt16(dr["MAXSIZE"]); i++)
                    {



                        //if (string.IsNullOrEmpty(dr["LY" + i].ToString()))
                        //{
                        //    actualStock = dr["QTY" + i].ToString();
                        //}
                        //else
                        //{
                        //    actualStock =
                        //        (Convert.ToInt32(dr["QTY" + i]) - Convert.ToInt32(dr["LY" + i]))
                        //        .ToString();
                        //}

                        //if (actualStock == "" || actualStock == null)
                        //    actualStock = "0";

                        var append = (1000 + i).ToString();
                        var groupSkus2 = dr["NewStyle"] + append.Substring(1, 3);


                        var size = "";
                        size = i < 10 ? dr["S0" + i].ToString() : dr["S" + i].ToString();
                        if (size.Contains("½"))
                            size = size.Replace("½", ".5");
                        var sku = (dr["NewStyle"] + append.Substring(1, 3)).ToString();

                        if (t2tRefs.Contains(sku))
                        {
                            contains.Add(sku);
                        }
                        else
                        {
                            doesNotContains.Add(sku);
                            var newLine = BuildChildImportProduct(groupSkus2, dr, sku, "", size, "");
                            bodyContent.AppendLine(newLine);
                        }



                        //var shortDescription = BuildShortDescription(FetchDescription(descriptions, reff));
                        //var descripto = FetchDescription(descriptions, reff)?.Descriptio;

                            //var size = "";
                            //size = i < 10 ? dr["S0" + i].ToString() : dr["S" + i].ToString();
                            //if (size.Contains("½"))
                            //    size = size.Replace("½", ".5");
                            //var eanDataset = Query(groupSkus2, SqlQuery.GetEanCodes);
                            //string eanCode = null;
                            //if (eanDataset.Tables[0].Rows.Count != 0)
                            //{
                            //    eanCode = eanDataset.Tables[0].Rows[0]["EAN_CODE"].ToString();
                            //}

                            //string newLine = null;
                            //sizes.Add(dr["SIZERANGE"].ToString() + size);
                            ////if (Convert.ToInt32(actualStock) != 0)
                            ////{
                            //newLine = BuildChildImportProduct(groupSkus2, dr, descriptions, reff, shortDescription, actualStock, descripto, size, imageLists, t2TreFs, eanCode, parentSKU);
                            ////}

                            ////if (newLine != null)
                            ////{
                            //csvLines.AppendLine(newLine);
                            ////}
                            ////else if (Convert.ToInt32(actualStock) != 0)
                            ////{
                            ////    errors.Add(new Error()
                            ////    {
                            ////        RefNumber = reff,
                            ////        ErrorMessage = "No description found!"
                            ////    });
                            ////    break;
                            ////}

                            //actualStock = "0";

                    }

                    ////_cancelToken.Token.ThrowIfCancellationRequested();
                    //if (string.IsNullOrEmpty(_descriptionsPath))
                    //{
                    //    MessageBox.Show("Please select a descriptions file");
                    //    return;
                    //}

                    //if (!refff.Contains(checkNumber))
                    //{
                    //    //if (!usedSkuNums.Contains(refff.Substring(0, 6)))
                    //    //{
                    //        //usedSkuNums.Add(refff.Substring(0, 6));
                    //        batchNumber++;
                    //        var dateFromFolder = ImagePath.Split('\\');
                    //        var result = job.DoJob(refff, t2tRefs, ref _errors);
                    //        if (result.Length != 0)
                    //        {
                    //            bodyContent.AppendLine(result.ToString());
                    //        }
                    //        if (bodyContent.Length != 0)
                    //        {
                    //            _csv.AppendLine(bodyContent.ToString());
                    //            if (first)
                    //            {
                    //                var heead = new StringBuilder();
                    //                heead.AppendLine(headers);
                    //                first = false;
                    //                File.AppendAllText(
                    //                    System.Configuration.ConfigurationManager.AppSettings["ImportProductsOutput"] + " " + dateFromFolder[dateFromFolder.Length - 1].Trim() + "" + batchInc + ".csv",
                    //                    heead.ToString() );

                    //            }

                    //            File.AppendAllText(
                    //                System.Configuration.ConfigurationManager.AppSettings["ImportProductsOutput"] + " " + dateFromFolder[dateFromFolder.Length - 1].Trim() + "" + batchInc + ".csv",
                    //                _csv.ToString().Trim() + Environment.NewLine);

                    //        }
                    //        _csv = new StringBuilder();
                    //        bodyContent = new StringBuilder();
                    //        checkNumber = refff.Substring(0, 9);
                    //        worker.ReportProgress(recCount++);
                    //    //}
                }
                //}
                _csv.AppendLine(bodyContent.ToString());
                worker.ReportProgress(100);
                var unquieErrors = _errors.GroupBy(i => i.RefNumber).Select(i => i.First()).ToList();
                foreach (var error in unquieErrors)
                {
                    error.ErrorMessage = error.ErrorMessage;
                    _errors.Add(error);
                }
                Errors = _errors;

                var heead = new StringBuilder();
                heead.AppendLine(headers);
                first = false;
                File.AppendAllText(
                    System.Configuration.ConfigurationManager.AppSettings["ImportProductsOutput"] + " " + "21" + ".csv",
                    heead.ToString());

                File.AppendAllText(
                    System.Configuration.ConfigurationManager.AppSettings["ImportProductsOutput"] + " " + "21" + ".csv",
                    bodyContent.ToString().Trim() + Environment.NewLine);
                MessageBox.Show("Import Product File Generated");
            }
            catch (OperationCanceledException ex)
            {
                MessageBox.Show("Operation cancelled" + ex.Message);
            }
            catch (NullReferenceException ex)
            {
                MessageBox.Show("Operation cancelled. Please ensure Excel file is labelled Sheet1.");
                new LogWriter().LogWrite(ex.Message);
                new LogWriter().LogWrite(ex.StackTrace);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Operation cancelled" + ex.Message);
                new LogWriter().LogWrite(ex.StackTrace);
            }
            finally
            {
                _cancelToken.Dispose();
                BtnGenerateImportCsv = true;
                BtnCancel = false;
                BtnLoadImages = true;
            }
        }
        private void ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.ProgressValue = e.ProgressPercentage;
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


        private static string BuildChildImportProduct(string groupSkus2, DataRow dr, string reff, string descripto, string size, string eanCode)
        {
            //var result = FetchDescription(descriptions, reff);
            //if (result == null)
            //    return null;

            //var description = result?.Description;
            //if (string.IsNullOrEmpty(description))
            //    return null;
            //else
            //    description = description.TrimEnd();
            return new ImportProducts()
                                          .Setattribut_set("Default")
                                          .SetStore("admin")
                                          .SetWebsites()
                                          .Settype("simple")
                                          .Setsku(groupSkus2?.TrimEnd())
                                          .SethasOption("1")
                                          .SetName(dr["MasterSupplier"] + " " + "test" + " in " + dr["MasterColour"])
                                          .SetpageLayout("No layout updates.")
                                          .SetoptionsContainer("Product Info Column")
                                          .Setprice(dr["BASESELL"].ToString().Trim())
                                          .Setweight("0.01")
                                          .Setstatus("Enabled")
                                          .Setvisibility("Not Visible Individually")
                                          .SetshortDescription("test")
                                          .Setgty("0")
                                          .SetproductName(descripto?.Trim())
                                          .Setcolor(dr["MasterColour"].ToString().TrimEnd())
                                          .SetsizeRange(dr["SIZERANGE"].ToString() + size)
                                          .SettaxClass("Taxable Goods")
                                          .SetconfigurableAttribute("")
                                          .SetManufactoring(dr["MasterSupplier"].ToString())
                                          .Setcategory(null)
                                          .SetsubCategory(null)
                                          .Setseason("")
                                          .SetstockType(dr["MasterStocktype"].ToString())
                                          .Setimage("test.jpg")
                                          .SetsmallImage("test.jpg")
                                          .Setthumbnail("test.jpg")
                                          .Setcondition("new")
                                          .Setinfocare("")
                                          .Setsizeguide("")
                                          .Setrrp(dr["SELL"].ToString())
                                          .Seturl_key((dr["MasterSupplier"] + " " + "test" + " in " + dr["MasterColour"]).Replace(" ", "-").ToLower() + groupSkus2)
                                          .Seturl_path((dr["MasterSupplier"] + " " + "test" + " in " + dr["MasterColour"]).Replace(" ", "-").ToLower() + ".html")
                                          .Setean(eanCode)
                                          .Setrem1(GetREMValue(dr["REM2"].ToString()))
                                          .Setrem2(GetREMValue(dr["REM"].ToString()))
                                          .Setmodel(dr["SUPPREF"].ToString())
                                          .SetsuSKU(reff.Substring(0,9))
                                          .SetDescription("test")
                                          .SetParentSku(reff.Substring(0,6))
                                          .SetUDef(String.IsNullOrEmpty(dr["MasterSubDept"].ToString()) ? "" : dr["MasterSubDept"].ToString())
                                          .SetSType(String.IsNullOrEmpty(dr["MasterDept"].ToString()) ? "" : dr["MasterDept"].ToString())
                                          .Seteuro_special_price(GenerateEuroPrice(Convert.ToDecimal(dr["BASESELL"].ToString().Trim()), 1.18m))
                                          .Setusd_special_price("1")
                                          .ToString();
        }

        public static string GenerateEuroPrice(decimal gbp, decimal conversion_rate)
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
            var matchingItems = t2TreFs.Select(x => x).Where(y => y.Any(z => y.Contains(refff))).ToList();
            if (matchingItems.Count == 0)
                return "";
            return String.Join(",", matchingItems);
        }

        class ImportProducts
        {
            private string attribut_set;
            private string category;
            private string color;
            private string condition;
            private string configurableAttribute;
            private string description;
            private string ean;
            private string gallery;
            private string gty;
            private string hasOption;
            private string image;
            private string infocare;
            private string manufactor;
            private string model;
            private string name;
            private string optionsContainer;
            private string pageLayout;
            private string price;
            private string productName;
            private string rrp;
            private string rem1;
            private string rem2;
            private string season;
            private string shortDescription;
            private string sizeRange;
            private string sizeguide;
            private string sku;
            private string smallImage;
            private string status;
            private string stockType;
            private string store;
            private string subCategory;
            private string taxClass;
            private string thumbnail;
            private string type;
            private string url_key;
            private string url_path;
            private string visibility;
            private string websites;
            private string weight;
            private string suSKU;
            private string parentSku;
            private string stype;
            private string udef2;
            private string euro_special_price;
            private string usd_special_price;

            public ImportProducts SetParentSku(string parentSku)
            {
                this.parentSku = "\"" + parentSku + "\"";
                return this;
            }

            public ImportProducts SetName(string name)
            {
                this.name = "\"" + name + "\"";
                return this;
            }

            public ImportProducts SetStore(string store)
            {
                this.store = "\"\"";
                return this;
            }

            public ImportProducts SetWebsites()
            {
                this.websites = "\"" + "Main Website,Ireland Website" + "\"";
                return this;
            }

            public ImportProducts Setattribut_set(string attribut_set)
            {
                this.attribut_set = "\"" + attribut_set + "\"";
                return this;
            }

            public ImportProducts Settype(string type)
            {
                this.type = "\"" + type + "\"";
                return this;
            }

            public ImportProducts Setsku(string sku)
            {
                this.sku = "\"" + sku + "\"";
                return this;
            }

            public ImportProducts SethasOption(string hasOption)
            {
                this.hasOption = "\"" + hasOption + "\"";
                return this;
            }

            public ImportProducts SetoptionsContainer(string optionsContainer)
            {
                this.optionsContainer = "\"" + optionsContainer + "\"";
                return this;
            }

            public ImportProducts SetpageLayout(string pageLayout)
            {
                this.pageLayout = "\"" + pageLayout + "\"";
                return this;
            }

            public ImportProducts Setprice(string price)
            {
                this.price = "\"" + price + "\"";
                return this;
            }

            public ImportProducts Setweight(string weight)
            {
                this.weight = "\"0.01\"";
                return this;
            }

            public ImportProducts Setstatus(string status)
            {
                this.status = "\"Enabled\"";
                return this;
            }

            public ImportProducts Setvisibility(string visibility)
            {
                if (visibility != null)
                {
                    this.visibility = "\"" + visibility + "\"";
                }
                else
                {
                    this.visibility = "\"" + CordnersImportProductsBuilder.Visibility() + "\"";
                }
                return this;
            }

            public ImportProducts SetshortDescription(string shortDescription)
            {
                if (shortDescription == null)
                {
                    this.shortDescription = "\"\"";
                }
                else
                {
                    this.shortDescription = "\"" + shortDescription + "\"";
                }
                return this;
            }

            public ImportProducts Setgty(string gty)
            {
                this.gty = "\"" + gty + "\"";
                return this;
            }

            public ImportProducts SetproductName(string productName)
            {
                this.productName = "\"" + productName + "\"";
                return this;
            }

            public ImportProducts Setcolor(string color)
            {
                this.color = "\"" + color + "\"";
                return this;
            }

            public ImportProducts SetsizeRange(string sizeRange)
            {
                if (sizeRange == null)
                {
                    this.sizeRange = "\"\"";
                }
                else
                {
                    this.sizeRange = "\"" + sizeRange + "\"";
                }

                return this;
            }

            public ImportProducts SettaxClass(string taxClass)
            {
                this.taxClass = "\"" + taxClass + "\"";
                return this;
            }

            public ImportProducts SetconfigurableAttribute(string configurableAttribute)
            {
                this.configurableAttribute = "\"" + configurableAttribute + "\"";
                return this;
            }

            public ImportProducts SetManufactoring(string manufactor)
            {
                this.manufactor = "\"" + manufactor + "\"";
                return this;
            }

            public ImportProducts Setcategory(DataRow category)
            {
                if (category == null)
                {
                    this.category = "\"\"";
                }
                else
                {
                    this.category = "\"" + category.Category() + "\"";
                }
                return this;
            }

            public ImportProducts SetsubCategory(string subCategory)
            {
                if (subCategory == null)
                {
                    this.subCategory = "\"\"";
                }
                else
                {
                    this.subCategory = "\"" + subCategory.SubCategory() + "\"";
                }

                return this;
            }

            public ImportProducts Setseason(string season)
            {
                this.season = "\"" + season + "\"";
                return this;
            }

            public ImportProducts SetstockType(string stockType)
            {
                this.stockType = "\"" + stockType + "\"";
                return this;
            }

            public ImportProducts Setimage(string image)
            {
                this.image = "\"" + image + "\"";
                return this;
            }

            public ImportProducts SetsmallImage(string smallImage)
            {
                this.smallImage = "\"" + smallImage + "\"";
                return this;
            }

            public ImportProducts Setthumbnail(string thumbnail)
            {
                this.thumbnail = "\"" + thumbnail + "\"";
                return this;
            }

            public ImportProducts Setgallery(IEnumerable<string> t2TreFs, string gallery)
            {
                this.gallery = "\"" + CordnersImportProductsBuilder.BuildGalleryImages(t2TreFs, gallery) + "\"";
                return this;
            }

            public ImportProducts Setcondition(string condition)
            {
                this.condition = "\"new\"";
                return this;
            }

            public ImportProducts Setmodel(string model)
            {
                this.model = "\"" + model + "\"";
                return this;
            }

            public ImportProducts Setinfocare(string infocare)
            {
                this.infocare = "\"" + "row-product-featured-shoe-care" + "\"";
                return this;
            }

            public ImportProducts Setsizeguide(string sizeguide)
            {
                this.sizeguide = "\"" + "product_tab_size_guide" + "\"";
                return this;
            }

            public ImportProducts Setrrp(string rrp)
            {
                this.rrp = "\"" + rrp + "\"";
                return this;
            }

            public ImportProducts Seturl_key(string url_key)
            {
                this.url_key = "\"" + url_key + "\"";
                return this;
            }

            public ImportProducts Seturl_path(string url_path)
            {
                this.url_path = "\"" + url_path + "\"";
                return this;
            }

            public ImportProducts Setrem1(string rem1)
            {
                this.rem1 = "\"" + rem1 + "\"";
                return this;
            }

            public ImportProducts Setrem2(string rem2)
            {
                this.rem2 = "\"" + rem2 + "\"";
                return this;
            }

            public ImportProducts SetsuSKU(string suSKU)
            {
                this.suSKU = "\"" + suSKU + "\"";
                return this;
            }

            public ImportProducts SetDescription(string desc)
            {
                this.description = "\"" + desc + "\"";
                return this;
            }

            public ImportProducts Setean(string ean)
            {
                if (ean == null)
                {
                    this.ean = "\"\"";
                }
                else
                {
                    this.ean = "\"" + RemoveLineEndings(ean.Trim().Replace(",", "")) + "\"";
                }
                return this;
            }

            public ImportProducts SetUDef(string udef2)
            {
                if (udef2 == "")
                {
                    this.udef2 = "\" \"";
                }
                else
                {
                    this.udef2 = "\"" + udef2 + "\"";
                }

                return this;
            }

            public ImportProducts SetSType(string stype)
            {
                if (stype == "")
                {
                    this.stype = "\" \"";
                }
                else
                {
                    this.stype = "\"" + stype + "\"";
                }
                return this;
            }

            public ImportProducts Seteuro_special_price(string euro_special_price)
            {
                if (euro_special_price == "")
                {
                    this.euro_special_price = "\" \"";
                }
                else
                {
                    this.euro_special_price = "\"" + euro_special_price + "\"";
                }
                return this;
            }

            public ImportProducts Setusd_special_price(string usd_special_price)
            {
                if (usd_special_price == "")
                {
                    this.usd_special_price = "\" \"";
                }
                else
                {
                    this.usd_special_price = "\"" + usd_special_price + "\"";
                }
                return this;
            }

            public override string ToString()
            {
                return $"{sku},{store}," +
                              $"{websites},{attribut_set},{type},{hasOption},{name.TrimEnd()},{pageLayout},{optionsContainer},{price},{weight},{status},{visibility}," +
                              $"{shortDescription},{gty},{productName},{color}," +
                              $"{sizeRange},{taxClass},{configurableAttribute},{manufactor}," +
                              $"{category},{subCategory},{season},{stockType},{image},{smallImage},{thumbnail},{gallery},{condition},{ean}," +
                              $"{description},{model},{infocare},{sizeguide},{rrp},{url_key},{url_path},{rem1},{rem2},{suSKU},{parentSku},{udef2},{stype},{euro_special_price},{usd_special_price}";
            }

            public string RemoveLineEndings(string value)
            {
                if (String.IsNullOrEmpty(value))
                {
                    return "";
                }

                string lineSeparator = ((char)0x2028).ToString();
                string paragraphSeparator = ((char)0x2029).ToString();

                return value.Replace("\r\n", string.Empty)
                    .Replace("\n", string.Empty)
                    .Replace("\r", string.Empty)
                    .Replace(lineSeparator, string.Empty)
                    .Replace(paragraphSeparator, string.Empty);
            }
        }
    }
}