using System;
using System.Drawing;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace TestAutomation.Tide.Ui.Controls
{
    /// <summary>
    /// Interaction logic for SharpDialog.xaml
    /// </summary>
    public partial class SharpDialog
    {
        protected string DialogTitle
        {
            get => (string) this.GetValue(DialogTitleProperty);
            set => this.SetValue(DialogTitleProperty, value);
        }

        protected string DialogMessage
        {
            get => (string) this.GetValue(DialogMessageProperty);
            set => this.SetValue(DialogMessageProperty, value);
        }

        protected int DialogWidth
        {
            get => (int) this.GetValue(DialogWidthProperty);
            set => this.SetValue(DialogWidthProperty, value);
        }

        protected int DialogHeight
        {
            get => (int) this.GetValue(DialogHeightProperty);
            set => this.SetValue(DialogHeightProperty, value);
        }

        public void SetCustomButtonLabel(int buttonId, string labelText)
        {
            switch (buttonId)
            {
                case 1:
                    this.ActionButton1.Content = labelText;
                    break;
                case 2:
                    this.ActionButton2.Content = labelText;
                    break;
                case 3:
                    this.ActionButton3.Content = labelText;
                    break;
                default:
#if DEBUG
                    Console.WriteLine("Incorrect ButtonID");
#endif
                    break;
            }
        }

        public static readonly DependencyProperty DialogTitleProperty =
            DependencyProperty.Register("DialogTitle", typeof(string), typeof(SharpDialog),
                new UIPropertyMetadata(null));

        public static readonly DependencyProperty DialogMessageProperty =
            DependencyProperty.Register("DialogMessage", typeof(string), typeof(SharpDialog),
                new UIPropertyMetadata(null));

        public static readonly DependencyProperty DialogWidthProperty =
            DependencyProperty.Register("DialogWidth", typeof(int), typeof(SharpDialog), new UIPropertyMetadata(null));

        public static readonly DependencyProperty DialogHeightProperty =
            DependencyProperty.Register("DialogHeight", typeof(int), typeof(SharpDialog), new UIPropertyMetadata(null));

        public SharpDialogResult Result = SharpDialogResult.None;

        public SharpDialog(string message, string title = "", SharpDialogButton buttons = SharpDialogButton.Ok,
            SharpDialogType type = SharpDialogType.None, int width = 0,
            int height = 0, Window parent = null)
        {
            if (parent != null)
            {
                this.Owner = parent;
                this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
            //this.Owner = parent;
            this.AllowsTransparency = true;
            this.DataContext = this;

            this.DialogTitle = title;
            this.DialogMessage = message;

            switch (width)
            {
                case 0 when height == 0:
                    this.SizeToContent = SizeToContent.WidthAndHeight;
                    break;
                case 0:
                    this.SizeToContent = SizeToContent.Width;
                    break;
                default:
                    if (height == 0)
                        this.SizeToContent = SizeToContent.Height;
                    else
                    {
                        this.DialogWidth = width;
                        this.DialogHeight = height;
                    }

                    break;
            }

            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            this.IsMaximizable = false;
            this.IsResizable = false;


            this.InitializeComponent();

            if (GenericResourceDict["GenericButton"] is Style buttonStyle)
            {
                this.ActionButton1.Style = buttonStyle;
                this.ActionButton2.Style = buttonStyle;
                this.ActionButton3.Style = buttonStyle;
            }

            switch (buttons)
            {
                case SharpDialogButton.Ok:
                    this.ActionButton1.Visibility = Visibility.Collapsed;
                    this.ActionButton2.Visibility = Visibility.Collapsed;
                    this.ActionButton3.Content = "OK";
                    break;
                case SharpDialogButton.OkCancel:
                    this.ActionButton1.Visibility = Visibility.Collapsed;
                    this.ActionButton2.Content = "OK";
                    this.ActionButton3.Content = "Cancel";
                    break;
                case SharpDialogButton.YesNo:
                    this.ActionButton1.Visibility = Visibility.Collapsed;
                    this.ActionButton2.Content = "Yes";
                    this.ActionButton3.Content = "No";
                    break;
                case SharpDialogButton.YesNoCancel:
                    this.ActionButton1.Content = "Yes";
                    this.ActionButton2.Content = "No";
                    this.ActionButton3.Content = "Cancel";
                    break;
                default:
                    this.ActionButton1.Visibility = Visibility.Collapsed;
                    this.ActionButton2.Visibility = Visibility.Collapsed;
                    this.ActionButton3.Visibility = Visibility.Collapsed;
                    break;
            }

            switch (type)
            {
                case SharpDialogType.Asterisk:
                    this.ErrorIcon.Source = Imaging.CreateBitmapSourceFromHIcon(SystemIcons.Asterisk.Handle,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                    SystemSounds.Asterisk.Play();
                    break;
                case SharpDialogType.Error:
                    this.ErrorIcon.Source = Imaging.CreateBitmapSourceFromHIcon(SystemIcons.Error.Handle,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                    SystemSounds.Asterisk.Play();
                    break;
                case SharpDialogType.Exclamation:
                    this.ErrorIcon.Source = Imaging.CreateBitmapSourceFromHIcon(SystemIcons.Exclamation.Handle,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                    SystemSounds.Exclamation.Play();
                    break;
                case SharpDialogType.Hand:
                    this.ErrorIcon.Source = Imaging.CreateBitmapSourceFromHIcon(SystemIcons.Hand.Handle,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                    SystemSounds.Hand.Play();
                    break;
                case SharpDialogType.Information:
                    this.ErrorIcon.Source = Imaging.CreateBitmapSourceFromHIcon(SystemIcons.Information.Handle,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                    SystemSounds.Asterisk.Play();
                    break;
                case SharpDialogType.None:
                    this.ErrorIcon.Visibility = Visibility.Collapsed;
                    SystemSounds.Beep.Play();
                    break;
                case SharpDialogType.Question:
                    this.ErrorIcon.Source = Imaging.CreateBitmapSourceFromHIcon(SystemIcons.Question.Handle,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                    SystemSounds.Question.Play();
                    break;
                case SharpDialogType.Stop:
                    this.ErrorIcon.Source = Imaging.CreateBitmapSourceFromHIcon(SystemIcons.Error.Handle,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                    SystemSounds.Asterisk.Play();
                    break;
                case SharpDialogType.Warning:
                    this.ErrorIcon.Source = Imaging.CreateBitmapSourceFromHIcon(SystemIcons.Warning.Handle,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                    SystemSounds.Asterisk.Play();
                    break;
                default:
                    throw new Exception(type.ToString());
            }
        }

        private void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
                switch (button.Content.ToString().ToLower())
                {
                    case "cancel":
                        this.Result = SharpDialogResult.Cancel;
                        break;
                    case "ok":
                        this.Result = SharpDialogResult.Ok;
                        break;
                    case "no":
                        this.Result = SharpDialogResult.No;
                        break;
                    case "yes":
                        this.Result = SharpDialogResult.Yes;
                        break;
                    default:
                        this.Result = SharpDialogResult.Cancel;
                        break;
                }

            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.Result == SharpDialogResult.None)
                this.Result = SharpDialogResult.Cancel;
        }
    }
}