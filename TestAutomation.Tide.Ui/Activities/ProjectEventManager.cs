using System;

namespace TAF.AutomationTool.Ui.Activities
{
    public class ProjectEventManager
    {
        public EventHandler FileSelected;
        public EventHandler MainWinowRendered;
        public EventHandler CreateNewFile;
        public EventHandler OpenFile;
        public EventHandler Closing;
        public EventHandler ReadyToClose;
        public EventHandler SqlDataReceived;
        public EventHandler RefreshSqlData;
        public EventHandler DisplayGoToFileDialog;
        public EventHandler GetData;
        public ProjectWindow ProjectWindow { get; }

        public ProjectEventManager(ProjectWindow projectWindow)
        {
            projectWindow.FileSelectedN += (sender, e) => this.FileSelected?.Invoke(sender, e);
            projectWindow.MainWindowRendered += (sender, e) => this.MainWinowRendered?.Invoke(sender, e);
            projectWindow.CreateNew += (sender, e) => this.CreateNewFile?.Invoke(sender, e);

            this.ProjectWindow = projectWindow;
        }

    }
}