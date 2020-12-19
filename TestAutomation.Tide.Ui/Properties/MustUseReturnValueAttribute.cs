using System;

namespace TAF.AutomationTool.Ui.Properties
{
    /// <summary>
    /// Indicates that the return value of method invocation must be used.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class MustUseReturnValueAttribute : Attribute
    {
        public MustUseReturnValueAttribute() { }

        public MustUseReturnValueAttribute([NotNull] string justification)
        {
            this.Justification = justification;
        }

        [CanBeNull] public string Justification { get; private set; }
    }
}