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
using System.IO;
using System.Windows.Markup;

namespace DockingLibrary
{
    internal class OverlayWindowDockingButton : Window, IDropSurface
    {
        private OverlayWindow _owner;
        public readonly Button _btnDock;
        public OverlayWindowDockingButton(Button btnDock, OverlayWindow owner) : this(btnDock, owner, true)
        {

        }
        public OverlayWindowDockingButton(Button btnDock, OverlayWindow owner, bool enabled)
        {
            this._btnDock = btnDock;
            this._owner = owner;
            this.Enabled = enabled;
        } 
        
        public bool Enabled = true;



        #region IDropSurface Membri di

        public Rect SurfaceRectangle
        {
            get 
            {
                if (!this._owner.IsLoaded)
                    return new Rect();

                return new Rect(this._btnDock.PointToScreen(new Point(0,0)), new Size(this._btnDock.ActualWidth, this._btnDock.ActualHeight)); 
            }
        }

        public void OnDragEnter(Point point)
        {
            
        }

        public void OnDragOver(Point point)
        {
            
        }

        public void OnDragLeave(Point point)
        {
            
        }

        public bool OnDrop(Point point)
        {
            if (!this.Enabled)
                return false;

            return this._owner.OnDrop(this._btnDock, point);
        }

        #endregion
    }

    /// <summary>
    /// Interaction logic for OverlayWindow.xaml
    /// </summary>
    public partial class OverlayWindow : Window
    {
        private OverlayWindowDockingButton owdBottom;
        private OverlayWindowDockingButton owdTop;
        private OverlayWindowDockingButton owdLeft;
        private OverlayWindowDockingButton owdRight;
        private OverlayWindowDockingButton owdInto;

        private DockManager _owner;

        public DockManager DockManager
        {
            get { return this._owner; }
        }


        public OverlayWindow(DockManager owner)
        {
            //if (!Application.Current.Resources.Contains("DockDownButtonSyle"))
            //{ 
            //    using (FileStream fs = new FileStream(@"generic.xaml", FileMode.Open, FileAccess.Read))
            //    {
            //        ResourceDictionary resources = (ResourceDictionary)XamlReader.Load(fs);
            //        Application.Current.Resources.Add("DockDownButtonSyle", resources["DockDownButtonSyle"]);
            //    }
            //}

            this.InitializeComponent();

            this._owner = owner;

            this.DockManager.DragPaneServices.Register(new OverlayWindowDockingButton(this.btnDockBottom, this));
            this.DockManager.DragPaneServices.Register(new OverlayWindowDockingButton(this.btnDockTop, this));
            this.DockManager.DragPaneServices.Register(new OverlayWindowDockingButton(this.btnDockLeft, this));
            this.DockManager.DragPaneServices.Register(new OverlayWindowDockingButton(this.btnDockRight, this));

            this.owdBottom   = new OverlayWindowDockingButton(this.btnDockPaneBottom, this, false);
            this.owdTop      = new OverlayWindowDockingButton(this.btnDockPaneTop,    this, false);
            this.owdLeft     = new OverlayWindowDockingButton(this.btnDockPaneLeft,   this, false);
            this.owdRight    = new OverlayWindowDockingButton(this.btnDockPaneRight,  this, false);
            this.owdInto     = new OverlayWindowDockingButton(this.btnDockPaneInto, this, false);


            this.DockManager.DragPaneServices.Register(this.owdBottom);
            this.DockManager.DragPaneServices.Register(this.owdTop);
            this.DockManager.DragPaneServices.Register(this.owdLeft);
            this.DockManager.DragPaneServices.Register(this.owdRight);
            this.DockManager.DragPaneServices.Register(this.owdInto);

            //gridPaneRelativeDockingOptions.Width = 88;
            //gridPaneRelativeDockingOptions.Height = 88;
        }

        private void OnLoaded(object sender, EventArgs e)
        { 
            
        }

        private void OnClosed(object sender, EventArgs e)
        { 
            
        }

        internal bool OnDrop(Button btnDock, Point point)
        {
            if (btnDock == this.btnDockBottom)
                this.DockManager.DragPaneServices.FloatingWindow.HostedPane.ReferencedPane.ChangeDock(Dock.Bottom);
            else if (btnDock == this.btnDockLeft)
                this.DockManager.DragPaneServices.FloatingWindow.HostedPane.ReferencedPane.ChangeDock(Dock.Left);
            else if (btnDock == this.btnDockRight)
                this.DockManager.DragPaneServices.FloatingWindow.HostedPane.ReferencedPane.ChangeDock(Dock.Right);
            else if (btnDock == this.btnDockTop)
                this.DockManager.DragPaneServices.FloatingWindow.HostedPane.ReferencedPane.ChangeDock(Dock.Top);
            else if (btnDock == this.btnDockPaneTop)
                this.DockManager.DragPaneServices.FloatingWindow.HostedPane.ReferencedPane.MoveTo(this.CurrentDropPane, Dock.Top);
            else if (btnDock == this.btnDockPaneBottom)
                this.DockManager.DragPaneServices.FloatingWindow.HostedPane.ReferencedPane.MoveTo(this.CurrentDropPane, Dock.Bottom);
            else if (btnDock == this.btnDockPaneLeft)
                this.DockManager.DragPaneServices.FloatingWindow.HostedPane.ReferencedPane.MoveTo(this.CurrentDropPane, Dock.Left);
            else if (btnDock == this.btnDockPaneRight)
                this.DockManager.DragPaneServices.FloatingWindow.HostedPane.ReferencedPane.MoveTo(this.CurrentDropPane, Dock.Right);
            else if (btnDock == this.btnDockPaneInto)
            {
                this.DockManager.DragPaneServices.FloatingWindow.HostedPane.ReferencedPane.MoveInto(this.CurrentDropPane);
                return true;
            }

            this.DockManager.DragPaneServices.FloatingWindow.HostedPane.ReferencedPane.Show();

            return true;
        }

        private Pane CurrentDropPane;

        public void ShowOverlayPaneDockingOptions(Pane pane)
        {
            var rectPane = pane.SurfaceRectangle;

            var myScreenTopLeft = this.PointToScreen(new Point(0, 0));
            rectPane.Offset(-myScreenTopLeft.X, -myScreenTopLeft.Y);//relative to me
            this.gridPaneRelativeDockingOptions.SetValue(Canvas.LeftProperty, rectPane.Left+rectPane.Width/2- this.gridPaneRelativeDockingOptions.Width/2);
            this.gridPaneRelativeDockingOptions.SetValue(Canvas.TopProperty, rectPane.Top + rectPane.Height/2- this.gridPaneRelativeDockingOptions.Height/2);
            this.gridPaneRelativeDockingOptions.Visibility = Visibility.Visible;

            this.owdBottom.Enabled = true;
            this.owdTop   .Enabled = true;
            this.owdLeft  .Enabled = true;
            this.owdRight .Enabled = true;
            this.owdInto  .Enabled = true;
            this.CurrentDropPane = pane;
        }

        public void HideOverlayPaneDockingOptions(Pane surfaceElement)
        {
            this.owdBottom.Enabled = false;
            this.owdTop.Enabled = false;
            this.owdLeft.Enabled = false;
            this.owdRight.Enabled = false;
            this.owdInto.Enabled = false;

            this.gridPaneRelativeDockingOptions.Visibility = Visibility.Collapsed;
            this.CurrentDropPane = null;
        }
    
    }
}