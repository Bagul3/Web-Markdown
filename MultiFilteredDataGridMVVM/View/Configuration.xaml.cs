using GalaSoft.MvvmLight.Messaging;
using MultiFilteredDataGridMVVM.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MultiFilteredDataGridMVVM.View
{
    /// <summary>
    /// Interaction logic for Configuration.xaml
    /// </summary>
    public partial class Configuration : UserControl
    {
        public Configuration()
        {
            InitializeComponent();
            Messenger.Default.Send(new ViewCollectionViewSourceMessageToken() { DiscountedStock = (CollectionViewSource)(this.Resources["REM1"]) });
            Messenger.Default.Send(new ViewCollectionViewSourceMessageToken() { DiscountedStock = (CollectionViewSource)(this.Resources["REM2"]) });
        }
    }
}
