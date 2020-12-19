using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace TestAutomation.Tide.Ui.Controls
{
    public class SharpWindow : Window, IDisposable
    {
        [DllImport("user32.dll")]
        internal static extern int SetWindowCompositionAttribute(IntPtr hwnd,
            ref UiInterop.WindowCompositionAttributeData data);

        // used to store window content
        private HwndSource _hwndSource;

        protected static ResourceDictionary GenericResourceDict;


        public bool IsMaximizable
        {
            get => (bool) this.GetValue(IsMaximizableProperty);
            set => this.SetValue(IsMaximizableProperty, value);
        }
        public object ToolbarTemplate
        {
            get => (object) this.GetValue(IsMaximizableProperty);
            set => this.SetValue(IsMaximizableProperty, value);
        }

        public static readonly DependencyProperty ToolbarTemplateProperty = DependencyProperty.Register
            ("ToolbarTemplate", typeof(object), typeof(SharpWindow), new PropertyMetadata(null));

        public static readonly DependencyProperty IsMaximizableProperty = DependencyProperty.Register
            ("IsMaximizable", typeof(bool), typeof(SharpWindow), new PropertyMetadata(true));

        public bool IsMinimizable
        {
            get => (bool) this.GetValue(IsMinimizableProperty);
            set => this.SetValue(IsMinimizableProperty, value);
        }

        public static readonly DependencyProperty IsMinimizableProperty = DependencyProperty.Register
            ("IsMinimizable", typeof(bool), typeof(SharpWindow), new PropertyMetadata(true));

        public bool IsClosable
        {
            get => (bool) this.GetValue(IsClosableProperty);
            set => this.SetValue(IsClosableProperty, value);
        }

        public static readonly DependencyProperty IsClosableProperty = DependencyProperty.Register
            ("IsClosable", typeof(bool), typeof(SharpWindow), new PropertyMetadata(true));

        public bool IsResizable
        {
            get => (bool) this.GetValue(IsResizableProperty);
            set => this.SetValue(IsResizableProperty, value);
        }

        public static readonly DependencyProperty IsResizableProperty = DependencyProperty.Register
            ("IsResizable", typeof(bool), typeof(SharpWindow), new PropertyMetadata(true));


        //
        //        private static void ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        //        {
        //            d.SetValue(e.Property, e.NewValue);
        //        }

        public bool HasBlur { get; }

        internal void EnableBlur()

        {
            var windowHelper = new WindowInteropHelper(this);
            var accent = new UiInterop.AccentPolicy();
            var accentStructSize = Marshal.SizeOf(accent);
            accent.AccentState = UiInterop.AccentState.ACCENT_ENABLE_BLURBEHIND;

            var accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accent, accentPtr, false);

            var data = CreateWindowCompositionAttributeData(accentStructSize, accentPtr);

            SetWindowCompositionAttribute(windowHelper.Handle, ref data);
            Marshal.FreeHGlobal(accentPtr);
        }

        private static UiInterop.WindowCompositionAttributeData CreateWindowCompositionAttributeData(
            int accentStructSize,
            IntPtr accentPtr)
        {
            var data = new UiInterop.WindowCompositionAttributeData
            {
                Attribute = UiInterop.WindowCompositionAttribute.WCA_ACCENT_POLICY,
                SizeOfData = accentStructSize,
                Data = accentPtr
            };
            return data;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.EnableBlur();
        }

        public SharpWindow(bool enableBlur = false)
        {
            if (enableBlur)
            {
                this.Loaded += this.OnLoaded;
                this.HasBlur = true;
            }

            this.InitializeWindow();
        }

        public SharpWindow()
        {
            this.InitializeWindow();
        }

        private void InitializeWindow()
        {
            this.PreviewMouseMove += this.SharpWindow_PreviewMouseMove;
            this.MaxHeight = SystemParameters.WorkArea.Height;
            this.MaxWidth = SystemParameters.WorkArea.Width;
            const string themeLocation = "TestAutomation.Tide.Ui.Controls";
            GenericResourceDict = new ResourceDictionary()
            {
                Source = new Uri($"/{themeLocation};component/Generic.xaml", UriKind.RelativeOrAbsolute)
            };
            this.Style = GenericResourceDict["ThisWindow"] as Style;
            this.AllowsTransparency = true;
        }

        // Import external dll user32
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool ReleaseCapture();

        private void ResizeWindow(ResizeDirection direction)
        {
            SendMessage(this._hwndSource.Handle, 0x112, (IntPtr) direction, IntPtr.Zero);
        }

        private enum ResizeDirection
        {
            Left = 61441,
            Right = 61442,
            Top = 61443,
            TopLeft = 61444,
            TopRight = 61445,
            Bottom = 61446,
            BottomLeft = 61447,
            BottomRight = 61448,
        }

        private void ResizeRectangle_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized || !this.IsResizable) return;
            var rectangle = sender as Rectangle;

            // ReSharper disable once SwitchStatementMissingSomeCases
            if (rectangle.Name != null)
            {
                switch (rectangle.Name)
                {
                    case "bottom":
                    case "top":
                        if (Math.Abs(this.Height - this.MinHeight) < 0.5) return;
                        this.Cursor = Cursors.SizeNS;
                        break;
                    case "left":
                    case "right":
                        if (Math.Abs(this.Width - this.MinWidth) < 0.5) return;
                        this.Cursor = Cursors.SizeWE;
                        break;
                    case "topLeft":
                    case "bottomRight":
                        this.Cursor = Cursors.SizeNWSE;
                        break;
                    case "topRight":
                    case "bottomLeft":
                        this.Cursor = Cursors.SizeNESW;
                        break;
                }
            }
        }

        private void ResizeRectangle_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized || !this.IsResizable) return;
            var rectangle = sender as Rectangle;

            if (rectangle?.Name == null) return;
            switch (rectangle.Name)
            {
                case "bottom":
                case "top":
                    this.Cursor = Cursors.SizeNS;
                    this.ResizeWindow((rectangle.Name == "top") ? ResizeDirection.Top : ResizeDirection.Bottom);
                    break;
                case "left":
                case "right":
                    this.Cursor = Cursors.SizeWE;
                    this.ResizeWindow((rectangle.Name == "left") ? ResizeDirection.Left : ResizeDirection.Right);
                    break;
                case "topLeft":
                case "bottomRight":
                    this.Cursor = Cursors.SizeNWSE;
                    this.ResizeWindow((rectangle.Name == "topLeft")
                        ? ResizeDirection.TopLeft
                        : ResizeDirection.BottomRight);
                    break;
                case "topRight":
                case "bottomLeft":
                    this.Cursor = Cursors.SizeNESW;
                    this.ResizeWindow((rectangle.Name == "topRight")
                        ? ResizeDirection.TopRight
                        : ResizeDirection.BottomLeft);
                    break;
            }
        }

        private void OnSourceInitialized(object sender, EventArgs e)
        {
            this._hwndSource = (HwndSource) PresentationSource.FromVisual(this);
        }

        private void SharpWindow_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (Mouse.LeftButton != MouseButtonState.Pressed) this.Cursor = Cursors.Arrow;
        }

        static SharpWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SharpWindow),
                new FrameworkPropertyMetadata(typeof(SharpWindow)));
        }

        protected void Minimize_OnClick(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        protected void Restore_OnClick(object sender, RoutedEventArgs e)
        {
            this.WindowState = (this.WindowState == WindowState.Normal)
                ? WindowState.Maximized
                : WindowState.Normal;
        }

        protected void Close_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Move_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.LeftButton != MouseButtonState.Pressed || this.WindowState == WindowState.Maximized) return;
            ReleaseCapture();
            SendMessage(this._hwndSource.Handle, 0xA1, (IntPtr) 0x2, (IntPtr) 0);
        }

        public override void OnApplyTemplate()
        {
            this.SetUpMinimizeButton();
            this.SetupRestoreButton();
            this.SetupCloseButton();
            this.SetupResizeGrid();

            if (this.GetTemplateChild("moveGrid") is Grid moveGrid)
                moveGrid.PreviewMouseDown += this.Move_PreviewMouseDown;
            base.OnApplyTemplate();
        }

        private void SetupResizeGrid()
        {
            // if the resize grid exists do the following procedure fore each element in the grid
            if (!(this.GetTemplateChild("resizeGrid") is Grid resizeGrid)) return;
            foreach (var resizeRectangle in resizeGrid.Children.OfType<Rectangle>())
            {
                resizeRectangle.PreviewMouseDown += this.ResizeRectangle_PreviewMouseDown;
                resizeRectangle.MouseMove += this.ResizeRectangle_MouseMove;
            }
        }

        private void SetupCloseButton()
        {
            if (!(this.GetTemplateChild("closeButton") is Button closeButton)) return;
            if (!this.IsClosable)
                closeButton.Visibility = Visibility.Collapsed;
            else
                closeButton.Click += this.Close_OnClick;
        }

        private void SetupRestoreButton()
        {
            if (!(this.GetTemplateChild("restoreButton") is Button restoreButton)) return;
            if (!this.IsMaximizable)
                restoreButton.Visibility = Visibility.Collapsed;
            else
                restoreButton.Click += this.Restore_OnClick;
        }

        private void SetUpMinimizeButton()
        {
            if (!(this.GetTemplateChild("minimizeButton") is Button minimizeButton)) return;
            if (!this.IsMinimizable)
                minimizeButton.Visibility = Visibility.Collapsed;
            else
                minimizeButton.Click += this.Minimize_OnClick;
        }

        protected void ReapplyStyle()
        {
            this.Style = GenericResourceDict["ThisWindow"] as Style;
        }

        protected void ReapplyStyle(Style style)
        {
            this.Style = style;
        }

        protected override void OnInitialized(EventArgs e)
        {
            this.SourceInitialized += this.OnSourceInitialized;
            base.OnInitialized(e);
        }

        protected virtual void Dispose(bool disposing)
        {
            GC.ReRegisterForFinalize(this);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}