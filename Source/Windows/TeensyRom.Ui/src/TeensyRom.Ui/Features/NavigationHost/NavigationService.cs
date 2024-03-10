using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Serial.State;

namespace TeensyRom.Ui.Features.NavigationHost
{
    public class NavigationService : INavigationService
    {
        public IObservable<List<NavigationItem>> NavigationItems => _navigationItems.AsObservable();
        public IObservable<NavigationItem> SelectedNavigationView => _selectedNavigation.AsObservable();        
        public IObservable<bool> IsNavOpen => _isNavOpen.AsObservable();

        private readonly BehaviorSubject<List<NavigationItem>> _navigationItems;
        private readonly BehaviorSubject<NavigationItem> _selectedNavigation;
        private readonly BehaviorSubject<bool> _isNavOpen;

        public NavigationService()
        {
            _isNavOpen = new BehaviorSubject<bool>(false);
            _navigationItems = new BehaviorSubject<List<NavigationItem>>(new List<NavigationItem>());
            _selectedNavigation = new BehaviorSubject<NavigationItem>(null!);
        }

        public void Initialize(NavigationLocation startingLocation, List<NavigationItem> navItems)
        {
            _navigationItems.OnNext(navItems);
            NavigateTo(startingLocation);
        }
        
        public Unit NavigateTo(Guid id)
        {
            NavigateTo(_navigationItems.Value.First(n => n.Id == id));
            return Unit.Default;
        }

        public Unit NavigateTo(NavigationLocation location)
        {
            var navItem = _navigationItems.Value.First(n => n.Type == location);
            NavigateTo(navItem);
            return Unit.Default;
        }

        private void NavigateTo(NavigationItem navItem)
        {
            var selectedNav = _selectedNavigation.Value;

            if (navItem != null && selectedNav?.Id != navItem.Id)
            {
                SetSelectedNav(navItem);
                _selectedNavigation.OnNext(navItem);
            }
        }

        private void SetSelectedNav(NavigationItem navItem)
        {
            navItem.IsSelected = true;

            _navigationItems.Value
                .Where(n => n.Id != navItem.Id)
                .ToList()
                .ForEach(n => n.IsSelected = false);
        }
    }
}
