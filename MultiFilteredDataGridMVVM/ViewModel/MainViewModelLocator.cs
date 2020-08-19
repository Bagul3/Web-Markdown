using Common;
using DataService;
using GalaSoft.MvvmLight;
using Microsoft.Practices.Unity;
using MultiFilteredDataGridMVVM.Design;
using MultiFilteredDataGridMVVM.View;

namespace MultiFilteredDataGridMVVM.ViewModel
{
    public class MainViewModelLocator
    {
        static MainViewModelLocator()
        {
            Container = new UnityContainer();

            if (ViewModelBase.IsInDesignModeStatic)
            {
                // if in design mode, use design data service
                Container.RegisterType<IDataService, DesignDummyService>();
            }
            else
            {
                // otherwise for runtime use real data source
                Container.RegisterType<IDataService, DummyService>();
            }

            // register as a singleton
            Container.RegisterType<MainViewModel>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ImportProductViewModel>(new ContainerControlledLifetimeManager());
            Container.RegisterType<SkuViewModel>(new ContainerControlledLifetimeManager());
        }

        public static IUnityContainer Container
        {
            get;
            private set;
        }

        public MainViewModel MainVM
        {
            get
            {
                var vm = Container.Resolve<MainViewModel>();
                return vm;
            }
        }

        public ImportProductViewModel ProductVM
        {
            get
            {
                var vm = Container.Resolve<ImportProductViewModel>();
                return vm;
            }
        }

        public SkuViewModel SKUVM
        {
            get
            {
                var vm = Container.Resolve<SkuViewModel>();
                return vm;
            }
        }

        public OnlineViewModel OnlineVM
        {
            get
            {
                var vm = Container.Resolve<OnlineViewModel>();
                return vm;
            }
        }

        public GenerateStockFileViewModel StockVM
        {
            get
            {
                var vm = Container.Resolve<GenerateStockFileViewModel>();
                return vm;
            }
        }

        public ConfigurationViewModel ConfigurationVM
        {
            get
            {
                var vm = Container.Resolve<ConfigurationViewModel>();
                return vm;
            }
        }

        public static void Cleanup()
        {
            Container.Resolve<SkuViewModel>().Cleanup();
        }
    }
}
