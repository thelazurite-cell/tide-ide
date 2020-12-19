using System.Collections.Generic;
using TestAutomation.SolutionHandler.Core;

namespace TestAutomation.SolutionHandler.ProgramConfiguration
{
    public class Bindings
    {
        [FriendlyName(FriendlyName = "basicHttpBinding")]
        [FriendlyName(FriendlyName = "customBinding")]
        public List<BindingBase> BindingBases { get; set; }
    }
}