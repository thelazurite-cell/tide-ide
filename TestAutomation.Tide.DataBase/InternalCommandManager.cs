using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using JsonFx.Json;

namespace TestAutomation.Tide.DataBase
{
    /// <summary>
    /// The Internal Command Manager class. Used by application commands.
    /// </summary>
    public static class InternalCommandManager
    {
        private static readonly Dictionary<string, string> InternalCommands;

        /// <summary>
        /// Initializes the internal command manager 
        /// </summary>
        static InternalCommandManager()
        {
            const string location = "internalCommands.json";
            var assembly = Assembly.GetExecutingAssembly();
            var rp = TryGetResourcePath(assembly, location);

            var contents = TryReadInternalResource(assembly, rp);
            
            var jsonReader = new JsonReader();
            InternalCommands = jsonReader.Read<Dictionary<string, string>>(contents);
        }

        private static string TryGetResourcePath(Assembly assembly, string location)
        {
            var rp = assembly.GetManifestResourceNames().SingleOrDefault(itm => itm.EndsWith(location));
            if (rp == null) throw new InvalidOperationException("Couldn't find a required internal resource.");
            return rp;
        }

        private static string TryReadInternalResource(Assembly assembly, string rp)
        {
            string contents;
            using (var stream = assembly.GetManifestResourceStream(rp))
            {
                if (stream == null)
                    throw new InvalidOperationException("Failed to open a required internal resource");
                using (var reader = new StreamReader(stream))
                {
                    contents = reader.ReadToEnd();
                }
            }
            if (string.IsNullOrWhiteSpace(contents))
                throw new InvalidOperationException("The internal resource did not contain any data");
            return contents;
        }

        /// <summary>
        /// Gets a command used internally by the application.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetInternalCommand(string key)
        {
            return InternalCommands.ContainsKey(key) ? InternalCommands[key] : string.Empty;
        }
    }
}
