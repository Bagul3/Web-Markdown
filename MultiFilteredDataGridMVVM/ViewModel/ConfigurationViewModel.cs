using Common;
using DataRepo;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MultiFilteredDataGridMVVM.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace MultiFilteredDataGridMVVM.ViewModel
{
    public class ConfigurationViewModel : ViewModelBase
    {
        SkuRepository _skuRepository;

        public ConfigurationViewModel()
        {
            _skuRepository = new SkuRepository();

            // Load REM collections from the repository.
            REM1 = new ObservableCollection<REM1>(FetchREM1());
            REM2 = new ObservableCollection<REM2>(FetchREM2());

            // Fetch the currency exchange rates.
            EuroPrice = FetcEuroLabel();
            USDPrice = FetchUSDLabel();
            AUSPrice = FetchAUSLabel();

            // Seasonal data used for configuration.
            var seasonalData = _skuRepository.RetrieveQuery(SqlQueries.FetchSeasonalData).Tables;
            _latestSeason = GetSeasonalDataFor(seasonalData, "TOPPAGE");
            _bottomSeason = GetSeasonalDataFor(seasonalData, "BOTTOMPAGE");

            InitializeCommands();
        }

        private string GetSeasonalDataFor(DataTableCollection seasonalData, string type)
        {
            try
            {
                var season = new StringBuilder();
                var remresult = _skuRepository.RetrieveQuery(SqlQueries.FetchSeasonalData).Tables;
                if (remresult != null)
                {
                    for (int i = 0; i < remresult[0].Rows.Count; i++)
                    {
                        if (remresult[0].Rows[i][$"{type}"].ToString() == "true")
                        {
                            season.Append($"{remresult[0].Rows[i]["SEASON"]},");
                        }
                    }
                }
                return season.ToString().TrimEnd(',');
            }
            catch (Exception ex)
            {
                new LogWriter().LogWrite(ex.StackTrace);
                return "";
            }
        }

        private string _latestSeason = "";
        public string LastestSeason
        {
            get { return _latestSeason; }
            set { _latestSeason = value; RaisePropertyChanged("LastestSeason"); }
        }

        private string _euroPrice = "";
        public string EuroPrice
        {
            get { return _euroPrice; }
            set { _euroPrice = value; RaisePropertyChanged("EuroPrice"); }
        }

        private string _usdPrice = "";
        public string USDPrice
        {
            get { return _usdPrice; }
            set { _usdPrice = value; RaisePropertyChanged("USDPrice"); }
        }

        private string _ausPrice = "";
        public string AUSPrice
        {
            get { return _ausPrice; }
            set { _ausPrice = value; RaisePropertyChanged("AUSPrice"); }
        }

        private string _bottomSeason = "";
        public string BottomSeason
        {
            get { return _bottomSeason; }
            set { _bottomSeason = value; RaisePropertyChanged("BottomSeason"); }
        }

        private ObservableCollection<REM1> _results;
        public ObservableCollection<REM1> REM1
        {
            get { return _results; }
            set { _results = value; RaisePropertyChanged("REM1"); }
        }

        private REM1 m_selectedREM1;
        public REM1 SelectedREM1
        {
            get { return m_selectedREM1; }
            set { m_selectedREM1 = value; RaisePropertyChanged("SelectedREM1"); }
        }

        private ICommand m_deleteCommandREM2;
        public ICommand DeleteCommandREM2
        {
            get
            {
                if (m_deleteCommandREM2 == null)
                {
                    m_deleteCommandREM2 = new RelayCommand(DeleteREM2);
                }
                return m_deleteCommandREM2;
            }
        }

        private ICommand m_deleteCommandREM1;
        public ICommand DeleteCommandREM1
        {
            get
            {
                if (m_deleteCommandREM1 == null)
                {
                    m_deleteCommandREM1 = new RelayCommand(DeleteREM1);
                }
                return m_deleteCommandREM1;
            }
        }

        private void DeleteREM1()
        {
            if (m_selectedREM1 != null)
                REM1.Remove(m_selectedREM1);
        }

        private void DeleteREM2()
        {
            if (m_selectedREM2 != null)
                REM2.Remove(m_selectedREM2);
        }

        public ICommand Add { get; private set; }

        private void InsertSeasonalData()
        {
            int id = 0;
            var latestSeasons = _latestSeason.Split(',');
            _skuRepository.RetrieveQuery(SqlQueries.DeleteConfigurables);
            foreach (var season in latestSeasons)
            {
                if (!string.IsNullOrWhiteSpace(season))
                {
                    id++;
                    _skuRepository.InsertSeasonData(season.Trim(), id.ToString(), "true", "false");
                }
            }
            var bottomSeasons = _bottomSeason.Split(',');
            foreach (var season in bottomSeasons)
            {
                if (!string.IsNullOrWhiteSpace(season))
                {
                    id++;
                    _skuRepository.InsertSeasonData(season.Trim(), id.ToString(), "false", "true");
                }
            }
        }

        private void InsertRems()
        {
            _skuRepository.RetrieveQuery(SqlQueries.DeleteREM);

            foreach (var rem in REM1)
            {
                _skuRepository.InsertREM(rem.Name, rem.REM, rem.T2T_Id, rem.Property, SqlQueries.InsertREM);
            }
            foreach (var rem in REM2)
            {
                _skuRepository.InsertREM(rem.Name, rem.REM, rem.T2T_Id, rem.Property, SqlQueries.InsertREM);
            }
        }

        private void Save()
        {
            try
            {
                try
                {
                    InsertSeasonalData();
                }
                catch (Exception ex)
                {
                    new LogWriter().LogWrite("ERROR: Inserting seasonal data");
                    new LogWriter().LogWrite("ERROR: " + ex.Message);
                    new LogWriter().LogWrite("ERROR: " + ex.StackTrace);
                    throw;
                }
                try
                {
                    InsertRems();
                }
                catch (Exception ex)
                {
                    new LogWriter().LogWrite("ERROR: Inserting REMs");
                    new LogWriter().LogWrite("ERROR: " + ex.Message);
                    new LogWriter().LogWrite("ERROR: " + ex.StackTrace);
                    throw;
                }
                MessageBox.Show("Successfully Saved.");
            }
            catch (Exception ex)
            {
                new LogWriter().LogWrite("ERROR: " + ex.Message);
                new LogWriter().LogWrite("ERROR: " + ex.StackTrace);
                MessageBox.Show("An error occurred trying to save configurables, if this continues contact Conor");
            }
        }

        private List<REM1> FetchREM1()
        {
            try
            {
                var ds = _skuRepository.RetrieveQuery("REM1", SqlQueries.FetchREM);
                var rem1 = new List<REM1>();

                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    rem1.Add(new REM1(dr["Name"].ToString(), dr["Id"].ToString(), dr["Property"].ToString()));
                }
                return rem1;
            }
            catch (Exception ex)
            {
                new LogWriter().LogWrite("ERROR: " + ex.StackTrace);
                MessageBox.Show("An error occurred, if this continues please contact Conor.");
            }
            return new List<REM1>();
        }

        private List<REM2> FetchREM2()
        {
            try
            {
                var ds = _skuRepository.RetrieveQuery("REM2", SqlQueries.FetchREM);
                var rem2 = new List<REM2>();

                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    rem2.Add(new REM2(dr["Name"].ToString(), dr["Id"].ToString(), dr["Property"].ToString()));
                }
                return rem2;
            }
            catch (Exception ex)
            {
                new LogWriter().LogWrite("ERROR: " + ex.StackTrace);
                MessageBox.Show("An error occurred, if this continues please contact Conor.");
            }
            return new List<REM2>();
        }

        private string FetcEuroLabel()
        {
            try
            {
                var ds = _skuRepository.RetrieveQuery(SqlQueries.FetchEUROPrice);
                string euroPrice = "Current Euro Exchange rate: " + ds.Tables[0].Rows[0]["PRICE"].ToString();
                return euroPrice;
            }
            catch (Exception ex)
            {
                new LogWriter().LogWrite("ERROR: " + ex.StackTrace);
                MessageBox.Show("An error occurred, if this continues please contact Conor.");
            }
            return "";
        }

        private string FetchUSDLabel()
        {
            try
            {
                var ds = _skuRepository.RetrieveQuery(SqlQueries.FetchUSDPrice);
                string usdPrice = "Current USD Exchange rate: " + ds.Tables[0].Rows[0]["PRICE"].ToString();
                return usdPrice;
            }
            catch (Exception ex)
            {
                new LogWriter().LogWrite("ERROR: " + ex.StackTrace);
                MessageBox.Show("An error occurred, if this continues please contact Conor.");
            }
            return "";
        }

        private string FetchAUSLabel()
        {
            try
            {
                var ds = _skuRepository.RetrieveQuery(SqlQueries.FetchAUDPrice);
                string ausPrice = "Current AUD Exchange rate: " + ds.Tables[0].Rows[0]["PRICE"].ToString();
                return ausPrice;
            }
            catch (Exception ex)
            {
                new LogWriter().LogWrite("ERROR: " + ex.StackTrace);
                MessageBox.Show("An error occurred, if this continues please contact Conor.");
            }
            return "";
        }

        private ObservableCollection<REM2> _rem2;
        public ObservableCollection<REM2> REM2
        {
            get { return _rem2; }
            set { _rem2 = value; RaisePropertyChanged("REM2"); }
        }

        private REM2 m_selectedREM2;
        public REM2 SelectedREM2
        {
            get { return m_selectedREM2; }
            set { m_selectedREM2 = value; RaisePropertyChanged("SelectedREM2"); }
        }

        private void InitializeCommands()
        {
            Add = new RelayCommand(Save);
        }
    }
}
