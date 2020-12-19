using System.Collections.Generic;
using TestAutomation.SolutionHandler.Core;

namespace TestAutomation.SolutionHandler.ProgramConfiguration
{
    [FriendlyName(FriendlyName = "staticContent")]
    public class StaticContent
    {
        public List<IProgramConfigItem> StaticContents { get; set; }
    }
}