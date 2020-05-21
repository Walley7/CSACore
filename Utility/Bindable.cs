using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace CSACore.Utility {

    public class Bindable : INotifyPropertyChanged {
        //================================================================================
        public event PropertyChangedEventHandler    PropertyChanged;


        // PROPERTIES ================================================================================
        //--------------------------------------------------------------------------------
        protected void SetProperty<T>(string name, ref T property, T value) {
            property = value;
            OnPropertyChanged(name);
        }

        //--------------------------------------------------------------------------------
        protected virtual void OnPropertyChanged(string property) {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
    }

}
