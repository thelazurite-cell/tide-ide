using System.Collections.Generic;
using System.Collections.ObjectModel;
using TestAutomation.SolutionHandler.ProgramConfiguration;

namespace TestAutomation.SolutionHandler.Core
{
    public class Configuration
    {
        [FriendlyName(FriendlyName = "configSections")]
        public List<Section> ConfigSections { get; set; } = new List<Section>();

        [FriendlyName(FriendlyName = "appSettings")]
        public List<Add> AppSettings { get; set; } = new List<Add>();

        [FriendlyName(FriendlyName = "connectionStrings")]
        public List<Add> ConnectionStrings { get; set; } = new List<Add>();

        [FriendlyName(FriendlyName = "runtime")]
        public List<AssemblyBinding> Runtime { get; set; } = new List<AssemblyBinding>();

        [FriendlyName(FriendlyName = "startup")]
        public Startup Startup { get; set; }

        [FriendlyName(FriendlyName = ("system.serviceModel"))]
        public ServiceModel ServiceModel { get; set; }
        [FriendlyName(FriendlyName = "system.web")]
        public Web Web { get; set; }
        [FriendlyName(FriendlyName = "system.webServer")]
        public WebServer WebServer { get; set; }
//        [FriendlyName(FriendlyName = "system.web.extensions")]
//        public WebExtensions WebExtensions { get; set; }
//        [FriendlyName(FriendlyName = "location")]
//        public Location Location { get; set; }
//        [FriendlyName(FriendlyName = "system.codedom")]
//        public CodeDom CodeDom { get; set; }

        public ObservableCollection<Connection> GetPotentialConnectionStrings()
        {
            var temp = new ObservableCollection<Connection>();
            foreach (var appSetting in this.AppSettings)
                if (appSetting.Key.ToLower().Contains("connectionstring"))
                    temp.Add(new Connection
                    {
                        Name = appSetting.Key,
                        ConnectionString = appSetting.Value,
                        MatchProbability = MatchesAConnectionString.Possible
                    });

            foreach (var connectionString in this.ConnectionStrings)
                temp.Add(new Connection
                {
                    Name = connectionString.Name,
                    ConnectionString = connectionString.ConnectionString,
                    MatchProbability = MatchesAConnectionString.Definite
                });

            return temp;
        }
    }
}