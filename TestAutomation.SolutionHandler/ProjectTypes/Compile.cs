namespace TestAutomation.SolutionHandler.ProjectTypes
{
    public class Compile : TargetObject
    {
        public string DependentUpon { get; set; }
        public string SubType { get; set; }
        public string ExcludeFromStylecop { get; set; }
        public string DesignTime { get; set; }
        public string AutoGen { get; set; }
    }
}