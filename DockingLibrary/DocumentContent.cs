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
    public class DocumentContent : ManagedContent
    {
        public DocumentContent(DockManager manager) : base(manager)
        {
        }

        public DocumentContent() { }

        public override void Show()
        {
            this.DockManager.AddDocument(this);
        }
    }
}
