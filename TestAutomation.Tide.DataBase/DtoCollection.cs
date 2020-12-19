using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TestAutomation.Tide.DataBase.Annotations;

namespace TestAutomation.Tide.DataBase
{
    public class DtoCollection : INotifyPropertyChanged
    {
        private string _timeTaken = string.Empty;
        public string ConnectionName { get; set; }
        public string ConnectionString { get; set; }
        public DataTable Table { get; set; }
        public string SqlCommand { get; set; }
        public Type DtoType { get; set; }

        public string TimeTaken
        {
            get => this._timeTaken;
            set
            {
                this._timeTaken = value;
                this.OnPropertyChanged(nameof(TimeTaken));
            }
        }

        public List<DataTransferObject> Dtos { get; set; } = new List<DataTransferObject>();
        public TablePagination Pagination { get; set; } = new TablePagination();
        public ObservableCollection<DbView> Columns { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}