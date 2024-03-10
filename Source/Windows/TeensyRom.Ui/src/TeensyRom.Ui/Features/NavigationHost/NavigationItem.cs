using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;

namespace TeensyRom.Ui.Features.NavigationHost
{
    public class NavigationItem : ReactiveObject
    {
        [Reactive] public object ViewModel { get; init; } = null!;
        [Reactive] public bool IsSelected { get; set; }
        public Guid Id { get; } = Guid.NewGuid();
        public string Name { get; init; } = string.Empty;
        public NavigationLocation Type { get; set; }
        public string Icon { get; init; } = string.Empty;
    }
}
