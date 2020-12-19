using System.Collections.Generic;
using System.Configuration;
using TestAutomation.SolutionHandler.Core;

namespace TestAutomation.SolutionHandler.ProgramConfiguration
{
    public class ServiceModel
    {
        [FriendlyName(FriendlyName = "extensions")]
        public List<ExtensionsBase> Extensions { get; set; }

        [FriendlyName(FriendlyName = "bindings")]
        public Bindings Bindings { get; set; }

        [FriendlyName(FriendlyName = "client")]
        public Client Client { get; set; }
    }
}