using System.Collections.Generic;
using FontAwesome.WPF;

namespace TestAutomation.Tide.DataBase
{
    public class TableColumn : DbView
    {
        private FontAwesomeIcon icon = FontAwesomeIcon.Columns;
        private string defaultValue;
        private bool hasDefaultValue;
        private bool isGenerated;
        public int ColumnId { get; set; }
        public string SchemaName { get; set; }
        public string TableName { get; set; }
        public override string Name { get; set; }
        public string QueryFriendlyName => $"[{this.Name}]";
        public override FontAwesomeIcon Icon
        {
            get => this.icon;
            set => this.icon = value;
        }
        public override string FriendlyName
        {
            get
            {
                var max = this.MaxLength == -1 ? "max" : "-1";
                var dt = max != "-1" ? $"{this.DataType}({max})" :
                    this.Precision != null ? $"{this.DataType}({this.Precision})" : this.DataType.ToString();
                return $"{this.Name} {dt}";
            }
        }

        public override DbType Type => DbType.Column;
        public DataType DataType { get; set; }
        public int MaxLength { get; set; }

        public bool IsGenerated
        {
            get => this.isGenerated;
            set
            {
                this.isGenerated = value;
                this.OnPropertyChanged(nameof(this.IsGenerated));
            }
        }

        public bool HasDefaultValue
        {
            get => this.hasDefaultValue;
            set
            {
                this.hasDefaultValue = value;
                this.OnPropertyChanged(nameof(this.HasDefaultValue));
            }
        }

        public string DefaultValue
        {
            get => this.defaultValue;
            set
            {
                this.defaultValue = value;
                this.OnPropertyChanged(nameof(this.DefaultValue));
                this.HasDefaultValue = !string.IsNullOrWhiteSpace(value);
            }
        }

        public int Precision { get; set; }
        public int ObjectId { get; set; }
        public string CollationName { get; set; }
        public bool IsNullable { get; set; }
        public bool IsAnsiPadding { get; set; }
        public bool IsRowGuidCol { get; set; }
        public bool IsIdentity { get; set; }
        public bool IsPrimaryKey { get; set; }
        public string Database { get; set; }
    }
}