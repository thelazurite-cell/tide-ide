using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Interop;
using System.Windows.Controls.Primitives;
using TestAutomation.Tide.Ui.Controls;

namespace DockingLibrary
{
    /// <summary>
    /// Interaction logic for FloatingWindow.xaml
    /// </summary>

    public partial class FloatingWindow
    {
        private const int WmMove = 0x0003;
        private const int WmSize = 0x0005;
        private const int WmNcmousemove = 0xa0;
        private const int WmNclbuttondown = 0xA1;
        private const int WmNclbuttonup = 0xA2;
        private const int WmNclbuttondblclk = 0xA3;
        private const int WmNcrbuttondown = 0xA4;
        private const int WmNcrbuttonup = 0xA5;
        private const int Htcaption = 2;
        private const int ScMove = 0xF010;
        private const int WmSyscommand = 0x0112;

        internal readonly FloatingWindowHostedPane HostedPane;
        //public readonly DockablePane ReferencedPane;

        public FloatingWindow(DockablePane pane)
        {
            this.InitializeComponent();

            #region Hosted Pane

            this.HostedPane = new FloatingWindowHostedPane(this, pane);
            this.HostedPane.ReferencedPane.OnTitleChanged += this.HostedPane_OnTitleChanged;
            this.Content = this.HostedPane;
            this.Title = this.HostedPane.Title;
            #endregion
        }

        private void HostedPane_OnTitleChanged(object sender, EventArgs e)
        {
            this.Title = this.HostedPane.Title;
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            this.HostedPane.ReferencedPane.SaveFloatingWindowSizeAndPosition(this);
            this.HostedPane.ReferencedPane.OnTitleChanged -= this.HostedPane_OnTitleChanged;
            this.HostedPane.Close();
            
            if (this.hwndSource != null) this.hwndSource.RemoveHook(this.wndProcHandler);

            base.OnClosing(e);
        }

        private HwndSource hwndSource;
        private HwndSourceHook wndProcHandler;

        protected void OnLoaded(object sender, EventArgs e)
        {
            var helper = new WindowInteropHelper(this);
            this.hwndSource = HwndSource.FromHwnd(helper.Handle);
            this.wndProcHandler = this.HookHandler;
            this.hwndSource.AddHook(this.wndProcHandler);
        }
        

        private IntPtr HookHandler(
            IntPtr hwnd,
            int msg,
            IntPtr wParam,
            IntPtr lParam,
            ref bool handled
        )
        {
            handled = false;

            switch (msg)
            {
                case WmSize:
                case WmMove:
                    this.HostedPane.ReferencedPane.SaveFloatingWindowSizeAndPosition(this);
                    break;
                case WmNclbuttondown:
                    if (this.HostedPane.ReferencedPane.State == PaneState.DockableWindow && wParam.ToInt32() == Htcaption)
                    {
                        var ui = UiInterop.GetCursorPosition();

                        this.HostedPane.ReferencedPane.DockManager.Drag(this, ui, new Point(ui.X-this.Left, ui.Y-this.Top));

//                        handled = true;
                    }
                    break;
                case WmNclbuttondblclk:
                    if (this.HostedPane.ReferencedPane.State == PaneState.DockableWindow && wParam.ToInt32() == Htcaption)
                    {
                        //
                        this.HostedPane.ReferencedPane.ChangeState(PaneState.Docked);
                        this.HostedPane.ReferencedPane.Show();
                        this.Close();

                        handled = true;
                    }
                    break;
                case WmNcrbuttondown:
                    if (wParam.ToInt32() == Htcaption)
                    {
                        var x = (short)(lParam.ToInt32() & 0xFFFF);
                        var y = (short)(lParam.ToInt32() >> 16);

                        var cxMenu = this.HostedPane.OptionsMenu;
                        cxMenu.Placement = PlacementMode.AbsolutePoint;
                        cxMenu.PlacementRectangle = new Rect(new Point(x, y), new Size(0, 0));
                        cxMenu.PlacementTarget = this;
                        cxMenu.IsOpen = true;

                        handled = true;
                    }
                    break;
                case WmNcrbuttonup:
                    if (wParam.ToInt32() == Htcaption)
                    {

                        handled = true;
                    }
                    break;
                
            }
            

            return IntPtr.Zero;
        }
    }
}