using System.Collections.Generic;
using System.IO;

namespace TestAutomation.SolutionHandler.Core
{
    /// <summary>
    /// The Solution class, containing Attributes for serialization and deserialization
    /// </summary>
    public class Solution : ITideFormattable
    {
        /// <summary>
        /// The Format used to find the start of a project block
        /// </summary>
        public const string ProjectFormat = "Project(\"{0}\") = \"{1}\", \"{2}\",\"{3}\"";
        
        /// <summary>
        /// The Format used to find the start of a Project Section Block.
        /// </summary>
        public const string ProjectSectionFormat = "ProjectSection({0}) = {1}";
        
        /// <summary>
        /// The Format used to find the start of a Global Section Block.
        /// </summary>
        public const string GlobalSectionFormat = "GlobalSection({0}) = {1}";
        
        public Solution(string solutionPath)
        {
            this.SolutionPath = solutionPath;
            this.SolutionFile = Path.GetFileName(solutionPath);
        }

        [Format(Format = "Microsoft Visual Studio Solution File, Format Version {0}")]
        public string FormatVersion { get; set; }
        
        [Format(Format = "# {0}")]
        public string LastSavedByVisualStudioVersion { get; set; }

        [Format(Format = nameof(VisualStudioVersion) + " = {0}")]
        public string VisualStudioVersion { get; set; }

        [Format(Format = nameof(MinimumVisualStudioVersion) + " = {0}")]
        public string MinimumVisualStudioVersion { get; set; }

        public Dictionary<string, CsProject> Projects { get; set; }

        [Format(Format = "{0} = {1}")] public List<ConfigurationPlatform> SolutionConfigurationItems { get; set; }

        [Format(Format = "{0}.{1} = {2}")]
        public Dictionary<string, ConfigurationPlatform> ProjectConfigurationPlatforms { get; set; }

        [Format(Format = "{0} = {1}")] public Dictionary<string, string> SolutionProperties { get; set; }

        [Format(Format = "{0} = {1}")] public Dictionary<string, string> ExtensibilityGlobals { get; set; }

        public string SolutionPath { get; set; }
        public string SolutionFile { get; set; }
    }
}