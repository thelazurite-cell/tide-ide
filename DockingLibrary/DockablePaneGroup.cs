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
    /// How group panes are splitted
    /// </summary>
    public enum SplitOrientation
    {
        Horizontal,
        Vertical
    }

    /// <summary>
    /// Group of one or more child groups
    /// </summary>
    internal class DockablePaneGroup : ILayoutSerializable
    {
        private Pane _attachedPane;
 
        /// <summary>
        /// Pane directly attached
        /// </summary>
        public Pane AttachedPane {get{return this._attachedPane;}}

        private DockablePaneGroup _firstChildGroup;

        public DockablePaneGroup FirstChildGroup
        {
            get { return this._firstChildGroup; }
            set
            {
                this._firstChildGroup = value;
                value._parentGroup = this;
            }
        }

        private DockablePaneGroup _secondChildGroup;

        public DockablePaneGroup SecondChildGroup
        {
            get { return this._secondChildGroup; }
            set
            {
                this._secondChildGroup = value;
                value._parentGroup = this;
            }
        }


        private DockablePaneGroup _parentGroup;

        public DockablePaneGroup ParentGroup
        {
            get { return this._parentGroup; }
            internal set 
            {
                this._parentGroup = value;
            }

        }

        private Dock _dock;

        public Dock Dock
        {
            get { return this._dock; }
        }

        /// <summary>
        /// Needed only for deserialization
        /// </summary>
        public DockablePaneGroup()
        { }

        /// <summary>
        /// Create a group with a single pane
        /// </summary>
        /// <param name="pane">Attached pane</param>
        public DockablePaneGroup(Pane pane)
        {
            this._attachedPane = pane;
        }

        /// <summary>
        /// Create a group with no panes
        /// </summary>
        public DockablePaneGroup(DockablePaneGroup firstGroup, DockablePaneGroup secondGroup, Dock groupDock)
        {
            this.FirstChildGroup = firstGroup;
            this.SecondChildGroup = secondGroup;
            this._dock = groupDock;
        }

        public Pane GetPaneFromContent(DockableContent content)
        {
            if (this.AttachedPane != null && this.AttachedPane.Contents.Contains(content))
                return this.AttachedPane;

            if (this.FirstChildGroup != null)
            {
                var pane = this.FirstChildGroup.GetPaneFromContent(content);
                if (pane != null)
                    return pane;
            }

            if (this.SecondChildGroup != null)
                return this.SecondChildGroup.GetPaneFromContent(content);

            return null;
        }

        private bool IsHidden
        {
            get 
            {
                if (this.AttachedPane != null)
                    return this.AttachedPane.IsHidden;

                return this.FirstChildGroup.IsHidden && this.SecondChildGroup.IsHidden;
            }
        }

        private GridLength GroupWidth
        {
            get
            {
                if (this.AttachedPane != null)
                    return new GridLength(this.AttachedPane.PaneWidth, GridUnitType.Pixel);
                else
                {
                    if (this.Dock == Dock.Left || this.Dock == Dock.Right)
                        return new GridLength(this.FirstChildGroup.GroupWidth.Value+ this.SecondChildGroup.GroupWidth.Value+4, GridUnitType.Pixel);
                    else
                        return this.FirstChildGroup.GroupWidth;
                }
            }
        }

        private GridLength GroupHeight
        {
            get
            {
                if (this.AttachedPane != null)
                    return new GridLength(this.AttachedPane.PaneHeight, GridUnitType.Pixel);
                else
                {
                    if (this.Dock == Dock.Top || this.Dock == Dock.Bottom)
                        return new GridLength(this.FirstChildGroup.GroupHeight.Value + this.SecondChildGroup.GroupHeight.Value + 4, GridUnitType.Pixel);
                    else
                        return this.FirstChildGroup.GroupHeight;
                }
            }
        }

        public void Arrange(Grid grid)
        {
            if (this.AttachedPane != null)//AttachedPane.IsHidden)
                grid.Children.Add(this.AttachedPane);
            else if (this.FirstChildGroup.IsHidden && !this.SecondChildGroup.IsHidden)
                this.SecondChildGroup.Arrange(grid);
            else if (!this.FirstChildGroup.IsHidden && this.SecondChildGroup.IsHidden)
                this.FirstChildGroup.Arrange(grid);
            else
            {
                if (this.Dock == Dock.Left || this.Dock == Dock.Right)
                {
                    grid.RowDefinitions.Add(new RowDefinition());
                    grid.ColumnDefinitions.Add(new ColumnDefinition());
                    grid.ColumnDefinitions.Add(new ColumnDefinition());
                    //grid.ColumnDefinitions[0].Width = (Dock == Dock.Left) ? new GridLength(AttachedPane.PaneWidth) : new GridLength(1, GridUnitType.Star);
                    //grid.ColumnDefinitions[1].Width = (Dock == Dock.Right) ? new GridLength(AttachedPane.PaneWidth) : new GridLength(1, GridUnitType.Star);
                    grid.ColumnDefinitions[0].Width = (this.Dock == Dock.Left) ? this.FirstChildGroup.GroupWidth : new GridLength(1, GridUnitType.Star);
                    grid.ColumnDefinitions[1].Width = (this.Dock == Dock.Right) ? this.SecondChildGroup.GroupWidth : new GridLength(1, GridUnitType.Star);
                    
                    //grid.ColumnDefinitions[0].MinWidth = 50;
                    //grid.ColumnDefinitions[1].MinWidth = 50;


                    var firstChildGrid = new Grid();
                    firstChildGrid.SetValue(Grid.ColumnProperty, 0);
                    firstChildGrid.Margin = new Thickness(0, 0, 4, 0);
                    this.FirstChildGroup.Arrange(firstChildGrid);
                    grid.Children.Add(firstChildGrid);

                    var secondChildGrid = new Grid();
                    secondChildGrid.SetValue(Grid.ColumnProperty, 1);
                    //secondChildGrid.Margin = (Dock == Dock.Right) ? new Thickness(0, 0, 4, 0) : new Thickness();
                    this.SecondChildGroup.Arrange(secondChildGrid);
                    grid.Children.Add(secondChildGrid);

                    //AttachedPane.SetValue(Grid.ColumnProperty, (Dock == Dock.Right) ? 1 : 0);
                    //AttachedPane.Margin = (Dock == Dock.Left) ? new Thickness(0, 0, 4, 0) : new Thickness();
                    //grid.Children.Add(AttachedPane);

                    var splitter = new GridSplitter();
                    splitter.Width = 4;
                    splitter.HorizontalAlignment = HorizontalAlignment.Right;
                    splitter.VerticalAlignment = VerticalAlignment.Stretch;
                    grid.Children.Add(splitter);
                }
                else // if (Dock == Dock.Top || Dock == Dock.Bottom)
                {
                    grid.ColumnDefinitions.Add(new ColumnDefinition());
                    grid.RowDefinitions.Add(new RowDefinition());
                    grid.RowDefinitions.Add(new RowDefinition());
                    //grid.RowDefinitions[0].Height = (Dock == Dock.Top) ? new GridLength(AttachedPane.PaneHeight) : new GridLength(1, GridUnitType.Star);
                    //grid.RowDefinitions[1].Height = (Dock == Dock.Bottom) ? new GridLength(AttachedPane.PaneHeight) : new GridLength(1, GridUnitType.Star);
                    grid.RowDefinitions[0].Height = (this.Dock == Dock.Top) ? this.FirstChildGroup.GroupHeight : new GridLength(1, GridUnitType.Star);
                    grid.RowDefinitions[1].Height = (this.Dock == Dock.Bottom) ? this.SecondChildGroup.GroupHeight : new GridLength(1, GridUnitType.Star);
                    
                    grid.RowDefinitions[0].MinHeight = 50;
                    grid.RowDefinitions[1].MinHeight = 50;

                    var firstChildGrid = new Grid();
                    //firstChildGrid.SetValue(Grid.RowProperty, (Dock == Dock.Bottom) ? 1 : 0);
                    firstChildGrid.SetValue(Grid.RowProperty, 0);
                    //firstChildGrid.Margin = (Dock == Dock.Bottom) ? new Thickness(0, 0, 0, 4) : new Thickness();
                    firstChildGrid.Margin = new Thickness(0, 0, 0, 4);
                    this.FirstChildGroup.Arrange(firstChildGrid);
                    grid.Children.Add(firstChildGrid);

                    var secondChildGrid = new Grid();
                    //secondChildGrid.SetValue(Grid.RowProperty, (Dock == Dock.Top) ? 1 : 0);
                    secondChildGrid.SetValue(Grid.RowProperty, 1);
                    //secondChildGrid.Margin = (Dock == Dock.Bottom) ? new Thickness(0, 0, 0, 4) : new Thickness();
                    this.SecondChildGroup.Arrange(secondChildGrid);
                    grid.Children.Add(secondChildGrid);

                    //AttachedPane.SetValue(Grid.RowProperty, (Dock == Dock.Bottom) ? 1 : 0);
                    //AttachedPane.Margin = (Dock == Dock.Top) ? new Thickness(0, 0, 0, 4) : new Thickness();
                    //grid.Children.Add(AttachedPane);

                    var splitter = new GridSplitter();
                    splitter.Height = 4;
                    splitter.HorizontalAlignment = HorizontalAlignment.Stretch;
                    splitter.VerticalAlignment = VerticalAlignment.Bottom;
                    grid.Children.Add(splitter);
                }
            }
        }

        ///// <summary>
        ///// Arrange passed grid layout
        ///// </summary>
        ///// <param name="grid">Grid to arrange</param>
        ///// <param name="addPaneToGrid">If true add atteched panes to the grid</param>
        ///// <remarks>Setting <paramref name="addToPaneToGrid"/> to false, this functions only arrange grids layout, without 
        ///// appending attached pane to grid children collection. This is useful for dragging operations.</remarks>
        //public void Arrange(Grid grid, bool addPaneToGrid)
        //{
        //    if (AttachedPane != null)
        //    {
        //        if (addPaneToGrid)
        //            grid.Children.Add(AttachedPane);
        //    }
        //    else
        //    {
        //        if (Group1.IsHidden && !Group2.IsHidden)
        //            Group2.Arrange(grid, addPaneToGrid);
        //        else if (!Group1.IsHidden && Group2.IsHidden)
        //            Group1.Arrange(grid, addPaneToGrid);
        //        else
        //        {
        //                //first child grid
        //                Grid grid1 = new Grid();
        //                //..and second one
        //                Grid grid2 = new Grid();                    
                
        //            #region Vertical orientation
        //            if (SplitOrientation == SplitOrientation.Vertical)
        //            {
        //                 //only one row
        //                grid.RowDefinitions.Add(new RowDefinition());
        //                //two cols
        //                grid.ColumnDefinitions.Add(new ColumnDefinition());
        //                grid.ColumnDefinitions.Add(new ColumnDefinition());

        //                //setup widths
        //                grid.ColumnDefinitions[0].MinWidth = 50;
        //                grid.ColumnDefinitions[0].Width = Group1.GridWidth;
        //                grid.ColumnDefinitions[1].MinWidth = 50;
        //                grid.ColumnDefinitions[1].Width = Group2.GridWidth;

        //                //ensure that at least one col has star length
        //                if (!grid.ColumnDefinitions[0].Width.IsStar &&
        //                    !grid.ColumnDefinitions[1].Width.IsStar)
        //                    grid.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Star);

        //                grid1.SetValue(Grid.ColumnProperty, 0);
        //                grid2.SetValue(Grid.ColumnProperty, 1);

        //                GridSplitter splitter = new GridSplitter();
        //                splitter.VerticalAlignment = VerticalAlignment.Stretch;
        //                splitter.HorizontalAlignment = HorizontalAlignment.Left;
        //                splitter.SetValue(Grid.ColumnProperty, 1);
        //                splitter.SetValue(Grid.RowProperty, 0);
        //                splitter.Width = 5;
        //                //make room for the splitter
        //                grid2.Margin = new Thickness(5,0,0,0);

        //                //ok, now add child grids and a splitter between them to current grid
        //                grid.Children.Add(grid1);
        //                grid.Children.Add(splitter);
        //                grid.Children.Add(grid2);

        //                //finally arrange child grids
        //                Group1.Arrange(grid1, addPaneToGrid);
        //                Group2.Arrange(grid2, addPaneToGrid);
        //            }
        //            #endregion

        //            #region Horizontal Orientation
        //            else //if (SplitOrientation == SplitOrientation.Horizontal)
        //            {
        //            }
        //            #endregion
        //        }
        //    }

        //}

        //DockPanel GetChildElement(DockablePaneGroup group, Dock dock)
        //{
        //    DockPanel childPanel = new DockPanel();
        //    childPanel.SetValue(DockPanel.DockProperty, dock);
            
        //    if (SplitOrientation == SplitOrientation.Vertical)
        //        childPanel.Width = group.DockPanelWidth;
        //    else
        //        childPanel.Height = group.DockPanelHeight;

        //    group.Arrange(childPanel);

        //    //childPanels.Add(childPanel);
        //    return childPanel;
        //}

        //internal void Arrange(DockPanel panel)
        //{

        //    if (AttachedPane != null)
        //    {
        //        AttachedPane.AttachPanel(panel);
        //        panel.Children.Add(AttachedPane);
        //    }
        //    else
        //    {
        //        #region Vertical split
        //        if (SplitOrientation == SplitOrientation.Vertical)
        //        {
        //            DockPanel lastPanel = null;
                    
        //            foreach (DockablePaneGroup group in ChildGroups)
        //            {
        //                if (group.IsHidden)
        //                    continue;

        //                if (double.IsNaN(group.DockPanelWidth))
        //                {
        //                    lastPanel = GetChildElement(group, Dock.Left);
        //                }
        //                else
        //                {
        //                    DockPanel panelToAdd = GetChildElement(group, lastPanel == null ? Dock.Left : Dock.Right);
        //                    panel.Children.Add(panelToAdd);
        //                }
        //            }

        //            if (lastPanel != null)
        //                panel.Children.Add(lastPanel);

        //            Dock currentDock = Dock.Left;

        //            for (int i = 0; i < panel.Children.Count-1; i++)
        //            {
        //                currentDock = (Dock)panel.Children[i].GetValue(DockPanel.DockProperty);
        //                DockPanel prevPanel = panel.Children[i] as DockPanel;
        //                DockPanel nextPanel = null;
        //                for (int j = i+1; j < panel.Children.Count; j++)
        //                    if ((Dock)panel.Children[j].GetValue(DockPanel.DockProperty) ==
        //                        currentDock)
        //                    {
        //                        nextPanel = panel.Children[j] as DockPanel;
        //                        break;
        //                    }
        //                if (nextPanel == null)
        //                    nextPanel = panel.Children[panel.Children.Count - 1] as DockPanel;

        //                DockPanelSplitter splitter = null;
                        
        //                if (currentDock == Dock.Left)
        //                    splitter = new DockPanelSplitter(prevPanel, nextPanel, SplitOrientation.Vertical);
        //                else
        //                    splitter = new DockPanelSplitter(nextPanel, prevPanel, SplitOrientation.Vertical);
        //                splitter.SetValue(DockPanel.DockProperty, currentDock);
                        
        //                i++;
        //                panel.Children.Insert(i, splitter);
        //            }
        //        }
        //        #endregion
        //        #region Horizontal split
        //        else //if (SplitOrientation == SplitOrientation.Vertical)
        //        {
        //            DockPanel lastPanel = null;

        //            foreach (DockablePaneGroup group in ChildGroups)
        //            {
        //                if (group.IsHidden)
        //                    continue;

        //                if (double.IsNaN(group.DockPanelWidth))
        //                {
        //                    lastPanel = GetChildElement(group, Dock.Top);
        //                }
        //                else
        //                {
        //                    DockPanel panelToAdd = GetChildElement(group, lastPanel == null ? Dock.Top : Dock.Bottom);
        //                    panel.Children.Add(panelToAdd);
        //                }
        //            }

        //            if (lastPanel != null)
        //                panel.Children.Add(lastPanel);

        //            Dock currentDock = Dock.Top;

        //            for (int i = 0; i < panel.Children.Count - 1; i++)
        //            {
        //                currentDock = (Dock)panel.Children[i].GetValue(DockPanel.DockProperty);
        //                DockPanel prevPanel = panel.Children[i] as DockPanel;
        //                DockPanel nextPanel = null;
        //                for (int j = i + 1; j < panel.Children.Count; j++)
        //                    if ((Dock)panel.Children[j].GetValue(DockPanel.DockProperty) ==
        //                        currentDock)
        //                    {
        //                        nextPanel = panel.Children[j] as DockPanel;
        //                        break;
        //                    }
        //                if (nextPanel == null)
        //                    nextPanel = panel.Children[panel.Children.Count - 1] as DockPanel;

        //                DockPanelSplitter splitter = null;

        //                if (currentDock == Dock.Top)
        //                    splitter = new DockPanelSplitter(prevPanel, nextPanel, SplitOrientation.Horizontal);
        //                else
        //                    splitter = new DockPanelSplitter(nextPanel, prevPanel, SplitOrientation.Horizontal);
        //                splitter.SetValue(DockPanel.DockProperty, currentDock);

        //                i++;
        //                panel.Children.Insert(i, splitter);
        //            }
        //        }
        //        #endregion
        //    }
        
        //}

        public void ReplaceChildGroup(DockablePaneGroup groupToFind, DockablePaneGroup groupToReplace)
        {
            if (this.FirstChildGroup == groupToFind)
                this.FirstChildGroup = groupToReplace;
            else if (this.SecondChildGroup == groupToFind)
                this.SecondChildGroup = groupToReplace;
            else
            {
                System.Diagnostics.Debug.Assert(false);
            }
        }

        //public void SaveChildPanesSize()
        //{
        //    if (AttachedPane != null && ParentGroup!=null)
        //        AttachedPane.SaveSize(ParentGroup.Dock);
        //    else
        //    {
        //        FirstChildGroup.SaveChildPanesSize();
        //        SecondChildGroup.SaveChildPanesSize();
        //    }
                
        //}

        public DockablePaneGroup AddPane(DockablePane pane)
        {
            switch (pane.Dock)
            {
                case Dock.Right:
                case Dock.Bottom:
                    return new DockablePaneGroup(this, new DockablePaneGroup(pane), pane.Dock);
                    
                case Dock.Left:
                case Dock.Top:
                    return new DockablePaneGroup(new DockablePaneGroup(pane), this, pane.Dock);
            }

            return null;
            //DockablePaneGroup resGroup = null;

            //if (AttachedPane != null)
            //{
            //    DockablePaneGroup newChildGroup = new DockablePaneGroup(AttachedPane);
            //    switch (pane.Dock)
            //    {
            //        case Dock.Left:
            //            resGroup = new DockablePaneGroup(new DockablePaneGroup(pane), newChildGroup, SplitOrientation.Vertical);
            //            break;
            //        case Dock.Right:
            //            resGroup = new DockablePaneGroup(newChildGroup, new DockablePaneGroup(pane), SplitOrientation.Vertical);
            //            break;
            //        case Dock.Top:
            //            resGroup = new DockablePaneGroup(new DockablePaneGroup(pane), newChildGroup, SplitOrientation.Horizontal);
            //            break;
            //        case Dock.Bottom:
            //            resGroup = new DockablePaneGroup(newChildGroup, new DockablePaneGroup(pane), SplitOrientation.Horizontal);
            //            break;
            //    }
            //}
            //else
            //{
            //    if (SplitOrientation == SplitOrientation.Vertical)
            //    {
            //        if (pane.Dock == Dock.Left)
            //        {
            //            ChildGroups.Insert(0, new DockablePaneGroup(pane));
            //            resGroup = this;
            //        }
            //        else if (pane.Dock == Dock.Right)
            //        {
            //            int index = 0; 
            //            for (int i = 0; i < ChildGroups.Count;i++)
            //                if (ChildGroups[i].do
                            
            //            ChildGroups.Add(new DockablePaneGroup(pane));
            //            resGroup = this;
            //        }
            //        else if (pane.Dock == Dock.Bottom)
            //            resGroup = new DockablePaneGroup(this, new DockablePaneGroup(pane), SplitOrientation.Horizontal);
            //        else if (pane.Dock == Dock.Top)
            //            resGroup = new DockablePaneGroup(new DockablePaneGroup(pane), this, SplitOrientation.Horizontal);
            //    }
            //    else //if (SplitOrientation == SplitOrientation.Horizontal)
            //    {
            //        if (pane.Dock == Dock.Top)
            //        {
            //            ChildGroups.Insert(0, new DockablePaneGroup(pane));
            //            resGroup = this;
            //        }
            //        else if (pane.Dock == Dock.Bottom)
            //        {
            //            ChildGroups.Add(new DockablePaneGroup(pane));
            //            resGroup = this;
            //        }
            //        else if (pane.Dock == Dock.Right)
            //            resGroup = new DockablePaneGroup(this, new DockablePaneGroup(pane), SplitOrientation.Vertical);
            //        else if (pane.Dock == Dock.Left)
            //            resGroup = new DockablePaneGroup(new DockablePaneGroup(pane), this, SplitOrientation.Vertical);
            //    }
            //}
            
            //return resGroup;
        }

  
        public DockablePaneGroup RemovePane(DockablePane pane)
        {
            if (this.AttachedPane != null)
                return null;

            if (this.FirstChildGroup.AttachedPane==pane)
            {
                return this.SecondChildGroup;
            }
            else if (this.SecondChildGroup.AttachedPane==pane)
            {
                return this.FirstChildGroup;
            }
            else
            {
                var group = this.FirstChildGroup.RemovePane(pane);

                if (group != null)
                {
                    this.FirstChildGroup = group;
                    group._parentGroup = this;
                    return null;
                }


                group = this.SecondChildGroup.RemovePane(pane);

                if (group != null)
                {
                    this.SecondChildGroup = group;
                    group._parentGroup = this;
                    return null;
                }
            }

            return null;
        }

        public DockablePaneGroup GetPaneGroup(Pane pane)
        {
            if (this.AttachedPane == pane)
                return this;
            
            if (this.FirstChildGroup != null)
            {
                var paneGroup = this.FirstChildGroup.GetPaneGroup(pane);
                if (paneGroup!=null)
                    return paneGroup;
            }
            if (this.SecondChildGroup != null)
            {
                var paneGroup = this.SecondChildGroup.GetPaneGroup(pane);
                if (paneGroup!=null)
                    return paneGroup;
            }

            return null;
        }


        #region ILayoutSerializable Membri di

        public void Serialize(XmlDocument doc, XmlNode parentNode)
        {
            parentNode.Attributes.Append(doc.CreateAttribute("Dock"));
            parentNode.Attributes["Dock"].Value = this._dock.ToString();

            if (this.AttachedPane != null)
            {
                XmlNode nodeAttachedPane = null;

                if (this.AttachedPane is DockablePane)
                    nodeAttachedPane = doc.CreateElement("DockablePane");
                else if (this.AttachedPane is DocumentsPane)
                    nodeAttachedPane = doc.CreateElement("DocumentsPane");

                this.AttachedPane.Serialize(doc, nodeAttachedPane);

                parentNode.AppendChild(nodeAttachedPane);
            }
            else
            {
                XmlNode nodeChildGroups = doc.CreateElement("ChildGroups");

                XmlNode nodeFirstChildGroup = doc.CreateElement("FirstChildGroup");
                this.FirstChildGroup.Serialize(doc, nodeFirstChildGroup);
                nodeChildGroups.AppendChild(nodeFirstChildGroup);

                XmlNode nodeSecondChildGroup = doc.CreateElement("SecondChildGroup");
                this.SecondChildGroup.Serialize(doc, nodeSecondChildGroup);
                nodeChildGroups.AppendChild(nodeSecondChildGroup);

                parentNode.AppendChild(nodeChildGroups);
            }
        }

        public void Deserialize(DockManager managerToAttach, XmlNode node, GetContentFromTypeString getObjectHandler)
        {
            this._dock = (Dock)Enum.Parse(typeof(Dock), node.Attributes["Dock"].Value);

            if (node.ChildNodes[0].Name == "DockablePane")
            {
                var pane = new DockablePane(managerToAttach);
                pane.Deserialize(managerToAttach, node.ChildNodes[0], getObjectHandler);
                this._attachedPane = pane;
            }
            else if (node.ChildNodes[0].Name == "DocumentsPane")
            {
                var pane = managerToAttach.GetDocumentsPane();
                pane.Deserialize(managerToAttach, node.ChildNodes[0], getObjectHandler);
                this._attachedPane = pane;
            }
            else
            {
                this._firstChildGroup = new DockablePaneGroup();
                this._firstChildGroup._parentGroup = this;
                this._firstChildGroup.Deserialize(managerToAttach, node.ChildNodes[0].ChildNodes[0], getObjectHandler);

                this._secondChildGroup = new DockablePaneGroup();
                this._secondChildGroup._parentGroup = this;
                this._secondChildGroup.Deserialize(managerToAttach, node.ChildNodes[0].ChildNodes[1], getObjectHandler);


            }
        }

        #endregion
    }
}
