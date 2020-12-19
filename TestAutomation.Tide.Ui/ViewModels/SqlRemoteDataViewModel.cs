using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TestAutomation.Tide.DataBase;
using TestAutomation.Tide.DataBase.Properties;

namespace TAF.AutomationTool.Ui.ViewModels
{
    public class SqlRemoteDataViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<int> ResultLength { get; set; } =
            new ObservableCollection<int>() {25, 50, 100, 250, 500, 1000};

        private bool _isClean = true;

        public bool IsClean
        {
            get => this._isClean;
            set
            {
                this._isClean = value;
                this.OnPropertyChanged(nameof(this.IsClean));
                this.OnPropertyChanged(nameof(this.IsDirty));
            }
        }

        public bool IsDirty
        {
            get => !this.IsClean;
        }

        public DtoCollection Context { get; set; } = new DtoCollection();
        public List<IDataChange> Changes { get; set; } = new List<IDataChange>();
        public List<DataTransferObject> AffectedRows { get; set; } = new List<DataTransferObject>();
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}