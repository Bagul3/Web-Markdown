﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
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
using ServiceStack;

namespace MultiFilteredDataGridMVVM.ViewModel
{

    public class SkuViewModel : ViewModelBase
    {
        #region Members

        private string _selectedAuthor;
        private string _selectedCountry;
        private string _selectedYear;

        private string _selectedColour;
        private string _selectedStyle;

        private string _selectedStockType;

        private ObservableCollection<string> _authors;
        private ObservableCollection<string> _countries;
        private ObservableCollection<string> _years;

        private ObservableCollection<string> _colour;
        private ObservableCollection<string> _style;
        private ObservableCollection<string> _stockType;

        ObservableCollection<SpecailOrders> _things;
        ObservableCollection<SpecailOrders> _online;
        private bool _canCanRemoveCountryFilter;
        private bool _canCanRemoveAuthorFilter;
        private bool _canCanRemoveYearFilter;

        private bool _canCanRemoveColourFilter;
        private bool _canCanRemoveStyleFilter;
        private bool _canCanRemoveStockTypeFilter;
        #endregion
        public ICommand StartCommand { get; private set; }
        private BackgroundWorker worker;
        private double _progressValue;
        private bool _isBusy;
        private SkuService _skuService;
        public SkuViewModel(IDataService dataService)
        {
            this.worker = new BackgroundWorker();
            this.worker.WorkerReportsProgress = true;
            this.worker.ProgressChanged += this.ProgressChanged;
            _skuService = new SkuService();
            InitializeCommands();
            DataService = dataService;
            LoadData();
            StartCommand = new CommandHandler(() =>
            {
                this.worker.RunWorkerAsync();
            }, () =>
            {
                return !this.worker.IsBusy;
            });
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
        private CollectionViewSource X_Online { get; set; }


        #region Properties (Displayable in View)

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

        /// <summary>
        /// Gets or sets the primary collection of Thing objects to be displayed
        /// </summary>
        public ObservableCollection<SpecailOrders> Things
        {
            get { return _things; }
            set
            {
                if (_things == value)
                    return;
                _things = value;
                RaisePropertyChanged("Things");
            }
        }

        public ObservableCollection<SpecailOrders> Online
        {
            get { return _online; }
            set
            {
                if (_online == value)
                    return;
                _online = value;
                RaisePropertyChanged("Online");
            }
        }

        // Filter properties =============

        /// <summary>
        /// Gets or sets the selected author in the list authors to filter the collection
        /// </summary>
        public string SelectedAuthor
        {
            get { return _selectedAuthor; }
            set
            {
                if (_selectedAuthor == value)
                    return;
                _selectedAuthor = value;
                RaisePropertyChanged("SelectedAuthor");
                ApplyFilter(!string.IsNullOrEmpty(_selectedAuthor) ? FilterField.Author : FilterField.None);
            }
        }
        /// <summary>
        /// Gets or sets the selected author in the list countries to filter the collection
        /// </summary>
        public string SelectedCountry
        {
            get { return _selectedCountry; }
            set
            {
                if (_selectedCountry == value)
                    return;
                _selectedCountry = value;
                RaisePropertyChanged("SelectedCountry");
                ApplyFilter(!string.IsNullOrEmpty(_selectedCountry) ? FilterField.Country : FilterField.None);
            }
        }
        /// <summary>
        /// Gets or sets the selected author in the list years to filter the collection
        /// </summary>
        public string SelectedYear
        {
            get { return _selectedYear; }
            set
            {
                if (_selectedYear == value)
                    return;
                _selectedYear = value;
                RaisePropertyChanged("SelectedYear");
                ApplyFilter(!string.IsNullOrEmpty(_selectedYear) ? FilterField.Year : FilterField.None);
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
                ApplyFilter(!string.IsNullOrEmpty(_selectedColour) ? FilterField.Colour : FilterField.None);
            }
        }
        public string SelectedStyle
        {
            get { return _selectedStyle; }
            set
            {
                if (_selectedStyle == value)
                    return;
                _selectedStyle = value;
                RaisePropertyChanged("SelectedStyle");
                ApplyFilter(!string.IsNullOrEmpty(_selectedStyle) ? FilterField.Style : FilterField.None);
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
                ApplyFilter(!string.IsNullOrEmpty(_selectedStockType) ? FilterField.StockType : FilterField.None);
            }
        }

        /// <summary>
        /// Gets or sets a list of authors which is used to populate the author filter
        /// drop down list.
        /// </summary>
        public ObservableCollection<string> Authors
        {
            get { return _authors; }
            set
            {
                if (_authors == value)
                    return;
                _authors = value;
                RaisePropertyChanged("Authors");
            }
        }
        /// <summary>
        /// Gets or sets a list of authors which is used to populate the country filter
        /// drop down list.
        /// </summary>
        public ObservableCollection<string> Countries
        {
            get { return _countries; }
            set
            {
                if (_countries == value)
                    return;
                _countries = value;
                RaisePropertyChanged("Countries");
            }
        }
        /// <summary>
        /// Gets or sets a list of authors which is used to populate the year filter
        /// drop down list.
        /// </summary>
        public ObservableCollection<string> Years
        {
            get { return _years; }
            set
            {
                if (_years == value)
                    return;
                _years = value;
                RaisePropertyChanged("Years");
            }
        }

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
        public bool CanRemoveCountryFilter
        {
            get { return _canCanRemoveCountryFilter; }
            set
            {
                _canCanRemoveCountryFilter = value;
                RaisePropertyChanged("CanRemoveCountryFilter");
            }
        }
        /// <summary>
        /// Gets or sets a flag indicating if the Author filter, if applied, can be removed.
        /// </summary>
        public bool CanRemoveAuthorFilter
        {
            get { return _canCanRemoveAuthorFilter; }
            set
            {
                _canCanRemoveAuthorFilter = value;
                RaisePropertyChanged("CanRemoveAuthorFilter");
            }
        }
        /// <summary>
        /// Gets or sets a flag indicating if the Year filter, if applied, can be removed.
        /// </summary>
        public bool CanRemoveYearFilter
        {
            get { return _canCanRemoveYearFilter; }
            set
            {
                _canCanRemoveYearFilter = value;
                RaisePropertyChanged("CanRemoveYearFilter");
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

        public bool CanRemoveStyleFilter
        {
            get { return _canCanRemoveStyleFilter; }
            set
            {
                _canCanRemoveStyleFilter = value;
                RaisePropertyChanged("CanRemoveStyleFilter");
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
        public ICommand RemoveColourFilterCommand
        {
            get;
            private set;
        }
        public ICommand RemoveStyleFilterCommand
        {
            get;
            private set;
        }
        public ICommand RemoveStockTypeFilterCommand
        {
            get;
            private set;
        }
        public ICommand CsvCommand { get; private set; }
        #endregion

        private void InitializeCommands()
        {
            CsvCommand = new RelayCommand(CsvGenerateCommand, null);
            ResetFiltersCommand = new RelayCommand(ResetFilters, null);
            RemoveCountryFilterCommand = new RelayCommand(RemoveCountryFilter, () => CanRemoveCountryFilter);
            RemoveAuthorFilterCommand = new RelayCommand(RemoveAuthorFilter, () => CanRemoveAuthorFilter);
            RemoveYearFilterCommand = new RelayCommand(RemoveYearFilter, () => CanRemoveYearFilter);
            RemoveColourFilterCommand = new RelayCommand(RemoveColourFilter, () => CanRemoveColourFilter);
            RemoveStyleFilterCommand = new RelayCommand(RemoveStyleFilter, () => CanRemoveStyleFilter);
            RemoveStockTypeFilterCommand = new RelayCommand(RemoveStockTypeFilter, () => CanRemoveStockTypeFilter);
        }

        private async Task LoadData()
        {
            IsBusy = true;

            await Task.Factory.StartNew(() =>
            {
                var allstock = _skuService.GetStock().Result;
                var onlineSKUs = _skuService.OnlineSKUs();                
                var online = _skuService.GetOnlineSKuValues(onlineSKUs);
                var missing = _skuService.GetMissingStock(online, allstock).Result;

                var q1 = from t in missing
                         select t.MasterSupplier;
                Authors = new ObservableCollection<string>(q1.Distinct().OrderBy(x => x));

                var q2 = from t in missing
                         select t.Category;
                Countries = new ObservableCollection<string>(q2.Distinct().OrderBy(x => x));

                var q3 = from t in missing
                         select t.Season;
                Years = new ObservableCollection<string>(q3.Distinct().OrderBy(x => x));

                var q4 = from t in missing
                         select t.Color;
                Colour = new ObservableCollection<string>(q4.Distinct().OrderBy(x => x));

                var q5 = from t in missing
                         select t.Style;
                Style = new ObservableCollection<string>(q5.Distinct().OrderBy(x => x));

                var q6 = from t in missing
                         select t.StockType;
                StockType = new ObservableCollection<string>(q6.Distinct().OrderBy(x => x));

                Things = new ObservableCollection<SpecailOrders>(missing.OrderBy(x => x.MasterSupplier));
        }).ContinueWith((task) =>
            {
            IsBusy = false;
        }, TaskScheduler.FromCurrentSynchronizationContext());
        }
        /// <summary>
        /// This method handles a message recieved from the View which enables a reference to the
        /// instantiated CollectionViewSource to be used in the ViewModel.
        /// </summary>
        private void Handle_ViewCollectionViewSourceMessageToken(ViewCollectionViewSourceMessageToken token)
        {
            CVS = token.CVS;
            //X_Online = token.X_Online;
        }

        public void CsvGenerateCommand()
        {
            if (File.Exists(System.Configuration.ConfigurationManager.AppSettings["CsvReport"]))
            {
                File.Delete(System.Configuration.ConfigurationManager.AppSettings["CsvReport"]);
            }
            File.AppendAllText(System.Configuration.ConfigurationManager.AppSettings["CsvReport"], CVS.Source.ToCsv());
            MessageBox.Show("CSV Generated");
        }

        // Command methods (called by the commands) ===============

        public void ResetFilters()
        {
            // clear filters 
            RemoveYearFilter();
            RemoveAuthorFilter();
            RemoveCountryFilter();
            RemoveColourFilter();
            RemoveStyleFilter();
            RemoveStockTypeFilter();
        }
        public void RemoveCountryFilter()
        {
            CVS.Filter -= new FilterEventHandler(FilterByCountry);
            //X_Online.Filter -= new FilterEventHandler(FilterByCountry);
            SelectedCountry = null;
            CanRemoveCountryFilter = false;
        }
        public void RemoveAuthorFilter()
        {
            CVS.Filter -= new FilterEventHandler(FilterByAuthor);
            //X_Online.Filter -= new FilterEventHandler(FilterByAuthor);
            SelectedAuthor = null;
            CanRemoveAuthorFilter = false;
        }
        public void RemoveYearFilter()
        {
            CVS.Filter -= new FilterEventHandler(FilterByYear);
            //X_Online.Filter -= new FilterEventHandler(FilterByYear);
            SelectedYear = null;
            CanRemoveYearFilter = false;
        }
        public void RemoveColourFilter()
        {
            CVS.Filter -= new FilterEventHandler(FilterByColour);
            //X_Online.Filter -= new FilterEventHandler(FilterByYear);
            SelectedColour = null;
            CanRemoveColourFilter = false;
        }
        public void RemoveStyleFilter()
        {
            CVS.Filter -= new FilterEventHandler(FilterByStyle);
            //X_Online.Filter -= new FilterEventHandler(FilterByYear);
            SelectedStyle = null;
            CanRemoveStyleFilter = false;
        }

        public void RemoveStockTypeFilter()
        {
            CVS.Filter -= new FilterEventHandler(FilterByStockType);
            //X_Online.Filter -= new FilterEventHandler(FilterByYear);
            SelectedStockType = null;
            CanRemoveStockTypeFilter = false;
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

        private void Insert(string reff, string query)
        {
            var dataset = new DataSet();
            using (var connectionHandler = new OleDbConnection(System.Configuration.ConfigurationManager.AppSettings["AccessConnectionString"]))
            {
                connectionHandler.Open();
                var myAccessCommand = new OleDbCommand(query, connectionHandler);
                if (reff != null)
                {
                    myAccessCommand.Parameters.AddWithValue("?", reff);
                }

                var myDataAdapter = new OleDbDataAdapter(myAccessCommand);
                myDataAdapter.Fill(dataset);
            }
        }

        // Other helper methods ==============

        /* Notes on Adding Filters:
         *   Each filter is added by subscribing a filter method to the Filter event
         *   of the CVS.  Filters are applied in the order in which they were added. 
         *   To prevent adding filters mulitple times ( because we are using drop down lists
         *   in the view), the CanRemove***Filter flags are used to ensure each filter
         *   is added only once.  If a filter has been added, its corresponding CanRemove***Filter
         *   is set to true.       
         *   
         *   If a filter has been applied already (for example someone selects "Canada" to filter by country
         *   and then they change their selection to another value (say "Mexico") we need to undo the previous
         *   country filter then apply the new one.  This does not completey Reset the filter, just
         *   allows it to be changed to another filter value. This applies to the other filters as well
         */

        public void AddCountryFilter()
        {
            // see Notes on Adding Filters:
            if (CanRemoveCountryFilter)
            {
                CVS.Filter -= new FilterEventHandler(FilterByCountry);
                CVS.Filter += new FilterEventHandler(FilterByCountry);

                //X_Online.Filter -= new FilterEventHandler(FilterByCountry);
                //X_Online.Filter += new FilterEventHandler(FilterByCountry);
            }
            else
            {
                CVS.Filter += new FilterEventHandler(FilterByCountry);
                //X_Online.Filter += new FilterEventHandler(FilterByCountry);
                CanRemoveCountryFilter = true;
            }
        }
        public void AddAuthorFilter()
        {
            // see Notes on Adding Filters:
            if (CanRemoveAuthorFilter)
            {
                CVS.Filter -= new FilterEventHandler(FilterByAuthor);
                CVS.Filter += new FilterEventHandler(FilterByAuthor);

                //X_Online.Filter -= new FilterEventHandler(FilterByAuthor);
                //X_Online.Filter += new FilterEventHandler(FilterByAuthor);
            }
            else
            {
                CVS.Filter += new FilterEventHandler(FilterByAuthor);

                //X_Online.Filter += new FilterEventHandler(FilterByAuthor);
                CanRemoveAuthorFilter = true;
            }
        }
        public void AddYearFilter()
        {
            // see Notes on Adding Filters:
            if (CanRemoveYearFilter)
            {
                CVS.Filter -= new FilterEventHandler(FilterByYear);
                CVS.Filter += new FilterEventHandler(FilterByYear);

                //X_Online.Filter -= new FilterEventHandler(FilterByYear);
                //X_Online.Filter += new FilterEventHandler(FilterByYear);
            }
            else
            {
                CVS.Filter += new FilterEventHandler(FilterByYear);
                //X_Online.Filter += new FilterEventHandler(FilterByYear);
                CanRemoveYearFilter = true;
            }
        }

        public void AddColourFilter()
        {
            // see Notes on Adding Filters:
            if (CanRemoveColourFilter)
            {
                CVS.Filter -= new FilterEventHandler(FilterByColour);
                CVS.Filter += new FilterEventHandler(FilterByColour);

                //X_Online.Filter -= new FilterEventHandler(FilterByYear);
                //X_Online.Filter += new FilterEventHandler(FilterByYear);
            }
            else
            {
                CVS.Filter += new FilterEventHandler(FilterByColour);
                //X_Online.Filter += new FilterEventHandler(FilterByYear);
                CanRemoveColourFilter = true;
            }
        }
        public void AddStyleFilter()
        {
            // see Notes on Adding Filters:
            if (CanRemoveStyleFilter)
            {
                CVS.Filter -= new FilterEventHandler(FilterByStyle);
                CVS.Filter += new FilterEventHandler(FilterByStyle);

                //X_Online.Filter -= new FilterEventHandler(FilterByYear);
                //X_Online.Filter += new FilterEventHandler(FilterByYear);
            }
            else
            {
                CVS.Filter += new FilterEventHandler(FilterByStyle);
                //X_Online.Filter += new FilterEventHandler(FilterByYear);
                CanRemoveStyleFilter = true;
            }
        }

        public void AddStockTypeFilter()
        {
            // see Notes on Adding Filters:
            if (CanRemoveStockTypeFilter)
            {
                CVS.Filter -= new FilterEventHandler(FilterByStockType);
                CVS.Filter += new FilterEventHandler(FilterByStockType);

                //X_Online.Filter -= new FilterEventHandler(FilterByYear);\\\\\\\\\\
                //X_Online.Filter += new FilterEventHandler(FilterByYear);
            }
            else
            {
                CVS.Filter += new FilterEventHandler(FilterByStockType);
                //X_Online.Filter += new FilterEventHandler(FilterByYear);
                CanRemoveStockTypeFilter = true;
            }
        }

        /* Notes on Filter Methods:
         * When using multiple filters, do not explicitly set anything to true.  Rather,
         * only hide things which do not match the filter criteria
         * by setting e.Accepted = false.  If you set e.Accept = true, if effectively
         * clears out any previous filters applied to it.  
         */

        private void FilterByAuthor(object sender, FilterEventArgs e)
        {
            // see Notes on Filter Methods:
            var src = e.Item as SpecailOrders;
            if (src == null)
                e.Accepted = false;
            else if (string.Compare(SelectedAuthor, src.MasterSupplier) != 0)
                e.Accepted = false;
        }
        private void FilterByYear(object sender, FilterEventArgs e)
        {
            // see Notes on Filter Methods:
            var src = e.Item as SpecailOrders;
            if (src == null)
                e.Accepted = false;
            else if (string.Compare(SelectedYear, src.Season) != 0)
                e.Accepted = false;
        }
        private void FilterByCountry(object sender, FilterEventArgs e)
        {
            // see Notes on Filter Methods:
            var src = e.Item as SpecailOrders;
            if (src == null)
                e.Accepted = false;
            else if (string.Compare(SelectedCountry, src.Category) != 0)
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
        private void FilterByStyle(object sender, FilterEventArgs e)
        {
            // see Notes on Filter Methods:
            var src = e.Item as SpecailOrders;
            if (src == null)
                e.Accepted = false;
            else if (string.Compare(SelectedStyle, src.Style) != 0)
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
            Author,
            Country,
            Year,
            Colour,
            Style,
            StockType,
            None
        }
        private void ApplyFilter(FilterField field)
        {
            switch (field)
            {
                case FilterField.Author:
                    AddAuthorFilter();
                    break;
                case FilterField.Country:
                    AddCountryFilter();
                    break;
                case FilterField.Year:
                    AddYearFilter();
                    break;
                case FilterField.Colour:
                    AddColourFilter();
                    break;
                case FilterField.Style:
                    AddStyleFilter();
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
