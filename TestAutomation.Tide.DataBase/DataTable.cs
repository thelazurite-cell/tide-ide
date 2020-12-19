using System.Collections.Generic;
using System.Collections.ObjectModel;
using FontAwesome.WPF;

namespace TestAutomation.Tide.DataBase
{
    public class DataTable : DbView
    {
        public string DatabaseName { get; set; }
        public override DbType Type => DbType.Table;
        public override string FriendlyName => $"{this.Schema}.{this.Name}";
        public override string Name { get; set; }

        public override FontAwesomeIcon Icon
        {
            get => this.TableType == DataTableType.View ? FontAwesomeIcon.Eye : FontAwesomeIcon.Table;
            set => throw new System.NotImplementedException();
        }

        public DataTableType TableType { get; set; }
        public string Schema { get; set; }
    }
}