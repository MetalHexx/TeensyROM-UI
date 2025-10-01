import { vi, describe, it, expect, beforeEach } from 'vitest';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { signal } from '@angular/core';
import { By } from '@angular/platform-browser';
import { FilterToolbarComponent } from './filter-toolbar.component';
import { PLAYER_CONTEXT, IPlayerContext } from '@teensyrom-nx/application';
import { LaunchMode, PlayerStatus } from '@teensyrom-nx/domain';

type MockedObject<T> = { [K in keyof T]: T[K] extends (...args: unknown[]) => unknown ? ReturnType<typeof vi.fn> : T[K] };

// Test constants
const TEST_DEVICE_ID = 'test-device-id';
const FILTER_BUTTON_COUNT = 4;
const TOTAL_BUTTON_COUNT = 5; // 4 filter buttons + 1 random button

// Test data selectors
const SELECTORS = {
  filterAllButton: '[data-testid="filter-all-button"]',
  filterGamesButton: '[data-testid="filter-games-button"]',
  filterMusicButton: '[data-testid="filter-music-button"]',
  filterImagesButton: '[data-testid="filter-images-button"]',
  randomLaunchButton: '[data-testid="random-launch-button"]',
  separator: '[data-testid="separator"]',
  filterButtons: '[data-testid^="filter-"]',
  iconButton: 'lib-icon-button',
  innerButton: 'button',
  joystickIcon: 'lib-joystick-icon',
  imageIcon: 'lib-image-icon',
  compactCardLayout: 'lib-compact-card-layout',
  filterButtonsContainer: '.filter-buttons',
  verticalSeparator: '.vertical-separator',
} as const;

// Expected labels for accessibility testing
const EXPECTED_LABELS = {
  filterAll: 'Filter: Allow All Files',
  filterGames: 'Filter: Games Only',
  filterMusic: 'Filter: Music Only',
  filterImages: 'Filter: Images Only',
  randomLaunch: 'Launch Random File',
} as const;

// Console log messages
const CONSOLE_MESSAGES = {
  allFilter: 'All filter clicked',
  gamesFilter: 'Games filter clicked',
  musicFilter: 'Music filter clicked',
  imagesFilter: 'Images filter clicked',
} as const;

describe('FilterToolbarComponent', () => {
  let component: FilterToolbarComponent;
  let fixture: ComponentFixture<FilterToolbarComponent>;
  let mockPlayerContext: MockedObject<IPlayerContext>;

  beforeEach(async () => {
    // Create strongly typed mock for IPlayerContext
    mockPlayerContext = {
      // Core player lifecycle
      initializePlayer: vi.fn(),
      removePlayer: vi.fn(),
      
      // File launching
      launchFileWithContext: vi.fn().mockResolvedValue(undefined),
      launchRandomFile: vi.fn().mockResolvedValue(undefined),
      
      // Playback control methods
      play: vi.fn().mockResolvedValue(undefined),
      pause: vi.fn().mockResolvedValue(undefined),
      stop: vi.fn().mockResolvedValue(undefined),
      next: vi.fn().mockResolvedValue(undefined),
      previous: vi.fn().mockResolvedValue(undefined),
      
      // State queries
      getCurrentFile: vi.fn().mockReturnValue(signal(null).asReadonly()),
      getFileContext: vi.fn().mockReturnValue(signal(null).asReadonly()),
      getPlayerStatus: vi.fn().mockReturnValue(signal(PlayerStatus.Stopped).asReadonly()),
      getStatus: vi.fn().mockReturnValue(signal(PlayerStatus.Stopped).asReadonly()),
      isLoading: vi.fn().mockReturnValue(signal(false).asReadonly()),
      getError: vi.fn().mockReturnValue(signal(null).asReadonly()),
      
      // Shuffle functionality
      toggleShuffleMode: vi.fn(),
      setShuffleScope: vi.fn(),
      setFilterMode: vi.fn(),
      getLaunchMode: vi.fn().mockReturnValue(signal(LaunchMode.Directory).asReadonly()),
      getShuffleSettings: vi.fn().mockReturnValue(signal(null).asReadonly()),
    };

    await TestBed.configureTestingModule({
      imports: [FilterToolbarComponent],
      providers: [
        { provide: PLAYER_CONTEXT, useValue: mockPlayerContext },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(FilterToolbarComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('deviceId', TEST_DEVICE_ID);
    fixture.detectChanges();
  });

  describe('Initialization', () => {
    it('should create', () => {
      expect(component).toBeTruthy();
    });

    it('should require deviceId input', () => {
      expect(component.deviceId).toBeDefined();
      expect(component.deviceId()).toBe(TEST_DEVICE_ID);
    });
  });

  describe('Filter Button Actions', () => {
    it('should log when All filter button is clicked', () => {
      const consoleSpy = vi.spyOn(console, 'log');
      
      component.onAllClick();
      
      expect(consoleSpy).toHaveBeenCalledWith(CONSOLE_MESSAGES.allFilter);
    });

    it('should log when Games filter button is clicked', () => {
      const consoleSpy = vi.spyOn(console, 'log');
      
      component.onGamesClick();
      
      expect(consoleSpy).toHaveBeenCalledWith(CONSOLE_MESSAGES.gamesFilter);
    });

    it('should log when Music filter button is clicked', () => {
      const consoleSpy = vi.spyOn(console, 'log');
      
      component.onMusicClick();
      
      expect(consoleSpy).toHaveBeenCalledWith(CONSOLE_MESSAGES.musicFilter);
    });

    it('should log when Images filter button is clicked', () => {
      const consoleSpy = vi.spyOn(console, 'log');
      
      component.onImagesClick();
      
      expect(consoleSpy).toHaveBeenCalledWith(CONSOLE_MESSAGES.imagesFilter);
    });
  });

  describe('Random Launch Functionality', () => {
    it('should call playerContext.launchRandomFile with correct deviceId', async () => {
      await component.launchRandomFile();
      
      expect(mockPlayerContext.launchRandomFile).toHaveBeenCalledWith(TEST_DEVICE_ID);
    });

    it('should not call launchRandomFile when deviceId is empty', async () => {
      fixture.componentRef.setInput('deviceId', '');
      
      await component.launchRandomFile();
      
      expect(mockPlayerContext.launchRandomFile).not.toHaveBeenCalled();
    });

    it('should not call launchRandomFile when deviceId is null', async () => {
      fixture.componentRef.setInput('deviceId', null);
      
      await component.launchRandomFile();
      
      expect(mockPlayerContext.launchRandomFile).not.toHaveBeenCalled();
    });

    it('should handle async errors gracefully', async () => {
      const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => { /* noop */ });
      mockPlayerContext.launchRandomFile.mockRejectedValue(new Error('Test error'));
      
      // Should not throw and should log error
      await component.launchRandomFile();
      expect(consoleSpy).toHaveBeenCalled();
      consoleSpy.mockRestore();
    });
  });

  describe('Template Rendering', () => {
    it('should render all filter buttons', () => {
      const filterButtons = fixture.debugElement.queryAll(By.css(SELECTORS.filterButtons));
      
      expect(filterButtons).toHaveLength(FILTER_BUTTON_COUNT);
      expect(fixture.debugElement.query(By.css(SELECTORS.filterAllButton))).toBeTruthy();
      expect(fixture.debugElement.query(By.css(SELECTORS.filterGamesButton))).toBeTruthy();
      expect(fixture.debugElement.query(By.css(SELECTORS.filterMusicButton))).toBeTruthy();
      expect(fixture.debugElement.query(By.css(SELECTORS.filterImagesButton))).toBeTruthy();
    });

    it('should render vertical separator', () => {
      const separator = fixture.debugElement.query(By.css(SELECTORS.separator));
      
      expect(separator).toBeTruthy();
      expect(separator.nativeElement.classList.contains('vertical-separator')).toBe(true);
    });

    it('should render random launch button', () => {
      const randomButton = fixture.debugElement.query(By.css(SELECTORS.randomLaunchButton));
      const innerButton = randomButton.query(By.css(SELECTORS.innerButton));
      
      expect(randomButton).toBeTruthy();
      expect(innerButton).toBeTruthy();
      expect(innerButton.nativeElement.getAttribute('aria-label')).toBe(EXPECTED_LABELS.randomLaunch);
    });

    it('should render custom joystick icon in games button', () => {
      const gamesButton = fixture.debugElement.query(By.css(SELECTORS.filterGamesButton));
      const joystickIcon = gamesButton.query(By.css(SELECTORS.joystickIcon));
      
      expect(joystickIcon).toBeTruthy();
    });

    it('should render custom image icon in images button', () => {
      const imagesButton = fixture.debugElement.query(By.css(SELECTORS.filterImagesButton));
      const imageIcon = imagesButton.query(By.css(SELECTORS.imageIcon));
      
      expect(imageIcon).toBeTruthy();
    });
  });

  describe('User Interactions', () => {
    // Helper function to click a button and verify method call
    const testButtonClick = (buttonSelector: string, methodName: keyof FilterToolbarComponent) => {
      const spy = vi.spyOn(component, methodName);
      const button = fixture.debugElement.query(By.css(buttonSelector));
      const innerButton = button.query(By.css(SELECTORS.innerButton));
      
      innerButton.nativeElement.click();
      
      expect(spy).toHaveBeenCalled();
    };

    it('should trigger onAllClick when All filter button is clicked', () => {
      testButtonClick(SELECTORS.filterAllButton, 'onAllClick');
    });

    it('should trigger onGamesClick when Games filter button is clicked', () => {
      testButtonClick(SELECTORS.filterGamesButton, 'onGamesClick');
    });

    it('should trigger onMusicClick when Music filter button is clicked', () => {
      testButtonClick(SELECTORS.filterMusicButton, 'onMusicClick');
    });

    it('should trigger onImagesClick when Images filter button is clicked', () => {
      testButtonClick(SELECTORS.filterImagesButton, 'onImagesClick');
    });

    it('should trigger launchRandomFile when random launch button is clicked', async () => {
      testButtonClick(SELECTORS.randomLaunchButton, 'launchRandomFile');
    });
  });

  describe('Component Layout', () => {
    it('should have proper CSS classes for layout', () => {
      const filterButtons = fixture.debugElement.query(By.css(SELECTORS.filterButtonsContainer));
      
      expect(filterButtons).toBeTruthy();
      expect(filterButtons.nativeElement.classList.contains('filter-buttons')).toBe(true);
    });

    it('should wrap content in compact card layout', () => {
      const cardLayout = fixture.debugElement.query(By.css(SELECTORS.compactCardLayout));
      
      expect(cardLayout).toBeTruthy();
    });

    it('should have buttons in correct order', () => {
      const buttons = fixture.debugElement.queryAll(By.css(SELECTORS.iconButton));
      const separator = fixture.debugElement.query(By.css(SELECTORS.verticalSeparator));
      
      // Should have 5 buttons total (4 filters + 1 random)
      expect(buttons).toHaveLength(TOTAL_BUTTON_COUNT);
      
      // Separator should be between filter buttons and random button
      const separatorElement = separator.nativeElement;
      const randomButton = fixture.debugElement.query(By.css(SELECTORS.randomLaunchButton)).nativeElement;
      
      expect(separatorElement.nextElementSibling).toBe(randomButton);
    });
  });

  describe('Accessibility', () => {
    it('should have proper aria-labels for all buttons', () => {
      const buttonTests = [
        { selector: SELECTORS.filterAllButton, expectedLabel: EXPECTED_LABELS.filterAll },
        { selector: SELECTORS.filterGamesButton, expectedLabel: EXPECTED_LABELS.filterGames },
        { selector: SELECTORS.filterMusicButton, expectedLabel: EXPECTED_LABELS.filterMusic },
        { selector: SELECTORS.filterImagesButton, expectedLabel: EXPECTED_LABELS.filterImages },
        { selector: SELECTORS.randomLaunchButton, expectedLabel: EXPECTED_LABELS.randomLaunch },
      ];

      buttonTests.forEach(({ selector, expectedLabel }) => {
        const iconButton = fixture.debugElement.query(By.css(selector));
        const innerButton = iconButton.query(By.css(SELECTORS.innerButton));
        expect(innerButton.nativeElement.getAttribute('aria-label')).toBe(expectedLabel);
      });
    });

    it('should have proper button size for touch targets', () => {
      const buttons = fixture.debugElement.queryAll(By.css(SELECTORS.iconButton));
      
      buttons.forEach(button => {
        expect(button.nativeElement.getAttribute('size')).toBe('large');
      });
    });
  });

  describe('Dice Roll Animation', () => {
    it('should initialize animation state correctly', () => {
      expect(component.isDiceRolling()).toBe(false);
    });

    it('should have random button template reference', () => {
      expect(component.randomButton).toBeDefined();
    });

    it('should trigger dice roll animation when launching random file', async () => {
      const animateSpy = vi.spyOn(component, 'animateDiceRoll');
      
      await component.launchRandomFile();
      
      expect(animateSpy).toHaveBeenCalled();
    });

    it('should set animation state during dice roll', () => {
      // Create a mock element for the random button
      const mockElement = {
        classList: {
          add: vi.fn(),
          remove: vi.fn(),
        },
      };
      
      // Mock the randomButton signal to return our mock element
      const mockButtonSignal = vi.fn().mockReturnValue({ nativeElement: mockElement });
      component.randomButton = mockButtonSignal;
      
      // Call the method
      component.animateDiceRoll();
      
      expect(component.isDiceRolling()).toBe(true);
      expect(mockElement.classList.add).toHaveBeenCalledWith('dice-roll');
    });

    it('should prevent multiple simultaneous animations', () => {
      // Set animation state to true
      component.isDiceRolling.set(true);
      
      const mockElement = {
        classList: {
          add: vi.fn(),
          remove: vi.fn(),
        },
      };
      
      const mockButtonSignal = vi.fn().mockReturnValue({ nativeElement: mockElement });
      component.randomButton = mockButtonSignal;
      
      // Try to animate again
      component.animateDiceRoll();
      
      // Should not add animation class
      expect(mockElement.classList.add).not.toHaveBeenCalled();
    });
  });

  describe('Error Handling', () => {
    it('should handle missing deviceId gracefully', async () => {
      fixture.componentRef.setInput('deviceId', undefined);
      
      // Should not throw errors
      expect(() => component.onAllClick()).not.toThrow();
      expect(() => component.onGamesClick()).not.toThrow();
      expect(() => component.onMusicClick()).not.toThrow();
      expect(() => component.onImagesClick()).not.toThrow();
      await expect(component.launchRandomFile()).resolves.toBeUndefined();
    });

    it('should handle playerContext service errors gracefully', async () => {
      const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => { /* noop */ });
      mockPlayerContext.launchRandomFile.mockRejectedValue(new Error('Service unavailable'));
      
      // Should not throw unhandled errors and should log
      await component.launchRandomFile();
      expect(mockPlayerContext.launchRandomFile).toHaveBeenCalled();
      expect(consoleSpy).toHaveBeenCalled();
      consoleSpy.mockRestore();
    });

    it('should handle animation gracefully when button element is not available', () => {
      // Mock randomButton to return null
      const mockButtonSignal = vi.fn().mockReturnValue(null);
      component.randomButton = mockButtonSignal;
      
      // Should not throw when element is not available
      expect(() => component.animateDiceRoll()).not.toThrow();
      expect(component.isDiceRolling()).toBe(true); // State still gets set
    });
  });
});