import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { NavService } from './nav.service';
import { NAV_ITEMS } from './navigation.constants';
import { NavItem } from './nav-item.model';

describe('NavService', () => {
  let service: NavService;
  let routerSpy: jasmine.SpyObj<Router>;

  beforeEach(() => {
    routerSpy = jasmine.createSpyObj('Router', ['navigate']);
    TestBed.configureTestingModule({
      providers: [NavService, { provide: Router, useValue: routerSpy }],
    });
    service = TestBed.inject(NavService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should have navItems equal to NAV_ITEMS', () => {
    expect(service.navItems()).toEqual(NAV_ITEMS);
  });

  it('should be closed by default', () => {
    expect(service.isNavOpen()).toBeFalse();
  });

  it('should open nav', () => {
    service.openNav();
    expect(service.isNavOpen()).toBeTrue();
  });

  it('should close nav', () => {
    service.openNav();
    service.closeNav();
    expect(service.isNavOpen()).toBeFalse();
  });

  it('should toggle nav open/close', () => {
    service.toggleNav();
    expect(service.isNavOpen()).toBeTrue();
    service.toggleNav();
    expect(service.isNavOpen()).toBeFalse();
  });

  it('should navigate and close nav when navItem has a route', () => {
    const navItem: NavItem = { name: 'Test', icon: 'test', route: '/test' };
    service.openNav();
    service.navigateTo(navItem);
    expect(routerSpy.navigate).toHaveBeenCalledWith(['/test']);
    expect(service.isNavOpen()).toBeFalse();
  });

  it('should not navigate or close nav when navItem has no route', () => {
    const navItem: NavItem = { name: 'Test', icon: '', route: '' };
    service.openNav();
    service.navigateTo(navItem);
    expect(routerSpy.navigate).not.toHaveBeenCalled();
    expect(service.isNavOpen()).toBeTrue();
  });
});
