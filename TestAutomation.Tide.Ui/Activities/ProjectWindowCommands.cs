using System.Windows.Input;

namespace TAF.AutomationTool.Ui.Activities
{
    public static class ProjectWindowCommands
    {
        public static RoutedCommand OpenFileCommand { get; } = new RoutedCommand();
        public static RoutedCommand CloseCurrentFileCommand { get; } = new RoutedCommand();
        public static RoutedCommand InitializeSearchEverywhere { get; } = new RoutedCommand();
        public static RoutedCommand InitializeGoToFile { get; } = new RoutedCommand();
        public static RoutedCommand SaveCurrentFile { get; } = new RoutedCommand();
        
        static ProjectWindowCommands()
        {
            OpenFileCommand.InputGestures.Add(new KeyGesture(Key.Enter));
            CloseCurrentFileCommand.InputGestures.Add(new KeyGesture(Key.W, ModifierKeys.Control));
            InitializeSearchEverywhere.InputGestures.Add(new KeyGesture(Key.F,
                ModifierKeys.Control | ModifierKeys.Shift));
            InitializeGoToFile.InputGestures.Add(new KeyGesture(Key.E, ModifierKeys.Control));
            SaveCurrentFile.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Control));
        }
    }
}