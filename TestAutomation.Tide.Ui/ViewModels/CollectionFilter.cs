using System.Collections.Generic;
using TestAutomation.SolutionHandler.Core;

namespace TAF.AutomationTool.Ui.ViewModels
{
    public class CollectionFilter
    {
        public List<ProjectNavigationItem> Original { get; set; }
        public List<ProjectNavigationItem> Filter { get; set; }
    }
}