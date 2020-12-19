using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interactivity;
using DataObject = System.Windows.DataObject;
using DragEventArgs = System.Windows.DragEventArgs;

namespace TAF.AutomationTool.Ui.CustomElements
{
    public class UiElementDropBehavior : Behavior<UIElement>
    {
        private AdornerManager adornerManager;
        public event EventHandler Dropped;

        protected override void OnAttached()
        {
            base.OnAttached();

            this.AssociatedObject.AllowDrop = true;
            this.AssociatedObject.DragEnter += this.AssociatedObject_DragEnter;
            this.AssociatedObject.DragOver += this.AssociatedObject_DragOver;
            this.AssociatedObject.DragLeave += this.AssociatedObject_DragLeave;
            this.AssociatedObject.Drop += this.AssociatedObject_Drop;
        }

        private void AssociatedObject_Drop(object sender, DragEventArgs e)
        {
            this.adornerManager?.Remove();

            if (e.Data is DataObject dto)
            {
                AdornerManagerList.ClearAll();
                var sourceElement = dto.GetData("Object") as UIElementCollection;
                var sourceContainer = dto.GetData("Source") as FrameworkElement;
                var location = this.AssociatedObject as FrameworkElement;
                if (Equals(sourceContainer, this.AssociatedObject))
                {
                    e.Handled = true;
                    return;
                }

                var elementMove = new UiElementMove
                {
                    SourceElements = sourceElement,
                    SourceContainerElement = sourceContainer,
                    LocationElement = location
                };
                
                this.Dropped?.Invoke(elementMove, EventArgs.Empty);
            }

            e.Handled = true;
        }

        private void AssociatedObject_DragLeave(object sender, DragEventArgs e)
        {
            if (this.adornerManager != null)
            {
                if (sender is IInputElement inputElement)
                {
                    var pt = e.GetPosition(inputElement);

                    if (sender is UIElement element)
                    {
                        this.adornerManager.Remove();
                    }
                }
            }

            e.Handled = true;
        }

        private void AssociatedObject_DragOver(object sender, DragEventArgs e)
        {
            if (Mouse.OverrideCursor == Cursors.No)
            {
                this.adornerManager?.Remove();
            }
            if (this.adornerManager != null && sender is UIElement element)
            {
                this.adornerManager.Update(element);
            }


            e.Handled = true;
        }

        private void AssociatedObject_DragEnter(object sender, DragEventArgs e)
        {
            if (this.adornerManager == null && sender is UIElement element)
            {
                this.adornerManager = new AdornerManager(
                    AdornerLayer.GetAdornerLayer(element),
                    adornedElement => new UiElementDropAdorner(adornedElement));
            }
            AdornerManagerList.AdornerManager.Add(this.adornerManager);

            e.Handled = true;
        }
    }
}