using System.Text;

namespace TestAutomation.SolutionHandler.Core
{
    /// <summary>
    /// The file data type, holds information on a file's content
    /// </summary>
    public class FileData
    {
        /// <summary>
        /// Gets or sets the file's encoding type
        /// </summary>
        public Encoding Encoding { get; set; }
        
        /// <summary>
        /// Gets or sets the file data
        /// </summary>
        public string Data { get; set; }
    }
}