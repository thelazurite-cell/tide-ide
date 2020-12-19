using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace TestAutomation.Tide.DataBase
{
    public class DataView
    {
        public ObservableCollection<DbView> Headers { get; set; }
        public ObservableCollection<List<KeyValuePair<string, object>>> Results { get; set; } = new ObservableCollection<List<KeyValuePair<string, object>>>();
        public DataTable Table { get; set; }
    }
}