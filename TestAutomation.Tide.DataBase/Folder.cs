using System;
using FontAwesome.WPF;

namespace TestAutomation.Tide.DataBase
{
    public class Folder : DbView
    {
        public override string Name { get; set; }
        public override string FriendlyName => $"{this.Name} ({this.Children?.Count})";

        public override FontAwesomeIcon Icon
        {
            get => FontAwesomeIcon.Folder;
            set => throw new NotImplementedException();
        }

        public override DbType Type => DbType.Folder;
    }
}