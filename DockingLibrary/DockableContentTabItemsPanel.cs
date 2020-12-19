using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DockingLibrary
{
    internal class DockableContentTabItemsPanel : Panel
    {
        private double totChildWidth;

        protected override Size MeasureOverride(Size availableSize)
        {
            var childSize = availableSize;
            var totalDesideredSize = new Size(availableSize.Width, 0.0);
            this.totChildWidth = 0.0;
            foreach (UIElement child in this.InternalChildren)
            {
                child.Measure(childSize);

                this.totChildWidth += child.DesiredSize.Width;
                if (totalDesideredSize.Height < child.DesiredSize.Height)
                    totalDesideredSize.Height = child.DesiredSize.Height;
            }

            return totalDesideredSize;
        }


        protected override Size ArrangeOverride(Size finalSize)
        {
            var inflate = new Size();

            if (finalSize.Width < this.totChildWidth)
                inflate.Width = (this.totChildWidth - finalSize.Width) / this.InternalChildren.Count;
  
            var offset = new Point();
            if (finalSize.Width > this.totChildWidth)
                offset.X = -(finalSize.Width - this.totChildWidth)/2;

            var totalFinalWidth = 0.0;
            foreach (UIElement child in this.InternalChildren)
            {
                var childFinalSize = child.DesiredSize;
                childFinalSize.Width -= inflate.Width;
                childFinalSize.Height = finalSize.Height;

                child.Arrange(new Rect(offset, childFinalSize));

                offset.Offset(childFinalSize.Width, 0);
                totalFinalWidth += childFinalSize.Width;
            }

            return new Size(totalFinalWidth, finalSize.Height);
        }



    }
}
