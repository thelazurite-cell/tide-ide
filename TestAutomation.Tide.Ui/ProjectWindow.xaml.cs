using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TAF.AutomationTool.Ui.Activities;
using TAF.AutomationTool.Ui.ViewModels;
using TestAutomation.SolutionHandler.Core;
using static TAF.AutomationTool.Ui.CustomElements.WpfExtensions;
using Binding = System.Windows.Data.Binding;
using DragEventArgs = System.Windows.DragEventArgs;
using Panel = System.Windows.Controls.Panel;

namespace TAF.AutomationTool.Ui
{
    public partial class ProjectWindow 
    {
        public EventHandler FileSelectedN;
        public EventHandler MainWindowRendered;
        public EventHandler CreateNew;
        private ProjectEventManager eventBindings;
        private Explorer explorer { get; set; }
        private ProcessViewer console { get; set; }
        private MainPanel mainPanel { get; set; }
        private DataConnections dataConnections { get; set; }
        private RemoteDataView remoteData { get; set; }

        private bool readyToClose;

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!this.readyToClose)
            {
                e.Cancel = true;
                Task.Run(() => this.eventBindings.Closing?.Invoke(this, EventArgs.Empty));
            }
        }
        

        public ProjectWindow(CsProject project)
        {
            this.DataContext = project;
            this.InitializeComponent();
            this.eventBindings = new ProjectEventManager(this);
            this.explorer = new Explorer(project, this.eventBindings);
            this.console = new ProcessViewer(this.eventBindings);
            this.mainPanel = new MainPanel(project, this.eventBindings);
            this.dataConnections = new DataConnections(project.Configuration.GetPotentialConnectionStrings(), this.eventBindings);
            this.remoteData = new RemoteDataView(this.eventBindings);
            this.eventBindings.DisplayGoToFileDialog += EventBindingsOnDisplayGoToFileDialog;
        }

        private void EventBindingsOnDisplayGoToFileDialog(object sender, EventArgs e)
        {
            this.DisplayGoToFile();
        }


        private void CreateItemButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.CreateNew?.Invoke(this, EventArgs.Empty);
        }


        public IEnumerable<ProjectNavigationItem> GetAllFiles(ProjectNavigationItem items)
        {
            foreach (var node in items.Children)
            {
                foreach (var projectNavigationItem in this.GetAllFiles(node))
                {
                    yield return projectNavigationItem;
                }

                if (node is ProjectFile file)
                    yield return file;
            }
        }

        private void GoToFileCommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.DisplayGoToFile();
        }

        private void DisplayGoToFile()
        {
            var dc = this.DataContext as CsProject;
            var files = this.GetAllFiles(dc).ToList();

            var context = new CollectionFilter
            {
                Original = files,
                Filter = files
            };

            var dlg = new GoToFileDialog(context) {Owner = this};
            dlg.FileSelected += this.FileSelected;
            dlg.Closing += (o, args) => this.Focus();
            dlg.Show();
        }

        private void FileSelected(object sender, EventArgs e)
        {
            if (sender is ProjectFile file)
            {
                this.FileSelectedN?.Invoke(file, EventArgs.Empty);
            }
        }


        private void SearchEverywhereCommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var dlg = new SearchEverywhereDialog {Owner = this};
            dlg.ShowDialog();
        }

        private void ProjectWindow_OnContentRendered(object sender, EventArgs e)
        {
            // ReSharper disable once HeapView.ObjectAllocation.Evident
            this.DockManager.ParentWindow = this;
            this.eventBindings.ReadyToClose += this.ReadyToClose;
            if (this.DataContext is CsProject project) 
                this.GitBranch.Text = project.repo.Head.FriendlyName;
            
            //Show PropertyWindow docked to the top border
            this.explorer.DockManager = this.DockManager;
            this.console.DockManager = this.DockManager;
            this.mainPanel.DockManager = this.DockManager;
            this.dataConnections.DockManager = this.explorer.DockManager;
            this.remoteData.DockManager = this.DockManager;
            this.explorer.Show(Dock.Left);
            this.dataConnections.Show(Dock.Left);
            this.console.Show(Dock.Bottom);
            
            this.remoteData.Show(Dock.Bottom);
            this.mainPanel.Show();
            this.MainWindowRendered?.Invoke(this, EventArgs.Empty);
        }

        private void ReadyToClose(object sender, EventArgs e)
        {
            if (!(sender is bool shouldClose)) return;
            this.readyToClose = shouldClose;
            if (this.readyToClose)
            {
                Invoke(() =>
                {
                    if (this.DataContext is CsProject project)
                    {
                        project.repo.Dispose();
                    }
                    Environment.Exit(0);
                        
                });
            }
        }


        private void UIElement_OnDragOver(object sender, DragEventArgs e)
        {
            Console.Write("#");
        }


        private static void InsertItems(IEnumerable<UIElement> locDpChildren, Panel originDp,
            IEnumerable<UIElement> originDpChildren, Panel locDp)
        {
            foreach (var child in locDpChildren)
                originDp.Children.Add(child);
            foreach (var child in originDpChildren)
                locDp.Children.Add(child);
        }

        private static void RebindContainers(DockPanel originDp, DockPanel locDp)
        {
            var bind = new Binding();
            originDp.SetBinding(ItemsControl.ItemsSourceProperty, bind);
            var bindLoc = new Binding();
            locDp.SetBinding(ItemsControl.ItemsSourceProperty, bindLoc);
        }

        private static void RemoveItems(IEnumerable<UIElement> originDpChildren, Panel originDp,
            IEnumerable<UIElement> locDpChildren, Panel locDp)
        {
            foreach (var child in originDpChildren)
                originDp.Children.Remove(child);
            foreach (var child in locDpChildren)
                locDp.Children.Remove(child);
        }
    }
}