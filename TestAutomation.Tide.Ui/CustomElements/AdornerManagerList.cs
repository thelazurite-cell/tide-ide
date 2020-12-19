using System.Collections.Generic;

namespace TAF.AutomationTool.Ui.CustomElements
{
    public static class AdornerManagerList
    {
        public static List<AdornerManager> AdornerManager { get; set; } = new List<AdornerManager>();

        public static void ClearAll()
        {
            foreach (var manager in AdornerManager)
            {
                manager?.Remove();
            }
            AdornerManager.Clear();
        }
    }
}