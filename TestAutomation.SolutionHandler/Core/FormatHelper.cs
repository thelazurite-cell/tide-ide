using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace TestAutomation.SolutionHandler.Core
{
    /// <summary>
    /// Helper for Serializing/Deserializing complex types.
    /// </summary>
    public static class FormatHelper
    {
        /// <summary>
        /// Reads a formatter attribute and turns it into its equivalent regex.
        /// </summary>
        /// <param name="formattable"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static Regex FormatterAttributeToRegex(this ITideFormattable formattable, string propertyName)
        {
            var attribute = GetFormatterAttribute(formattable, propertyName);
            var format = attribute.Format;
            return ConvertToRegex(format);
        }

        /// <summary>
        /// Reads a formatter string and turns it into its equivalent regex.
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        public static Regex FormatterToRegex(string format) => ConvertToRegex(format);

        /// <summary>
        /// Performs the conversion of a formatter string to a regex. 
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        private static Regex ConvertToRegex(string val)
        {
            var parameter = new Regex("{[0-9]{1,2}}");
            val = val.Replace(".", @"\.").Replace("(", @"\(").Replace(")", @"\)");
            val = parameter.Replace(val, "(.+)");
            try
            {
                return new Regex(val);
            }
            catch (Exception e)
            {
                var innerExceptionMessage = e.InnerException?.Message ?? "No Inner Exception";
                Console.WriteLine($"Couldn't Create Regex Formatter...\n{e.Message}\n{innerExceptionMessage}");
                return null;
            }
        }

        /// <summary>
        /// Gets the variable groups from a regex. 
        /// </summary>
        /// <param name="line"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static string[] GetVariables(string line, Regex expression)
        {
            var match = expression.Match(line);
            var export = new List<string>();

            for (var i = 0; i < match.Groups.Count; i++)
            {
                export.Add(match.Groups[i].Value);
            }

            return export.ToArray();
        }

        /// <summary>
        /// Gets the formatter attribute so we can use its formatter string.
        /// </summary>
        /// <param name="formatter"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static FormatAttribute GetFormatterAttribute(this ITideFormattable formatter, string propertyName)
        {
            var asType = formatter.GetType();
            var requiredProperty = asType.GetProperty(propertyName);

            var attribute =
                requiredProperty?.GetCustomAttributes(typeof(FormatAttribute), false).SingleOrDefault();
            return (FormatAttribute) attribute;
        }
    }
}