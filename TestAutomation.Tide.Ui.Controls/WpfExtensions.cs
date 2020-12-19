using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace TAF.AutomationTool.Ui.CustomElements
{
    public static class WpfExtensions
    {
        public static T FindControlAbove<T>(this DependencyObject thi, int occurrence = 0)
        {
            var amtOfOccurrences = 0;
            var found = false;
            var ele = thi as FrameworkElement;
            while (!found)
            {
                //var ty = ele?.GetType().ToString() ?? "Null";
                //Console.Write($"{ty}({ele.Name}) -> ");
                switch (ele.Parent)
                {
                    case T expected:
                        if (amtOfOccurrences == occurrence)
                            return expected;
                        else
                            amtOfOccurrences++;
                        continue;
                    case null:
                        return default;
                }

                ele = ele.Parent as FrameworkElement;
            }

            return default;
        }

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

    }
}