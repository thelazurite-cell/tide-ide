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
    internal class OverlayDockablePane : DockablePane
    {
        public readonly DockablePane ReferencedPane;
        public readonly DockableContent ReferencedContent;

        public OverlayDockablePane(DockManager dockManager, DockableContent content, Dock initialDock) : base(dockManager, initialDock)
        {
            this.btnAutoHide.LayoutTransform = new RotateTransform(90);
            this.ReferencedPane = content.ContainerPane as DockablePane;
            this.ReferencedContent = content;
            this.Add(this.ReferencedContent);
            this.Show(this.ReferencedContent);
            this.ReferencedContent.SetContainerPane(this.ReferencedPane);

            this.PaneState = PaneState.AutoHide;
        }

        public override void Show()
        {
            this.ChangeState(PaneState.Docked);
        }

        public override void Close()
        {
            this.ChangeState(PaneState.Hidden);
        }

        public override void Close(DockableContent content)
        {
            this.ChangeState(PaneState.Hidden);
        }
     }
}
