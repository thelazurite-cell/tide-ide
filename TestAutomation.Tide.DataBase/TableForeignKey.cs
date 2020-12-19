using System.Collections.Generic;
using System.Collections.ObjectModel;
using FontAwesome.WPF;

namespace TestAutomation.Tide.DataBase
{
    public class TableForeignKey: DbView
    {
        public string DatabaseName { get; set; }
        public override string Name { get; set; }
        public override FontAwesomeIcon Icon
        {
            get => FontAwesomeIcon.ExternalLink;
            set => throw new System.NotImplementedException();
        }

        public override DbType Type => DbType.Secondary;
        public string PrimaryColumn { get; set; }
        public string PrimaryTable { get; set; }
        public string ForeignColumn { get; set; }
        public string ForeignTable { get; set; }
    }
}