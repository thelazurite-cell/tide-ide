using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MessageBox = System.Windows.Forms.MessageBox;

namespace TAF.AutomationTool.Ui
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public EventHandler CloseOtherTabs;
        private void CloseOthers_OnClick(object sender, RoutedEventArgs e)
        {
           this.CloseOtherTabs?.Invoke(sender, EventArgs.Empty);
        }

        private void UIElement_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var viewer = (ScrollViewer) sender;
            viewer.ScrollToHorizontalOffset(viewer.HorizontalOffset - e.Delta);
            e.Handled = true;
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            if (!(sender is FrameworkElement element && element.ContextMenu != null)) return;
            element.ContextMenu.IsOpen = !element.ContextMenu.IsOpen;
        }
    }
}