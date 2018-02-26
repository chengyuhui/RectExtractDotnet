using System.ComponentModel;

namespace RectExtractDotnet
{
    public class State : INotifyPropertyChanged
    {
        private uint counter = 1;
        private string filename = "";

        public uint Counter
        {
            get => counter; set
            {
                counter = value;
                OnPropertyChanged("Counter");
            }
        }

        public string Filename
        {
            get => filename; set
            {
                filename = value;
                OnPropertyChanged("Filename");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
