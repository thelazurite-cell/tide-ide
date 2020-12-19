using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Xml;
// ReSharper disable DelegateSubtraction

namespace DockingLibrary
{
    /// <summary>
    /// Manages and controls panes layout
    /// </summary>
    /// <remarks>This is the main user control which is usually embedded in a window. DockManager can control
    /// other windows arraging them in panes like VS. Each pane can be docked to a DockManager border, can be shown/hidden or auto-hidden.</remarks>
    public partial class DockManager : UserControl, IDropSurface
    {
        public DockManager()
        {
            this.InitializeComponent();

            this.DragPaneServices.Register(this);

            this.overlayWindow = new OverlayWindow(this);

            this.gridDocking.AttachDockManager(this);
            this.gridDocking.DocumentsPane.Show(); // .DockManager = this;
        }

        /// <summary>
        /// List of managed contents (hiddens too)
        /// </summary>
        private List<DockableContent> contents = new List<DockableContent>();

        /// <summary>
        /// Returns a documents list
        /// </summary>
        public IEnumerable<DocumentContent> Documents
        {
            get
            {
                var docs = new DocumentContent[this.gridDocking.DocumentsPane.Documents.Count -
                                               this.gridDocking.DocumentsPane.Contents.Count];
                var i = 0;
                foreach (var content in this.gridDocking.DocumentsPane.Documents)
                {
                    if (content is DocumentContent documentContent)
                        docs[i++] = documentContent;
                }

                //gridDocking.DocumentsPane.Documents.CopyTo(docs);
                return docs;
            }
        }

        /// <summary>
        /// Return active document
        /// </summary>
        /// <remarks>If no document is present or a dockable content is active in the Documents pane return null</remarks>
        public DocumentContent ActiveDocument
        {
            get { return this.gridDocking.DocumentsPane.ActiveDocument; }
        }

        /// <summary>
        /// Add dockable content to layout management
        /// </summary>
        /// <param name="content">Content to add</param>
        /// <returns></returns>
        internal void Add(DockableContent content)
        {
            if (!this.contents.Contains(content)) this.contents.Add(content);
        }

        /// <summary>
        /// Add a dockapble to layout management
        /// </summary>
        /// <param name="pane">Pane to manage</param>
        internal void Add(DockablePane pane)
        {
            this.gridDocking.Add(pane);
            this.AttachPaneEvents(pane);
        }

        /// <summary>
        /// Remove a dockable pane from layout management
        /// </summary>
        /// <param name="dockablePane">Dockable pane to remove</param>
        /// <remarks>Also pane event handlers are detached</remarks>
        internal void Remove(DockablePane dockablePane)
        {
            this.gridDocking.Remove(dockablePane);
            this.DetachPaneEvents(dockablePane);
        }

        /// <summary>
        /// Remove a dockable content from internal contents list
        /// </summary>
        /// <param name="content"></param>
        internal void Remove(DockableContent content)
        {
            this.contents.Remove(content);
        }

        /// <summary>
        /// Add a document content
        /// </summary>
        /// <param name="content">Document content to adde</param>
        /// <returns>Returns DocumentsPane where document is added</returns>
        internal DocumentsPane AddDocument(DocumentContent content)
        {
            Debug.Assert(!this.gridDocking.DocumentsPane.Documents.Contains(content));
            //gridDocking.DocumentsPane.DockManager = this;
            if (!this.gridDocking.DocumentsPane.Documents.Contains(content))
            {
                this.gridDocking.DocumentsPane.Add(content);
                //gridDocking.ArrangeLayout();
            }

            return this.gridDocking.DocumentsPane;
        }

        /// <summary>
        /// Add a docable content to default documents pane
        /// </summary>
        /// <param name="content">Dockable content to add</param>
        /// <returns>Documents pane where dockable content is added</returns>
        internal DocumentsPane AddDocument(DockableContent content)
        {
            Debug.Assert(!this.contents.Contains(content));
            Debug.Assert(!this.gridDocking.DocumentsPane.Contents.Contains(content));

            if (this.gridDocking.DocumentsPane.Contents.Contains(content)) return this.gridDocking.DocumentsPane;
            this.gridDocking.DocumentsPane.Add(content);
            this.gridDocking.ArrangeLayout();

            return this.gridDocking.DocumentsPane;
        }

        /// <summary>
        /// Returns currently active documents pane (at the moment this is only one per DockManager control)
        /// </summary>
        /// <returns>The DocumentsPane</returns>
        internal DocumentsPane GetDocumentsPane()
        {
            return this.gridDocking.DocumentsPane;
        }

        /// <summary>
        /// During unolad process close active contents windows
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUnloaded(object sender, EventArgs e)
        {
            foreach (var content in this.contents)
                content.Close();
            foreach (var docContent in this.Documents)
                docContent.Close();

            this.overlayWindow.Close();
        }

        /// <summary>
        /// Attach pane events handler
        /// </summary>
        /// <param name="pane"></param>
        internal void AttachPaneEvents(DockablePane pane)
        {
            pane.OnStateChanged += this.pane_OnStateChanged;

            this.gridDocking.AttachPaneEvents(pane);
        }

        /// <summary>
        /// Detach pane events handler
        /// </summary>
        /// <param name="pane"></param>
        private void DetachPaneEvents(DockablePane pane)
        {
            pane.OnStateChanged -= this.pane_OnStateChanged;

            this.gridDocking.DetachPaneEvents(pane);
        }

        /// <summary>
        /// List of managed docking button groups currently shown in border stack panels
        /// </summary>
        private readonly List<DockingButtonGroup> dockingBtnGroups = new List<DockingButtonGroup>();

        /// <summary>
        /// Handles pane state changed event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pane_OnStateChanged(object sender, EventArgs e)
        {
            if (!(sender is DockablePane pane)) return;
            if (pane.State != PaneState.AutoHide) return;
            this.AddPaneDockingButtons(pane);
            this.ShowTempPane(false);
            this.HideTempPane(true);
        }

        /// <summary>
        /// Add a group of docking buttons for a pane docked to a dockingmanager border
        /// </summary>
        /// <param name="pane"></param>
        private void AddPaneDockingButtons(DockablePane pane)
        {
            var buttonGroup = new DockingButtonGroup {Dock = pane.Dock};

            foreach (var content in pane.Contents)
            {
                var btn = new DockingButton
                {
                    DockableContent = content,
                    DockingButtonGroup = buttonGroup
                };

                if (this.currentButton == null) this.currentButton = btn;

                buttonGroup.Buttons.Add(btn);
            }

            this.dockingBtnGroups.Add(buttonGroup);

            this.AddDockingButtons(buttonGroup);
        }

        /// <summary>
        /// Add a group of docking buttons to the relative border stack panel
        /// </summary>
        /// <param name="group">Group to add</param>
        private void AddDockingButtons(DockingButtonGroup group)
        {
            foreach (var btn in group.Buttons)
                btn.MouseEnter += this.OnShowAutoHidePane;

            var br = new Border();
            br.Width = br.Height = 10;
            switch (group.Dock)
            {
                case Dock.Left:
                    foreach (var btn in group.Buttons)
                    {
                        btn.LayoutTransform = new RotateTransform(90);
                        this.btnPanelLeft.Children.Add(btn);
                    }

                    this.btnPanelLeft.Children.Add(br);
                    break;
                case Dock.Right:
                    foreach (var btn in group.Buttons)
                    {
                        btn.LayoutTransform = new RotateTransform(90);
                        this.btnPanelRight.Children.Add(btn);
                    }

                    this.btnPanelRight.Children.Add(br);
                    break;
                case Dock.Top:
                    foreach (var btn in group.Buttons) this.btnPanelTop.Children.Add(btn);
                    this.btnPanelTop.Children.Add(br);
                    break;
                case Dock.Bottom:
                    foreach (var btn in group.Buttons) this.btnPanelBottom.Children.Add(btn);
                    this.btnPanelBottom.Children.Add(br);
                    break;
            }
        }

        /// <summary>
        /// Remove a group of docking buttons from the relative border stack panel
        /// </summary>
        /// <param name="group">Group to remove</param>
        private void RemoveDockingButtons(DockingButtonGroup group)
        {
            foreach (var btn in group.Buttons)
                btn.MouseEnter -= this.OnShowAutoHidePane;

            switch (group.Dock)
            {
                case Dock.Left:
                    this.btnPanelLeft.Children.RemoveAt(
                        this.btnPanelLeft.Children.IndexOf(group.Buttons[group.Buttons.Count - 1]) + 1);
                    foreach (var btn in group.Buttons) this.btnPanelLeft.Children.Remove(btn);
                    break;
                case Dock.Right:
                    this.btnPanelRight.Children.RemoveAt(
                        this.btnPanelRight.Children.IndexOf(group.Buttons[group.Buttons.Count - 1]) + 1);
                    foreach (var btn in group.Buttons) this.btnPanelRight.Children.Remove(btn);
                    break;
                case Dock.Top:
                    this.btnPanelTop.Children.RemoveAt(
                        this.btnPanelTop.Children.IndexOf(group.Buttons[group.Buttons.Count - 1]) + 1);
                    foreach (var btn in group.Buttons) this.btnPanelTop.Children.Remove(btn);
                    break;
                case Dock.Bottom:
                    this.btnPanelBottom.Children.RemoveAt(
                        this.btnPanelBottom.Children.IndexOf(group.Buttons[group.Buttons.Count - 1]) + 1);
                    foreach (var btn in group.Buttons) this.btnPanelBottom.Children.Remove(btn);
                    break;
            }
        }

        #region Overlay Panel

        /// <summary>
        /// Temporary pane used to host orginal content which is autohidden
        /// </summary>
        private OverlayDockablePane tempPane;

        /// <summary>
        /// Current docking button attached to current temporary pane
        /// </summary>
        private DockingButton currentButton;

        /// <summary>
        /// Event handler which show a temporary pane with a single content attached to a docking button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnShowAutoHidePane(object sender, MouseEventArgs e)
        {
            if (ReferenceEquals(this.currentButton, sender))
                return;

            this.HideTempPane(true);

            this.currentButton = sender as DockingButton;

            this.ShowTempPane(true);
        }

        /// <summary>
        /// Event handler which hide temporary pane
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnHideAutoHidePane(object sender, MouseEventArgs e)
        {
            this.HideTempPane(true);
        }

        /// <summary>
        /// Hide temporay pane and reset current docking button
        /// </summary>
        /// <param name="smooth">True if resize animation is enabled</param>
        private void HideTempPane(bool smooth)
        {
            if (this.tempPane == null) return;
            if (!(this.gridDocking.GetPaneFromContent(this.tempPane.Contents[0]) is DockablePane pane)) return;
            var rightLeft = false;
            var length = 0.0;

            switch (this.currentButton.DockingButtonGroup.Dock)
            {
                case Dock.Left:
                case Dock.Right:
                    pane.PaneWidth = this.tempPaneAnimation != null ? this.lengthAnimation : this.tempPane.Width;
                    length = this.tempPane.Width;
                    rightLeft = true;
                    break;
                case Dock.Top:
                case Dock.Bottom:
                    pane.PaneHeight = this.tempPaneAnimation != null ? this.lengthAnimation : this.tempPane.Height;
                    length = this.tempPane.Height;
                    break;
            }

            this.tempPane.OnStateChanged -= this._tempPane_OnStateChanged;

            if (smooth)
            {
                this.HideOverlayPanel(length, rightLeft);
            }
            else
            {
                this.ForceHideOverlayPanel();
                this.panelFront.BeginAnimation(OpacityProperty, null);
                this.panelFront.Children.Clear();
                this.panelFront.Opacity = 0.0;
                this.tempPane.Close();
            }

            this.currentButton = null;
            this.tempPane = null;
        }

        /// <summary>
        /// Show tampoary pane attached to current docking buttno
        /// </summary>
        /// <param name="smooth">True if resize animation is enabled</param>
        private void ShowTempPane(bool smooth)
        {
            this.tempPane = new OverlayDockablePane(this, this.currentButton.DockableContent,
                this.currentButton.DockingButtonGroup.Dock);
            this.tempPane.OnStateChanged += this._tempPane_OnStateChanged;

            if (!(this.gridDocking.GetPaneFromContent(this.currentButton.DockableContent) is DockablePane pane)) return;
            this.panelFront.Children.Clear();
            this.tempPane.SetValue(DockPanel.DockProperty, this.currentButton.DockingButtonGroup.Dock);
            this.panelFront.Children.Add(this.tempPane);
            DockPanelSplitter splitter = null;
            var rightLeft = false;
            var length = 0.0;

            switch (this.currentButton.DockingButtonGroup.Dock)
            {
                case Dock.Left:
                    splitter = new DockPanelSplitter(this.tempPane, null, SplitOrientation.Vertical);
                    length = pane.PaneWidth;
                    rightLeft = true;
                    break;
                case Dock.Right:
                    splitter = new DockPanelSplitter(null, this.tempPane, SplitOrientation.Vertical);
                    length = pane.PaneWidth;
                    rightLeft = true;
                    break;
                case Dock.Top:
                    splitter = new DockPanelSplitter(this.tempPane, null, SplitOrientation.Horizontal);
                    length = pane.PaneHeight;
                    break;
                case Dock.Bottom:
                    splitter = new DockPanelSplitter(null, this.tempPane, SplitOrientation.Horizontal);
                    length = pane.PaneHeight;
                    break;
            }

            if (splitter == null) return;
            splitter.SetValue(DockPanel.DockProperty, this.currentButton.DockingButtonGroup.Dock);
            this.panelFront.Children.Add(splitter);

            if (smooth)
                this.ShowOverlayPanel(length, rightLeft);
            else
            {
                if (rightLeft)
                    this.tempPane.Width = length;
                else
                    this.tempPane.Height = length;
                this.panelFront.Opacity = 1.0;
            }
        }

        /// <summary>
        /// Handle AutoHide/Hide commande issued by user on temporary pane
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _tempPane_OnStateChanged(object sender, EventArgs e)
        {
            var pane = this.gridDocking.GetPaneFromContent(this.currentButton.DockableContent);

            if (this.currentButton != null)
            {
                switch (this.currentButton.DockingButtonGroup.Dock)
                {
                    case Dock.Left:
                    case Dock.Right:
                        pane.PaneWidth = this.tempPane.PaneWidth;
                        break;
                    case Dock.Top:
                    case Dock.Bottom:
                        pane.PaneHeight = this.tempPane.PaneHeight;
                        break;
                }

                this.RemoveDockingButtons(this.currentButton.DockingButtonGroup);
                this.dockingBtnGroups.Remove(this.currentButton.DockingButtonGroup);
            }


            var showOriginalPane = (this.tempPane.State == PaneState.Docked);

            this.HideTempPane(false);

            if (showOriginalPane)
                pane.Show();
            else
                pane.Hide();
        }


        #region Temporary pane animation methods

        /// <summary>
        /// Current resize orientation
        /// </summary>
        private bool leftRightAnimation;

        /// <summary>
        /// Target size of animation
        /// </summary>
        private double lengthAnimation;

        /// <summary>
        /// Temporary overaly pane used for animation
        /// </summary>
        private OverlayDockablePane tempPaneAnimation;

        /// <summary>
        /// Current animation object itself
        /// </summary>
        private DoubleAnimation animation;

        /// <summary>
        /// Show current overlay pane which hosts current auto-hiding content
        /// </summary>
        /// <param name="length">Target length</param>
        /// <param name="leftRight">Resize orientaion</param>
        private void ShowOverlayPanel(double length, bool leftRight)
        {
            this.ForceHideOverlayPanel();

            this.leftRightAnimation = leftRight;
            this.tempPaneAnimation = this.tempPane;
            this.lengthAnimation = length;

            this.animation = new DoubleAnimation();
            this.animation.From = 0.0;
            this.animation.To = length;
            this.animation.Duration = new Duration(TimeSpan.FromMilliseconds(200));
            this.animation.Completed += this.ShowOverlayPanel_Completed;
            if (this.leftRightAnimation)
                this.tempPaneAnimation.BeginAnimation(WidthProperty, this.animation);
            else
                this.tempPaneAnimation.BeginAnimation(HeightProperty, this.animation);

            var anOpacity = new DoubleAnimation();
            anOpacity.From = 0.0;
            anOpacity.To = 1.0;
            anOpacity.Duration = new Duration(TimeSpan.FromMilliseconds(200));
            this.panelFront.BeginAnimation(OpacityProperty, anOpacity);
        }

        /// <summary>
        /// Showing animation completed event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>Set final lenght and reset animation object on temp overlay panel</remarks>
        private void ShowOverlayPanel_Completed(object sender, EventArgs e)
        {
            this.animation.Completed -= this.ShowOverlayPanel_Completed;

            if (this.tempPaneAnimation != null)
            {
                if (this.leftRightAnimation)
                {
                    this.tempPaneAnimation.BeginAnimation(WidthProperty, null);
                    this.tempPaneAnimation.Width = this.lengthAnimation;
                }
                else
                {
                    this.tempPaneAnimation.BeginAnimation(HeightProperty, null);
                    this.tempPaneAnimation.Height = this.lengthAnimation;
                }
            }

            this.tempPaneAnimation = null;
        }

        /// <summary>
        /// Hide current overlay panel
        /// </summary>
        /// <param name="length"></param>
        /// <param name="leftRight"></param>
        private void HideOverlayPanel(double length, bool leftRight)
        {
            this.leftRightAnimation = leftRight;
            this.tempPaneAnimation = this.tempPane;
            this.lengthAnimation = length;

            this.animation = new DoubleAnimation
            {
                From = length,
                To = 0.0,
                Duration = new Duration(TimeSpan.FromMilliseconds(200))
            };
            this.animation.Completed += this.HideOverlayPanel_Completed;

            this.tempPaneAnimation.BeginAnimation(leftRight ? WidthProperty : HeightProperty, this.animation);

            var anOpacity = new DoubleAnimation
            {
                From = 1.0,
                To = 0.0,
                Duration = new Duration(TimeSpan.FromMilliseconds(200))
            };
            this.panelFront.BeginAnimation(OpacityProperty, anOpacity);
        }

        /// <summary>
        /// Hiding animation completed event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>Set final lenght to 0 and reset animation object on temp overlay panel</remarks>
        private void HideOverlayPanel_Completed(object sender, EventArgs e)
        {
            this.ForceHideOverlayPanel();
            this.panelFront.Children.Clear();
        }

        /// <summary>
        /// Forces to hide current overlay panel
        /// </summary>
        /// <remarks>Usually used when a second animation is about to start from a different button</remarks>
        private void ForceHideOverlayPanel()
        {
            if (this.tempPaneAnimation != null)
            {
                this.animation.Completed -= this.HideOverlayPanel_Completed;
                this.animation.Completed -= this.ShowOverlayPanel_Completed;
                this.tempPaneAnimation.BeginAnimation(this.leftRightAnimation ? WidthProperty : HeightProperty, null);

                this.tempPaneAnimation.Close();
                this.tempPaneAnimation = null;
            }
        }

        #endregion

        #endregion

        /// <summary>
        /// Handle dockable pane layout changing
        /// </summary>
        /// <param name="sourcePane">Source pane to move</param>
        /// <param name="destinationPane">Relative pane</param>
        /// <param name="relativeDock"></param>
        internal void MoveTo(DockablePane sourcePane, Pane destinationPane, Dock relativeDock)
        {
            this.gridDocking.MoveTo(sourcePane, destinationPane, relativeDock);
        }

        /// <summary>
        /// Called from a pane when it's dropped into an other pane
        /// </summary>
        /// <param name="sourcePane">Source pane which is going to be closed</param>
        /// <param name="destinationPane">Destination pane which is about to host contents from SourcePane</param>
        internal void MoveInto(DockablePane sourcePane, Pane destinationPane)
        {
            this.gridDocking.MoveInto(sourcePane, destinationPane);
        }

        #region DragDrop Operations

        /// <summary>
        /// Parent window hosting DockManager user control
        /// </summary>
        public Window ParentWindow = null;

        /// <summary>
        /// Begins dragging operations
        /// </summary>
        /// <param name="floatingWindow">Floating window containing pane which is dragged by user</param>
        /// <param name="point">Current mouse position</param>
        /// <param name="offset">Offset to be use to set floating window screen position</param>
        /// <returns>Retruns True is drag is completed, false otherwise</returns>
        public bool Drag(FloatingWindow floatingWindow, Point point, Point offset)
        {
            if (!this.IsMouseCaptured)
            {
                if (this.CaptureMouse())
                {
                    floatingWindow.Owner = this.ParentWindow;
                    this.DragPaneServices.StartDrag(floatingWindow, point, offset);

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Handles OnMouseDown event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMouseDown(object sender, MouseEventArgs e)
        {
        }

        /// <summary>
        /// Handles mousemove event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (this.IsMouseCaptured) this.DragPaneServices.MoveDrag(this.PointToScreen(e.GetPosition(this)));
        }

        /// <summary>
        /// Handles mouseUp event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>Releases eventually camptured mouse events</remarks>
        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            if (this.IsMouseCaptured)
            {
                this.DragPaneServices.EndDrag(this.PointToScreen(e.GetPosition(this)));
                this.ReleaseMouseCapture();
            }
        }

        private DragPaneServices dragPaneServices;

        internal DragPaneServices DragPaneServices
        {
            get
            {
                if (this.dragPaneServices == null) this.dragPaneServices = new DragPaneServices(this);

                return this.dragPaneServices;
            }
        }

        #endregion


        #region IDropSurface

        /// <summary>
        /// Returns a rectangle where this surface is active
        /// </summary>
        public Rect SurfaceRectangle => new Rect(this.PointToScreen(new Point(0, 0)), new Size(this.ActualWidth, this.ActualHeight));

        /// <summary>
        /// Overlay window which shows docking placeholders
        /// </summary>
        private readonly OverlayWindow overlayWindow;

        /// <summary>
        /// Returns current overlay window
        /// </summary>
        internal OverlayWindow OverlayWindow => this.overlayWindow;

        /// <summary>
        /// Handles this sourface mouse entering (show current overlay window)
        /// </summary>
        /// <param name="point">Current mouse position</param>
        public void OnDragEnter(Point point)
        {
            this.OverlayWindow.Owner = this.DragPaneServices.FloatingWindow;
            this.OverlayWindow.Left = this.PointToScreen(new Point(0, 0)).X;
            this.OverlayWindow.Top = this.PointToScreen(new Point(0, 0)).Y;
            this.OverlayWindow.Width = this.ActualWidth;
            this.OverlayWindow.Height = this.ActualHeight;
            this.OverlayWindow.Show();
        }

        /// <summary>
        /// Handles mouse overing this surface
        /// </summary>
        /// <param name="point"></param>
        public void OnDragOver(Point point)
        {
        }

        /// <summary>
        /// Handles mouse leave event during drag (hide overlay window)
        /// </summary>
        /// <param name="point"></param>
        public void OnDragLeave(Point point)
        {
            this.overlayWindow.Owner = null;
            this.overlayWindow.Hide();
            this.ParentWindow.Activate();
        }

        /// <summary>
        /// Handler drop events
        /// </summary>
        /// <param name="point">Current mouse position</param>
        /// <returns>Returns alwasy false because this surface doesn't support direct drop</returns>
        public bool OnDrop(Point point)
        {
            return false;
        }

        #endregion


        #region Persistence

        /// <summary>
        /// Serialize layout state of panes and contents into a xml string
        /// </summary>
        /// <returns>Xml containing layout state</returns>
        public string GetLayoutAsXml()
        {
            var doc = new XmlDocument();
            doc.AppendChild(doc.CreateElement("DockingLibrary_Layout"));
            this.gridDocking.Serialize(doc, doc.DocumentElement);
            return doc.OuterXml;
        }

        /// <summary>
        /// Restore docking layout reading a xml string which is previously generated by a call to GetLayoutState
        /// </summary>
        /// <param name="xml">Xml containing layout state</param>
        /// <param name="getContentHandler">Delegate used by serializer to get user defined dockable contents</param>
        public void RestoreLayoutFromXml(string xml, GetContentFromTypeString getContentHandler)
        {
            var doc = new XmlDocument();
            doc.LoadXml(xml);

            this.gridDocking.Deserialize(this, doc.ChildNodes[0], getContentHandler);

            var addedPanes = new List<Pane>();
            foreach (var content in this.contents)
            {
                var pane = content.ContainerPane as DockablePane;
                if (pane == null || addedPanes.Contains(pane)) continue;
                if (pane.State != PaneState.AutoHide) continue;
                addedPanes.Add(pane);
                this.AddPaneDockingButtons(pane);
            }

            this.currentButton = null;
        }

        #endregion
    }
}