using System;
using System.Collections.Generic;
using TestAutomation.SolutionHandler.Core;

namespace TestAutomation.SolutionHandler.ProgramConfiguration
{
    [FriendlyName(FriendlyName = "verbs")]
    public class Verbs
    {
        public String AllowUnlisted { get; set; }
        public List<Add> verb { get; set; }
    }
}