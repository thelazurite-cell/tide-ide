using System;

namespace TAF.AutomationTool.Ui.Properties
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class HtmlElementAttributesAttribute : Attribute
    {
        public HtmlElementAttributesAttribute() { }

        public HtmlElementAttributesAttribute([NotNull] string name)
        {
            this.Name = name;
        }

        [CanBeNull] public string Name { get; private set; }
    }
}