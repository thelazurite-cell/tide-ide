namespace TestAutomation.SolutionHandler.Core
{
    public interface INavigableItem
    {
        bool IsExpanded { get; set; }
        bool IsSelected { get; set; }
    }
}