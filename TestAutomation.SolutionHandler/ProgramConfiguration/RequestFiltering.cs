namespace TestAutomation.SolutionHandler.ProgramConfiguration
{
    public class RequestFiltering
    {
        public string removeServerHeader { get; set; }
        public RequestLimits RequestLimits { get; set; }
        public Verbs Verbs { get; set; }
        public HiddenSegments HiddenSegments { get; set; }
    }
}