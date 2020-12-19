using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
using TestAutomation.SolutionHandler.ProjectTypes;

namespace TestAutomation.SolutionHandler.Core
{
    public class ProjectReader
    {
        private string projectName;
        private static readonly List<Type> Types;

        public string ProjectName
        {
            get => this.projectName;
            set
            {
                if (!value.EndsWith(".csproj"))
                {
                    this.WasSuccessful = false;
                    this.ErrorMessage = "Expected a *.csproj file.";
                    return;
                }

                this.ErrorMessage = string.Empty;
                this.WasSuccessful = true;
                this.projectName = value;
            }
        }

        public CsProject CsProject { get; set; }
        
        public string ErrorMessage { get; set; }
        public bool WasSuccessful { get; set; }

        public ProjectReader(string projectFile)
        {
            this.ProjectName = projectFile;
        }

        static ProjectReader()
        {
            Types = new List<Type>();
            const string projectTypesNs = "TestAutomation.SolutionHandler.ProjectTypes";
            Types.AddRange(Assembly.GetExecutingAssembly().GetTypes()
                .Where(itm => itm.Namespace == projectTypesNs && !itm.Name.Contains("<>")));
        }

        public async Task AttemptFileLoad()
        {
            try
            {
                var settings = new XmlReaderSettings()
                {
                    DtdProcessing = DtdProcessing.Parse,
                    Async = true
                };
                var reader = XmlReader.Create(this.projectName, settings);

                var project = new CsProject(this.projectName);

                while (await reader.ReadAsync())
                {
                    if (reader.NodeType != XmlNodeType.Element) continue;

                    var elementName = GetTypeFromXml(reader, out var t);

                    object instance = null;
                    if (elementName != "Project")
                        instance = Instance(t);
                    if (instance == null) continue;
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
                            if (elementName == nameof(this.CsProject))
                                ProjectSpecificDeserialization(reader, project);
                            else
                                PopulateAttributes(t, reader, instance);
                        }

                        reader.MoveToElement();
                    }

                    if (elementName != "Project")
                    {
                        await PopulateProjectResources(instance, reader, project);
                    }
                }
                
                this.CsProject = project;
                var configItem = this.CsProject.Descendants().SingleOrDefault(itm =>
                    (itm?.Absoloute?.ToLower().EndsWith(ConfigurationReader.AppConfig) ?? false) ||
                    (itm?.Absoloute?.ToLower().EndsWith(ConfigurationReader.WebConfig) ?? false));
                var configPath =  Path.Combine(Path.GetDirectoryName(this.ProjectName), configItem.Absoloute);
                var cnfReader = new ConfigurationReader(configPath);
                cnfReader.AttemptFileLoad();
                this.CsProject.Configuration = cnfReader.Configuration;
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

        private static object Instance(Type t)
        {
            if (t == null) return null;

            var instance = Activator.CreateInstance(t);
            return instance;
        }

        private static string GetTypeFromXml(XmlReader reader, out Type t)
        {
            var elementName = reader.Name;
            t = Types.SingleOrDefault(item =>
                string.Equals(item.Name, elementName, StringComparison.CurrentCultureIgnoreCase));
            return elementName;
        }

        private static void DebugRootAttribs(XmlReader reader)
        {
#if DEBUG
            Console.BackgroundColor = ConsoleColor.DarkCyan;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("{0}={1}", reader.Name, reader.Value);
            Console.ResetColor();
            Console.Write("\t");
#endif
        }

        private static void PopulateAttributes(Type t, XmlReader reader, object instance)
        {
            var prop = t.GetProperties().SingleOrDefault(item =>
                item.Name.ToLower() == reader.Name.ToLower());
            if (prop != null)
                prop.SetValue(instance, reader.Value);
        }

        private static async Task PopulateProjectResources(object instance, XmlReader reader, CsProject csProject)
        {
            switch (instance)
            {
                case Import import:
                    csProject.Imports.Add(import);
                    break;
                case Target target:
                    var targetRes = await GetTargetData(target, reader.ReadSubtree());
                    csProject.Targets.Add(targetRes);
                    break;
                case ItemGroup itemGroup:
                    var igrpRes = await GetItemGroup(itemGroup, reader.ReadSubtree());
                    csProject.ItemGroups.Add(igrpRes);
                    break;
                case PropertyGroup propGroup:
                    var result = await GetExtraProperties(propGroup, reader.ReadSubtree());
                    if (result is PropertyGroup propertyGroup)
                        csProject.PropertyGroups.Add(propertyGroup);
                    break;
            }
        }

        private static async Task<Target> GetTargetData(Target target, XmlReader reader)
        {
            while (await reader.ReadAsync())
            {
                if (reader.NodeType != XmlNodeType.Element) continue;

                GetTypeFromXml(reader, out var t);
                var instance = Instance(t);

                ReadAttributes(reader, t, instance);

                switch (instance)
                {
                    case ItemGroup itemGroup:
                        target.ItemGroups.Add(await GetItemGroup(itemGroup, reader.ReadSubtree()));
                        break;
                    case PropertyGroup propGroup:
                        var result = await GetExtraProperties(propGroup, reader.ReadSubtree());
                        if (result is PropertyGroup propertyGroup)
                            target.PropertyGroups.Add(propertyGroup);
                        break;
                    case Error error:
                        target.Errors.Add(error);
                        break;
                }
            }

            return target;
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

        private static async Task<ItemGroup> GetItemGroup(ItemGroup itemGroup, XmlReader reader)
        {
            while (await reader.ReadAsync())
            {
                if (reader.NodeType != XmlNodeType.Element) continue;
                GetTypeFromXml(reader, out var t);
                var instance = Instance(t);

                ReadAttributes(reader, t, instance);

                if (!(instance is TargetObject target)) continue;
                if (target is Reference || target is Content)
                {
                    var upd = await GetExtraProperties(target, reader.ReadSubtree());
                    if (upd is TargetObject updatedTarget)
                        itemGroup.AddObject(updatedTarget);
                }
                else
                    itemGroup.AddObject(target);
            }

            return itemGroup;
        }

        private static async Task<object> GetExtraProperties(object target, XmlReader reader)
        {
            while (await reader.ReadAsync())
            {
                if (reader.NodeType != XmlNodeType.Element) continue;

                var elementName = GetTypeFromXml(reader, out var t);
                var property = target.GetType().GetProperties().SingleOrDefault(prop =>
                    string.Equals(prop.Name, elementName, StringComparison.CurrentCultureIgnoreCase));
                
                await ReadProperty(target, reader, t, property);
            }

            return target;
        }

        private static async Task ReadProperty(object target, XmlReader reader, Type t, PropertyInfo property)
        {
            if (t == null)
            {
                await ReadSimpleProperty(target, reader, property);
            }
            else if (property != null)
            {
                await ReadConcreteProperty(target, reader, t, property);
            }
        }

        private static async Task ReadConcreteProperty(object target, XmlReader reader, Type t, PropertyInfo property)
        {
            var instance = Instance(t);
            ReadAttributes(reader, t, instance);
            instance = await GetExtraProperties(instance, reader.ReadSubtree());
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

        private static void ProjectSpecificDeserialization(XmlReader reader, CsProject csProject)
        {
            var props = typeof(CsProject).GetProperties();
            var crr = props.FirstOrDefault(itm =>
                string.Equals(itm.Name, reader.Name, StringComparison.CurrentCultureIgnoreCase));
            if (crr != null)
            {
                crr.SetValue(csProject, reader.Value);
            }
        }
    }
}