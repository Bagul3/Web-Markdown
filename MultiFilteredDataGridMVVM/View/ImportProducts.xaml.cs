using System.Windows.Data;
using GalaSoft.MvvmLight.Messaging;
using MultiFilteredDataGridMVVM.Helpers;
using UserControl = System.Windows.Controls.UserControl;

namespace MultiFilteredDataGridMVVM.View
{
    /// <summary>
    /// Interaction logic for ImportProducts.xaml
    /// </summary>
    public partial class ImportProducts : UserControl
    {
        public ImportProducts()
        {
            InitializeComponent();
            Messenger.Default.Send(new ViewCollectionViewSourceMessageToken() { DiscountedStock = (CollectionViewSource)(this.Resources["ImageName"]) });
            Messenger.Default.Send(new ViewCollectionViewSourceMessageToken() { DiscountedStock = (CollectionViewSource)(this.Resources["Errors"]) });
        }
    }
}
