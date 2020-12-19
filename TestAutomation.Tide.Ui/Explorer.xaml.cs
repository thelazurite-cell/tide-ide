using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using TAF.AutomationTool.Ui.Activities;
using TestAutomation.SolutionHandler.Core;
using static TAF.AutomationTool.Ui.CustomElements.WpfExtensions;

namespace TAF.AutomationTool.Ui
{
    public partial class Explorer
    {
        private readonly ProjectEventManager eventBindings;

        public Explorer(CsProject project, ProjectEventManager eventBindings)
        {
            this.InitializeComponent();
            eventBindings.FileSelected += this.FileSelected;
            this.eventBindings = eventBindings;
            this.eventBindings.MainWinowRendered += this.MainWinowRendered;
            this.ProjectView.DataContext = project.Children;
            var bind = new Binding();
            this.ProjectView.SetBinding(ItemsControl.ItemsSourceProperty, bind);
        }
        
        private void MainWinowRendered(object sender, EventArgs e)
        {
            new HorizontalMouseMove(this.ProjectView_Scroll, this.eventBindings.ProjectWindow);
        }

        private void FileSelected(object sender, EventArgs e)
        {
            if (sender is ProjectFile file)
                Invoke(() => this.SetExpandedStateInView(
                    GetAncestors((this.ProjectView.DataContext as ObservableCollection<ProjectNavigationItem>)?.First(),
                        file), isSelected: true));
        }

        private void OpenFileCommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.eventBindings.OpenFile?.Invoke(this.ProjectView.SelectedItem, EventArgs.Empty);
        }
        private void ProjectView_Scroll_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var viewer = (ScrollViewer) sender;
            viewer.ScrollToVerticalOffset(viewer.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        private void ProjectView_Scroll_OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
        }

        private void TreeViewItem_OnDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is TreeViewItem tvi) || !(tvi.DataContext is ProjectFile file)) return;

            e.Handled = true;
            this.eventBindings.OpenFile?.Invoke(file, EventArgs.Empty);
        }
        private void SetExpandedStateInView(IEnumerable<ProjectNavigationItem> itemsToExpand, bool isExpanded = true,
            bool isSelected = false)
        {
            var projectNavigationItems = itemsToExpand.ToList();
            if (!projectNavigationItems.Any()) return;
            // Grab hold of the current ItemsSource binding.
            var bindingExpression = this.ProjectView.GetBindingExpression(
                ItemsControl.ItemsSourceProperty);
            if (bindingExpression == null)
            {
                return;
            }

            // Clear that binding.
            var itemsSourceBinding = bindingExpression.ParentBinding;

            BindingOperations.ClearBinding(
                this.ProjectView, ItemsControl.ItemsSourceProperty);

            // Wait for the binding to clear and then set the expanded state of the view model.
            this.Dispatcher.BeginInvoke(
                DispatcherPriority.DataBind,
                new Action(() =>
                    this.SetExpandedStateInModel(projectNavigationItems, isExpanded, isSelected: isSelected)));

            // Now rebind the ItemsSource.
            this.Dispatcher.BeginInvoke(
                DispatcherPriority.DataBind,
                new Action(
                    () => this.ProjectView.SetBinding(
                        ItemsControl.ItemsSourceProperty, itemsSourceBinding)));
        }
        
        private void SetExpandedStateInModel(IEnumerable<ProjectNavigationItem> modelItems, bool isExpanded,
            bool recursive = false, bool isSelected = false)
        {
            if (modelItems == null)
            {
                return;
            }

            var projectNavigationItems = modelItems.ToList();
            foreach (var modelItem in projectNavigationItems)
            {
                if (!(modelItem is INavigableItem expandable))
                {
                    continue;
                }

                expandable.IsExpanded = isExpanded;
                expandable.IsSelected = isSelected;

                if (recursive)
                {
                    this.SetExpandedStateInModel(modelItem.Children, isExpanded, true, isSelected);
                }
            }
        }

        private void Explorer_OnLoaded(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("OnLoaded");

        }

        private void Explorer_OnContentRendered(object sender, EventArgs e)
        {
            MessageBox.Show("OnContentRendered");
        }
    }
}