using System;

namespace TestAutomation.Tide.DataBase
{
    public class DtoException
    {
        /// <summary>
        /// Gets or sets the transfer object that had an issue.
        /// </summary>
        public DataTransferObject DataTransferObject { get; set; }

        /// <summary>
        /// Gets or sets the exception details.
        /// </summary>
        public Exception Exception { get; set; }
    }
}
