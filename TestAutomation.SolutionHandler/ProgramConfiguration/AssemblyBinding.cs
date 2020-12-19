using System.Collections.Generic;

namespace TestAutomation.SolutionHandler.ProgramConfiguration
{
    public class AssemblyBinding
    {
        public string Xmlns { get; set; }
        public List<DependentAssembly> DependentAssemblies { get; set; } = new List<DependentAssembly>();
    }
}