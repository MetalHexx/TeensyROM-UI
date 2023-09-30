using System.Collections.Generic;
using System;
using System.Reactive;

namespace TeensyRom.Ui.Features.NavigationHost
{
    public interface INavigationService
    {
        IObservable<List<NavigationItem>> NavigationItems { get; }
        IObservable<NavigationItem> SelectedNavigationView { get; }
        IObservable<bool> IsNavOpen { get; }
        Unit NavigateTo(Guid locationId);
        Unit ToggleNav();
        void Initialize(NavigationLocation startingLocation, List<NavigationItem> navItems);
    }
}
