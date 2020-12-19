using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using FontAwesome.WPF;

namespace TestAutomation.Tide.DataBase
{
    public class Database: DbView
    {
        public override string Name { get; set; }
        public override FontAwesomeIcon Icon
        {
            get => FontAwesomeIcon.Database;
            set => throw new NotImplementedException();
        }

        public override DbType Type => DbType.Database;
        public int Id { get; set; }
        public DateTime CreationDate { get; set; }
        public string CollationName { get; set; }
    }
}