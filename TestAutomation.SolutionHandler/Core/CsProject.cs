using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using LibGit2Sharp;
using TestAutomation.SolutionHandler.ProjectTypes;
using Configuration = TestAutomation.SolutionHandler.Core.Configuration;

namespace TestAutomation.SolutionHandler.Core
{
    public class CsProject : ProjectNavigationItem
    {
        /// <summary>
        /// Creates a new instance of the <see cref="CsProject"/> class.
        /// Used when the project is being directly loaded OR the CsProject definition in the solution file does not have
        /// a project items section.
        /// </summary>
        /// <param name="projectFullPath"></param>
        public CsProject(string projectFullPath)
        {
            this.ProjectPath = Path.GetDirectoryName(projectFullPath);

            var directories = this.ProjectPath?.Split(Path.DirectorySeparatorChar) ?? new[] {string.Empty};
            var temp = string.Empty;
            const string gitFile = ".git";
            foreach (var directory in directories)
            {
                temp += $"{directory}{Path.DirectorySeparatorChar}";
                var git = Path.Combine(temp, gitFile);
                try
                {
                    var fileInfo = new DirectoryInfo(git);
                    if (!fileInfo.Exists || !fileInfo.Attributes.HasFlag(FileAttributes.Hidden)) continue;
                    this.repo = new Repository(temp);
                    Console.WriteLine($"Found Git Repository: {git}");
                    break;
                }
                catch (Exception)
                {
                }
            }

            this.ProjectFile = Path.GetFileName(projectFullPath);
            this.Name = this.ProjectFile;
            this.Type = NavigationItemType.Project;
        }

        public Repository repo { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="CsProject"/> class.
        /// Used when the definitions in the Solution file for a CSProject contains a project items section - Thanks past me
        /// </summary>
        /// <param name="projectFullPath">the full location for the project file</param>
        /// <param name="solutionItems">the project files from the project items section</param>
        public CsProject(string projectFullPath, List<None> solutionItems)
        {
            this.ProjectPath = Path.GetDirectoryName(projectFullPath);
            this.ProjectFile = Path.GetFileName(projectFullPath);
            this.Name = this.ProjectFile;
            this.Type = NavigationItemType.Project;
            var itemGroup = new ItemGroup();
            itemGroup.Objects.AddRange(solutionItems);
            this.ItemGroups.Add(itemGroup);
        }

        /// <summary>
        /// Gets or sets the project path.
        /// </summary>
        public string ProjectPath { get; set; }

        /// <summary>
        /// Gets or sets the project file.
        /// </summary>
        public string ProjectFile { get; set; }

        /// <summary>
        /// Gets or sets the version of the build tools being used by this project.
        /// </summary>
        public string ToolsVersion { get; set; }

        public string DefaultTargets { get; set; }

        /// <summary>
        /// Gets or sets the imports included by this project 
        /// </summary>
        public List<Import> Imports { get; set; } = new List<Import>();

        /// <summary>
        /// Gets or sets the targets for this project
        /// </summary>
        public List<Target> Targets { get; set; } = new List<Target>();

        /// <summary>
        /// Gets or sets the list of item groups that are in the project file
        /// </summary>
        public List<ItemGroup> ItemGroups { get; set; } = new List<ItemGroup>();

        /// <summary>
        /// Gets or sets the list of property groups that are in the project file
        /// </summary>
        public List<PropertyGroup> PropertyGroups { get; set; } = new List<PropertyGroup>();
        
        public Configuration Configuration { get; set; }

        public string xmlns { get; set; }

        /// <summary>
        /// Gets an observable list of nodes this nav item contains.
        /// </summary>
        public override ObservableCollection<ProjectNavigationItem> Children => this.GetAllFilesAndDirectories();

        /// <summary>
        /// Gets all of the files and directories the CsProject contains based on the accepted Item Types from all item groups.
        /// The data is then reordered into a tree structure. 
        /// </summary>
        /// <returns></returns>
        public ObservableCollection<ProjectNavigationItem> GetAllFilesAndDirectories()
        {
            var items = this.GetAllAcceptedItemsUsing(AcceptedProjectNodes());

            var tree = new ObservableCollection<ProjectNavigationItem>
            {
                new ProjectDirectory(".", Path.GetFileNameWithoutExtension(this.ProjectFile))
            };

            try
            {
                foreach (var item in items)
                    this.DetermineProjectItemCreation(tree, item);
            }
            catch (Exception e)
            {
#if DEBUG
                Console.BackgroundColor = ConsoleColor.Red;
                Console.WriteLine(e.Message);
                Console.ResetColor();
#endif
                throw;
            }


            return (ObservableCollection<ProjectNavigationItem>) this.OrderStructure(tree);
        }

        private IEnumerable<TargetObject> GetAllAcceptedItemsUsing(IEnumerable<Type> fileTypes)
        {
            var items = this.ItemGroups.SelectMany(
                group => @group.Objects.Where(
                    item => fileTypes.Any(
                        type => type == item.GetType()
                    )
                )
            ).ToList();
            return items;
        }

        private ICollection<ProjectNavigationItem> OrderStructure(ICollection<ProjectNavigationItem> tree)
        {
            foreach (var itm in tree.ToList())
            {
                if (!(itm is ProjectDirectory dir)) continue;
                var projectNavigationItems =
                    new ObservableCollection<ProjectNavigationItem>(
                        dir.Children?.OrderByDescending(d => d.Type).ToList() ?? new List<ProjectNavigationItem>());
                tree.Remove(itm);
                dir.Children =
                    (ObservableCollection<ProjectNavigationItem>) this.OrderStructure(projectNavigationItems);
                tree.Add(dir);
            }

            return tree;
        }

        private static Type[] AcceptedProjectNodes()
        {
            return new[]
            {
                typeof(None),
                typeof(Compile),
                typeof(Content),
                typeof(Folder)
            };
        }

        private void DetermineProjectItemCreation(ObservableCollection<ProjectNavigationItem> tree, TargetObject item)
        {
            var root = GetRoot(tree);
            var split = item.Include.Split(Path.DirectorySeparatorChar);
            var combo = ".";

            foreach (var t in split)
            {
                combo += $"{Path.DirectorySeparatorChar}{t}";
                var found = this.FoundPath(combo, tree);

                if (Path.HasExtension(combo))
                {
                    root?.Children.Add(new ProjectFile {Name = t, Absoloute = combo});
                }
                else if (found == null)
                {
                    root?.Children.Add(new ProjectDirectory(combo));
                    root = (ProjectDirectory) root?.Children.SingleOrDefault(itm =>
                        itm is ProjectDirectory dir && dir.Absoloute == combo);
                }
                else
                {
                    root = found;
                }
            }
        }

        private ProjectDirectory FoundPath(string combo, ICollection<ProjectNavigationItem> tree)
        {
            ProjectDirectory directory = null;
            foreach (var currentSearch in tree.Select(itm => itm as ProjectDirectory).Where(itm => itm != null))
            {
                var childSearch = this.FoundPath(combo, currentSearch.Children);
                if (childSearch != null)
                {
                    directory = childSearch;
                    break;
                }

                if (currentSearch.Absoloute != combo) continue;
                directory = currentSearch;
                break;
            }

            return directory;
        }

        private static ProjectDirectory GetRoot(ObservableCollection<ProjectNavigationItem> tree)
        {
            return (ProjectDirectory) tree.SingleOrDefault(itm => itm is ProjectDirectory dir && dir.Absoloute == ".");
        }

        public async Task<FileData> ReadFile(string absoloute)
        {
            var fullpath = Path.Combine(this.ProjectPath, absoloute);
            using (var fs = new FileStream(fullpath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (var sr =
                    FileHandler.AutoDetect(fs, (byte) fs.ReadByte(), (byte) fs.ReadByte(), Encoding.UTF8))
                {
                    var enc = sr.CurrentEncoding;
                    var data = await sr.ReadToEndAsync();
                    return new FileData {Data = data, Encoding = enc};
                }
            }
        }
    }
}