using System.Collections.Generic;
using System;
using System.Reactive;

namespace TeensyRom.Ui.Features.NavigationHost
{
    public interface INavigationService
    {
        IObservable<List<NavigationItem>> NavigationItems { get; }
        IObservable<NavigationItem> SelectedNavigationView { get; }
        Unit NavigateTo(Guid locationId);
        void Initialize(NavigationLocation startingLocation, List<NavigationItem> navItems);
        Unit NavigateTo(NavigationLocation location);
        Unit Enable(NavigationLocation location);
    }
}
