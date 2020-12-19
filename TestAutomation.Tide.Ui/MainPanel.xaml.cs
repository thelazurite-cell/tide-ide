using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using DockingLibrary;
using FontAwesome.WPF;
using TAF.AutomationTool.Ui.Activities;
using TAF.AutomationTool.Ui.CustomElements;
using TAF.AutomationTool.Ui.ViewModels;
using TestAutomation.SolutionHandler.Core;
using TestAutomation.Tide.Ui.Controls;
using static TAF.AutomationTool.Ui.CustomElements.WpfExtensions;


namespace TAF.AutomationTool.Ui
{
    public partial class MainPanel : DocumentContent
    {
        private readonly ProjectEventManager eventBindings;

        public MainPanel(CsProject project, ProjectEventManager eventBindings)
        {
            this.InitializeComponent();
            this.DataContext = project;
            this.eventBindings = eventBindings;
            this.eventBindings.FileSelected += this.FileSelected;
            this.eventBindings.MainWinowRendered += this.MainWinowRendered;
            this.eventBindings.CreateNewFile += this.CreateNewFile;
            this.eventBindings.OpenFile += this.OpenFileCommandBinding_OnExecuted;
            this.eventBindings.Closing += this.Attempt_Closing;
           
            this.Title = project.FriendlyName;
        }

        private async void Attempt_Closing(object sender, EventArgs e)
        {
            var shouldClose = await this.PerformCloseAllFiles();
            this.eventBindings.ReadyToClose?.Invoke(shouldClose, EventArgs.Empty);
        }

        private async void CloseAllFiles(object sender, EventArgs e)
        {
            await this.PerformCloseAllFiles();
        }

        private async Task<bool> PerformCloseAllFiles()
        {
            var mainTabItems = this.MainTab.Items.OfType<ClosableTabItem>().ToList();
            if (!mainTabItems.Any()) return true;
            for (var i = mainTabItems.Count - 1; i >= 0; i--)
            {
                if (!(this.MainTab.Items[i] is ClosableTabItem editor)) continue;
                if (!await this.CloseCurrentTab(editor))
                {
                    return false;
                }
            }

            return true;
        }

        private void CreateNewFile(object sender, EventArgs e)
        {
            Invoke(() => this.MainTab.Items.Add(new AddNewItem()));
        }

        private void MainWinowRendered(object sender, EventArgs e)
        {
            this.CreateStaticMenu();
        }

        private async void FileSelected(object sender, EventArgs e)
        {
            if (sender is ProjectFile file)
                await this.PerformProjectFileOpen(file);
        }

        private async void ItmOnCloseTab(object sender, RoutedEventArgs e)
        {
            if (!(sender is ClosableTabItem editor)) return;
            await this.CloseCurrentTab(editor);
        }

        private async Task<bool> CloseCurrentTab(ClosableTabItem tabItem)
        {
            var close = true;

            if (tabItem.IsModified)
            { 
                Invoke(async () =>
                {
                    var dlg = new SharpDialog("You have unsaved changes do you want to save them?",
                        "Unsaved ChangesManager",
                        SharpDialog.SharpDialogButton.YesNoCancel, SharpDialog.SharpDialogType.Question, parent: this.eventBindings.ProjectWindow);
                    dlg.ShowDialog();

                    close = await ShouldClose(dlg.Result, tabItem);
                });
            }

            if (!close) return false;
            Invoke(() =>
            {
                var i = this.MainTab.SelectedIndex;
                if (i > 0) this.MainTab.SelectedIndex--;
                else
                {
                    this.SpShortcuts.Visibility = Visibility.Visible;

                    this.MainTab.SelectedItem = null;
                }

                var pws = this.GetWindowSelector() as Button;
                if (pws?.ContextMenu != null)
                {
                    var contextMenuItems = pws.ContextMenu.Items;
                    var itms = pws.ContextMenu.Items.OfType<MenuItem>();
                    var itm = itms?.FirstOrDefault(mi => mi.Tag is UiIndexFriendlyName uifn && uifn.Index == i);
                    pws.ContextMenu.Items.Remove(itm);

                    var bind = new Binding();
                    pws.SetBinding(ItemsControl.ItemsSourceProperty, bind);
                }

                this.MainTab.Items.Remove(tabItem);
            });
            return true;
        }

        private void ItmOnCloseOtherTabs(object sender, RoutedEventArgs e)
        {
            if (!(sender is FileEditor editor)) return;
            this.MainTab.Items.Clear();
            this.MainTab.Items.Add(editor);
            this.MainTab.SelectedItem = editor;
            var pws = this.GetWindowSelector() as Button;
            if (pws?.ContextMenu == null) return;
            pws.ContextMenu.Items.Clear();
            pws.ContextMenu.Items.Insert(0, this.CreateMenuItem(editor));
            this.CreateStaticMenu();
        }

        private async void RequestReload(object sender, EventArgs e)
        {
            if (!(sender is ClosableTabItem tab) || !(this.DataContext is CsProject project)) return;
            var curr = this.MainTab.SelectedIndex;
            var ap = tab.Absoloute;
            var items = this.MainTab.Items.Cast<object>().ToList();

            this.MainTab.Items.Clear();
            for (var i = 0; i < items.Count; i++)
            {
                if (i != curr)
                    Invoke(() => this.MainTab.Items.Add(items[i]));
                else
                {
                    if (tab is FileEditor editor)
                    {
                        var pf = editor.ProjectFile;

                        var filedata = await project.ReadFile(ap);
                        this.OpenFileEditorTab(pf, filedata);
                    }
                    else
                    {
                        Console.WriteLine(tab.GetType());
                        Invoke(() => this.MainTab.Items.Add(items[i]));
                    }
                }
            }
        }

        private async Task PerformProjectFileOpen(ProjectFile projectFile)
        {
            if (!(this.DataContext is CsProject project)) return;
            var ft = Path.GetExtension(projectFile.Name);
            try
            {
                var fileData = await project.ReadFile(projectFile.Absoloute);
                foreach (var mainTabItem in this.MainTab.Items)
                {
                    if (!(mainTabItem is FileEditor fe) ||
                        !fe.AbsoluteFilePath.Equals(projectFile.Absoloute, StringComparison.OrdinalIgnoreCase))
                        continue;
                    this.MainTab.SelectedItem = mainTabItem;
                    break;
                }

                this.OpenFileEditorTab(projectFile, fileData);
            }
            catch (Exception ex)
            {
                Invoke(() => MessageBox.Show(this,
                    $"Couldn't open file '{projectFile.Name}' because an exception was thrown.\n{ex.Message}",
                    "Couldn't open file", MessageBoxButton.OK, MessageBoxImage.Error));
            }
        }

        private void OpenFileEditorTab(ProjectFile projectFile, FileData txt)
        {
            var itm = new FileEditor(projectFile, txt) {DataContext = this.DataContext, Owner = this};
            this.OpenContext(itm);
        }

        private void OpenContext(ClosableTabItem itm)
        {
            itm.CloseOtherTabs += this.ItmOnCloseOtherTabs;
            itm.CloseTab += this.ItmOnCloseTab;
            itm.RequestReload += this.RequestReload;

            Invoke(() =>
            {
                if (this.MainTab.Items.Count == 0)
                {
                    this.SpShortcuts.Visibility = Visibility.Collapsed;
                }

                this.MainTab.Items.Add(itm);
                this.MainTab.SelectedItem = itm;

                var pws = this.GetWindowSelector();
                if (!(pws is Button btn)) return;
                var menuItem = this.CreateMenuItem(itm);
                btn.ContextMenu?.Items.Insert(0, menuItem);
            });
        }

        private MenuItem CreateMenuItem(ClosableTabItem itm)
        {
            var menuItem = new MenuItem
            {
                Tag = new UiIndexFriendlyName
                {
                    FriendlyName = itm.Name,
                    Absoloute = itm.Absoloute,
                    Index = this.MainTab.Items.Count - 1
                },
                Header = itm.Name,
                Icon = new ImageAwesome {Icon = itm.Icon}
            };
            menuItem.Click += this.MenuItemOnClick;
            return menuItem;
        }

        private object GetWindowSelector()
        {
            return this.MainTab.Template.FindName("PART_WindowSelector", this.MainTab);
        }

        private void MenuItemOnClick(object sender, RoutedEventArgs e)
        {
            if (!(sender is MenuItem item) || !(item.Tag is UiIndexFriendlyName uifn)) return;
            this.MainTab.SelectedIndex = uifn.Index;
            if (!(this.MainTab.SelectedItem is ClosableTabItem editor)) return;
            var scrollViewer = this.MainTab.Template.FindName("PART_ScrollViewer", this.MainTab) as ScrollViewer;
            var loc = editor.GetTabLocation(scrollViewer);

            scrollViewer?.ScrollToHorizontalOffset(loc.X);
        }

        private static async Task<bool> ShouldClose(SharpDialog.SharpDialogResult result, ClosableTabItem fileEditor)
        {
            switch (result)
            {
                case SharpDialog.SharpDialogResult.Yes:
                    var res = await fileEditor.SaveFile();
                    return res;
                case SharpDialog.SharpDialogResult.No:
                    return true;
                default:
                    return false;
            }
        }


        private async void OpenFileCommandBinding_OnExecuted(object sender, EventArgs e)
        {
            if (sender is ProjectFile file)
                await this.PerformProjectFileOpen(file);
        }

        private async void CloseCurrentCommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.MainTab.SelectedItem is FileEditor editor)
                await this.CloseCurrentTab(editor);
        }

        private void CreateStaticMenu()
        {
            // ReSharper disable once ObjectCreationAsStatement - We just need to create the class, it'll manage itself
            var pws = this.GetWindowSelector() as Button;
            if (pws?.ContextMenu == null) return;
            pws.ContextMenu.Items.Add(new Separator());
            var closeAllButton = new MenuItem() {Header = "Close All"};
            closeAllButton.Click += this.CloseAllFiles;
            pws.ContextMenu.Items.Add(closeAllButton);
        }

        private async void SaveCurrentFileCommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.MainTab.SelectedItem is FileEditor editor)
            {
                await editor.SaveFile();
            }
        }

        private void GoToFileButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.eventBindings.DisplayGoToFileDialog?.Invoke(this, EventArgs.Empty);
        }
    }
}