using System;
using System.Windows;
using System.Windows.Documents;

namespace TAF.AutomationTool.Ui.CustomElements
{
    public class AdornerManager
    {
        private readonly AdornerLayer adornerLayer;
        private readonly Func<UIElement, Adorner> adornerFactory;

        private Adorner adorner;

        public AdornerManager(
                  AdornerLayer adornerLayer,
                  Func<UIElement, Adorner> adornerFactory)
        {
            this.adornerLayer = adornerLayer;
            this.adornerFactory = adornerFactory;
        }

        public void Update(UIElement adornedElement)
        {
            if (this.adorner == null || !this.adorner.AdornedElement.Equals(adornedElement))
            {
                this.Remove();
                this.adorner = this.adornerFactory(adornedElement);
                this.adornerLayer.Add(this.adorner);
                this.adornerLayer.Update(adornedElement);
                this.adorner.Visibility = Visibility.Visible;
            }
        }

        public void Remove()
        {
            if (this.adorner != null)
            {
                this.adorner.Visibility = Visibility.Collapsed;
                this.adornerLayer.Remove(this.adorner);
                this.adorner = null;
            }
        }
    }
}
