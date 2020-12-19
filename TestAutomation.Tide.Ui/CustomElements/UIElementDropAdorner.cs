using System.Globalization;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace TAF.AutomationTool.Ui.CustomElements
{
    public class UiElementDropAdorner : Adorner
    {
        public UiElementDropAdorner(UIElement adornedElement) :
            base(adornedElement)
        {
            this.Focusable = false;
            this.IsHitTestVisible = false;
        }

        private static SolidColorBrush ScbByColorOpacity(Color color, double opacity)
        {
            var solidColorBrush = new SolidColorBrush
            {
                Color = color,
                Opacity = opacity
            };
            solidColorBrush.Freeze();
            return solidColorBrush;
        }
        
        protected override void OnRender(DrawingContext drawingContext)
        {
            const int penWidth = 8;
            var adornedRect = new Rect(this.AdornedElement.RenderSize);

            var brsh = ScbByColorOpacity(Color.FromRgb(87,169,245),0.7);
            drawingContext.DrawRectangle(brsh,
                new Pen(Brushes.LightGray, penWidth),
                adornedRect);

            var typeface = new Typeface(
                new FontFamily("Segoe UI"),
                FontStyles.Normal,
                FontWeights.Normal, FontStretches.Normal);
            var formattedText = new FormattedText(
                "Drop Panel Here",
                CultureInfo.CurrentUICulture,
                FlowDirection.LeftToRight,
                typeface,
                24,
                Brushes.LightGray);

            var centre = new Point(
                this.AdornedElement.RenderSize.Width / 2,
                this.AdornedElement.RenderSize.Height / 2);

            var textLocation = new Point(
                centre.X - formattedText.WidthIncludingTrailingWhitespace / 2,
                centre.Y - formattedText.LineHeight / 2);

            drawingContext.DrawText(formattedText, textLocation);
        }
    }
}