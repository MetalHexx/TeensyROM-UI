namespace TeensyRom.Core.Storage.Entities
{
    public interface ILaunchableItem : IFileItem { }
    public interface IViewableItem : IFileItem 
    {
        List<ViewableItemImage> Images { get; }
        
    }
}
