using System;

namespace TeensyRom.Ui.Features.NavigationHost
{
    public class NavigationItem
    {
        public object ViewModel { get; set; }
        public Guid Id { get; } = Guid.NewGuid();
        public string Name { get; set; }
        public NavigationLocation Type { get; set; }
        public string Icon { get; set; }

    }
}
