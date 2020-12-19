namespace TAF.AutomationTool.Ui.ViewModels
{
    public class UiIndexFriendlyName
    {
        public int Index { get; set; }
        public string FriendlyName { get; set; }
        public string Absoloute { get; set; }

        public override string ToString() => this.FriendlyName;
    }
}