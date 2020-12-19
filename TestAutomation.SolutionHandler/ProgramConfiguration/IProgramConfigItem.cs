namespace TestAutomation.SolutionHandler.ProgramConfiguration
{
    public interface IProgramConfigItem
    {
        string Name { get; set; }
        string StatusCode { get; set; }
        string SubStatusCode { get; set; }
        string Type { get; set; }
        string FileExtension { get; set; }
        string MimeType { get; set; }
    }
}