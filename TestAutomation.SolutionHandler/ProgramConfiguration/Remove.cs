namespace TestAutomation.SolutionHandler.ProgramConfiguration
{
    public class Remove : IProgramConfigItem
    {
        public string Name { get; set; }
        public string StatusCode { get; set; }
        public string SubStatusCode { get; set; }
        public string Type { get; set; }
        public string FileExtension { get; set; }
        public string MimeType { get; set; }
    }
}