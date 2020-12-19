using System.Threading.Tasks;

namespace TAF.AutomationTool.Ui.CustomElements
{
    public partial class AddNewItem : ClosableTabItem
    {
        public AddNewItem()
        {
            this.InitializeComponent();
        }

        public async override Task<bool> SaveFile()
        {
            return true;
        }
    }
}
