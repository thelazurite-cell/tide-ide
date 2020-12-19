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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

namespace DockingLibrary
{
    /// <summary>
    /// Interaction logic for DockingGrid.xaml
    /// </summary>

    public partial class DockingGrid : UserControl, ILayoutSerializable
    {
        public DockingGrid()
        {
            this.InitializeComponent();

        }

        internal void AttachDockManager(DockManager dockManager)
        {
            this._docsPane = new DocumentsPane(dockManager);
            this._rootGroup = new DockablePaneGroup(this.DocumentsPane);
            this.ArrangeLayout();
        }
        
        //Creates a root group with a DocumentsPane
        private DockablePaneGroup _rootGroup;

        private DocumentsPane _docsPane;

        public DocumentsPane DocumentsPane
        {
            get { return this._docsPane; }
        }

        internal void AttachPaneEvents(DockablePane pane)
        {
            pane.OnStateChanged += this.pane_OnStateChanged;
            pane.OnDockChanged += this.pane_OnDockChanged;
        }

        internal void DetachPaneEvents(DockablePane pane)
        {
            pane.OnStateChanged -= this.pane_OnStateChanged;
            pane.OnDockChanged -= this.pane_OnDockChanged;
        }

        private void pane_OnDockChanged(object sender, EventArgs e)
        {
            var pane = sender as DockablePane;
            this.Remove(pane);
            this.Add(pane);
        }

        private void pane_OnStateChanged(object sender, EventArgs e)
        {
            var pane = sender as DockablePane;

            //if (pane.PaneState == PaneState.FloatingWindow)
            //    Remove(pane);
            //else
            this.ArrangeLayout();
        }


        public void Add(DockablePane pane)
        {
            this._rootGroup = this._rootGroup.AddPane(pane);
            this.ArrangeLayout();
        }

        private void Dump(DockablePaneGroup group, int indent)
        {
            if (indent == 0)
                Console.WriteLine("Dump()");
            for (var i = 0; i < indent; i++)
                Console.Write("-");
            Console.Write(">");
            if (group.AttachedPane == null)
            {
                Console.WriteLine(group.Dock);
                this.Dump(group.FirstChildGroup, indent + 4);
                Console.WriteLine();
                this.Dump(group.SecondChildGroup, indent + 4);
            }
            else if (group.AttachedPane.ActiveContent!=null)
                Console.WriteLine(group.AttachedPane.ActiveContent.Title);
            else
                Console.WriteLine(group.AttachedPane.ToString() + " {null}");
        }

        public void Add(DockablePane pane, Pane relativePane, Dock relativeDock)
        {
            Console.WriteLine("Add(...)");
            this.AttachPaneEvents(pane);
            var group = this.GetPaneGroup(relativePane);
            //group.ParentGroup.ReplaceChildGroup(group, new DockablePaneGroup(group, new DockablePaneGroup(relativePane), relativeDock));

            switch (relativeDock)
            {
                case Dock.Right:
                case Dock.Bottom:
                    {
                        if (group == this._rootGroup)
                        {
                            this._rootGroup = new DockablePaneGroup(group, new DockablePaneGroup(pane), relativeDock);
                        }
                        else
                        {
                            var parentGroup = group.ParentGroup;
                            var newChildGroup = new DockablePaneGroup(group, new DockablePaneGroup(pane), relativeDock);
                            parentGroup.ReplaceChildGroup(group, newChildGroup);
                        }
                    }
                    break;
                case Dock.Left:
                case Dock.Top:
                    {
                        if (group == this._rootGroup)
                        {
                            this._rootGroup = new DockablePaneGroup(new DockablePaneGroup(pane), group, relativeDock);
                        }
                        else
                        {
                            var parentGroup = group.ParentGroup;
                            var newChildGroup = new DockablePaneGroup(new DockablePaneGroup(pane), group, relativeDock);
                            parentGroup.ReplaceChildGroup(group, newChildGroup);
                        }
                    }
                    break;
                    //return new DockablePaneGroup(new DockablePaneGroup(pane), this, pane.Dock);
            }

            
            //group.ChildGroup = new DockablePaneGroup(group.ChildGroup, pane, relativeDock);
            this.ArrangeLayout();
            
        }

        public void Remove(DockablePane pane)
        {
            var groupToAttach = this._rootGroup.RemovePane(pane);
            if (groupToAttach != null)
            {
                this._rootGroup = groupToAttach;
                this._rootGroup.ParentGroup = null;
            }

            this.ArrangeLayout();
        }

        public void MoveTo(DockablePane sourcePane, Pane destinationPane, Dock relativeDock)
        {
            this.Remove(sourcePane);
            this.Add(sourcePane, destinationPane, relativeDock);
        }

        public void MoveInto(DockablePane sourcePane, Pane destinationPane)
        {
            this.Remove(sourcePane);
            while (sourcePane.Contents.Count > 0)
            {
                var content = sourcePane.Contents[0];
                sourcePane.Remove(content);
                destinationPane.Add(content);
                destinationPane.Show(content);
            }
            sourcePane.Close();
        }

        public Pane GetPaneFromContent(DockableContent content)
        {
            return this._rootGroup.GetPaneFromContent(content);
        }

        private DockablePaneGroup GetPaneGroup(Pane pane)
        {
            return this._rootGroup.GetPaneGroup(pane);
        }

        internal void ArrangeLayout()
        {
            this.Clear(this._panel);
            this._rootGroup.Arrange(this._panel);
            this.Dump(this._rootGroup, 0);
        }

      
        private void Clear(Grid grid)
        {
            foreach (UIElement child in grid.Children)
            {
                if (child is Grid) this.Clear(child as Grid);
            }

            grid.Children.Clear();
            grid.ColumnDefinitions.Clear();
            grid.RowDefinitions.Clear();
        }


        #region ILayoutSerializable Membri di

        public void Serialize(XmlDocument doc, XmlNode parentNode)
        {
            XmlNode node_rootGroup = doc.CreateElement("_rootGroup");

            this._rootGroup.Serialize(doc, parentNode);

            parentNode.AppendChild(node_rootGroup);
        }

        public void Deserialize(DockManager managerToAttach, XmlNode node, GetContentFromTypeString getObjectHandler)
        {
            this._rootGroup = new DockablePaneGroup();
            this._rootGroup.Deserialize(managerToAttach, node, getObjectHandler);

            //_docsPane = FindDocumentsPane(_rootGroup);

            this.ArrangeLayout();
        }

        private DocumentsPane FindDocumentsPane(DockablePaneGroup group)
        {
            if (group == null)
                return null;

            if (group.AttachedPane is DocumentsPane)
                return group.AttachedPane as DocumentsPane;
            else
            {
                var docsPane = this.FindDocumentsPane(group.FirstChildGroup);
                if (docsPane != null)
                    return docsPane;

                docsPane = this.FindDocumentsPane(group.SecondChildGroup);
                if (docsPane != null)
                    return docsPane;
            }

            return null;
        }

        #endregion
    }
}