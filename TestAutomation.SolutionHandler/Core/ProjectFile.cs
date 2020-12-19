using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.AvalonEdit.Document;

namespace TestAutomation.SolutionHandler.Core
{
    public class ProjectFile : ProjectNavigationItem
    {

        public override string ToString() => this.Name;

        private Encoding _encoding;

        /// <summary>
        /// Creates a new instance of a <see cref="ProjectFile"/>
        /// </summary>
        public ProjectFile()
        {
            this.Type = NavigationItemType.File;
        }

        /// <summary>
        /// Save the current file. 
        /// </summary>
        /// <param name="rootDir"></param>
        /// <param name="content"></param>
        /// <param name="Encoding"></param>
        /// <returns></returns>
        public async Task<Exception> SaveFile(string rootDir, TextDocument content, Encoding Encoding)
        {
            try
            {
                await this.WriteToFile(rootDir, content, Encoding);
                return null;
            }
            catch (Exception e)
            {
                return e;
            }
        }

        public async Task SaveProject()
        {
            return;
        }

        private async Task WriteToFile(string rootDir, TextDocument content, Encoding encoding)
        {
            var lines = content.Lines.Where(itm => !itm.IsDeleted).ToList();
            var txt = lines.Aggregate(string.Empty, (current, line) =>
            {
                var shouldPopulateNewLine = line.LineNumber != lines.Count;
                var newLine = shouldPopulateNewLine ? Environment.NewLine : string.Empty;
                return current + $"{content.GetText(line.Offset, line.Length)}{newLine}";
            });
            using (var fs = new FileStream(Path.Combine(rootDir, this.Absoloute), FileMode.Create,
                FileAccess.Write, FileShare.None))
            {    
                var writer = encoding != null ? new StreamWriter(fs, encoding) : new StreamWriter(fs);
                writer.Write(txt);
                writer.Flush();
            }
        }
    }
}