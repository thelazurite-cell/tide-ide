namespace TestAutomation.SolutionHandler.ProgramConfiguration
{
    public class WebServer
    {
        public Rewrite Rewrite { get; set; }
        public Validation Validaation { get; set; }
        public HttpErrors HttpErrors { get; set; }
        public Security Security { get; set; }
        public DefaultDocument DefaultDocument { get; set; }
        public Modules Modules { get; set; }
        public StaticContent StaticContent { get; set; }
        public Handlers Handlers { get; set; }
        public HttpProtocol HttpProtocol { get; set; }
    }
}