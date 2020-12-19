using System.Collections.Generic;
using TestAutomation.SolutionHandler.Core;

namespace TestAutomation.SolutionHandler.ProgramConfiguration
{
    [FriendlyName(FriendlyName = "customHeaders")]
    public class CustomHeaders
    {
        public List<IProgramConfigItem> customHeader { get; set; }
    }
}