using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using Common;
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
        private bool _isBusy;
        private string headers = $"{"sku"},{"store_view_codes"},{"websites"},{"attribut_set"},{"type"},{"has_options"},{"name"},{"page_layout"},{"options_container"},{"price"},{"weight"},{"status"},{"visibility"},{"short_description"},{"qty"},{"product_name"},{"color"}," +
        $"{"size"},{"tax_class_id"},{"code"},{"configurable_attributes"},{"manufacturer"},{"categories"},{"sub_categories"},{"season"},{"stock_type"},{"image"},{"small_image"},{"thumbnail"},{"gallery"}," +
        $"{"condition"},{"ean"},{"description"},{"model"},{"infocare"},{"sizeguide"},{"RRP"},{"url_key"},{"url_path"},{"rem"},{"rem2"},{"susku"},{"parent_sku"},{"uDef2"},{"d_type"},{"euro_price"},{"usd_price"},{"aud_price"},{"new_from_date"},{"new_to_date"},{"created_date"}";
        public ImportProductViewModel()
        {
            InitializeCommands();
            _fileNames = new ObservableCollection<Image>();
            _error = new ObservableCollection<Error>();
            _btnCancel = false;
            _btnLoadImages = true;

            _csv = new StringBuilder();
            _errors = new ObservableCollection<Error>();
            ImageName = new ObservableCollection<Image>();
            this.worker = new BackgroundWorker();
            this.worker.WorkerReportsProgress = true;
            this.worker.DoWork += this.GenerateImportCsv;
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
            var batchInc = 19;
            var recCount = 3;
            var usedSkuNums = new List<string>();
            _errors = new ObservableCollection<Error>();
            try
            {
                TxtStatus = "Removing exisiting import product file...";
                job = new ImportCsvJob(_descriptionsPath);
                job.DoCleanup(ImagePath);
                TxtStatus = "Generating import product csv file, please wait this can take several minutes....";
                var t2tRefs = new ImageService().ReadImageDetails(ImagePath);
                worker.ReportProgress(1);
                var dateFromFolder = ImagePath.Split('\\');
                buildHeader(batchInc, dateFromFolder);
                foreach (var refff in t2tRefs.Distinct())
                {
                    if (string.IsNullOrEmpty(_descriptionsPath))
                    {
                        MessageBox.Show("Please select a descriptions file");
                        return;
                    }

                    if (!usedSkuNums.Contains(refff.Substring(0, 6)))
                    {
                        usedSkuNums.Add(refff.Substring(0, 6));
                        buildBody(bodyContent, batchInc, t2tRefs, dateFromFolder, refff);
                        _csv = new StringBuilder();
                        bodyContent = new StringBuilder();
                        worker.ReportProgress(recCount++);
                    }
                }
                worker.ReportProgress(100);
                LogErrors();
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
                job = null;
                _cancelToken.Dispose();
                BtnGenerateImportCsv = true;
                BtnCancel = false;
                BtnLoadImages = true;
            }
        }

        private void LogErrors()
        {
            var unquieErrors = _errors.GroupBy(i => i.RefNumber).Select(i => i.First()).ToList();
            foreach (var error in unquieErrors)
            {
                error.ErrorMessage = error.ErrorMessage;
                _errors.Add(error);
            }
            Errors = _errors;
        }

        private void buildBody(StringBuilder bodyContent, int batchInc, IEnumerable<string> t2tRefs, string[] dateFromFolder, string refff)
        {
            var result = job.DoJob(refff, t2tRefs, ref _errors);

            if (result.Length != 0)
            {
                bodyContent.AppendLine(result.ToString());
            }
            if (bodyContent.Length != 0)
            {
                _csv.AppendLine(bodyContent.ToString());

                File.AppendAllText(
                    System.Configuration.ConfigurationManager.AppSettings["ImportProductsOutput"] + " " + dateFromFolder[dateFromFolder.Length - 1].Trim() + "" + batchInc + ".csv",
                    _csv.ToString().Trim() + Environment.NewLine);

            }
        }

        private void buildHeader(int batchInc, string[] dateFromFolder)
        {
            var heead = new StringBuilder();
            heead.AppendLine(headers);
            File.AppendAllText(
                System.Configuration.ConfigurationManager.AppSettings["ImportProductsOutput"] + " " + dateFromFolder[dateFromFolder.Length - 1].Trim() + "" + batchInc + ".csv",
                heead.ToString());
        }

        private void ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.ProgressValue = e.ProgressPercentage;
        }
    }
}