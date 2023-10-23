using TeensyRom.Ui.Helpers;

namespace TeensyRom.Ui.Features.FileTransfer
{
    /// <summary>
    /// Base class for file / directory nodes
    /// </summary>
    public class NodeBase : BindableBase
    {
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
    }
}
