using System.Collections;
using System.Collections.ObjectModel;
using FontAwesome.WPF;

namespace TestAutomation.SolutionHandler.Core
{
    public class ProjectNavigationItem : INavigableItem
    {
        public string Absoloute { get; set; }
        
        /// <summary>
        /// The system name of this navigation item.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets a friendly name to be displayed by the UI / Other Application
        /// </summary>
        public virtual string FriendlyName => this.Name;

        /// <summary>
        /// The type of navigation item this class is.
        /// </summary>
        public NavigationItemType Type { get; protected set; }

        public virtual ObservableCollection<ProjectNavigationItem> Children { get; set; } =
            new ObservableCollection<ProjectNavigationItem>();

        /// <summary>
        /// The navigation icon associated with this type of navigation item 
        /// </summary>
        public FontAwesomeIcon Icon
        {
            get
            {
                switch (this.Type)
                {
                    case NavigationItemType.Solution:
                        return FontAwesomeIcon.Terminal;
                    case NavigationItemType.Project:
                        return FontAwesomeIcon.Gear;
                    case NavigationItemType.Directory:
                        return FontAwesomeIcon.Folder;
                    case NavigationItemType.File:
                        return FontAwesomeIcon.File;
                    default:
                        return FontAwesomeIcon.None;
                }
            }
        }

        public bool IsExpanded { get; set; }
        public bool IsSelected { get; set; }
        
    }
}