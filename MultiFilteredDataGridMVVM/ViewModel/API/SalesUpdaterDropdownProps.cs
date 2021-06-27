using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Data;
using Common;
using DataService;
using GalaSoft.MvvmLight;


namespace Cordners.ViewModel.API
{
    public class SalesUpdaterDropdownProps : ViewModelBase
    {
        protected Dictionary<string, object> _category;
        protected Dictionary<string, object> _supplier;
        protected Dictionary<string, object> _style;
        protected Dictionary<string, object> _season;
        protected Dictionary<string, object> _stockType;
        protected Dictionary<string, object> _size;
        protected Dictionary<string, object> _colour;

        protected Dictionary<string, object> _selectedSize;
        protected Dictionary<string, object> _selectedSupplier;
        protected Dictionary<string, object> _selectedCategory;
        protected Dictionary<string, object> _selectedSeason;
        protected Dictionary<string, object> _selectedStockType;
        protected Dictionary<string, object> _selectedStyle;
        protected Dictionary<string, object> _selectedColour;
        protected double _progressValue;
        protected SalesService _specailOrdersService;
        protected Dictionary<string, object> _items;
        protected Dictionary<string, object> _selectedItems;
        protected bool _isBusy;

        protected bool _canCanRemoveSizeFilter;
        protected bool _canCanRemoveSupplierFilter;
        protected bool _canCanRemoveStyleFilter;
        protected bool _canCanRemoveColourFilter;
        protected bool _canCanRemoveStockTypeFilter;
        public ObservableCollection<SpecailOrders> _cordners;
        public ObservableCollection<SpecailOrders> _specailOrders = new ObservableCollection<SpecailOrders>();
        public CollectionViewSource StockItems { get; set; }

        public CollectionViewSource DiscountedStock { get; set; }
        public IList _cordnersSelectedModels = new ArrayList();

        public IList CordnersCordnersSelected
        {
            get { return _cordnersSelectedModels; }
            set
            {
                _cordnersSelectedModels = value;
                RaisePropertyChanged("CordnersCordnersSelected");
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
            }
        }

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
        public Dictionary<string, object> SelectedStyle
        {
            get { return _selectedStyle; }
            set
            {
                if (_selectedStyle == value)
                    return;
                _selectedStyle = value;
                RaisePropertyChanged("SelectedStyle");
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
            }
        }

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

        public Dictionary<string, object> Colour
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

        public bool CanRemoveSizeFilter
        {
            get { return _canCanRemoveSizeFilter; }
            set
            {
                _canCanRemoveSizeFilter = value;
                RaisePropertyChanged("CanRemoveSizeFilter");
            }
        }

        public bool CanRemoveSupplierFilter
        {
            get { return _canCanRemoveSupplierFilter; }
            set
            {
                _canCanRemoveSupplierFilter = value;
                RaisePropertyChanged("CanRemoveSupplierFilter");
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

        public void AddSizeFilter()
        {
            if (CanRemoveSizeFilter)
            {
                StockItems.Filter -= new FilterEventHandler(FilterBySize);
                StockItems.Filter += new FilterEventHandler(FilterBySize);
            }
            else
            {
                StockItems.Filter += new FilterEventHandler(FilterBySize);
                CanRemoveSizeFilter = true;
            }
        }
        public void AddSupplierFilter()
        {
            if (CanRemoveSupplierFilter)
            {
                StockItems.Filter -= new FilterEventHandler(FilterBySupplier);
                StockItems.Filter += new FilterEventHandler(FilterBySupplier);
            }
            else
            {
                StockItems.Filter += new FilterEventHandler(FilterBySupplier);
                CanRemoveSupplierFilter = true;
            }
        }
        public void AddStyleFilter()
        {
            if (CanRemoveStyleFilter)
            {
                StockItems.Filter -= new FilterEventHandler(FilterByStyle);
                StockItems.Filter += new FilterEventHandler(FilterByStyle);
            }
            else
            {
                StockItems.Filter += new FilterEventHandler(FilterByStyle);
                CanRemoveStyleFilter = true;
            }
        }

        public void AddColourFilter()
        {
            if (CanRemoveColourFilter)
            {
                StockItems.Filter -= new FilterEventHandler(FilterByColour);
                StockItems.Filter += new FilterEventHandler(FilterByColour);
            }
            else
            {
                StockItems.Filter += new FilterEventHandler(FilterByColour);
                CanRemoveColourFilter = true;
            }
        }

        public void AddStockTypeFilter()
        {
            if (CanRemoveStockTypeFilter)
            {
                StockItems.Filter -= new FilterEventHandler(FilterByStockType);
                StockItems.Filter += new FilterEventHandler(FilterByStockType);
            }
            else
            {
                StockItems.Filter += new FilterEventHandler(FilterByStockType);
                CanRemoveStockTypeFilter = true;
            }
        }

        private void FilterBySize(object sender, FilterEventArgs e)
        {
            var src = e.Item as SpecailOrders;
            if (src == null)
                e.Accepted = false;
        }
        private void FilterBySupplier(object sender, FilterEventArgs e)
        {
            var src = e.Item as SpecailOrders;
            if (src == null)
                e.Accepted = false;

        }
        private void FilterByStyle(object sender, FilterEventArgs e)
        {
            var src = e.Item as SpecailOrders;
            if (src == null)
                e.Accepted = false;
        }
        private void FilterByColour(object sender, FilterEventArgs e)
        {
            var src = e.Item as SpecailOrders;
            if (src == null)
                e.Accepted = false;
        }
        private void FilterByStockType(object sender, FilterEventArgs e)
        {
            var src = e.Item as SpecailOrders;
            if (src == null)
                e.Accepted = false;
        }

        public enum FilterField
        {
            Size,
            Supplier,
            Style,
            Colour,
            StockType,
            None
        }

        public void ApplyFilter(FilterField field)
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
            StockItems.Filter -= new FilterEventHandler(FilterBySize);
            SelectedSize = null;
            CanRemoveSizeFilter = false;
        }
        public void RemoveSupplierFilter()
        {
            StockItems.Filter -= new FilterEventHandler(FilterBySupplier);
            SelectedSupplier = null;
            CanRemoveSupplierFilter = false;
        }
        public void RemoveStyleFilter()
        {
            StockItems.Filter -= new FilterEventHandler(FilterByStyle);
            SelectedStyle = null;
            CanRemoveStyleFilter = false;
        }

        public void RemoveColourFilter()
        {
            StockItems.Filter -= new FilterEventHandler(FilterByColour);
            SelectedColour = null;
            CanRemoveColourFilter = false;
        }

        public void RemoveStockTypeFilter()
        {
            StockItems.Filter -= new FilterEventHandler(FilterByStockType);
            SelectedStockType = null;
            CanRemoveStockTypeFilter = false;
        }
    }
}
