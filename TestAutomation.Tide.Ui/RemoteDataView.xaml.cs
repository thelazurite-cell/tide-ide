using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TAF.AutomationTool.Ui.Activities;

namespace TAF.AutomationTool.Ui
{
    /// <summary>
    /// Interaction logic for RemoteDataView.xaml
    /// </summary>
    public partial class RemoteDataView 
    {
        private readonly ProjectEventManager eventBindings;

        public RemoteDataView(ProjectEventManager eventBindings)
        {
            this.eventBindings = eventBindings;
            this.InitializeComponent();
            this.SqlRemoteData.SetEventBindings(this.eventBindings);
        }
    }
}
