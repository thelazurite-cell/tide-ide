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
    /// Interaction logic for DocumentsPane.xaml
    /// </summary>

    partial class DocumentsPane : Pane
    {
        public readonly List<ManagedContent> Documents = new List<ManagedContent>();

        public DocumentsPane(DockManager dockManager) : base(dockManager)
        {
            this.InitializeComponent();
        }

        public DocumentContent ActiveDocument
        {
            get
            {
                if (this.tbcDocuments.SelectedContent == null)
                    return null;

                return this.Documents[this.tbcDocuments.SelectedIndex] as DocumentContent;
            }
        }

        //public override DockManager DockManager
        //{
        //    get
        //    {
        //        return base.DockManager;
        //    }
        //    set
        //    {
        //        base.DockManager = value;

        //        Show();
        //    }
        //}

        public new ManagedContent ActiveContent
        {
            get
            {
                if (this.tbcDocuments.SelectedContent == null)
                    return null;
                return this.Documents[this.tbcDocuments.SelectedIndex];
            }
            set
            {
                if (this.Documents.Count > 1)
                {
                    this.tbcDocuments.SelectedIndex = this.Documents.IndexOf(value);
                }
            }
        }

        //void OnUnloaded(object sender, EventArgs e)
        //{
        //    foreach (ManagedContent content in Documents)
        //        content.Close();

            
        //    Documents.Clear();
        //}

        

        public override void Add(DockableContent content)
        {
            System.Diagnostics.Debug.Assert(content != null);
            if (content == null)
                return;


            this.Documents.Add(content);
            this.AddItem(content);

            base.Add(content);
        }

        public void Add(DocumentContent content)
        {
            System.Diagnostics.Debug.Assert(content != null);
            if (content == null)
                return;

            this.Documents.Add(content);
            this.AddItem(content);
        }

        public override void Remove(DockableContent content)
        {
            System.Diagnostics.Debug.Assert(content != null);
            if (content == null)
                return;

            this.RemoveItem(content);
            this.Documents.Remove(content);
            this.DockManager.Remove(content);

            base.Remove(content);
        }

        public void Remove(DocumentContent content)
        {
            System.Diagnostics.Debug.Assert(content != null);
            if (content == null)
                return;

            this.RemoveItem(content);
            this.Documents.Remove(content);
            content.Close();
        }

        public override void Show(DockableContent content)
        {
            if (!this.Contents.Contains(content)) this.Add(content);

            base.Show(content);
        }


        public override void RefreshTitle()
        {
            var tabItem = this.tbcDocuments.Items[this.Documents.IndexOf(this.ActiveContent)] as TabItem;
            tabItem.Header = this.ActiveContent.Title;

            base.RefreshTitle();
        }

        protected virtual void AddItem(ManagedContent content)
        {
            var item = new TabItem();
            var tabPanel = new DockPanel();
            item.Header = content.Title;
            item.Content = new ContentPresenter();
            (item.Content as ContentPresenter).Content = content.Content;
            item.Style = this.FindResource("DocTabItemStyle") as Style;

            this.tbcDocuments.Items.Add(item);
            this.tbcDocuments.SelectedItem = item;

            if (this.tbcDocuments.Items.Count == 1) this.tbcDocuments.Visibility = Visibility.Visible;

        }

        protected virtual void RemoveItem(ManagedContent content)
        {
            var tabItem = this.tbcDocuments.Items[this.Documents.IndexOf(content)] as TabItem;

            tabItem.Content = null;

            this.tbcDocuments.Items.Remove(tabItem);

            if (this.tbcDocuments.Items.Count == 0) this.tbcDocuments.Visibility = Visibility.Collapsed;

        }

        #region Drag inner contents

        private Point ptStartDrag;

        private void OnTabItemMouseDown(object sender, MouseButtonEventArgs e)
        {
            var senderElement = sender as FrameworkElement;
            var item = senderElement.TemplatedParent as TabItem;
            if (!senderElement.IsMouseCaptured)
            {
                this.ptStartDrag = e.GetPosition(this);
                senderElement.CaptureMouse();
            }
            
        }

        private void OnTabItemMouseMove(object sender, MouseEventArgs e)
        {
            var senderElement = sender as FrameworkElement;
            var item = senderElement.TemplatedParent as TabItem;
            if (senderElement.IsMouseCaptured && Math.Abs(this.ptStartDrag.X - e.GetPosition(this).X) > 4)
            {
                senderElement.ReleaseMouseCapture();
                var index = this.tbcDocuments.Items.IndexOf(item);
                DockableContent contentToDrag = null;
                //foreach (DockableContent content in Contents)
                //    if (content.Content == (item.Content as ContentPresenter).Content)
                //        contentToDrag = content;

                contentToDrag = this.Documents[index] as DockableContent;
                
                if (contentToDrag != null) this.DragContent(contentToDrag, e.GetPosition(this.DockManager), e.GetPosition(item));
                
                e.Handled = true;
            }
        }

        private void OnTabItemMouseUp(object sender, MouseButtonEventArgs e)
        {
            var senderElement = sender as FrameworkElement;
            var item = senderElement.TemplatedParent as TabItem;
            senderElement.ReleaseMouseCapture();

        }
        #endregion

        #region TabControl commands (switch menu / close current content)

        private void OnDocumentSwitch(object sender, EventArgs e)
        {
            var index = this.cxDocumentSwitchMenu.Items.IndexOf((sender as MenuItem));

            var tabItem = this.tbcDocuments.Items[index] as TabItem;
            this.tbcDocuments.Items.RemoveAt(index);
            this.tbcDocuments.Items.Insert(0, tabItem);
            this.tbcDocuments.SelectedIndex = 0;

            var contentToSwap = this.Documents[index];
            this.Documents.RemoveAt(index);
            this.Documents.Insert(0, contentToSwap);

            foreach(MenuItem item in this.cxDocumentSwitchMenu.Items)
                item.Click -= this.OnDocumentSwitch;
        }

        private ContextMenu cxDocumentSwitchMenu;

        private void OnBtnDocumentsMenu(object sender, MouseButtonEventArgs e)
        {
            if (this.Documents.Count <= 1)
                return;

            this.cxDocumentSwitchMenu = new ContextMenu();

            foreach (var content in this.Documents)
            {
                var item = new MenuItem();
                var imgIncon = new Image();
                imgIncon.Source = content.Icon;
                item.Icon = imgIncon;
                item.Header = content.Title;
                item.Click += this.OnDocumentSwitch;
                this.cxDocumentSwitchMenu.Items.Add(item);
            }

            this.cxDocumentSwitchMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.AbsolutePoint;
            this.cxDocumentSwitchMenu.PlacementRectangle = new Rect(this.PointToScreen(e.GetPosition(this)), new Size(0, 0));
            this.cxDocumentSwitchMenu.PlacementTarget = this;
            this.cxDocumentSwitchMenu.IsOpen = true;
        }

        private void OnBtnDocumentClose(object sender, MouseButtonEventArgs e)
        {
            if (this.ActiveContent == null)
                return;

            if (this.ActiveContent is DockableContent)
                this.Remove(this.ActiveContent as DockableContent);
            else
                this.Remove(this.ActiveDocument);
        }

        //void OnShowDocumentsMenu(object sender, DependencyPropertyChangedEventArgs e)
        //{ 
            
        //}
        #endregion
    }
}