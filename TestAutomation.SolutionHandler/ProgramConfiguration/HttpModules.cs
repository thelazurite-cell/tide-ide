using System.Collections.Generic;
using TestAutomation.SolutionHandler.Core;

namespace TestAutomation.SolutionHandler.ProgramConfiguration
{
    [FriendlyName(FriendlyName = "httpModules")]
    public class HttpModules
    {
        public List<Add> HttpModule { get; set; }
    }
}