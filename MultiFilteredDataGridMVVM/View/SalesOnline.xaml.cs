using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using GalaSoft.MvvmLight.Messaging;
using MultiFilteredDataGridMVVM.Helpers;
using MultiFilteredDataGridMVVM.ViewModel;

namespace MultiFilteredDataGridMVVM.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class SalesOnline : UserControl
    {
        public SalesOnline()
        {
            InitializeComponent();

            // Here we send a message which is caught by the view model.  The message contains a reference
            // to the CollectionViewSource which is instantiated when the view is instantiated (before the view model). 
            Messenger.Default.Send(new ViewCollectionViewSourceMessageToken() { DiscountedStock = (CollectionViewSource)(this.Resources["X_CVS"]) });
            Messenger.Default.Send(new ViewCollectionViewSourceMessageToken() { CVS = (CollectionViewSource)(this.Resources["DiscountedStock"]) });

            // Note to MVVM purists:  Not an ideal solution.  But based on the amount if time spent on this it was acceptable, especially to the client.
        }

        private void MenuItem_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {

        }
    }
}
