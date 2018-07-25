using System.Windows.Controls;
using System.Windows.Data;
using GalaSoft.MvvmLight.Messaging;
using MultiFilteredDataGridMVVM.Helpers;

namespace MultiFilteredDataGridMVVM.View
{
    /// <summary>
    /// Interaction logic for SkuChecker.xaml
    /// </summary>
    public partial class SkuChecker : UserControl
    {
        public SkuChecker()
        {
            InitializeComponent();
            Messenger.Default.Send(new ViewCollectionViewSourceMessageToken() { CVS = (CollectionViewSource)(this.Resources["X_CVS"]) }); 
            //Messenger.Default.Send(new ViewCollectionViewSourceMessageToken() { X_Online = (CollectionViewSource)(this.Resources["X_Online"]) });
            //Messenger.Default.Send(new ViewCollectionViewSourceMessageToken() { Stock = (CollectionViewSource)(this.Resources["Stock"]) });
            //Messenger.Default.Send(new ViewCollectionViewSourceMessageToken() { MissingOnlineStockToken = (CollectionViewSource)(this.Resources["MissingOnlineStockToken"]) });
        }
    }
}
