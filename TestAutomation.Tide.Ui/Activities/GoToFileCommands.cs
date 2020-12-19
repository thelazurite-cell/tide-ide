using System.Windows.Input;

namespace TAF.AutomationTool.Ui.Activities
{
    public static class GoToFileCommands
    {
        public static RoutedCommand CloseDialogCommand { get; } = new RoutedCommand();
        public static RoutedCommand SelectFileCommand { get; } = new RoutedCommand();

        static GoToFileCommands()
        {
            CloseDialogCommand.InputGestures.Add(new KeyGesture(Key.Escape));
            SelectFileCommand.InputGestures.Add(new KeyGesture(Key.Enter));
        }
    }
}