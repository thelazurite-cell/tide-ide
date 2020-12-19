using System.Collections.Generic;
using TestAutomation.Tide.DataBase;

namespace TAF.AutomationTool.Ui.ViewModels
{
    public class AffectedRowChangePair
    {
        public bool IsUpdating { get; set; }
        public bool IsInserting { get; set; }
        public bool IsDeleting { get; set; }
        public List<string> Affected { get; set; }
        public List<IDataChange> Changes { get; set; }
    }
}