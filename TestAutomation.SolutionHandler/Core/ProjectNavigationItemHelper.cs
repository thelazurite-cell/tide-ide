using System.Collections.Generic;
using System.Linq;

namespace TestAutomation.SolutionHandler.Core
{
    public static class ProjectNavigationItemHelper
    {
        public static IEnumerable<ProjectNavigationItem> Descendants(this ProjectNavigationItem root)
        {
            var nodes = new Stack<ProjectNavigationItem>(new[] {root});
            while (nodes.Any())
            {
                ProjectNavigationItem node = nodes.Pop();
                yield return node;
                foreach (var n in node.Children) nodes.Push(n);
            }
        }
    }
}