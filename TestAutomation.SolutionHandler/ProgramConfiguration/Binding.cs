namespace TestAutomation.SolutionHandler.ProgramConfiguration
{
    public class Binding
    {
        public string Name { get; set; }
        public TextMessageEncoding TextMessageEncoding { get; set; }
        public HttpsTransport HttpsTransport { get; set; }
        public Security Security { get; set; }
    }
}