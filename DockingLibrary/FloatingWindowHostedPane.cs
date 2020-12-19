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
    internal class FloatingWindowHostedPane : DockablePane
    {
        public readonly DockablePane ReferencedPane;
        private FloatingWindow _floatingWindow;

        public FloatingWindowHostedPane(FloatingWindow floatingWindow, DockablePane referencedPane) : base(referencedPane.DockManager)
        {
            this.ReferencedPane = referencedPane;
            this._floatingWindow = floatingWindow;

            var lastSelectedContent = this.ReferencedPane.ActiveContent;

            this.ChangeState(this.ReferencedPane.State);
            //ReferencedPane.Hide();

            //DockManager = ReferencedPane.DockManager;
            foreach (var content in this.ReferencedPane.Contents)
            {
                this.ReferencedPane.Hide(content);
                this.Add(content);
                this.Show(content);
                content.SetContainerPane(this.ReferencedPane);
            }

            this.ActiveContent = lastSelectedContent;
            this.ShowHeader = false;

        }

        

        public override void Remove(DockableContent content)
        {
            this.ReferencedPane.Remove(content);
            base.Remove(content);
        }

        protected override void OnDockingMenu(object sender, EventArgs e)
        {
            if (sender == this.menuFloatingWindow)
            {
                this.ReferencedPane.ChangeState(PaneState.FloatingWindow);
                this.ChangeState(this.ReferencedPane.State);
            }

            if (sender == this.menuDockedWindow)
            {
                this.ReferencedPane.ChangeState(PaneState.DockableWindow);
                this.ChangeState(this.ReferencedPane.State);
            }

            if (sender == this.menuTabbedDocument || sender == this.menuClose || sender == this.menuAutoHide)
            {
                foreach (var content in this.Contents)
                    content.SetContainerPane(this.ReferencedPane);

                this.Close();

                this._floatingWindow.Close();
            }

            if (sender == this.menuTabbedDocument) this.ReferencedPane.TabbedDocument();
            if (sender == this.menuClose) this.ReferencedPane.Close();
            if (sender == this.menuAutoHide)
            {
                this.ReferencedPane.Show();
                this.ReferencedPane.AutoHide();
            }
        }


        //protected override void DragContent(DockableContent contentToDrag, Point startDragPoint, Point offset)
        //{
        //    Remove(contentToDrag);
        //    DockablePane pane = new DockablePane();
        //    pane = new DockablePane();
        //    pane.DockManager = DockManager;
        //    pane.Add(contentToDrag);
        //    pane.Show();
        //    DockManager.Add(pane);
        //    //DockManager.Add(contentToDrag);
        //    //pane.ChangeState(PaneState.DockableWindow);
        //    FloatingWindow wnd = new FloatingWindow(pane);
        //    pane.ChangeState(PaneState.DockableWindow);
        //    DockManager.Drag(wnd, startDragPoint, offset);            
        //}
    }
}
