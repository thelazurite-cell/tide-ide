using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using TAF.AutomationTool.Ui.ViewModels;
using TestAutomation.SolutionHandler.Core;

namespace TAF.AutomationTool.Ui
{
    public partial class GoToFileDialog : Window
    {
        private bool closing;
        private double originalHeight;

        public GoToFileDialog(CollectionFilter filter)
        {
            this.InitializeComponent();
            this.originalHeight = this.Height;
            this.DataContext = filter;
            this.Result.DataContext = filter.Filter;
            var bind = new Binding();
            this.Result.SetBinding(ItemsControl.ItemsSourceProperty, bind);
            this.SearchTerm.Focus();
        }

        public EventHandler FileSelected { get; set; }

        protected override void OnDeactivated(EventArgs e)
        {
            base.OnDeactivated(e);
            if (!this.closing)
                this.Close();
        }

        private void CloseCommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.WillClose();
        }

        private void WillClose()
        {
            this.closing = true;
            this.Close();
        }

        private void SelectFileCommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.SelectFile();
        }

        private void SelectFile()
        {
            if (this.Result.SelectedItem == null) return;
            this.FileSelected?.Invoke(this.Result.SelectedItem, EventArgs.Empty);
            this.WillClose();
        }

        private void SearchTerm_OnPreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (this.SearchTerm.Text.Length > 0)
            {
                this.Result.Visibility = Visibility.Visible;
                var files = this.DataContext as CollectionFilter;
                var projectNavigationItems = files?.Original.Where(this.MatchesSearchQuery);

                if (projectNavigationItems.Any()) this.Result.DataContext = projectNavigationItems;
                else
                    this.Result.DataContext = new
                        List<ProjectNavigationItem> {new ProjectNavigationItem() {Name = "No Results Found"}};
                var bind = new Binding();
                this.Result.SetBinding(ItemsControl.ItemsSourceProperty, bind);
                return;
            }

            if (Application.Current.MainWindow != null) Application.Current.MainWindow.Height = this.originalHeight;
            this.Result.Visibility = Visibility.Collapsed;
        }

        private bool MatchesSearchQuery(ProjectNavigationItem itm)
        {
            var query = this.SearchTerm.Text.ToLower();
            var name = itm.Name.ToLower();

            return name.StartsWith(query) ||
                   name.Contains(query) ||
                   query.ToCharArray().Intersect(name.ToCharArray()).Count() == query.ToCharArray().Length;
        }

        private void UIElement_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.SelectFile();
        }
    }
}