namespace TestAutomation.Tide.DataBase
{
    public class SqlDataUpdate : IDataChange
    {
        public int Index { get; set; }
        public string Property { get; set; }
        public object ColumnValue { get; set; }
        public object ColumnOriginalValue { get; set; }
    }
}