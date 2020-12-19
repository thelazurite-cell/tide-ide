namespace TestAutomation.Tide.DataBase
{
    public class SqlDataInsert : IDataChange
    {
        public int Index { get; set; }
        public DataTransferObject Source { get; set; }
    }
}