namespace TestAutomation.SolutionHandler.ProjectTypes
{
    public static class ItemGroupExts
    {
        public static ItemGroup AddObject(this ItemGroup group, TargetObject @object)
        {
            group.Objects.Add(@object);
            return group;
        }
    }
}