﻿using System;

namespace TAF.AutomationTool.Ui.Properties
{
    /// <summary>
    /// Indicates that parameter is regular expression pattern.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class RegexPatternAttribute : Attribute { }
}