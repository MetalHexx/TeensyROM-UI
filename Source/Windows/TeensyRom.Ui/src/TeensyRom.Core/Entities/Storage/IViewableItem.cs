namespace TeensyRom.Core.Entities.Storage
{
    public interface IViewableItem : IFileItem
    {
        List<ViewableItemImage> Images { get; }

    }
}
