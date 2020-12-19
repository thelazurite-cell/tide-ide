using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using FontAwesome.WPF;
using TAF.AutomationTool.Ui.Properties;
using Button = System.Windows.Controls.Button;
using MenuItem = System.Windows.Controls.MenuItem;

namespace TAF.AutomationTool.Ui.CustomElements
{
    public abstract class ClosableTabItem : TabItem, INotifyPropertyChanged
    {
        public EventHandler RequestReload;

        static ClosableTabItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ClosableTabItem),
                new FrameworkPropertyMetadata(typeof(ClosableTabItem)));
        }

        private bool isModified;

        public bool IsModified
        {
            get => this.isModified;
            protected set
            {
                if (value == this.isModified) return;
                this.UpdateVisual(value);
                this.isModified = value;
            }
        }
        
        public virtual string Name { get; }
        public virtual string Absoloute { get; }
        public virtual FontAwesomeIcon Icon { get; }

        private void UpdateVisual(bool value)
        {
            var template = this.Template;
            var ctrl = (TextBlock) template.FindName("PART_TAB_Heading", this);
            ctrl.FontWeight= value ? FontWeights.Bold : FontWeights.Normal;
        }

        public Point GetTabLocation(Visual ancestor)
        {
            var ctrl = (TextBlock) this.Template.FindName("PART_TAB_Heading", this);
            var rect = ctrl.TransformToAncestor(ancestor)
                .Transform(new Point(0, 0));
            Console.BackgroundColor = ConsoleColor.DarkMagenta;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"[TABPOS] {{ {rect.X} }} , {{ {rect.Y} }}");
            Console.ResetColor();

            return rect;
        }

        public static RoutedEvent CloseTabEvent { get; } = EventManager.RegisterRoutedEvent("CloseTab", RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(ClosableTabItem));

        public static RoutedEvent CloseOtherTabsEvent { get; } = EventManager.RegisterRoutedEvent("CloseOtherTabs", RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(ClosableTabItem));

        public event RoutedEventHandler CloseTab
        {
            add => this.AddHandler(CloseTabEvent, value);
            remove => this.RemoveHandler(CloseTabEvent, value);
        }
        public event RoutedEventHandler CloseOtherTabs
        {
            add => this.AddHandler(CloseOtherTabsEvent, value);
            remove => this.RemoveHandler(CloseOtherTabsEvent, value);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if(this.GetTemplateChild("PART_Close") is Button closeButton)
                closeButton.Click += (this.CloseButtonOnClick);

            if(this.GetTemplateChild("PART_CloseOthers") is MenuItem closeOthersButton)
                closeOthersButton.Click += this.CloseOthersButtonOnClick;
        }

        private void CloseOthersButtonOnClick(object sender, RoutedEventArgs e)
        {
            this.RaiseEvent(new RoutedEventArgs(CloseOtherTabsEvent, this));
        }

        private void CloseButtonOnClick(object sender, RoutedEventArgs e)
        {
            this.RaiseEvent(new RoutedEventArgs(CloseTabEvent, this));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public abstract Task<bool> SaveFile();
    }
}