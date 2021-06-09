using System;
using System.Diagnostics;
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

        private void Configuration_Clicked(object sender, RoutedEventArgs e)
        {
            DataContext = new Configuration();
        }

        private void Generate_Simple_CSV(object sender, RoutedEventArgs e)
        {
            Process.Start(System.Configuration.ConfigurationManager.AppSettings["ExecuteSimpleJob"]);
            MessageBox.Show("Simple Job has been executed. Please check C:\\WebUpdates for Simple files.");
        }

        private void Generate_Config_CSV(object sender, RoutedEventArgs e)
        {
            Process.Start(System.Configuration.ConfigurationManager.AppSettings["ExecuteConfigJob"]);
            MessageBox.Show("Config Job has been executed. Please check C:\\WebUpdates\\Config for Configurable files.");
        }
    }
}
