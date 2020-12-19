using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using FontAwesome.WPF;
using TestAutomation.Tide.DataBase.Properties;

namespace TestAutomation.Tide.DataBase
{
    public abstract class DbView : INotifyPropertyChanged
    {
        public DbView Parent { get; set; }
        public abstract string Name { get; set; }
        public virtual string FriendlyName => this.Name;
        public abstract FontAwesomeIcon Icon { get; set; }
        private ObservableCollection<DbView> children = new ObservableCollection<DbView>();

        public ObservableCollection<DbView> Children
        {
            get => this.children;
            set
            {
                this.children = value;
                this.OnPropertyChanged(nameof(this.Children));
            }
        }

        public abstract DbType Type { get; }
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}