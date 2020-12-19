using System.Windows;

namespace TAF.AutomationTool.Ui.CustomElements
{
    public static class PointExtensions
    {
        public static bool Within(this Point pt, Rect r)
        {
            return pt.X >= r.X &&
               pt.Y >= r.Y &&
               pt.X < r.Width &&
               pt.Y < r.Height;
        }

        public static bool Within(this Point pt, Size s)
        {
            return pt.Within(new Rect(s));
        }
    }
}
