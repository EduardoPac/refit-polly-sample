using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace PollyRefitSample.ViewModels
{
    public class BaseViewModel
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
            {
                return false;
            }

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        private string _title;
        public string Title
        {
            get { return _title; }
            set
            {
                SetProperty(ref _title, value);
            }
        }

        private bool _busy;
        public bool Busy
        {
            get { return _busy; }
            set
            {
                SetProperty(ref _busy, value);
            }
        }

        public virtual Task LoadAsync(object[] args) => Task.FromResult(true);

        public virtual Task LoadAsync() => Task.FromResult(true);
    }
}