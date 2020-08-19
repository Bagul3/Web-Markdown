using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiFilteredDataGridMVVM.Model
{
    public class REM1 : INotifyPropertyChanged
    {
        public string Property { get; set; }
        public string Name { get; set; }
        public string T2T_Id { get; set; }
        public string REM { get; set; } = "REM1";

        public REM1()
        {

        }

        public REM1(string name, string id, string rem_property)
        {
            this.Name = name;
            this.T2T_Id = id;
            this.Property = rem_property;
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string property)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        #endregion
    }
}
