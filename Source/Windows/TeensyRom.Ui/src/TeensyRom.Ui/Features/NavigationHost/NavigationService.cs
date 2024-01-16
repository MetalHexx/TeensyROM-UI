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
        public IObservable<List<NavigationItem>> NavigationItems
        {
            get { return _navigationItems.AsObservable(); }
        }
        private BehaviorSubject<List<NavigationItem>> _navigationItems;
        public IObservable<NavigationItem> SelectedNavigationView
        {
            get { return _selectedNavigation.AsObservable(); }
        }
        private BehaviorSubject<NavigationItem> _selectedNavigation;
        public IObservable<bool> IsNavOpen
        {
            get { return _isNavOpen.AsObservable(); }
        }
        private BehaviorSubject<bool> _isNavOpen;

        public NavigationService()
        {
            _isNavOpen = new BehaviorSubject<bool>(false);
            _navigationItems = new BehaviorSubject<List<NavigationItem>>(new List<NavigationItem>());
            _selectedNavigation = new BehaviorSubject<NavigationItem>(null);
        }

        public void Initialize(NavigationLocation startingLocation, List<NavigationItem> navItems)
        {
            _navigationItems.OnNext(navItems);
            NavigateTo(startingLocation);
        }
        
        public Unit NavigateTo(Guid id)
        {
            _selectedNavigation.OnNext(
                _navigationItems.Value.First(n => n.Id == id));

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
                _selectedNavigation.OnNext(navItem);
            }
        }

        public Unit ToggleNav()
        {
            _isNavOpen.OnNext(!_isNavOpen.Value);
            return Unit.Default;
        }
    }
}
