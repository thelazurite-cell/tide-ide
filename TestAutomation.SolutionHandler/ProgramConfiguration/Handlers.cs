using System.Collections.Generic;
using TestAutomation.SolutionHandler.Core;

namespace TestAutomation.SolutionHandler.ProgramConfiguration
{
    [FriendlyName(FriendlyName = "handlers")]
    public class Handlers
    {
        public List<IProgramConfigItem> handler { get; set; }
    }
}