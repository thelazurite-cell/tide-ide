using System;

namespace TAF.AutomationTool.Ui.Properties
{
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public sealed class AspMvcViewLocationFormatAttribute : Attribute
    {
        public AspMvcViewLocationFormatAttribute([NotNull] string format)
        {
            this.Format = format;
        }

        [NotNull] public string Format { get; private set; }
    }
}