namespace TestAutomation.SolutionHandler.ProgramConfiguration
{
    public class Add: IProgramConfigItem
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public string Type { get; set; }
        public string FileExtension { get; set; }
        public string MimeType { get; set; }
        public string Name { get; set; }
        public string StatusCode { get; set; }
        public string SubStatusCode { get; set; }
        public string ConnectionString { get; set; }
        public string Assembly { get; set; }
        public string Namespace { get; set; }
        public string TagPrefix { get; set; }
        public string Verb { get; set; }
        public string Path { get; set; }
        public string Allowed { get; set; }
        public string Segment { get; set; }
        public string Input { get; set; }
        public string Pattern { get; set; }
        public string MatchType { get; set; }
        public string Negate { get; set; }
    }
}