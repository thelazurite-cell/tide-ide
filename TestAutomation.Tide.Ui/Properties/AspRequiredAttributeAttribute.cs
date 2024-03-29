﻿using System;

namespace TAF.AutomationTool.Ui.Properties
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class AspRequiredAttributeAttribute : Attribute
    {
        public AspRequiredAttributeAttribute([NotNull] string attribute)
        {
            this.Attribute = attribute;
        }

        [NotNull] public string Attribute { get; private set; }
    }
}