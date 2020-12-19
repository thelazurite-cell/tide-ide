using System.Collections.Generic;
using TestAutomation.SolutionHandler.Core;

namespace TestAutomation.SolutionHandler.ProgramConfiguration
{
    public class Client
    {
        [FriendlyName(FriendlyName = "endpoint")]
        public List<Endpoint> Endpoints { get; set; }
    }
}