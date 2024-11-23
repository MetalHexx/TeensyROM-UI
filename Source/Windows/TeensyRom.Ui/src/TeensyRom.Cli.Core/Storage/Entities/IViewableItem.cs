namespace TeensyRom.Cli.Core.Storage.Entities
{
    public interface IViewableItem : IFileItem 
    {
        List<ViewableItemImage> Images { get; }
        
    }
}
