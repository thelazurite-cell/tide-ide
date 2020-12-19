using System;

namespace TestAutomation.SolutionHandler.Core
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class FriendlyNameAttribute : Attribute
    {
        public string FriendlyName { get; set; }
    }
}