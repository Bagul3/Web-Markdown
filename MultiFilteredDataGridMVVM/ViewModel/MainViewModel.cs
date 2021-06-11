using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Common;
using DataService;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using MultiFilteredDataGridMVVM.Helpers;
using MultiFilteredDataGridMVVM.WpfElements;

namespace MultiFilteredDataGridMVVM.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        #region Members

        private DateTime _startDate;
        private DateTime _endDate;

        private Dictionary<string, object> _category;
        private Dictionary<string, object> _supplier;
        private Dictionary<string, object> _style;
        private Dictionary<string, object> _season;
        private Dictionary<string, object> _stockType;
        private Dictionary<string, object> _size;

        private Dictionary<string, object> _selectedSize;
        private Dictionary<string, object> _selectedSupplier;
        private Dictionary<string, object> _selectedCategory;
        private Dictionary<string, object> _selectedSeason;
        private Dictionary<string, object> _selectedStockType;
        private Dictionary<string, object> _selectedStyle;
        private Dictionary<string, object> _selectedColour;

        private ObservableCollection<SpecailOrders> _cordners;
        private ObservableCollection<SpecailOrders> _specailOrders = new ObservableCollection<SpecailOrders>();
        private bool _canCanRemoveSizeFilter;
        private bool _canCanRemoveSupplierFilter;
        private bool _canCanRemoveStyleFilter;
        private bool _canCanRemoveColourFilter;
        private bool _canCanRemoveStockTypeFilter;        

        private double _progressValue;

        private SpecailOrdersService _specailOrdersService;
        private BackgroundWorker worker;

        private Dictionary<string, object> _items;
        private Dictionary<string, object> _selectedItems;

        private bool _isBusy;

        private double _adjustmentPrice = 0.0;
        private int _adjustPricePercentage = 0;

        public ICommand StartCommand { get; private set; }


        private Dictionary<string, object> _supplierLoader;
        private Dictionary<string, object> _sizeLoader;
        private Dictionary<string, object> _categoryLoader;
        private Dictionary<string, object> _seasonLoader;
        private Dictionary<string, object> _stockTypeLoader;
        private Dictionary<string, object> _styleLoader;

        public double AdjustPrice
        {
            get
            {
                return _adjustmentPrice;

            }
            set
            {
                _adjustmentPrice = value;
                RaisePropertyChanged("AdjustPrice");
            }
        }

        public int AdjustPricePercentage
        {
            get
            {
                return _adjustPricePercentage;

            }
            set
            {
                _adjustPricePercentage = value;
                RaisePropertyChanged("AdjustPricePercentage");
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

        public Dictionary<string, object> Items
        {
            get
            {
                return _items;
            }
            set
            {
                _items = value;
                RaisePropertyChanged("Items");
            }
        }

        public Dictionary<string, object> SelectedItems
        {
            get
            {
                return _selectedItems;
            }
            set
            {
                _selectedItems = value;
                RaisePropertyChanged("SelectedItems");
            }
        }

        #endregion

        public MainViewModel()
        {
            try
            {
                IsBusy = true;

                this.worker = new BackgroundWorker
                {
                    WorkerReportsProgress = true
                };
                this.worker.DoWork += this.GenerateSpecailOrders;
                this.worker.ProgressChanged += this.ProgressChanged;

                Task.Factory.StartNew(() =>
                {
                    LoadData();
                }).ContinueWith((task) =>
                {
                    Supplier = _supplierLoader;
                    StockType = _stockTypeLoader;
                    Category = _categoryLoader;
                    Season = _seasonLoader;
                    Size = _sizeLoader;
                    Style = _styleLoader;
                    
                    IsBusy = false;
                }, TaskScheduler.FromCurrentSynchronizationContext());

                DoLoading();

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }            
        }

        private void DoLoading()
        { 
            IsBusy = true;
            StartDate = DateTime.Now;
            EndDate = DateTime.Now;
            _specailOrdersService = new SpecailOrdersService();
            ProgressValue = 0;
            InitializeCommands();
            Messenger.Default.Register<ViewCollectionViewSourceMessageToken>(this,
                Handle_ViewCollectionViewSourceMessageToken);
        }

        public override void Cleanup()
        {
            Messenger.Default.Unregister<ViewCollectionViewSourceMessageToken>(this);
            base.Cleanup();
        }

        /// <summary>
        /// Gets or sets the IDownloadDataService member
        /// </summary>
        internal IDataService DataService { get; set; }
        /// <summary>
        /// Gets or sets the CollectionViewSource which is the proxy for the 
        /// collection of Things and the datagrid in which each thing is displayed.
        /// </summary>
        private CollectionViewSource CVS { get; set; }

        private CollectionViewSource DiscountedStock { get; set; }

        #region Properties (Displayable in View)

        /// <summary>
        /// Gets or sets the primary collection of Thing objects to be displayed
        /// </summary>
        public ObservableCollection<SpecailOrders> Cordners
        {
            get { return _cordners; }
            set
            {
                if (_cordners == value)
                    return;
                _cordners = value;
                RaisePropertyChanged("Cordners");
            }
        }

        public ObservableCollection<SpecailOrders> SpecailOrders
        {
            get { return _specailOrders; }
            set
            {
                if (_specailOrders == value)
                    return;
                _specailOrders = value;
                RaisePropertyChanged("SpecailOrders");
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

        private IList _cordnersSelectedModels = new ArrayList();

        public IList CordnersCordnersSelected
        {
            get { return _cordnersSelectedModels; }
            set
            {
                _cordnersSelectedModels = value;
                RaisePropertyChanged("CordnersCordnersSelected");
            }
        }

        public DateTime StartDate
        {
            get { return _startDate; }
            set
            {
                _startDate = value;
                RaisePropertyChanged("StartDate");
            }
        }

        public DateTime EndDate
        {
            get { return _endDate; }
            set
            {
                _endDate = value;
                RaisePropertyChanged("EndDate");
            }
        }

        private IList _specailOrdersSelectedModels = new ArrayList();

        public IList SpecailSelected
        {
            get { return _specailOrdersSelectedModels; }
            set
            {
                _specailOrdersSelectedModels = value;
                RaisePropertyChanged("SpecailSelected");
            }
        }

        public Dictionary<string, object> SelectedSize
        {
            get { return _selectedSize; }
            set
            {
                if (_selectedSize == value)
                    return;
                _selectedSize = value;
                RaisePropertyChanged("SelectedSize");
                //ApplyFilter(!string.IsNullOrEmpty(_selectedColour) ? FilterField.Colour : FilterField.None);
            }
        }

        /// <summary>
        /// Gets or sets the selected author in the list countries to filter the collection
        /// </summary>
        public Dictionary<string, object> SelectedSupplier
        {
            get { return _selectedSupplier; }
            set
            {
                if (_selectedSupplier == value)
                    return;
                _selectedSupplier = value;


                foreach (var selected in _selectedSupplier)
                {
                    if ((bool)selected.Value)
                    {
                        ApplyFilter(FilterField.Supplier);
                        RaisePropertyChanged("SelectedSupplier");
                        return;
                    }
                }
                ApplyFilter(FilterField.None);
                RaisePropertyChanged("SelectedSupplier");
            }
        }

        public Dictionary<string, object> SelectedCategory
        {
            get { return _selectedCategory; }
            set
            {
                if (_selectedCategory == value)
                    return;
                _selectedCategory = value;


                foreach (var selected in _selectedCategory)
                {
                    if ((bool)selected.Value)
                    {
                        ApplyFilter(FilterField.Supplier);
                        RaisePropertyChanged("SelectedCategory");
                        return;
                    }
                }
                ApplyFilter(FilterField.None);
                RaisePropertyChanged("SelectedCategory");
            }
        }


        //        public Dictionary<string, object> SelectedSupplier
        //        {
        //            get { return _selectedSupplier; }
        //            set
        //            {
        //                if (_selectedSupplier == value)
        //                    return;
        //                _selectedSupplier = value;
        //                RaisePropertyChanged("SelectedSupplier");
        //                //ApplyFilter(!string.IsNullOrEmpty(_selectedSupplier) ? FilterField.Supplier : FilterField.None);
        //            }
        //        }

        /// <summary>
        /// Gets or sets the selected author in the list years to filter the collection
        /// </summary>
        public Dictionary<string, object> SelectedStyle
        {
            get { return _selectedStyle; }
            set
            {
                if (_selectedStyle == value)
                    return;
                _selectedStyle = value;
                RaisePropertyChanged("SelectedStyle");
                //ApplyFilter(!string.IsNullOrEmpty(_selectedStyle) ? FilterField.Style : FilterField.None);
            }
        }

        public Dictionary<string, object> SelectedColour
        {
            get { return _selectedColour; }
            set
            {
                if (_selectedColour == value)
                    return;
                _selectedColour = value;
                RaisePropertyChanged("SelectedColour");
                //ApplyFilter(!string.IsNullOrEmpty(_selectedColour) ? FilterField.Colour : FilterField.None);
            }
        }

        public Dictionary<string, object> SelectedStockType
        {
            get { return _selectedStockType; }
            set
            {
                if (_selectedStockType == value)
                    return;
                _selectedStockType = value;
                RaisePropertyChanged("SelectedStockType");
                //ApplyFilter(!string.IsNullOrEmpty(_selectedStockType) ? FilterField.StockType : FilterField.None);
            }
        }

        public Dictionary<string, object> SelectedSeasons
        {
            get { return _selectedSeason; }
            set
            {
                if (_selectedSeason == value)
                    return;
                _selectedSeason = value;
                RaisePropertyChanged("SelectedSeasons");
                //ApplyFilter(!string.IsNullOrEmpty(_selectedStockType) ? FilterField.StockType : FilterField.None);
            }
        }

        /// <summary>
        /// Gets or sets a list of authors which is used to populate the author filter
        /// drop down list.
        /// </summary>
        public Dictionary<string, object> Season
        {
            get { return _season; }
            set
            {
                if (_season == value)
                    return;
                _season = value;
                RaisePropertyChanged("Season");
            }
        }

        public Dictionary<string, object> Size
        {
            get { return _size; }
            set
            {
                if (_size == value)
                    return;
                _size = value;
                RaisePropertyChanged("Size");
            }
        }

        private string _GenerateButton;
        public string GenerateButton
        {
            get { return _GenerateButton ?? (_GenerateButton = "Generate"); }
            set
            {
                _GenerateButton = value;
                RaisePropertyChanged("GenerateButton");
            }
        }
        /// <summary>
        /// Gets or sets a list of authors which is used to populate the country filter
        /// drop down list.
        /// </summary>
        //        public ObservableCollection<string> Supplier
        //        {
        //            get { return _supplier; }
        //            set
        //            {
        //                if (_supplier == value)
        //                    return;
        //                _supplier = value;
        //                RaisePropertyChanged("Supplier");
        //            }
        //        }

        public Dictionary<string, object> Supplier
        {
            get { return _supplier; }
            set
            {
                if (_supplier == value)
                    return;
                _supplier = value;
                RaisePropertyChanged("Supplier");
            }
        }

        public Dictionary<string, object> Category
        {
            get { return _category; }
            set
            {
                if (_category == value)
                    return;
                _category = value;
                RaisePropertyChanged("Category");
            }
        }

        /// <summary>
        /// Gets or sets a list of authors which is used to populate the year filter
        /// drop down list.
        /// </summary>
        public Dictionary<string, object> Style
        {
            get { return _style; }
            set
            {
                if (_style == value)
                    return;
                _style = value;
                RaisePropertyChanged("Style");
            }
        }

        public Dictionary<string, object> StockType
        {
            get { return _stockType; }
            set
            {
                if (_stockType == value)
                    return;
                _stockType = value;
                RaisePropertyChanged("StockType");
            }
        }


        /// <summary>
        /// Gets or sets a flag indicating if the Country filter, if applied, can be removed.
        /// </summary>
        public bool CanRemoveSizeFilter
        {
            get { return _canCanRemoveSizeFilter; }
            set
            {
                _canCanRemoveSizeFilter = value;
                RaisePropertyChanged("CanRemoveSizeFilter");
            }
        }

        /// <summary>
        /// Gets or sets a flag indicating if the Author filter, if applied, can be removed.
        /// </summary>
        public bool CanRemoveSupplierFilter
        {
            get { return _canCanRemoveSupplierFilter; }
            set
            {
                _canCanRemoveSupplierFilter = value;
                RaisePropertyChanged("CanRemoveSupplierFilter");
            }
        }
        /// <summary>
        /// Gets or sets a flag indicating if the Year filter, if applied, can be removed.
        /// </summary>
        public bool CanRemoveStyleFilter
        {
            get { return _canCanRemoveStyleFilter; }
            set
            {
                _canCanRemoveStyleFilter = value;
                RaisePropertyChanged("CanRemoveStyleFilter");
            }
        }

        public bool CanRemoveColourFilter
        {
            get { return _canCanRemoveColourFilter; }
            set
            {
                _canCanRemoveColourFilter = value;
                RaisePropertyChanged("CanRemoveColourFilter");
            }
        }

        public bool CanRemoveStockTypeFilter
        {
            get { return _canCanRemoveStockTypeFilter; }
            set
            {
                _canCanRemoveStockTypeFilter = value;
                RaisePropertyChanged("CanRemoveStockTypeFilter");
            }
        }

        #endregion

        #region Commands

        public ICommand ResetFiltersCommand
        {
            get;
            private set;
        }
        public ICommand RemoveCountryFilterCommand
        {
            get;
            private set;
        }
        public ICommand RemoveAuthorFilterCommand
        {
            get;
            private set;
        }
        public ICommand RemoveYearFilterCommand
        {
            get;
            private set;
        }

        public ICommand RemoveSizeFilterCommand
        {
            get;
            private set;
        }
        public ICommand RemoveSupplierFilterCommand
        {
            get;
            private set;
        }
        public ICommand RemoveStyleFilterCommand
        {
            get;
            private set;
        }
        public ICommand RemoveColourFilterCommand
        {
            get;
            private set;
        }

        public ICommand RemoveStockTypeFilterCommand
        {
            get;
            private set;
        }

        public ICommand AddSelectedItems
        {
            get;
            private set;
        }

        public ICommand RemoveSelectedItems
        {
            get;
            private set;
        }

        public ICommand Generate
        {
            get;
            private set;
        }

        public ICommand GetStock
        {
            get;
            private set;
        }

        #endregion

        private void InitializeCommands()
        {
                ResetFiltersCommand = new RelayCommand(ResetFilters, null);
            RemoveSizeFilterCommand = new RelayCommand(RemoveSizeFilter, () => CanRemoveSizeFilter);
            RemoveSupplierFilterCommand = new RelayCommand(RemoveSupplierFilter, () => CanRemoveSupplierFilter);
            RemoveStyleFilterCommand = new RelayCommand(RemoveStyleFilter, () => CanRemoveStyleFilter);
            RemoveColourFilterCommand = new RelayCommand(RemoveColourFilter, () => CanRemoveColourFilter);
            RemoveStockTypeFilterCommand = new RelayCommand(RemoveStockTypeFilter, () => CanRemoveStockTypeFilter);
            AddSelectedItems = new RelayCommand(AddSelectedListItems, null);
            RemoveSelectedItems = new RelayCommand(RemoveSelectedListItems, null);
            GetStock = new RelayCommand(UpdateStockList, null);

            Generate = new CommandHandler(() =>
            {
                this.worker.RunWorkerAsync();
            }, () =>
            {
                return !this.worker.IsBusy;
            });
        }

        public void AddSelectedListItems()
        {
            var selectedList = new ArrayList(CordnersCordnersSelected);
            foreach (var o in selectedList.ToArray())
            {
                SpecailOrders.Add(o as SpecailOrders);
                Cordners.Remove(o as SpecailOrders);
            }
        }

        public void UpdateStockList()
        {
            try
            {
                Cordners.Clear();
                List<SpecailOrders> selectedList = new List<SpecailOrders>();
                IsBusy = true;
                Task.Factory.StartNew(() =>
                {
                    selectedList = _specailOrdersService.GetSaleStock(SelectedSupplier, SelectedCategory, SelectedSeasons, SelectedStyle, SelectedStockType);
                }).ContinueWith((task) =>
                {
                    foreach (var o in selectedList.ToArray().Distinct())
                    {
                        Cordners.Add(o);
                    }
                    IsBusy = false;
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }    
            catch (Exception e)
            {
                new LogWriter().LogWrite(e.Message);
                new LogWriter().LogWrite(e.StackTrace);
                MessageBox.Show(e.Message);
            }
        }

        public async void GenerateSpecailOrders(object sender, DoWorkEventArgs e)
        {
            try
            {
                GenerateButton = "Generating...";
                var count = 0;
                var csv = new StringBuilder();
                var headers = $"{"sku"},{"special_price"},{"special_price-1"},{"special_from_date"},{"special_from_date-1"},{"special_to_date"},{"special_to_date-1"},{"RRP"}";
                csv.AppendLine(headers);
                var stamp = DateTime.Now.Millisecond;
                File.AppendAllText(System.Configuration.ConfigurationManager.AppSettings["SalesPriceOutput"] + stamp + ".csv", csv.ToString());

                // Load all sku data;
                var skuData = _specailOrdersService.RetrieveAllSkuData();

                if (SpecailOrders.Count() == 0)
                {
                    MessageBox.Show("No Stock Items have been selected. \n Please move columns to the right for processing.");
                    return;
                }

                foreach (var specailOrder in SpecailOrders)
                {
                    if (count >= 100)
                    {
                        count = 1;
                    }
                    count++;
                    List<DataRow> dataRows = skuData.Tables[0].AsEnumerable().Where(x => (string)x["NEWSTYLE"] == specailOrder.NEWSTYLE).Distinct().ToList();

                    _specailOrdersService.GenerateCSVAsync(specailOrder, StartDate.ToString("yyyy-MM-dd"), EndDate.ToString("yyyy-MM-dd"), 
                        stamp,
                        dataRows,
                        Convert.ToDecimal(AdjustPrice), AdjustPricePercentage);
                    worker.ReportProgress(count);
                }
                worker.ReportProgress(100);
                AdjustPrice = 0;
                GenerateButton = "Generate";
                MessageBox.Show("Sales Price CSV Generated to Input/Output Folder");
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }           
        }

        public void RemoveSelectedListItems()
        {
            var selectedList = new ArrayList(SpecailSelected);
            foreach (var o in selectedList.ToArray())
            {
                SpecailOrders.Remove(o as SpecailOrders);
                Cordners.Add(o as SpecailOrders);
            }
        }

        public void ResetFilters()
        {
            // clear filters 
            RemoveSizeFilter();
            RemoveSupplierFilter();
            RemoveStyleFilter();
            RemoveColourFilter();
            RemoveStockTypeFilter();
        }
        public void RemoveSizeFilter()
        {
            CVS.Filter -= new FilterEventHandler(FilterBySize);
            SelectedSize = null;
            CanRemoveSizeFilter = false;
        }
        public void RemoveSupplierFilter()
        {
            CVS.Filter -= new FilterEventHandler(FilterBySupplier);
            SelectedSupplier = null;
            CanRemoveSupplierFilter = false;
            //Cordner.View.Refresh();
        }
        public void RemoveStyleFilter()
        {
            CVS.Filter -= new FilterEventHandler(FilterByStyle);
            SelectedStyle = null;
            CanRemoveStyleFilter = false;
        }

        public void RemoveColourFilter()
        {
            CVS.Filter -= new FilterEventHandler(FilterByColour);
            SelectedColour = null;
            CanRemoveColourFilter = false;
        }

        public void RemoveStockTypeFilter()
        {
            CVS.Filter -= new FilterEventHandler(FilterByStockType);
            SelectedStockType = null;
            CanRemoveStockTypeFilter = false;
        }

        private void LoadData()
        {
            IsBusy = true;

            //await Task.Factory.StartNew(() =>
            //{
                
                _sizeLoader = new Dictionary<string, object>();
                _supplierLoader = new Dictionary<string, object>();
                _seasonLoader = new Dictionary<string, object>();
                _categoryLoader = new Dictionary<string, object>();
                _styleLoader = new Dictionary<string, object>();
                _stockTypeLoader = new Dictionary<string, object>();
                Cordners = new ObservableCollection<SpecailOrders>();

                var things = new SpecailOrdersService().GetCordners();

                var q1 = from t in things
                            select t.Season;
                var obj = q1.Distinct().Zip(q1.Distinct(), (k, v) => new { k, v })
                    .ToDictionary(x => x.k, x => x.v).OrderBy(x => x.Key);

                foreach (var ob in obj)
                {
                    _seasonLoader.Add(ob.Key, ob.Value);
                }

                var q2 = from t in things
                            select t.MasterSupplier;
                obj = q2.Distinct().Zip(q2.Distinct(), (k, v) => new { k, v })
                    .ToDictionary(x => x.k, x => x.v).OrderBy(x => x.Key);

                foreach (var ob in obj)
                {
                    _supplierLoader.Add(ob.Key, ob.Value);
                }

                var q3 = from t in things
                            select t.Category;
                obj = q3.Distinct().Zip(q3.Distinct(), (k, v) => new { k, v })
                    .ToDictionary(x => x.k, x => x.v).OrderBy(x => x.Key);

                foreach (var ob in obj)
                {
                    _categoryLoader.Add(ob.Key, ob.Value);
                }

                var q4 = from t in things
                            select t.Style;
                obj = q4.Distinct().Zip(q4.Distinct(), (k, v) => new { k, v })
                    .ToDictionary(x => x.k, x => x.v).OrderBy(x => x.Key);

                foreach (var ob in obj)
                {
                    _styleLoader.Add(ob.Key, ob.Value);
                }

                var q5 = from t in things
                            select t.StockType;
                obj = q5.Distinct().Zip(q5.Distinct(), (k, v) => new { k, v })
                    .ToDictionary(x => x.k, x => x.v).OrderBy(x => x.Key);

                foreach (var ob in obj)
                {
                    _stockTypeLoader.Add(ob.Key, ob.Value);
                }
            //}).ContinueWith((task) =>
            //{
            //    IsBusy = false;
            //}, TaskScheduler.FromCurrentSynchronizationContext());
        }
        /// <summary>
        /// This method handles a message recieved from the View which enables a reference to the
        /// instantiated CollectionViewSource to be used in the ViewModel.
        /// </summary>
        private void Handle_ViewCollectionViewSourceMessageToken(ViewCollectionViewSourceMessageToken token)
        {
            CVS = token.CVS;
            DiscountedStock = token.DiscountedStock;
        }

        // Command methods (called by the commands) ===============

        public void AddSizeFilter()
        {
            // see Notes on Adding Filters:
            if (CanRemoveSizeFilter)
            {
                CVS.Filter -= new FilterEventHandler(FilterBySize);
                CVS.Filter += new FilterEventHandler(FilterBySize);
            }
            else
            {
                CVS.Filter += new FilterEventHandler(FilterBySize);
                CanRemoveSizeFilter = true;
            }
        }
        public void AddSupplierFilter()
        {
            // see Notes on Adding Filters:
            if (CanRemoveSupplierFilter)
            {
                CVS.Filter -= new FilterEventHandler(FilterBySupplier);
                CVS.Filter += new FilterEventHandler(FilterBySupplier);
            }
            else
            {
                CVS.Filter += new FilterEventHandler(FilterBySupplier);
                CanRemoveSupplierFilter = true;
            }
        }
        public void AddStyleFilter()
        {
            // see Notes on Adding Filters:
            if (CanRemoveStyleFilter)
            {
                CVS.Filter -= new FilterEventHandler(FilterByStyle);
                CVS.Filter += new FilterEventHandler(FilterByStyle);
            }
            else
            {
                CVS.Filter += new FilterEventHandler(FilterByStyle);
                CanRemoveStyleFilter = true;
            }
        }

        public void AddColourFilter()
        {
            // see Notes on Adding Filters:
            if (CanRemoveColourFilter)
            {
                CVS.Filter -= new FilterEventHandler(FilterByColour);
                CVS.Filter += new FilterEventHandler(FilterByColour);
            }
            else
            {
                CVS.Filter += new FilterEventHandler(FilterByColour);
                CanRemoveColourFilter = true;
            }
        }

        public void AddStockTypeFilter()
        {
            // see Notes on Adding Filters:
            if (CanRemoveStockTypeFilter)
            {
                CVS.Filter -= new FilterEventHandler(FilterByStockType);
                CVS.Filter += new FilterEventHandler(FilterByStockType);
            }
            else
            {
                CVS.Filter += new FilterEventHandler(FilterByStockType);
                CanRemoveStockTypeFilter = true;
            }
        }

        /* Notes on Filter Methods:
         * When using multiple filters, do not explicitly set anything to true.  Rather,
         * only hide things which do not match the filter criteria
         * by setting e.Accepted = false.  If you set e.Accept = true, if effectively
         * clears out any previous filters applied to it.  
         */

        private void FilterBySize(object sender, FilterEventArgs e)
        {
            // see Notes on Filter Methods:
            var src = e.Item as SpecailOrders;
            if (src == null)
                e.Accepted = false;
            //            else if (string.Compare(SelectedSize, src.Size) != 0)
            //                e.Accepted = false;
        }
        private void FilterBySupplier(object sender, FilterEventArgs e)
        {
            // see Notes on Filter Methods:
            var src = e.Item as SpecailOrders;
            if (src == null)
                e.Accepted = false;
            foreach (var selected in SelectedSupplier)
            {
                if (String.CompareOrdinal(selected.Key, src.MasterSupplier) != 0)
                    e.Accepted = false;
            }

        }
        private void FilterByStyle(object sender, FilterEventArgs e)
        {
            // see Notes on Filter Methods:
            var src = e.Item as SpecailOrders;
            if (src == null)
                e.Accepted = false;
            //else if (string.Compare(SelectedStyle, src.Style) != 0)
            //    e.Accepted = false;
        }
        private void FilterByColour(object sender, FilterEventArgs e)
        {
            // see Notes on Filter Methods:
            var src = e.Item as SpecailOrders;
            if (src == null)
                e.Accepted = false;
            //else if (string.Compare(SelectedColour, src.Color) != 0)
            //    e.Accepted = false;
        }
        private void FilterByStockType(object sender, FilterEventArgs e)
        {
            // see Notes on Filter Methods:
            var src = e.Item as SpecailOrders;
            if (src == null)
                e.Accepted = false;
            //else if (string.Compare(SelectedStockType, src.StockType) != 0)
            //    e.Accepted = false;
        }

        private enum FilterField
        {
            Size,
            Supplier,
            Style,
            Colour,
            StockType,
            None
        }
        private void ApplyFilter(FilterField field)
        {
            switch (field)
            {
                case FilterField.Size:
                    AddSizeFilter();
                    break;
                case FilterField.Supplier:
                    AddSupplierFilter();
                    break;
                case FilterField.Style:
                    AddStyleFilter();
                    break;
                case FilterField.Colour:
                    AddColourFilter();
                    break;
                case FilterField.StockType:
                    AddStockTypeFilter();
                    break;
                default:
                    break;
            }
        }

        private void ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.ProgressValue = e.ProgressPercentage;
        }
    }
}
