using System.Collections.Generic;
using TestAutomation.Tide.DataBase;

namespace TAF.AutomationTool.Ui.ViewModels
{
    public class UnpersistedChangesViewModel
    {
        public DtoCollection Context { get; set; }
        public List<AffectedRowChangePair> Modifications { get; set; } = new List<AffectedRowChangePair>();
    }
}