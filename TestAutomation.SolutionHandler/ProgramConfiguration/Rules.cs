using System.Collections.Generic;
using TestAutomation.SolutionHandler.Core;

namespace TestAutomation.SolutionHandler.ProgramConfiguration
{
    [FriendlyName(FriendlyName = "rules")]
    public class Rules
    {
        public List<IProgramConfigItem> rule { get; set; }
    }
}