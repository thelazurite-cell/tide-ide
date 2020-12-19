namespace TestAutomation.SolutionHandler.ProgramConfiguration
{
    public class Rule: IProgramConfigItem
    {
        public string Name { get; set; }
        public string StatusCode { get; set; }
        public string SubStatusCode { get; set; }
        public string Type { get; set; }
        public string FileExtension { get; set; }
        public string MimeType { get; set; }
        public string StopProcessing { get; set; }
        public Match Match { get; set; }
        public Conditions Conditions { get; set; }
        public Action Action { get; set; }
    }
}