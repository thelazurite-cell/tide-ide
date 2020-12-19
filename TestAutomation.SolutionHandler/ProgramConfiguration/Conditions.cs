using System.Collections.Generic;
using TestAutomation.SolutionHandler.Core;

namespace TestAutomation.SolutionHandler.ProgramConfiguration
{
    [FriendlyName(FriendlyName = "conditions")]
    public class Conditions
    {
        public List<IProgramConfigItem> Condition { get; set; }
    }
}