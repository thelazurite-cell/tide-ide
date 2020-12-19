using System;

namespace TestAutomation.SolutionHandler.Core
{
    /// <summary>
    /// The format attribute class - states how a value should be de/serialized. 
    /// </summary>
    public class FormatAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the accepted format of the current property
        /// </summary>
        public string Format { get; set; }
    }
}