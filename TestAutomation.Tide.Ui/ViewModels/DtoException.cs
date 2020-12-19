using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestAutomation.Tide.DataBase;

namespace TAF.AutomationTool.Ui.ViewModels
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
