using System.Collections.Generic;
using TestAutomation.SolutionHandler.Core;

namespace TestAutomation.SolutionHandler.ProgramConfiguration
{
    [FriendlyName(FriendlyName = "modules")]
    public class Modules
    {
        public List<IProgramConfigItem> Module { get; set; }
    }
}