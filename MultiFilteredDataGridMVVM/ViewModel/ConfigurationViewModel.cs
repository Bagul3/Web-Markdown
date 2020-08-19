using Common;
using DataRepo;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MultiFilteredDataGridMVVM.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows.Forms;
using System.Windows.Input;

namespace MultiFilteredDataGridMVVM.ViewModel
{
    public class ConfigurationViewModel : ViewModelBase
    {
        SkuRepository _skuRepository;

        public ConfigurationViewModel()
        {
            _skuRepository = new SkuRepository();
            REM1 = new ObservableCollection<REM1>(FetchREM1());
            REM2 = new ObservableCollection<REM2>(FetchREM2());
            _latestSeason = GetLatestSeason();
            InitializeCommands();
        }

        private string GetLatestSeason()
        {
            try
            {
                var remresult = _skuRepository.RetrieveQuery(SqlQueries.FetchLatestSeaosn).Tables;
                if (remresult != null)
                {
                    return remresult[0].Rows[0]["SEASON"].ToString();
                }
                return "";
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
            get
            {
                return _latestSeason;

            }
            set
            {
                _latestSeason = value;
                RaisePropertyChanged("LastestSeason");
            }
        }

        private ObservableCollection<REM1> _results;

        public ObservableCollection<REM1> REM1
        {
            get { return _results; }
            set
            {
                if (_results == value)
                    return;
                _results = value;
                RaisePropertyChanged("REM1");
            }
        }

        private REM1 m_selectedREM1;
        public REM1 SelectedREM1
        {
            get { return m_selectedREM1; }
            set
            {
                m_selectedREM1 = value;
                RaisePropertyChanged("SelectedREM1");
            }
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
            REM1.Remove(m_selectedREM1);
        }

        private void DeleteREM2()
        {
            REM2.Remove(m_selectedREM2);
        }

        public ICommand Add
        {
            get;
            private set;
        }

        private void AddREMS()
        {
            try
            {
                _skuRepository.RetrieveQuery(SqlQueries.DeleteREM);
                _skuRepository.RetrieveQuery(SqlQueries.DeleteConfigurables);

                _skuRepository.RetrieveQuery(_latestSeason, SqlQueries.InsertLatestSeason);

                foreach (var rem in REM1)
                {
                    _skuRepository.InsertREM(rem.Name, rem.REM, rem.T2T_Id, rem.Property, SqlQueries.InsertREM);
                }
                foreach (var rem in REM2)
                {
                    _skuRepository.InsertREM(rem.Name, rem.REM, rem.T2T_Id, rem.Property, SqlQueries.InsertREM);
                }
                MessageBox.Show("Successfully Saved.");
            }
            catch (Exception ex)
            {
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
            catch(Exception ex)
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
            catch(Exception ex)
            {
                new LogWriter().LogWrite("ERROR: " + ex.StackTrace);
                MessageBox.Show("An error occurred, if this continues please contact Conor.");
            }
            return new List<REM2>();
        }

        private ObservableCollection<REM2> _rem2;

        public ObservableCollection<REM2> REM2
        {
            get { return _rem2; }
            set
            {
                if (_rem2 == value)
                    return;
                _rem2 = value;
                RaisePropertyChanged("REM2");
            }
        }

        private REM2 m_selectedREM2;
        public REM2 SelectedREM2
        {
            get { return m_selectedREM2; }
            set
            {
                m_selectedREM2 = value;
                RaisePropertyChanged("SelectedREM2");
            }
        }

        private void InitializeCommands()
        {
            Add = new RelayCommand(AddREMS);
        }
    }
}
