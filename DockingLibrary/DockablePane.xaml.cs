using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using TAF.AutomationTool.Ui.CustomElements;

namespace DockingLibrary
{
    /// <summary>
    /// States that a dockable pane can assume
    /// </summary>
    public enum PaneState
    { 
        Hidden,

        AutoHide,

        Docked,

        TabbedDocument,

        FloatingWindow,

        DockableWindow
    }


    /// <summary>
    /// A dockable pane is a resizable and movable window region which can host one or more dockable content
    /// </summary>
    /// <remarks>A dockable pane occupies a window region. It can be in two different states: docked to a border or hosted in a floating window.
    /// When is docked it can be resizes only in a direction. User can switch between pane states using mouse or context menus.
    /// Contents whitin a dockable pane are arranged through a tabcontrol.</remarks>
    partial class DockablePane : Pane
    {
 
        /// <summary>
        /// When created pane is hidden
        /// </summary>
        protected PaneState PaneState = PaneState.Hidden;

        /// <summary>
        /// Get pane state
        /// </summary>
        public PaneState State
        {
            get
            {
                return this.PaneState;
            }
        }

        /// <summary>
        /// Show/hide pane header (title, buttons etc...)
        /// </summary>
        public bool ShowHeader
        {
            get 
            {
                return this.PaneHeader.Visibility == Visibility.Visible;
            }
            set 
            {
                if (value)
                    this.PaneHeader.Visibility = Visibility.Visible;
                else
                    this.PaneHeader.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Current docking border
        /// </summary>
        private Dock _dock = Dock.Right;

        /// <summary>
        /// Current docking border
        /// </summary>
        public Dock Dock
        {
            get
            {
                return this._dock;
            }
        }


        public DockablePane(DockManager dockManager) : this(dockManager, Dock.Right) { }
        public DockablePane(DockManager dockManager, Dock initialDock) : base(dockManager)
        {
            this._dock = initialDock;
            this.InitializeComponent();

            //this.GotFocus += new RoutedEventHandler(item_GotFocus);

        }

        /// <summary>
        /// Active visible content
        /// </summary>
        public override DockableContent ActiveContent
        {
            get 
            {
                if (this.VisibleContents.Count == 1)
                    return this.VisibleContents[0];
                else if (this.VisibleContents.Count > 1)
                    return this.VisibleContents[this.tbcContents.SelectedIndex];

                return null;
            }
            set
            {
                if (this.VisibleContents.Count > 1)
                {
                    this.tbcContents.SelectedIndex = this.VisibleContents.IndexOf(value);
                }
            }
        }
        /// <summary>
        /// Event raised when title is changed
        /// </summary>
        public event EventHandler OnTitleChanged;

        /// <summary>
        /// Change pane's title and fires OnTitleChanged event
        /// </summary>
        public override void RefreshTitle()
        {
            if (this.ActiveContent != null)
            {
                this.tbTitle.Text = this.Title;

                if (this.tbcContents.Items.Count > 0)
                {
                    this.SetTabItemHeader(this.tbcContents.Items[this.VisibleContents.IndexOf(this.ActiveContent)] as TabItem, this.ActiveContent);
                }
                if (this.OnTitleChanged != null) this.OnTitleChanged(this, new EventArgs());
            }
        }


        /// <summary>
        /// Get pane title
        /// </summary>
        public virtual string Title
        {
            get 
            {
                if (this.ActiveContent != null)
                    return this.ActiveContent.Title;

                return null;
            }
        }
        /// <summary>
        /// Get visible contents
        /// </summary>
        public readonly List<DockableContent> VisibleContents = new List<DockableContent>();

        /// <summary>
        /// Add a dockable content to Contents list
        /// </summary>
        /// <param name="content">Content to add</param>
        /// <remarks>Content is automatically shown.</remarks>
        public override void Add(DockableContent content)
        {
            if (this.Contents.Count == 0)
            {
                this.SaveFloatingWindowSizeAndPosition(content);
            }


            base.Add(content);
        }

        /// <summary>
        /// Remove a content from pane Contents list
        /// </summary>
        /// <param name="content">Content to remove</param>
        /// <remarks>Notice that when no more contents are present in a pane, it is automatically removed</remarks>
        public override void Remove(DockableContent content)
        {
            this.Hide(content);

            base.Remove(content);

            if (this.Contents.Count == 0) this.DockManager.Remove(this);
        }

        /// <summary>
        /// Show a content previuosly added
        /// </summary>
        /// <param name="content">DockableContent object to show</param>
        public override void Show(DockableContent content)
        {
            this.AddVisibleContent(content);

            if (this.VisibleContents.Count == 1 && this.State == PaneState.Hidden)
            {
                this.ChangeState(PaneState.Docked);
                this.DockManager.DragPaneServices.Register(this);
            }


            base.Show(content);
        }

        /// <summary>
        /// Hide a contained dockable content
        /// </summary>
        /// <param name="content">DockableContent object to hide</param>
        /// <remarks>Pane is automatically hidden if no more visible contents are shown</remarks>
        public override void Hide(DockableContent content)
        {
            this.RemoveVisibleContent(content);

            if (this.VisibleContents.Count == 0 && this.State==PaneState.Docked) this.Hide();

            base.Hide(content);
        }

        /// <summary>
        /// Add a visible content
        /// </summary>
        /// <param name="content">DockableContent object to add</param>
        /// <remarks>If more then one contents are visible, this method dinamically creates a tab control and
        /// adds new content to it.</remarks>
        private void AddVisibleContent(DockableContent content)
        {
            if (this.VisibleContents.Contains(content))
                return;

            if (this.VisibleContents.Count == 0)
            {
                this.VisibleContents.Add(content);
                this.ShowSingleContent(content);
            }
            else if (this.VisibleContents.Count == 1)
            {
                this.HideSingleContent(this.VisibleContents[0]);
                this.AddItem(this.VisibleContents[0]);
                this.VisibleContents.Add(content);
                this.AddItem(content);
                this.ShowTabs();
            }
            else
            {
                this.VisibleContents.Add(content);
                this.AddItem(content);
            }
        }

        /// <summary>
        /// Remove a visible content from pane
        /// </summary>
        /// <param name="content">DockableContent object to remove</param>
        /// <remarks>Remove related tab item from contents tab control. if only one content is visible than hide tab control.</remarks>
        private void RemoveVisibleContent(DockableContent content)
        {
            if (!this.VisibleContents.Contains(content))
                return;

            if (this.VisibleContents.Count == 1)
            {
                this.VisibleContents.Remove(content);
                this.HideSingleContent(content);
                this.HideTabs();
            }
            else if (this.VisibleContents.Count == 2)
            {
                this.RemoveItem(this.VisibleContents[0]);
                this.RemoveItem(this.VisibleContents[1]);
                this.VisibleContents.Remove(content);
                this.ShowSingleContent(this.VisibleContents[0]);
                this.HideTabs();
            }
            else
            {
                this.VisibleContents.Remove(content);
                this.RemoveItem(content);
            }
        }

        /// <summary>
        /// Close a dockable content
        /// </summary>
        /// <param name="content">DockableContent object to close</param>
        /// <remarks>In this library version this method simply hide the content</remarks>
        public override void Close(DockableContent content)
        {
            this.Hide(content);
        }

        #region Contents management
        private bool IsSingleContentVisible
        {
            get { return this.cpClientWindowContent.Visibility == Visibility.Visible; }
        }

        private void ShowSingleContent(DockableContent content)
        {
            this.cpClientWindowContent.Content = content.Content;
            this.cpClientWindowContent.Visibility = Visibility.Visible;
            this.RefreshTitle();
        }

        private void HideSingleContent(DockableContent content)
        {
            this.cpClientWindowContent.Content = null;
            this.cpClientWindowContent.Visibility = Visibility.Collapsed;
        }


        private bool IsContentsTbcVisible
        {
            get { return this.tbcContents.Visibility == Visibility.Visible; }
        }

        protected void ShowTabs()
        {
            this.tbcContents.SelectionChanged += this._tabs_SelectionChanged;
            this.tbcContents.Visibility = Visibility.Visible;
        }

        protected void HideTabs()
        {
            this.tbcContents.SelectionChanged -= this._tabs_SelectionChanged;
            this.tbcContents.Visibility = Visibility.Collapsed;
        }

        private void SetTabItemHeader(TabItem item, DockableContent content)
        {
            try
            {
                var spHeader = new StackPanel {Orientation = Orientation.Horizontal};
                var iconContent = new Image {Source = content.Icon};
                spHeader.Children.Add(iconContent);
                var titleContent = new TextBlock
                {
                    Text = content.Title,
                    Margin = new Thickness(2, 0, 0, 0)
                };
                spHeader.Children.Add(titleContent);
                item.Header = spHeader;
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine(e.Message);
                new TaskFactory().StartNew(() =>
                {
                    Thread.Sleep(10);
                    WpfExtensions.Invoke(() => this.SetTabItemHeader(item, content));
                });
            }
        }

        protected virtual void AddItem(DockableContent content)
        {
            var item = new TabItem();
            var tabPanel = new DockPanel();

            //SetTabItemHeader(item, content);

            item.Style = this.FindResource("DockablePaneTabItemStyle") as Style;
            item.Content = new ContentPresenter();
            (item.Content as ContentPresenter).Content = content.Content;

            //item.PreviewMouseDown += new MouseButtonEventHandler(OnTabItemMouseDown);
            //item.MouseMove += new MouseEventHandler(OnTabItemMouseMove);
            //item.MouseUp += new MouseButtonEventHandler(OnTabItemMouseUp);
            //item.Focusable = true;
            //item.GotFocus += new RoutedEventHandler(item_GotFocus);
            this.tbcContents.Items.Add(item);
            this.tbcContents.SelectedItem = item;

            this.RefreshTitle();
        }

        private void item_GotFocus(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("GotFocus");
        }

        protected virtual void RemoveItem(DockableContent content)
        {
            foreach (TabItem item in this.tbcContents.Items)
                if ((item.Content as ContentPresenter).Content == content.Content)
                {
                    //item.PreviewMouseDown -= new MouseButtonEventHandler(OnTabItemMouseDown);
                    //item.MouseMove -= new MouseEventHandler(OnTabItemMouseMove);
                    //item.MouseUp -= new MouseButtonEventHandler(OnTabItemMouseUp);

                    item.Content = null;
                    this.tbcContents.Items.Remove(item);
                    //ChangeTitle();
                    break;
                }
        }


        private void _tabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.tbcContents.SelectedIndex >= 0) this.RefreshTitle();
        }

        #endregion

        //List<EventHandler> _list = new List<EventHandler>();
        
        ///// <summary>
        ///// Event fired when pane internal state is changed
        ///// </summary>
        //public event EventHandler OnStateChanged
        //{
        //    add { _list.Add(value); }
        //    remove { _list.Remove(value); }
        //}

        public EventHandler OnStateChanged;

        /// <summary>
        /// Fires OnStateChanged event
        /// </summary>
        private void FireOnOnStateChanged()
        {
            if (this.OnStateChanged != null) this.OnStateChanged(this, EventArgs.Empty);
            
        }
        
        /// <summary>
        /// Change pane internal state
        /// </summary>
        /// <param name="newState">New pane state</param>
        /// <remarks>OnStateChanged event is raised only if newState is different from PaneState.</remarks>
        internal void ChangeState(PaneState newState)
        {
            if (this.State != newState)
            {
                this.SaveSize();

                this._lastState = this.PaneState;
                this.PaneState = newState;

                this.FireOnOnStateChanged();
            }
        }


        /// <summary>
        /// Return true if pane is hidden, ie PaneState is different from PaneState.Docked
        /// </summary>
        public override bool IsHidden
        {
            get
            {
                return this.State != PaneState.Docked;
            }
        }

        /// <summary>
        /// Internal last pane state
        /// </summary>
        private PaneState _lastState = PaneState.Docked;

        /// <summary>
        /// Show this pane and all contained contents
        /// </summary>
        public override void Show()
        {
            foreach (var content in this.Contents) this.Show(content);

            if (this.State == PaneState.AutoHide || this.State == PaneState.Hidden) this.ChangeState(PaneState.Docked);

            base.Show();
        }

        /// <summary>
        /// Hide this pane and all contained contents
        /// </summary>
        public override void Hide()
        {
            foreach (var content in this.Contents) this.RemoveVisibleContent(content);

            this.ChangeState(PaneState.Hidden);
            base.Hide();
        }

        /// <summary>
        /// Close this pane
        /// </summary>
        /// <remarks>Consider that in this version library this method simply hides the pane.</remarks>
        public override  void Close()
        {
            this.Hide();

            base.Close();
        }

        /// <summary>
        /// Create and show a floating window hosting this pane
        /// </summary>
        public virtual void FloatingWindow()
        {
            this.ChangeState(PaneState.FloatingWindow); 
            

            var wnd = new FloatingWindow(this);
            this.SetFloatingWindowSizeAndPosition(wnd);

            wnd.Owner = this.DockManager.ParentWindow;
            wnd.Show();
        }

        /// <summary>
        /// Create and show a dockable window hosting this pane
        /// </summary>
        public virtual void DockableWindow()
        {
            var wnd = new FloatingWindow(this);
            this.SetFloatingWindowSizeAndPosition(wnd);

            this.ChangeState(PaneState.DockableWindow);

            wnd.Owner = this.DockManager.ParentWindow;
            wnd.Show();
        }

        /// <summary>
        /// Show contained contents as documents and close this pane
        /// </summary>
        public virtual void TabbedDocument()
        {
            while (this.Contents.Count > 0)
            {
                var contentToRemove = this.Contents[0];
                this.Remove(contentToRemove);
                this.DockManager.AddDocument(contentToRemove);
            }

            this.ChangeState(PaneState.TabbedDocument);
        }

        /// <summary>
        /// Dock this pane to a destination pane border
        /// </summary>
        /// <param name="destinationPane"></param>
        /// <param name="relativeDock"></param>
        internal void MoveTo(Pane destinationPane, Dock relativeDock)
        {
            var dockableDestPane = destinationPane as DockablePane;
            if (dockableDestPane != null)
                this.ChangeDock(dockableDestPane.Dock);
            else
                this.ChangeDock(relativeDock);


            this.DockManager.MoveTo(this, destinationPane, relativeDock);
            this.ChangeState(PaneState.Docked);
            //Show();
            //ChangeState(PaneState.Docked);
        }

        /// <summary>
        /// Move contained contents into a destination pane and close this one
        /// </summary>
        /// <param name="destinationPane"></param>
        internal void MoveInto(Pane destinationPane)
        {
            DockablePane dockableDestPane = destinationPane as DockablePane;
            if (dockableDestPane != null)
                ChangeDock(dockableDestPane.Dock);

            this.DockManager.MoveInto(this, destinationPane);

            if (destinationPane is DocumentsPane)
                ChangeState(PaneState.TabbedDocument);
            else
                ChangeState(PaneState.Docked);
        }

        /// <summary>
        /// Event raised when Dock property is changed
        /// </summary>
        public event EventHandler OnDockChanged;

        /// <summary>
        /// Fires OnDockChanged
        /// </summary>
        private void FireOnOnDockChanged()
        {
            if (this.OnDockChanged != null) this.OnDockChanged(this, EventArgs.Empty);
        }
        
        /// <summary>
        /// Change dock border
        /// </summary>
        /// <param name="dock">New dock border</param>
        public void ChangeDock(Dock dock)
        {
            //if (dock != _dock)
            {
                //SaveSize();

                this._dock = dock;

                this.FireOnOnDockChanged();

                this.ChangeState(PaneState.Docked);
                //Show();
            }
            
        }

        /// <summary>
        /// Auto-hide this pane 
        /// </summary>
        public void AutoHide()
        {
            foreach (var content in this.Contents) this.RemoveVisibleContent(content);

            this.ChangeState(PaneState.AutoHide);

            this.DockManager.DragPaneServices.Unregister(this);
        }

        /// <summary>
        /// Handles effective pane resizing 
        /// </summary>
        /// <param name="sizeInfo"></param>
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            //SaveSize();
            base.OnRenderSizeChanged(sizeInfo);
        }


        /// <summary>
        /// Save current pane size
        /// </summary>
        public override void SaveSize()
        {
            if (this.IsHidden)
                return;

            if (this.Dock == Dock.Left || this.Dock == Dock.Right)
                this.PaneWidth = this.ActualWidth > 150 ? this.ActualWidth : 150;
            else
                this.PaneHeight = this.ActualHeight > 150 ? this.ActualHeight : 150;

            base.SaveSize();
        }


        private Point ptFloatingWindow = new Point(0,0);
        private Size sizeFloatingWindow = new Size(300, 300);

        internal void SaveFloatingWindowSizeAndPosition(Window fw)
        {
            if (!double.IsNaN(fw.Left) && !double.IsNaN(fw.Top)) this.ptFloatingWindow = new Point(fw.Left, fw.Top);
            
            if (!double.IsNaN(fw.Width) && !double.IsNaN(fw.Height)) this.sizeFloatingWindow = new Size(fw.Width, fw.Height);
        }

        internal void SetFloatingWindowSizeAndPosition(FloatingWindow fw)
        {
            fw.Left = this.ptFloatingWindow.X;
            fw.Top = this.ptFloatingWindow.Y;
            fw.Width = this.sizeFloatingWindow.Width;
            fw.Height = this.sizeFloatingWindow.Height;
        }

        /// <summary>
        /// Get swith options context menu
        /// </summary>
        internal ContextMenu OptionsMenu
        {
            get 
            {
                return this.btnMenu.ContextMenu;
            }
        }

        /// <summary>
        /// Handles user click on OptionsMenu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks></remarks>
        protected virtual void OnDockingMenu(object sender, EventArgs e)
        {
            if (sender == this.menuTabbedDocument) this.TabbedDocument();
            if (sender == this.menuFloatingWindow) this.FloatingWindow();
            if (sender == this.menuDockedWindow) this.DockableWindow();


            if (sender == this.menuAutoHide)
            {
                if (this.menuAutoHide.IsChecked)
                    this.ChangeState(PaneState.Docked);
                else
                    this.AutoHide();
            }
            if (sender == this.menuClose)
            {
                this.Close(this.ActiveContent);
            }
        }
        /// <summary>
        /// Show switch options menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBtnMenuMouseDown(object sender, RoutedEventArgs e)
        {
            this.btnMenu.ContextMenu.IsOpen = true;
            e.Handled = true;
        }

        /// <summary>
        /// Handles user click event on auto-hide button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBtnAutoHideMouseDown(object sender, RoutedEventArgs e)
        {
            if (this.State == PaneState.AutoHide)
                this.ChangeState(PaneState.Docked);
            else
                this.AutoHide();

            e.Handled = true;
        }

        /// <summary>
        /// Handles user click event on close button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBtnCloseMouseDown(object sender, RoutedEventArgs e)
        {
            this.Close(this.ActiveContent);
            e.Handled = true;
        }
 
        /// <summary>
        /// Enable/disable switch options menu items
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBtnMenuPopup(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.btnMenu.ContextMenu.IsOpen)
            {
                this.menuFloatingWindow.IsEnabled = this.PaneState != PaneState.AutoHide && this.PaneState != PaneState.Hidden;
                this.menuFloatingWindow.IsChecked = this.PaneState == PaneState.FloatingWindow;
                this.menuDockedWindow.IsEnabled = this.PaneState != PaneState.AutoHide && this.PaneState != PaneState.Hidden;
                this.menuDockedWindow.IsChecked = this.PaneState == PaneState.Docked || this.PaneState == PaneState.DockableWindow;
                this.menuTabbedDocument.IsEnabled = this.PaneState != PaneState.AutoHide && this.PaneState != PaneState.Hidden;
                this.menuAutoHide.IsChecked = this.PaneState == PaneState.AutoHide;
            }
        }

        /// <summary>
        /// Drag starting point
        /// </summary>
        private Point ptStartDrag;

        /// <summary>
        /// Handles mouse douwn event on pane header
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>Save current mouse position in ptStartDrag and capture mouse event on PaneHeader object.</remarks>
        private void OnHeaderMouseDown(object sender, MouseEventArgs e)
        {
            if (this.DockManager == null)
                return;

            if (!this.PaneHeader.IsMouseCaptured)
            {
                this.ptStartDrag = e.GetPosition(this);
                this.PaneHeader.CaptureMouse();
            }
        }
        /// <summary>
        /// Handles mouse up event on pane header
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>Release any mouse capture</remarks>
        private void OnHeaderMouseUp(object sender, MouseEventArgs e)
        {
            this.PaneHeader.ReleaseMouseCapture();
        }

        /// <summary>
        /// Handles mouse move event and eventually starts draging this pane
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnHeaderMouseMove(object sender, MouseEventArgs e)
        {
            if (this.PaneHeader.IsMouseCaptured && Math.Abs(this.ptStartDrag.X - e.GetPosition(this).X) > 4)
            {
                this.PaneHeader.ReleaseMouseCapture();
                this.DragPane(this.DockManager.PointToScreen(e.GetPosition(this.DockManager)), e.GetPosition(this.PaneHeader));
            }
        }

        /// <summary>
        /// Initiate a dragging operation of this pane, relative DockManager is also involved
        /// </summary>
        /// <param name="startDragPoint"></param>
        /// <param name="offset"></param>
        protected virtual void DragPane(Point startDragPoint, Point offset)
        {
            var wnd = new FloatingWindow(this);
            this.SetFloatingWindowSizeAndPosition(wnd);

            this.ChangeState(PaneState.DockableWindow);

            this.DockManager.Drag(wnd, startDragPoint, offset);        
        }


        /// <summary>
        /// Mouse down event on a content tab item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTabItemMouseDown(object sender, MouseButtonEventArgs e)
        {
            //TabItem item = sender as TabItem;
            var senderElement = sender as FrameworkElement;
            var item = senderElement.TemplatedParent as TabItem;

            if (!senderElement.IsMouseCaptured)
            {
                this.ptStartDrag = e.GetPosition(this);
                senderElement.CaptureMouse();
            }

        }

        /// <summary>
        /// Mouse move event on a content tab item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>If mouse is moved when left button is pressed than this method starts a dragging content operations. Also in this case relative DockManager is involved.</remarks>
        private void OnTabItemMouseMove(object sender, MouseEventArgs e)
        {
            //TabItem item = sender as TabItem;
            var senderElement = sender as FrameworkElement;
            var item = senderElement.TemplatedParent as TabItem;

            if (senderElement.IsMouseCaptured && Math.Abs(this.ptStartDrag.X - e.GetPosition(this).X) > 4)
            {
                senderElement.ReleaseMouseCapture();
                var contentToDrag = this.Contents[this.tbcContents.Items.IndexOf(item)] as DockableContent;
                if (contentToDrag != null) this.DragContent(contentToDrag, e.GetPosition(this.DockManager), e.GetPosition(item));
            }
        }

        /// <summary>
        /// Handles MouseUp event fired from a content tab item and eventually release mouse event capture 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTabItemMouseUp(object sender, MouseButtonEventArgs e)
        {
            //TabItem item = sender as TabItem;
            var senderElement = sender as FrameworkElement;
            var item = senderElement.TemplatedParent as TabItem;

            senderElement.ReleaseMouseCapture();
        }


        #region persistence

        public override void Serialize(XmlDocument doc, XmlNode parentNode)
        {
            this.SaveSize();

            parentNode.Attributes.Append(doc.CreateAttribute("Dock"));
            parentNode.Attributes["Dock"].Value = this._dock.ToString();
            parentNode.Attributes.Append(doc.CreateAttribute("PaneState"));
            parentNode.Attributes["PaneState"].Value = this.PaneState.ToString();
            parentNode.Attributes.Append(doc.CreateAttribute("LastState"));
            parentNode.Attributes["LastState"].Value = this._lastState.ToString();

            
            parentNode.Attributes.Append(doc.CreateAttribute("ptFloatingWindow"));
            parentNode.Attributes["ptFloatingWindow"].Value = System.ComponentModel.TypeDescriptor.GetConverter(typeof(Point)).ConvertToInvariantString(this.ptFloatingWindow);
            parentNode.Attributes.Append(doc.CreateAttribute("sizeFloatingWindow"));
            parentNode.Attributes["sizeFloatingWindow"].Value = System.ComponentModel.TypeDescriptor.GetConverter(typeof(Size)).ConvertToInvariantString(this.sizeFloatingWindow);

            base.Serialize(doc, parentNode);
        }

        public override void Deserialize(DockManager managerToAttach, XmlNode node, GetContentFromTypeString getObjectHandler)
        {
            base.Deserialize(managerToAttach, node, getObjectHandler);

            this._dock = (Dock)Enum.Parse(typeof(Dock), node.Attributes["Dock"].Value);
            this.PaneState = (PaneState)Enum.Parse(typeof(PaneState), node.Attributes["PaneState"].Value);
            this._lastState = (PaneState)Enum.Parse(typeof(PaneState), node.Attributes["LastState"].Value);

            this.ptFloatingWindow = (Point)System.ComponentModel.TypeDescriptor.GetConverter(typeof(Point)).ConvertFromInvariantString(node.Attributes["ptFloatingWindow"].Value);
            this.sizeFloatingWindow = (Size)System.ComponentModel.TypeDescriptor.GetConverter(typeof(Size)).ConvertFromInvariantString(node.Attributes["sizeFloatingWindow"].Value);

            

            if (this.State == PaneState.FloatingWindow)
                this.FloatingWindow();
            else if (this.State == PaneState.DockableWindow) this.DockableWindow();

            this.DockManager.AttachPaneEvents(this);
        }

        #endregion
    }
}