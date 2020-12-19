using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using TestAutomation.SolutionHandler.Core;

namespace TAF.AutomationTool.Ui.CustomElements
{
    public static class WpfExtensions
    {

        public static Rect GetAbsolutePlacement(this FrameworkElement element, bool relativeToScreen = false)
        {
            var absolutePos = element.PointToScreen(new Point(0, 0));

            if (relativeToScreen || Application.Current.MainWindow == null)
                return new Rect(absolutePos.X, absolutePos.Y, element.ActualWidth, element.ActualHeight);

            var absoluteFromWin = Application.Current.MainWindow.PointToScreen(new Point(0, 0));
            absolutePos = new Point(absolutePos.X - absoluteFromWin.X, absolutePos.Y - absoluteFromWin.Y);

            return new Rect(absolutePos.X, absolutePos.Y, element.ActualWidth, element.ActualHeight);
        }

        public static void Invoke(Action action) => Application.Current.Dispatcher.Invoke(() => action?.Invoke()); 
        public static IEnumerable<ProjectNavigationItem> GetAncestors(ProjectNavigationItem root, ProjectNavigationItem current) 
        {
            var fullList = new List<ProjectNavigationItem>();
            var directory = current.Absoloute.Split(Path.DirectorySeparatorChar);
            fullList.Add(root);
            fullList.AddRange(AllAncestors(root, directory));
            fullList.Add(current);
            return fullList;
        }

        private static List<ProjectNavigationItem> AllAncestors(ProjectNavigationItem root, string[] directory)
        {
            var children = new List<ProjectNavigationItem>();
            for (var i = 1; i < directory.Length - 1; i++)
            {
                var directoryToLookFor = directory[i];
                foreach (var child in root.Children)
                {
                    if (child.Name != directoryToLookFor)
                        continue;
                    children.Add(child);
                    root = child;
                    break;
                }
            }

            return children;
        }
    }
}