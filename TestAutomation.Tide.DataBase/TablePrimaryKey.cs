using System;
using FontAwesome.WPF;

namespace TestAutomation.Tide.DataBase
{
    public class TablePrimaryKey : DbView
    {
        public string DatabaseName { get; set; }
        public string TableName { get; set; }
        public override string Name { get; set; }

        public override FontAwesomeIcon Icon
        {
            get => FontAwesomeIcon.Key;
            set => throw new NotImplementedException();
        }

        public string[] IndexColumnsNames { get; set; }
        public string[] IncludedColumnsNames { get; set; }
        public bool IsUnique { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsUniqueConstraint { get; set; }
        public bool IsAutoIncrement { get; set; }
        public override DbType Type => DbType.Primary;
    }
}