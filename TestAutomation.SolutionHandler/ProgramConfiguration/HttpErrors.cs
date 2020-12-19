using System;
using System.Collections.Generic;
using TestAutomation.SolutionHandler.Core;

namespace TestAutomation.SolutionHandler.ProgramConfiguration
{
    [FriendlyName(FriendlyName = "httpErrors")]
    public class HttpErrors
    {
        public String ErrorMode { get; set; }
        public string ExistingResponse { get; set; }
        public string DefaultResponseMode { get; set; }
        public List<IProgramConfigItem> HttpError { get; set; }
    }
}