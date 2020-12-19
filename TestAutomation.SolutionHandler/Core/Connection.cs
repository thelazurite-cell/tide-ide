using System.Collections.Generic;
using System.Collections.ObjectModel;
using FontAwesome.WPF;
using TestAutomation.Tide.DataBase;

namespace TestAutomation.SolutionHandler.Core
{
    public class Connection: DbView
    {
        public override string Name { get; set; }
        public override FontAwesomeIcon Icon
        {
            get => FontAwesomeIcon.Server;
            set => throw new System.NotImplementedException();
        }

        public override DbType Type => DbType.Connection;
        public string ConnectionString { get; set; }
        public MatchesAConnectionString MatchProbability { get; set; }
    }
}