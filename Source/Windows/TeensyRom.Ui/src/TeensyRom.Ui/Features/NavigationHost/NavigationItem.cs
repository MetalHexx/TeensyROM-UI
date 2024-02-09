using System;

namespace TeensyRom.Ui.Features.NavigationHost
{
    public class NavigationItem
    {
        public object ViewModel { get; init; } = null!;
        public Guid Id { get; } = Guid.NewGuid();
        public string Name { get; init; } = string.Empty;
        public NavigationLocation Type { get; set; }
        public string Icon { get; init; } = string.Empty;

    }
}
