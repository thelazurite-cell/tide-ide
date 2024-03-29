﻿using System;

namespace TAF.AutomationTool.Ui.Properties
{
    /// <summary>
    /// Indicates that a parameter is a path to a file or a folder within a web project.
    /// Path can be relative or absolute, starting from web root (~).
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class PathReferenceAttribute : Attribute
    {
        public PathReferenceAttribute() { }

        public PathReferenceAttribute([NotNull, PathReference] string basePath)
        {
            this.BasePath = basePath;
        }

        [CanBeNull] public string BasePath { get; private set; }
    }
}