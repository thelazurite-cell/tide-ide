using System;
using System.Windows;
using Microsoft.Win32;
using TestAutomation.SolutionHandler.Core;

namespace TAF.AutomationTool.Ui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            this.InitializeComponent();
        }

        private async void OpenProject_OnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "C# Project Files | *.csproj";
            if (!(dialog.ShowDialog() ?? false)) return;

            var result = dialog.FileName;
            Console.WriteLine(result);
            var reader = new ProjectReader(result);

            Console.WriteLine($"{reader.WasSuccessful} : {reader.ErrorMessage}");
            await reader.AttemptFileLoad();
            reader.CsProject.GetAllFilesAndDirectories();

            this.Visibility = Visibility.Hidden;
            var win = new ProjectWindow(reader.CsProject);
            win.ShowDialog();
            this.Close();
        }
    }
}