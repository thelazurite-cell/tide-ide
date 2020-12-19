using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace TestAutomation.SolutionHandler.Core
{
    /// <summary>
    /// The Project directory class.
    /// </summary>
    public class ProjectDirectory : ProjectNavigationItem
    {
        public override string FriendlyName { get; }

        /// <summary>
        /// Creates a new instance of a <see cref="ProjectDirectory"/>
        /// </summary>
        /// <param name="absoloute"></param>
        /// <param name="friendlyName"></param>
        public ProjectDirectory(string absoloute, string friendlyName = null)
        {
            this.Absoloute = absoloute;
            this.Name = absoloute.Split(Path.DirectorySeparatorChar).LastOrDefault();
            this.FriendlyName = friendlyName ?? this.Name;
            this.Type = NavigationItemType.Directory;
        }

        /// <summary>
        /// Provides a friendly name to display to the UI / Other Application.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => this.Name;
    }
}