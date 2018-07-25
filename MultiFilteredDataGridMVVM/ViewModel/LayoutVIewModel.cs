using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    class LayoutViewModel : ViewModelBase
    {
        private DateTime _startDate;
        private DateTime _endDate;

        private ObservableCollection<string> _size;
        private ObservableCollection<string> _supplier;
        private ObservableCollection<string> _style;
        private ObservableCollection<string> _colour;
        private ObservableCollection<string> _stockType;

        private string _selectedSize;
        private string _selectedSupplier;
        private string _selectedStyle;
        private string _selectedColour;
        private string _selectedStockType;

        private ObservableCollection<SpecailOrders> _cordners;
        private ObservableCollection<SpecailOrders> _specailOrders = new ObservableCollection<SpecailOrders>();
        private bool _canCanRemoveSizeFilter;
        private bool _canCanRemoveSupplierFilter;
        private bool _canCanRemoveStyleFilter;
        private bool _canCanRemoveColourFilter;
        private bool _canCanRemoveStockTypeFilter;

        private double _progressValue;

        private readonly SpecailOrdersService _specailOrdersService;
        private BackgroundWorker worker;


        public LayoutViewModel(IDataService dataService)
        {
            StartDate = DateTime.Now;
            EndDate = DateTime.Now;
            _specailOrdersService = new SpecailOrdersService();
            ProgressValue = 0;
            InitializeCommands();
            DataService = dataService;
            LoadData();

            this.worker = new BackgroundWorker();
            this.worker.WorkerReportsProgress = true;
            this.worker.DoWork += this.GenerateSpecailOrders;
            this.worker.ProgressChanged += this.ProgressChanged;
            //--------------------------------------------------------------
            // This 'registers' the instance of this view model to recieve messages with this type of token.  This 
            // is used to recieve a reference from the view that the collectionViewSource has been instantiated
            // and to recieve a reference to the CollectionViewSource which will be used in the view model for 
            // filtering
            Messenger.Default.Register<ViewCollectionViewSourceMessageToken>(this, Handle_ViewCollectionViewSourceMessageToken);
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

        public string SelectedSize
        {
            get { return _selectedSize; }
            set
            {
                if (_selectedSize == value)
                    return;
                _selectedSize = value;
                RaisePropertyChanged("SelectedSize");
                ApplyFilter(!string.IsNullOrEmpty(_selectedSize) ? MainViewModel.FilterField.Size : MainViewModel.FilterField.None);
            }
        }
        /// <summary>
        /// Gets or sets the selected author in the list countries to filter the collection
        /// </summary>
        public string SelectedSupplier
        {
            get { return _selectedSupplier; }
            set
            {
                if (_selectedSupplier == value)
                    return;
                _selectedSupplier = value;
                RaisePropertyChanged("SelectedSupplier");
                ApplyFilter(!string.IsNullOrEmpty(_selectedSupplier) ? MainViewModel.FilterField.Supplier : MainViewModel.FilterField.None);
            }
        }
        /// <summary>
        /// Gets or sets the selected author in the list years to filter the collection
        /// </summary>
        public string SelectedStyle
        {
            get { return _selectedStyle; }
            set
            {
                if (_selectedStyle == value)
                    return;
                _selectedStyle = value;
                RaisePropertyChanged("SelectedStyle");
                ApplyFilter(!string.IsNullOrEmpty(_selectedStyle) ? MainViewModel.FilterField.Style : MainViewModel.FilterField.None);
            }
        }

        public string SelectedColour
        {
            get { return _selectedColour; }
            set
            {
                if (_selectedColour == value)
                    return;
                _selectedColour = value;
                RaisePropertyChanged("SelectedColour");
                ApplyFilter(!string.IsNullOrEmpty(_selectedColour) ? MainViewModel.FilterField.Colour : MainViewModel.FilterField.None);
            }
        }

        public string SelectedStockType
        {
            get { return _selectedStockType; }
            set
            {
                if (_selectedStockType == value)
                    return;
                _selectedStockType = value;
                RaisePropertyChanged("SelectedStockType");
                ApplyFilter(!string.IsNullOrEmpty(_selectedStockType) ? MainViewModel.FilterField.StockType : MainViewModel.FilterField.None);
            }
        }

        /// <summary>
        /// Gets or sets a list of authors which is used to populate the author filter
        /// drop down list.
        /// </summary>
        public ObservableCollection<string> Colour
        {
            get { return _colour; }
            set
            {
                if (_colour == value)
                    return;
                _colour = value;
                RaisePropertyChanged("Colour");
            }
        }
        /// <summary>
        /// Gets or sets a list of authors which is used to populate the country filter
        /// drop down list.
        /// </summary>
        public ObservableCollection<string> Supplier
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

        public ObservableCollection<string> Size
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

        /// <summary>
        /// Gets or sets a list of authors which is used to populate the year filter
        /// drop down list.
        /// </summary>
        public ObservableCollection<string> Style
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

        public ObservableCollection<string> StockType
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

        public async void GenerateSpecailOrders(object sender, DoWorkEventArgs e)
        {
            worker.ReportProgress(50);
            _specailOrdersService.GenerateCSVAsync(SpecailOrders, StartDate.ToString(), EndDate.ToString());
            worker.ReportProgress(100);
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

        private async Task LoadData()
        {
            var things = await _specailOrdersService.GetCordners();
            var q1 = from t in things
                     select t.Color;
            Colour = new ObservableCollection<string>(q1.Distinct());

            var q2 = from t in things
                     select t.MasterSupplier;
            Supplier = new ObservableCollection<string>(q2.Distinct());

            var q3 = from t in things
                     select t.Size;
            Size = new ObservableCollection<string>(q3.Distinct());

            var q4 = from t in things
                     select t.Style;
            Style = new ObservableCollection<string>(q4.Distinct());

            var q5 = from t in things
                     select t.StockType;
            StockType = new ObservableCollection<string>(q5.Distinct());

            Cordners = new ObservableCollection<SpecailOrders>(things);
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
            else if (string.Compare(SelectedSize, src.Size) != 0)
                e.Accepted = false;
        }
        private void FilterBySupplier(object sender, FilterEventArgs e)
        {
            // see Notes on Filter Methods:
            var src = e.Item as SpecailOrders;
            if (src == null)
                e.Accepted = false;
            else if (string.Compare(SelectedSupplier, src.MasterSupplier) != 0)
                e.Accepted = false;
        }
        private void FilterByStyle(object sender, FilterEventArgs e)
        {
            // see Notes on Filter Methods:
            var src = e.Item as SpecailOrders;
            if (src == null)
                e.Accepted = false;
            else if (string.Compare(SelectedStyle, src.Style) != 0)
                e.Accepted = false;
        }
        private void FilterByColour(object sender, FilterEventArgs e)
        {
            // see Notes on Filter Methods:
            var src = e.Item as SpecailOrders;
            if (src == null)
                e.Accepted = false;
            else if (string.Compare(SelectedColour, src.Color) != 0)
                e.Accepted = false;
        }
        private void FilterByStockType(object sender, FilterEventArgs e)
        {
            // see Notes on Filter Methods:
            var src = e.Item as SpecailOrders;
            if (src == null)
                e.Accepted = false;
            else if (string.Compare(SelectedStockType, src.StockType) != 0)
                e.Accepted = false;
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
        private void ApplyFilter(MainViewModel.FilterField field)
        {
            switch (field)
            {
                case MainViewModel.FilterField.Size:
                    AddSizeFilter();
                    break;
                case MainViewModel.FilterField.Supplier:
                    AddSupplierFilter();
                    break;
                case MainViewModel.FilterField.Style:
                    AddStyleFilter();
                    break;
                case MainViewModel.FilterField.Colour:
                    AddColourFilter();
                    break;
                case MainViewModel.FilterField.StockType:
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
