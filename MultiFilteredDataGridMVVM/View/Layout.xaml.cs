using System;
using System.Windows;
using MultiFilteredDataGridMVVM.ViewModel;

namespace MultiFilteredDataGridMVVM.View
{
    /// <summary>
    /// Interaction logic for Layout.xaml
    /// </summary>
    public partial class Layout : Window
    {
        public Layout()
        {
            InitializeComponent();
        }

        private void MainView_Clicked(object sender, RoutedEventArgs e)
        {
            DataContext = new MainView();
        }

        private void ImportProductsView_Clicked(object sender, RoutedEventArgs e)
        {
            DataContext = new ImportProducts();
        }

        private void SKUCheckerView_Clicked(object sender, RoutedEventArgs e)
        {
            DataContext = new SkuChecker();
        }

        private void OnlineCheckerView_Clicked(object sender, RoutedEventArgs e)
        {
            DataContext = new OnlineStock();
        }

        private void GenerateStockFile_Clicked(object sender, RoutedEventArgs e)
        {
            DataContext = new StockFile();
        }

        private void Configuration_Clicked(object sender, RoutedEventArgs e)
        {
            DataContext = new Configuration();
        }
    }
}
