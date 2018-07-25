using System.Windows.Data;

namespace MultiFilteredDataGridMVVM.Helpers
{
    /// <summary>
    /// This is a simple data class used to transport a reference to the CollectionViewSource
    /// from the view to the view model.
    /// </summary>
    public class ViewCollectionViewSourceMessageToken
    {
        public CollectionViewSource CVS { get; set; }

        public CollectionViewSource DiscountedStock { get; set; }
        public CollectionViewSource X_Online { get; set; }
        public CollectionViewSource MissingOnlineStockToken { get; set; }
        public CollectionViewSource Stock { get; set; }
    }
}