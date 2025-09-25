# Player Domain - Product Requirements Document 🎵

## Executive Summary 🎯

The TeensyROM Player is a sophisticated multi-device media player and DJ system designed to handle Games, Music, and Images across multiple storage devices. Each TeensyROM device maintains independent player state, enabling simultaneous operation of multiple devices as part of a comprehensive media management and mixing system.

The system provides both casual media consumption and professional DJ functionality, with advanced features for beat matching, live mixing, and creative sound manipulation specifically optimized for SID file playback.

## Product Overview 📋

### Core Capabilities

- **Multi-Device Support**: Independent player state per TeensyROM device
- **Cross-Media Playback**: Unified controls for Games, Music, and Images
- **Professional DJ Features**: Speed control, beat matching, live effects
- **Intelligent Navigation**: Smart file sequencing with multiple playback modes
- **Custom Timing**: Flexible duration control for different content types

### Target Use Cases

- **Live DJ Performance**: Beat matching, mixing, and effects
- **Casual Media Consumption**: Sequential playback through media libraries
- **Gaming Sessions**: Timed game play with automatic progression
- **Content Discovery**: Random exploration across different media types

---

## Core Concepts & Terminology 📚

### Current File

The active media file currently being played or selected for playback. This concept is central to all player operations and determines the context for navigation and control behaviors.

### Filter Modes

Content filtering system that determines which file types are considered for playback operations:

- **Games**: Game files only
- **Music**: SID files only
- **Images**: Image files only
- **All**: Any supported file type

### Launch Modes

Determines the sequence logic for next/previous file selection:

- **[Directory Mode](#directory-mode)**: Sequential playback within current folder
- **[Shuffle Mode](#shuffle-mode)**: Random file selection with configurable scope

### Play History

Navigation tracking system that maintains a browser-like history of played files, enabling forward and backward navigation through previously played content.

### Speed Control Concepts

- **Base Speed**: User-defined speed setting that persists across operations
- **Altered Speed**: Temporary speed modifications calculated from base speed
- **Speed Delta**: Percentage change from original playback speed (±%)

### SID Voices

SID song files feature three independent voice tracks (Voice 1, Voice 2, Voice 3) that can be individually enabled or disabled.

---

## User Personas & Scenarios 👥

### The Live DJ 🎧

**Scenario**: Performing live mix sets with beat matching between multiple devices
**Needs**: Speed control, nudging, voice manipulation, hold functions
**Workflow**: Load tracks, match tempos, apply effects, seamless transitions

### The Casual Listener 🎶

**Scenario**: Enjoying music collections with simple navigation
**Needs**: Play/pause, next/previous, progress tracking
**Workflow**: Select directory, play, let system handle progression

### The Gaming Enthusiast 🎮

**Scenario**: Playing games with time limits and automatic progression
**Needs**: Custom timers, automatic next-game loading
**Workflow**: Set timer duration, start game, automatic progression when time expires

### The Content Explorer 🔍

**Scenario**: Discovering new content across different media types
**Needs**: Shuffle mode, cross-device discovery, filtering
**Workflow**: Enable shuffle, set scope, explore randomized content

---

## Core Playback Features ⏯️

### Universal Controls

All media types support these fundamental operations:

#### Play/Stop Control

- **Play**: Launches the [Current File](#current-file)
- **Stop**: Halts playback and resets position
- **File Selection**: Double-clicking any file automatically starts playback

#### Navigation Controls

- **Next**: Advances to next file based on active [Launch Mode](#launch-modes)
- **Previous**: Returns to previous file with media-specific behaviors
- **Current File Tracking**: System maintains awareness of active media

### External System Integration

All playback operations trigger corresponding calls to external media handling systems, ensuring synchronized state management across the entire platform.

---

## Content Organization 🗂️

### Filter System

Controls which file types are considered for playback operations:

#### Filter Types

- **Games Filter**: Only game files eligible for next/previous navigation
- **Music Filter**: Only SID files included in playback sequence
- **Images Filter**: Only image files considered for progression
- **All Filter**: Any supported file type available for playback

#### Filter Behavior

- Independent control per filter type
- Affects [Shuffle Mode](#shuffle-mode) file selection
- Influences next/previous navigation results
- User-configurable per session

---

## Playback Modes & Navigation 🔀

### Directory Mode

Sequential file playback within the current folder context.

#### Behavior

- Files play in directory order
- Next/Previous navigates through folder contents
- Scope limited to [Current File](#current-file) directory
- Automatic activation when file launched directly

#### Use Cases

- Playing complete albums in order
- Systematic content consumption
- Predictable navigation experience

### Shuffle Mode

Random file selection with sophisticated scoping options and [Play History](#play-history) management.

#### Scope Options

##### TeensyROM Device Global Mode

- Selects files across all storage devices (USB and SD)
- Widest possible content pool
- Cross-device discovery experience

##### Storage Device Global Mode

- Limited to same storage device as [Current File](#current-file)
- Maintains device context
- Balances discovery with organization

##### Directory Pinning

Enhanced directory-based scoping:

**Directory Shallow Mode**

- Random selection within pinned directory only
- Excludes subdirectories
- Focused content area

**Directory Deep Mode**

- Includes pinned directory and all subdirectories

#### Play History Management

Browser-style navigation system:

##### Forward/Backward Navigation

- **Previous**: Returns to last played file in history
- **History Tracking**: Maintains chronological play sequence
- **Forward History**: Available when navigating backward

##### History Rules

- New file launches clear forward history
- History preserved across [Launch Mode](#launch-modes) switches
- Independent history per TeensyROM device
- History Kept regardless of storage device the file was launched from.

#### Mode Switching Behavior

- Direct file launch automatically switches from Shuffle to [Directory Mode](#directory-mode)
- Preserves [Play History](#play-history) across mode changes
- User can manually switch between modes

---

## Timer & Progress System ⏱️

### Automatic Song Duration

Music files provide built-in timing through metadata.

#### Behavior

- Timer automatically set to song duration
- Progress bar displays current position
- Automatic next file launch when song completes
- Timer speed adjusts with [Speed Control](#speed-control-system)

### Custom Play Timer

User-defined timing system for Games and Images.

#### Duration Options

Flexible timing options: 5s, 10s, 15s, 30s, 1m, 3m, 5m, 10m, 15m, 30m, 1hr

#### Timer Behavior

- **Activation Control**: Toggle on/off independently
- **Progress Display**: Visual progress bar when enabled
- **Playback Dependency**: Timer only runs during active playback
- **Pause Integration**: Timer pauses with stopped/paused state
- **Next File Action**: Automatic progression when timer expires

#### Song Timer Override

Special mode allowing custom duration for music files:

- **Override Option**: Replace metadata duration with custom timer
- **Normal Operation**: Songs use metadata duration by default
- **Sequential Play**: Maintains normal playback flow with custom timing

### Progress Bar Management

Visual timing indicators with intelligent display rules:

#### Display Logic

- **Songs**: Always visible (using metadata duration)
- **Games/Images**: Visible only when [Custom Play Timer](#custom-play-timer) enabled
- **Timer Override**: Visible when song override activated

---

## Advanced Music Controls 🎛️

### Speed Control System

Sophisticated speed manipulation with persistent and temporary adjustments.

#### Set Song Speed - Coarse Control

Primary speed adjustment mechanism:

- **Increment**: One-tenth percent (0.1%) adjustments
- **Curve Options**: Linear or Logarithmic speed curves
- **Base Speed**: Becomes foundation for other speed calculations
- **Timer Sync**: Progress timer adjusts proportionally
- **Persistence**: Setting maintained across operations

#### Set Song Speed - Fine Control

Precision speed adjustment:

- **Increment**: One-hundredth percent (0.01%) adjustments
- **Same Behavior**: Functions identically to coarse control
- **Use Case**: Precise beat matching and fine-tuning

#### Home Speed Function

Speed reset mechanism:

- **Action**: Returns [Base Speed](#speed-control-concepts) to 0% (original speed)
- **Single Control**: One-button speed normalization
- **Timer Reset**: Progress timing returns to original rate

### Seeking System

Advanced navigation optimized for SID files.

#### SID File Constraints

Specialized seeking required for SID Files:

- **No Random Access**: Files cannot jump to arbitrary time positions
- **Forward Seeking**: Fast-forward to target position
- **Backward Seeking**: Restart file, then fast-forward to target

#### Seek Speed Modes

**Accurate Mode**: +1000% speed

- Maintains timer synchronization
- Suitable for precise positioning
- Preferred for most use cases

**Insane Mode**: +10,000% speed

- May lose timer synchronization
- Limited accuracy applications
- Creates unique DJ effects opportunities

#### Seek Behavior

- **Timer Adjustment**: Progress timer matches seek speed
- **Speed Restoration**: Returns to [Base Speed](#speed-control-concepts) upon arrival
- **Cancellation**: Any playback control cancels seek and restores normal speed

### Nudging System

Temporary speed adjustments designed for beat matching between multiple tracks.

#### Nudge Controls

- **Positive Nudge**: +5% temporary speed increase
- **Negative Nudge**: -5% temporary speed decrease
- **Separate Controls**: Independent activation for each direction
- **Release Behavior**: Returns to [Base Speed](#speed-control-concepts) when released

#### Speed Calculation

- **Base Speed Integration**: Always calculated from current [Base Speed](#speed-control-concepts)
- **Dynamic Adjustment**: Responds to [Base Speed](#speed-control-concepts) changes while engaged
- **Example**: 10% base speed + 5% nudge = 15% total speed

#### Interactive Behavior

- **Base Speed Changes**: Nudge recalculates when user adjusts [Base Speed](#speed-control-concepts)
- **Release Target**: Always returns to current [Base Speed](#speed-control-concepts)
- **Professional Use**: Essential for live DJ beat matching

### Speed Jump System

Dramatic temporary speed changes for creative effects.

#### Jump Controls

- **Positive Jump**: +50% temporary speed increase
- **Negative Jump**: -50% temporary speed decrease
- **Effect Purpose**: Half-time or double-time creative effects
- **Release Behavior**: Returns to [Base Speed](#speed-control-concepts)

#### Integration with Nudging

- **Cumulative Effects**: Can combine with active [Nudging](#nudging-system)
- **Combined Range**: ±55% when both systems engaged
- **Independent Release**: Each system releases to appropriate intermediate state
- **Example**: 50% jump + 5% nudge = 55% total alteration

### Fast Forward System

Multi-step speed progression for dramatic effects.

#### Speed Steps

Progressive speed increases:

1. **Fast**: +50% speed increase
2. **Faster**: +100% speed increase
3. **Even Faster**: +200% speed increase
4. **Fastest**: +1000% speed increase
5. **Reset**: Returns to [Base Speed](#speed-control-concepts)

#### Behavior

- **Sequential Activation**: Each control press advances to next step
- **Cycle Completion**: After fastest step, returns to normal speed
- **Base Speed Integration**: All calculations from current [Base Speed](#speed-control-concepts)

### Hold Function

DJ-style pause control optimized for live performance.

#### Control Behavior

- **Mouse Down**: Immediately pauses playback
- **Mouse Up**: Immediately resumes playback
- **Live Performance**: Enables precise timing control
- **DJ Utility**: Simulates record hold-and-release technique

#### Distinction from Pause

- **Standard Pause**: Toggle behavior (click to pause, click to resume)
- **Hold Function**: Momentary behavior (press and hold)
- **Use Cases**: Different applications for different scenarios

### Restart Song Function

Immediate song reset without timing conditions.

#### Behavior

- **Immediate Restart**: Always restarts song from beginning
- **No Time Limit**: Unlike [Previous Button](#previous-button-behavior), no 5-second rule
- **Mouse Down Trigger**: Activates immediately on press
- **Separate Control**: Distinct from Previous button

---

## SID Voice Management 🎼

### Voice System Overview

Three independent synthesizer voice channels (Voice 1, Voice 2, Voice 3) with individual control capabilities.

### Toggle Voice System

Persistent voice enable/disable functionality.

#### Voice Controls

- **Individual Controls**: Separate toggle for each voice (1, 2, 3)
- **Toggle Behavior**: Mouse up/down combination switches state
- **State Persistence**: Setting maintained throughout playback
- **Combination Control**: Any combination of voices can be active

### Kill/Activate Voice System

Temporary voice control with intelligent state management.

#### Default Kill Switch Behavior

- **Mouse Down**: Temporarily disables voice
- **Mouse Up**: Re-enables voice
- **Momentary Control**: Designed for live performance

#### Smart State Integration

Advanced logic for interaction with [Toggle Voice](#toggle-voice-system):

**When Voice Already Disabled by Toggle**:

- Kill switch becomes "Activate Voice"
- Temporarily enables disabled voice while pressed
- Returns to disabled state on release

**When Voice Enabled by Toggle**:

- Functions as normal kill switch
- Temporarily disables voice while pressed
- Returns to enabled state on release

#### Utility Benefits

- **Simplified Logic**: Automatic behavior adaptation
- **Live Performance**: Intuitive momentary control regardless of base state
- **State Respect**: Never conflicts with toggle settings

---

## File Type Behaviors 📁

### Games 🎮

Specialized behavior for interactive content.

#### Basic Controls

- **Play**: Launches game
- **Stop**: Resets TeensyROM (pause not supported)
- **No Standard Timer**: Games don't include duration metadata

#### Timer Integration

- **[Custom Play Timer](#custom-play-timer) Support**: Optional timing system
- **Timer-Based Progression**: Automatic next file when timer expires
- **Filter Integration**: Next file determined by active [Filter Mode](#filter-system)

### Music 🎶

Comprehensive SID file playback with advanced controls.

#### Standard Controls

- **Play/Pause**: Full pause and resume capability
- **Toggle Behavior**: Single control switches between play and pause states
- **Progress Tracking**: Automatic duration from song metadata

#### Advanced Features

- **All [Speed Controls](#advanced-music-controls)**: Complete speed manipulation suite
- **[Seeking System](#seeking-system)**: SID-optimized navigation
- **[Voice Controls](#sid-voice-management)**: SID-specific voice manipulation
- **Timer Integration**: Uses metadata duration unless [Timer Override](#song-timer-override) active

#### Previous Button Behavior

Specialized logic for music navigation:

- **Under 5 Seconds**: Launches previous song in sequence
- **Over 5 Seconds**: Restarts current song from beginning
- **Smart Navigation**: Contextual behavior based on playback time

#### Loop Mode

Special repeat functionality:

- **No External Call Required**: System handles repetition automatically
- **Timer Restart**: Progress timer resets and continues
- **Current Song Focus**: Repeats active track without changing [Current File](#current-file)

### Images 🖼️

Visual content with timer-based progression.

#### Basic Controls

- **Play**: Displays image
- **Stop**: Resets the TeensyROM
- **No Standard Timer**: Images don't include duration metadata

#### Timer Integration

- **[Custom Play Timer](#custom-play-timer) Support**: Required for automatic progression
- **Timer-Based Progression**: Automatic next file when timer expires
- **Filter Integration**: Next file determined by active [Filter Mode](#filter-system)

---

## Business Rules & Constraints ⚖️

### Device Independence

- Each TeensyROM device maintains separate player state
- Multiple devices can operate simultaneously
- State changes affect only the target device

### Mode Switching Logic

- Direct file launch automatically switches from [Shuffle Mode](#shuffle-mode) to [Directory Mode](#directory-mode)
- [Play History](#play-history) preserved across mode switches
- User can manually override automatic mode switching

### Speed Control Hierarchy

- [Base Speed](#speed-control-concepts) provides foundation for all calculations
- [Altered Speed](#speed-control-concepts) modifications always calculated from current [Base Speed](#speed-control-concepts)
- Multiple speed modifications can combine cumulatively
- All temporary modifications return to [Base Speed](#speed-control-concepts) on release

### History Management

- New file launches clear forward [Play History](#play-history)
- Backward navigation preserves forward history
- History maintained independently per device
- History survives [Launch Mode](#launch-modes) changes

### Timer Synchronization

- Progress timers adjust proportionally with speed changes
- Timer display follows [Custom Play Timer](#custom-play-timer) activation rules
- Timer state pauses with playback pause/stop

### External System Coordination

- All playback state changes trigger corresponding external system calls
- State synchronization maintained across platform components
- Device-specific calls routed to appropriate hardware

---

## User Interface Requirements 🖥️

### Control Types

#### Toggle Controls

- **Behavior**: State switches on mouse up/down combination
- **Applications**: [Filter Modes](#filter-system), [Toggle Voice](#toggle-voice-system), [Launch Modes](#launch-modes)
- **Visual Feedback**: Clear indication of current state

#### Momentary Controls

- **Behavior**: Active during mouse press, releases on mouse up
- **Applications**: [Hold Function](#hold-function), [Kill/Activate Voice](#killactivate-voice-system), [Nudging](#nudging-system)
- **Visual Feedback**: Immediate response indication

#### Immediate Action Controls

- **Behavior**: Triggers on mouse down
- **Applications**: [Restart Song](#restart-song-function)
- **Use Cases**: Time-sensitive operations requiring instant response

### Progress Display Rules

- **Songs**: Progress bar always visible during playback
- **Games/Images**: Progress bar visible only when [Custom Play Timer](#custom-play-timer) enabled
- **Timer Override**: Progress bar visible when song override active
- **Visual Consistency**: Uniform progress indication across media types

### Mode Indicators

- **Current [Filter Mode](#filter-system)**: Clear indication of active content filter
- **Active [Launch Mode](#launch-modes)**: Visual distinction between Directory and Shuffle modes
- **[Shuffle Scope](#shuffle-mode)**: Indication of current shuffle scope setting
- **Speed Status**: Display of current [Base Speed](#speed-control-concepts) and any active [Altered Speed](#speed-control-concepts)

---

## Success Criteria ✅

### User Experience Goals

- **Intuitive Navigation**: Users can easily switch between casual listening and DJ functionality
- **Performance Reliability**: All controls respond immediately without lag or synchronization issues
- **Mode Clarity**: Users always understand current playback mode and scope
- **Creative Freedom**: DJ features enable expressive live performance capabilities

### Technical Performance

- **Multi-Device Stability**: Simultaneous operation of multiple TeensyROM devices without conflicts
- **State Consistency**: Player state remains synchronized across all system components
- **Timer Accuracy**: Progress tracking maintains precision across all speed modifications
- **History Reliability**: Navigation history functions consistently in all playback modes

### Business Value

- **Professional DJ Capability**: System supports live performance requirements
- **Casual User Satisfaction**: Simple operation for basic media consumption
- **Content Discovery**: Shuffle system encourages exploration of media libraries
- **Device Utilization**: Multi-device support maximizes TeensyROM hardware value

---

_This document represents the complete business requirements for the TeensyROM Player system. All features described require integration with external media handling systems and must maintain device-independent operation across multiple TeensyROM units._
