using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using FontAwesome.WPF;
using TAF.AutomationTool.Ui.Activities;
using TestAutomation.SolutionHandler.Core;
using TestAutomation.Tide.Ui.Controls;
using static TAF.AutomationTool.Ui.CustomElements.WpfExtensions;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace TAF.AutomationTool.Ui.CustomElements
{
    public partial class FileEditor
    {
        public Window Owner;
        public readonly ProjectFile ProjectFile;
        private string original;

        public FileEditor(ProjectFile projectFile, FileData content)
        {
            this.OriginalFile = content;
            this.ProjectFile = projectFile;
            this.InitializeComponent();
            this.SetContent(content.Data);
            this.Header = projectFile.Name;
        }

        private FileData OriginalFile { get; set; }

        public FileEditor()
        {
            this.InitializeComponent();
        }

        private void SetContent(string content)
        {
            this.original = content;
            this.TextEditor.Text = content;
        }

        public string AbsoluteFilePath => this.ProjectFile.Absoloute;

        private void TextEditor_OnKeyDown(object sender, KeyEventArgs e)
        {
            this.IsModified = this.TextEditor.Text != this.original;
        }

        private async void Save_OnClick(object sender, RoutedEventArgs e)
        {
            await this.SaveFile();
        }

        public override string Name => this.ProjectFile.Name;
        public override string Absoloute => this.ProjectFile.Absoloute;
        public override FontAwesomeIcon Icon => this.ProjectFile.Icon;


        public override async Task<bool> SaveFile()
        {
            Invoke(() => this.Save.IsEnabled = false);
            if (!(this.DataContext is CsProject project)) return true;
            using (var task = this.SaveFileTask(project))
            {
                await task;
                return !task.IsFaulted;
            }
        }

        private Task SaveFileTask(CsProject csProject)
        {
            return this.ProjectFile
                .SaveFile(csProject.ProjectPath, this.TextEditor.Document, this.OriginalFile.Encoding)
                .ContinueWith(itm =>
                {
                    var saved = itm.Result;
                    Invoke(() =>
                    {
                        if (saved != null)
                        {
                            var dlg  = new SharpDialog(MessageBoxText(saved), "Unable to save", SharpDialog.SharpDialogButton.Ok,
                                SharpDialog.SharpDialogType.Asterisk);
                            dlg.ShowDialog();
                            this.Save.IsEnabled = true;
                            throw new InvalidOperationException("Unable to save file");
                        }

                        this.RequestReload?.Invoke(this, EventArgs.Empty);
                    });
                });
        }

        private static string MessageBoxText(Exception saved)
        {
#if DEBUG
            var messageBoxText =
                $"\n{saved.Message}==\n{saved.StackTrace}\n{saved.InnerException?.Message}\n{saved.InnerException?.StackTrace}";
            return $"The file could not be saved.\n{messageBoxText}";
#endif
#if RELEASE
            return "The File could not be saved";
#endif
        }

        private void FileEditor_OnLoaded(object sender, RoutedEventArgs e)
        {
            var editor = new HorizontalMouseMove(this.TextEditor, this.Owner);
        }

        private void TextEditor_OnDragEnter(object sender, MouseEventArgs e)
        {
            AdornerManagerList.ClearAll();
        }

        private void TextEditor_OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            AdornerManagerList.ClearAll();
        }

        private void TextEditor_OnMouseLeave(object sender, MouseEventArgs e)
        {
            AdornerManagerList.ClearAll();
        }

        private void UIElement_OnMouseLeave(object sender, MouseEventArgs e)
        {
            AdornerManagerList.ClearAll();
        }
    }
}