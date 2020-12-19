using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
using TestAutomation.SolutionHandler.ProgramConfiguration;

namespace TestAutomation.SolutionHandler.Core
{
    public class ConfigurationReader
    {
        public static string AppConfig = "app.config";
        public static string WebConfig = "web.config";
        private static List<Type> Types;

        private string appConfigLocation;

        public string ErrorMessage { get; set; }

        public string AppConfigLocation
        {
            get => this.appConfigLocation;
            set
            {
                var tolower = value.ToLower();
                if (!tolower.EndsWith(AppConfig) || !tolower.EndsWith(WebConfig))
                {
                    this.ErrorMessage = $"Did not match the expected name of {AppConfig} or {WebConfig}";
                    this.WasSuccessful = false;
                    return;
                }

                this.appConfigLocation = value;
                this.WasSuccessful = true;
                this.ErrorMessage = string.Empty;
            }
        }

        public bool WasSuccessful { get; set; }

        public string FileName { get; }

        public ConfigurationReader(string appConfigLocation)
        {
            this.appConfigLocation = appConfigLocation;
        }

        static ConfigurationReader()
        {
            Types = new List<Type>();
            const string projectTypesNs = "TestAutomation.SolutionHandler.ProgramConfiguration";
            Types.AddRange(Assembly.GetExecutingAssembly().GetTypes()
                .Where(itm => itm.Namespace == projectTypesNs && !itm.Name.Contains("<>")));
        }

        public void AttemptFileLoad()
        {
            try
            {
                var settings = new XmlReaderSettings()
                {
                    DtdProcessing = DtdProcessing.Parse,
                    Async = true
                };
                var reader = XmlReader.Create(this.AppConfigLocation, settings);

                this.Configuration = new Configuration();
                var currentParent = string.Empty;
                while (reader.Read())
                {
                    var elementName = GetTypeFromXml(this.Configuration, reader, out var t);

                    object instance = null;
                    if (elementName != "configuration")
                        instance = Instance(t);
                    if (instance == null) continue;
                    currentParent = elementName;
#if DEBUG
                    Console.WriteLine("\n\n<{0}>", elementName);
#endif
                    if (reader.HasAttributes)
                    {
#if DEBUG
                        Console.Write(": HasAttributes - {0}\t", reader.AttributeCount);
#endif
                        while (reader.MoveToNextAttribute())
                        {
                            DebugRootAttribs(reader);
                            PopulateAttributes(t, reader, instance);
                        }

                        reader.MoveToElement();
                    }

                    if (reader.NodeType == XmlNodeType.Element)
                        this.IterateSubTree(reader, currentParent, instance);
                    var prop = GetFromFriendlyName(this.Configuration, ref t, elementName);
                    
                    prop.SetValue(this.Configuration, instance);
                }
            }
            catch (Exception e)
            {
#if DEBUG
                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(e);
                Console.ResetColor();
#endif
                throw;
            }
        }

        private void IterateSubTree(XmlReader reader, string currentParent, object instance)
        {
            var subtree = reader.ReadSubtree();
            var lastElement = string.Empty;
            while (subtree.Read())
            {
                if (subtree.NodeType == XmlNodeType.Element && subtree.Name != currentParent)
                {
                var subElementName = GetTypeFromXml(this.Configuration, reader, out var subType, instance);

                var subInstance = Instance(subType);
                    Console.WriteLine("\n\n<{0}>", subtree.Name);

                if (subtree.HasAttributes)
                {
#if DEBUG
                    Console.Write(": HasAttributes - {0}\t", subtree.AttributeCount);
#endif
                    while (subtree.MoveToNextAttribute())
                    {
                        DebugRootAttribs(subtree);
                        PopulateAttributes(subType, subtree, subInstance);
                    }

                    subtree.MoveToElement();
                }

                var instanceIsCollectionOfSubType =
                    (instance?.GetType().IsGenericType ?? false) && (instance?.GetType().GetGenericArguments()
                        .Any(x => x == subType) ?? false);

                var instanceHasSinglePropOfSubType = instance?.GetType().GetProperties()
                    .FirstOrDefault(itm => itm.PropertyType == subType);

               
                    this.IterateSubTree(subtree, subtree.Name, subInstance);

                    if (instanceIsCollectionOfSubType)
                    {
                        instance.GetType().GetMethod("Add")?.Invoke(instance, new[] {subInstance});
                    }
                    else if (instanceHasSinglePropOfSubType != null)
                    {
                        instanceHasSinglePropOfSubType.SetValue(instance, subInstance);
                    }
                }
            }

            while (reader.Name != currentParent && reader.NodeType != XmlNodeType.EndElement)
            {
                reader.Skip();
            }
        }

        private static object Instance(Type t)
        {
            if (t == null) return null;

            var instance = Activator.CreateInstance(t);
            return instance;
        }

        private static string GetTypeFromXml(Configuration configuration, XmlReader reader, out Type t, object instance = null)
        {
            var elementName = reader.Name;
            GetFromTypeName(out t, elementName);
            if (t != null) return elementName;
            GetFromFriendlyName(configuration, ref t, elementName);
            if (t == null && instance != null)
            GetFromFriendlyName(instance, ref t, elementName);
            if (t == null && instance != null)
            {
                var fna = instance.GetType().GetCustomAttribute<FriendlyNameAttribute>();
                if (fna?.FriendlyName == elementName)
                {
                    t = instance.GetType();
                }
            }

            return elementName;
        }

        private static void GetFromTypeName(out Type t, string elementName)
        {
            t = Types.SingleOrDefault(item =>
                string.Equals(item.Name, elementName, StringComparison.CurrentCultureIgnoreCase));
        }

        private static PropertyInfo GetFromFriendlyName(object item, ref Type t, string elementName)
        {
            foreach (var propertyInfo in item.GetType().GetProperties())
            {
                var fna = propertyInfo.GetCustomAttributes<FriendlyNameAttribute>().FirstOrDefault(itm => itm.FriendlyName == elementName);
                if (fna == null) continue;

                t = propertyInfo.PropertyType;
                return propertyInfo;
            }

            return null;
        }

        private static void PopulateAttributes(Type t, XmlReader reader, object instance)
        {
            var prop = t.GetProperties().SingleOrDefault(item =>
                item.Name.ToLower() == reader.Name.ToLower());
            if (prop != null)
                prop.SetValue(instance, reader.Value);
        }

        private static void DebugRootAttribs(XmlReader reader)
        {
#if DEBUG
            Console.BackgroundColor = ConsoleColor.DarkMagenta;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("{0}={1}", reader.Name, reader.Value);
            Console.ResetColor();
            Console.Write("\t");
#endif
        }

        private async Task<object> GetExtraProperties(object target, XmlReader reader)
        {
            while (await reader.ReadAsync())
            {
                if (reader.NodeType != XmlNodeType.Element) continue;

                var elementName = GetTypeFromXml(this.Configuration, reader, out var t);
                var property = target.GetType().GetProperties().SingleOrDefault(prop =>
                    string.Equals(prop.Name, elementName, StringComparison.CurrentCultureIgnoreCase));

                await this.ReadProperty(target, reader, t, property);
            }

            return target;
        }

        private async Task ReadProperty(object target, XmlReader reader, Type t, PropertyInfo property)
        {
            if (t == null)
            {
                await ReadSimpleProperty(target, reader, property);
            }
            else if (property != null)
            {
                await this.ReadConcreteProperty(target, reader, t, property);
            }
        }

        private async Task ReadConcreteProperty(object target, XmlReader reader, Type t, PropertyInfo property)
        {
            var instance = Instance(t);
            ReadAttributes(reader, t, instance);
            instance = await this.GetExtraProperties(instance, reader.ReadSubtree());
            property.SetValue(target, instance);
        }

        private static async Task ReadSimpleProperty(object target, XmlReader reader, PropertyInfo property)
        {
            if (property?.PropertyType != typeof(string) && property?.PropertyType != typeof(bool) &&
                property?.PropertyType != typeof(int)) return;
            var propString = reader.ReadSubtree();
            while (await propString.ReadAsync())
            {
                if (propString.NodeType != XmlNodeType.Text) continue;
                property.SetValue(target, propString.Value);
                break;
            }
        }

        private static void ReadAttributes(XmlReader reader, Type t, object instance)
        {
            if (!reader.HasAttributes) return;
            while (reader.MoveToNextAttribute())
            {
                DebugRootAttribs(reader);
                PopulateAttributes(t, reader, instance);
            }

            reader.MoveToElement();
        }

        public Configuration Configuration { get; set; }
    }
}