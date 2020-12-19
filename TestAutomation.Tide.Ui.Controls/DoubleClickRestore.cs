using System;
using System.Windows;
using System.Windows.Input;

namespace TestAutomation.Tide.Ui.Controls
{
  public class DoubleClickRestore : ICommand
  {
    public bool CanExecute(object parameter)
    {
      return true;
    }
    /// IGNORE:CS0067
    public event EventHandler CanExecuteChanged;
    public void Execute(object parameter)
    {
      var window = parameter as SharpWindow;
      if (window != null && window.IsMaximizable)
        window.WindowState = (window.WindowState == WindowState.Normal)
          ? WindowState.Maximized
          : WindowState.Normal;
    }
  }
}
