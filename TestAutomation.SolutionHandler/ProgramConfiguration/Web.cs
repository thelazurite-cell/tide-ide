namespace TestAutomation.SolutionHandler.ProgramConfiguration
{
    public class Web
    {
        public CustomErrors CustomErrors { get; set; }
        public Globalization Globalization { get; set; }
        public Compilation Compilation { get; set; }
        public HttpRuntime HttpRuntime { get; set; }
        public HttpCookies HttpCookies { get; set; }
        public HttpModules HttpModules { get; set; }
        public Pages Pages { get; set; }
        public HttpHandlers HttpHandlers { get; set; }
        public SessionState SessionState { get; set; }
    }
}