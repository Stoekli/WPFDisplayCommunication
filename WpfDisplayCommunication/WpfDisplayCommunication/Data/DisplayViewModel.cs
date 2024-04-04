using System.ComponentModel;


namespace WpfDisplayCommunication.Data
{
    internal class DisplayViewModel : INotifyPropertyChanged
    {

        private string _myString;

        public string Temperatur1
        {
            get { return _myString; }
            set
            {
                if (_myString != value)
                {
                    _myString = value;
                    OnPropertyChanged("Temperatur1");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
