using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DockingLibrary;
using TAF.AutomationTool.Ui.Activities;
using TAF.AutomationTool.Ui.ViewModels;
using static TAF.AutomationTool.Ui.CustomElements.WpfExtensions;

namespace TAF.AutomationTool.Ui
{
    public partial class ProcessViewer : DockableContent
    {
        private string processFileName;
        public ProjectEventManager EventBinding { get; }

        public ProcessViewer(ProjectEventManager eventBinding, ProcessViewerConfig config = null)
        {
            eventBinding.MainWinowRendered += this.MainWinowRendered;
            this.EventBinding = eventBinding;
            this.InitializeComponent();
            this.SetUpInterface(config);
        }

        private void SetUpInterface(ProcessViewerConfig config)
        {
            this.Title = config?.Title ?? "Console";
            this.processFileName = config?.ProcessFileName ?? "cmd.exe";
            this.processArguments = config?.ProcessArguments ?? string.Empty;
            this.Out.Text = string.Empty;
        }

        private void MainWinowRendered(object sender, EventArgs e)
        {
            this.ConfigureProcess();
            new HorizontalMouseMove(this.Console_View, this.EventBinding.ProjectWindow);
            this.process.OutputDataReceived += this.Console_OnOutputDataReceived;
            this.process.ErrorDataReceived += this.Console_OnOutputDataReceived;
            this.EventBinding.ReadyToClose += this.ReadyToClose;
            this.process.Start();
            this.process.BeginErrorReadLine();
            this.process.BeginOutputReadLine();
            this.writer = this.process.StandardInput;
            this.process.Exited += this.Console_OnExited;
        }

        private void ReadyToClose(object sender, EventArgs e)
        {
            if (!(sender is bool shouldClose)) return;

            Invoke(this.Close);
        }

        private void ConfigureProcess()
        {
            this.process = new Process
            {
                StartInfo =
                {
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    FileName = this.processFileName,
                    Arguments = this.processArguments
                },
                EnableRaisingEvents = true
            };
        }


        private void UIElement_OnPreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            this.HandleUserInput();
        }

        private Process process;
        private StreamWriter writer;
        private bool shouldWrite = true;
        private string processArguments;

        private void HandleUserInput()
        {
            this.writer = this.process.StandardInput;
            this.DetermineInput();
        }

        private void DetermineInput()
        {
            this.writer.Write(this.In.Text);
            this.writer.Write("\n");
            switch (this.In.Text.ToLower())
            {
                case "clear":
                case "cls":
                    Invoke(() =>
                    {
                        this.shouldWrite = false;

                        this.Out.Text = string.Empty;
                    });
                    break;
                default:
                    break;
            }

            Invoke(() => this.In.Text = string.Empty);
        }

        private void Console_OnExited(object sender, EventArgs e)
        {
            Invoke(() =>
            {
                this.Out.Text += "\r\n\r\n[Process Complete]";
                this.In_Grid.Visibility = Visibility.Collapsed;
                Grid.SetRowSpan(this.Console_View, 2);
            });
        }

        private void Console_OnOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            this.WriteConsoleData(e);
        }

        private void WriteConsoleData(DataReceivedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(e.Data) && this.shouldWrite)
            {
                var eData = e.Data.Replace("", "").Replace("\n", "\r\n");
                var pattern = new Regex(@"\[[A-Za-z0-9]{0,3}[;]{0,1}[^]{0,1}[A-Za-z0-9]{0,3}[-]{0,1}[A-Za-z0-9]{0,3}");
                eData = pattern.Replace(eData, "");
                Invoke(() =>
                {
                    this.Out.Text += $"\r\n{eData}";
                    this.Console_View.ScrollToBottom();
                });
            }
            else if (this.shouldWrite)
            {
                this.writer.Write("\n");
            }

            this.shouldWrite = true;
        }

        private void ConsoleWin_OnLoaded(object sender, RoutedEventArgs e)
        {
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            this.process.Dispose();
        }

        private void Out_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!this.Console_View.IsMouseOver) return;
            this.In.Focus();
            e.Handled = true;
        }
    }
}