using Common;
using Cordners.ViewModel;
using DataService;
using GalaSoft.MvvmLight;
using Microsoft.Practices.Unity;
using MultiFilteredDataGridMVVM.Design;

namespace MultiFilteredDataGridMVVM.ViewModel
{
    public class ContainerLocator
    {
        static ContainerLocator()
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
            Container.RegisterType<SalesViewModel>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ImportProductViewModel>(new ContainerControlledLifetimeManager());
            //Container.RegisterType<MissingOnline>(new ContainerControlledLifetimeManager());
        }

        public static IUnityContainer Container
        {
            get;
            private set;
        }

        public SalesViewModel MainVM
        {
            get
            {
                var vm = Container.Resolve<SalesViewModel>();
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

        public MissingOnline SKUVM
        {
            get
            {
                var vm = Container.Resolve<MissingOnline>();
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

        public ConfigurationViewModel ConfigurationVM
        {
            get
            {
                var vm = Container.Resolve<ConfigurationViewModel>();
                return vm;
            }
        }

        public SalesViewModel SalesVM
        {
            get
            {
                var vm = Container.Resolve<SalesViewModel>();
                return vm;
            }
        }

        public SalesOnlineViewModel SalesOnlineVM
        {
            get
            {
                var vm = Container.Resolve<SalesOnlineViewModel>();
                return vm;
            }
        }


        public static void Cleanup()
        {
            Container.Resolve<MissingOnline>().Cleanup();
        }
    }
}
