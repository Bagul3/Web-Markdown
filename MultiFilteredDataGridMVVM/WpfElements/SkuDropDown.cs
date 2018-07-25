
using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace MultiFilteredDataGridMVVM.WpfElements
{
    public class SkuDropDown : System.Windows.Controls.DataGrid
    {
        public SkuDropDown()
        {
            EnableColumnVirtualization = true;
            this.SelectionChanged += CustomDataGrid_SelectionChanged;
        }

        void CustomDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.SelectedItemsList = this.SelectedItems;
        }
        #region SelectedItemsList

        public IList SelectedItemsList
        {
            get { return (IList)GetValue(SelectedItemsListProperty); }
            set { SetValue(SelectedItemsListProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemsListProperty =
            DependencyProperty.Register("SelectedItemsList", typeof(IList), typeof(SkuDropDown), new PropertyMetadata(null));

        #endregion
    }
}
