using System.Collections.Generic;

namespace TestAutomation.SolutionHandler.ProjectTypes
{
    public class Target
    {
        public string Name { get; set; }
        public string BeforeTargets { get; set; }
        public List<ItemGroup> ItemGroups { get; set; } = new List<ItemGroup>();
        public List<PropertyGroup> PropertyGroups { get; set; } = new List<PropertyGroup>();
        public List<Error> Errors { get; set; } = new List<Error>();
    }
}