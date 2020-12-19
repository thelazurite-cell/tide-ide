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

namespace DockingLibrary
{
    /// <summary>
    /// Rappresents a content embeddable in a dockable pane or in a documents pane
    /// </summary>
    public class DockableContent : ManagedContent
    {
        public DockableContent(DockManager manager) : base(manager)
        {
        }

        public DockableContent() { }

        

        /// <summary>
        /// Show this content
        /// </summary>
        /// <remarks>Show this content in a dockable pane. If no pane was previuosly created, it creates a new one with default right dock. </remarks>
        public override void Show()
        {
            if (this.ContainerPane != null)
            {
                this.ContainerPane.Show(this);
                //ContainerPane.Show();
            }
            else
                this.Show(Dock.Right);
        }

        /// <summary>
        /// Show this content
        /// </summary>
        /// <remarks>Show this content in a dockable pane. If no pane was previuosly created, it creates a new one with passed initial dock. </remarks>
        public void Show(Dock dock)
        {
            if (this.ContainerPane == null)
            {
                this._containerPane = new DockablePane(this.DockManager, dock);
                //_containerPane.DockManager = DockManager;
                this._containerPane.Add(this);
                this._containerPane.Show();
                //DockManager.Add(this);
                this.DockManager.Add(this._containerPane as DockablePane);
            }
            else
            {
                this.ContainerPane.Show(this);
                this.ContainerPane.Show();
            }
        }

        /// <summary>
        /// Show content into default documents pane
        /// </summary>
        public void ShowAsDocument()
        {
            if (this.ContainerPane == null) this._containerPane = this.DockManager.AddDocument(this);

            this.ContainerPane.Show(this);
        }

        /// <summary>
        /// Hides content from container pane
        /// </summary>
        /// <remarks>If container pane doesn't contain any more content, it is automaticly hidden.</remarks>
        public virtual new void Hide()
        {
            this.ContainerPane.Hide(this);
        }


        public virtual void ChangeDock(Dock dock)
        { 
        
        }

        public virtual void Float()
        { 
        
        }

        public virtual void AutoHide()
        { 
        
        }

        /// <summary>
        /// Set/get content title whish is shown at top of dockable panes and in tab items
        /// </summary>
        public new string Title
        {
            get { return base.Title; }
            set 
            { 
                base.Title = value;

                if (this.ContainerPane != null) this.ContainerPane.RefreshTitle();
            }
        }
    }
}
