using System.Collections.Generic;
using System.Text;
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

namespace DockingLibrary
{
    public abstract class Pane : UserControl , IDropSurface, ILayoutSerializable
    {
        public double PaneWidth = 150;
        public double PaneHeight = 150;

        public readonly List<DockableContent> Contents = new List<DockableContent>();


        private DockManager _dockManager;
        public virtual DockManager DockManager
        {
            get { return this._dockManager; }
            //set { _dockManager = value; }
        }


        public Pane(DockManager dockManager):this(dockManager, null) { }

        public Pane(DockManager dockManager, DockableContent content)
        {
            this._dockManager = dockManager;

            if (content != null) this.Add(content);
        }

        public virtual void Add(DockableContent content)
        {
            if (this.DockManager != null) this.DockManager.Add(content);
            content.SetContainerPane(this);
            this.Contents.Add(content);
        }

        public virtual void Remove(DockableContent content)
        {
            if (this.DockManager != null) this.DockManager.Remove(content);
            content.SetContainerPane(null);
            this.Contents.Remove(content);
        }

        public virtual void Show(DockableContent content)
        {
            System.Diagnostics.Debug.Assert(this.Contents.Contains(content));
        }

        public virtual void Hide(DockableContent content)
        { 
        
        }

        public virtual void Show()
        {
            this.DockManager.DragPaneServices.Register(this);
        }

        public virtual void Hide()
        {
            this.DockManager.DragPaneServices.Unregister(this);
        }

        public virtual void Close()
        {
            this.DockManager.DragPaneServices.Unregister(this);
        }

        public virtual void Close(DockableContent content)
        {

        }
        
        
        public virtual bool IsHidden
        {
            get { return false; }
        }

        public virtual void SaveSize()
        {

        }


        public virtual DockableContent ActiveContent
        {
            get { return null; }
            set { }
        }

        protected virtual void DragContent(DockableContent contentToDrag, Point startDragPoint, Point offset)
        {
            this.Remove(contentToDrag);
            var pane = new DockablePane(this.DockManager);
            //pane = new DockablePane();
            //pane.DockManager = DockManager;
            pane.Add(contentToDrag);
            pane.Show();
            this.DockManager.Add(pane);
            //DockManager.Add(contentToDrag);
            var wnd = new FloatingWindow(pane);
            pane.ChangeState(PaneState.DockableWindow);
            this.DockManager.Drag(wnd, startDragPoint, offset);
        }

        public virtual void RefreshTitle()
        { 
        
        }

        #region Membri di IDropSurface 

        public Rect SurfaceRectangle
        {
            get 
            {
                if (this.IsHidden)
                    return new Rect();
                
                return new Rect(this.PointToScreen(new Point(0,0)), new Size(this.ActualWidth, this.ActualHeight)); 
            }
        }

        public virtual void OnDragEnter(Point point)
        {
            this.DockManager.OverlayWindow.ShowOverlayPaneDockingOptions(this);
        }

        public virtual void OnDragOver(Point point)
        {
            
        }

        public virtual void OnDragLeave(Point point)
        {
            this.DockManager.OverlayWindow.HideOverlayPaneDockingOptions(this);
        }

        public virtual bool OnDrop(Point point)
        {
            return false;
        }

        #endregion

        #region ILayoutSerializable Membri di

        public virtual void Serialize(XmlDocument doc, XmlNode parentNode)
        {
            parentNode.Attributes.Append(doc.CreateAttribute("Width"));
            parentNode.Attributes["Width"].Value = this.PaneWidth.ToString();
            parentNode.Attributes.Append(doc.CreateAttribute("Height"));
            parentNode.Attributes["Height"].Value = this.PaneHeight.ToString();

            foreach (ManagedContent content in this.Contents)
            {
                var dockableContent = content as DockableContent;
                if (dockableContent != null)
                {
                    XmlNode nodeDockableContent = doc.CreateElement(dockableContent.GetType().ToString());
                    parentNode.AppendChild(nodeDockableContent);
                }
            }
        }

        public virtual void Deserialize(DockManager managerToAttach, XmlNode node, GetContentFromTypeString getObjectHandler)
        {
            this._dockManager = managerToAttach;

            this.PaneWidth = double.Parse(node.Attributes["Width"].Value);
            this.PaneHeight = double.Parse(node.Attributes["Height"].Value);

            foreach (XmlNode nodeDockableContent in node.ChildNodes)
            {
                var content = getObjectHandler(nodeDockableContent.Name);
                this.Add(content);
                this.Show(content);
            }

            this.DockManager.DragPaneServices.Register(this);
        }

        #endregion

    }
}
