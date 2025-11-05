# TeensyROM Player UI Controls Guide

## Quick Start for AI Agents

To interact with the TeensyROM Player UI, follow these steps:

### 1. Start Browser Session

```
Use the mcp_chrome-devtoo_new_page tool to create a new browser page:
- URL: http://localhost:4200
- Wait for the page to load completely
```

### 2. Take Initial Snapshot

```
Before any interaction, capture the current UI state:
- Use: mcp_chrome-devtoo_take_snapshot
- This returns the accessibility tree with all element UIDs
- Use UIDs from snapshot for all subsequent interactions
```

### 3. Interact with Controls

```
All interactions use these tools:
- mcp_chrome-devtoo_click(uid) - Click any button/link
- mcp_chrome-devtoo_fill(uid, value) - Type into textbox
- mcp_chrome-devtoo_wait_for(text, timeout) - Wait for text to appear
- mcp_chrome-devtoo_take_screenshot() - Capture visual state
```

### 4. Update Element References

```
After major UI changes (navigation, page reload):
- Always call mcp_chrome-devtoo_take_snapshot again
- UIDs change when DOM updates
- Use new UIDs from latest snapshot
```

### 5. Reference Control Map

```
Use the "Technical Control Mapping (Master Reference)" section below
- Find the control you need by name
- Use the Selector column to understand what you're looking for
- Match Selector pattern to UIDs in current snapshot
- Check State_Check column to verify action worked
- Wait using Wait_Signal column before next action
```

## Important Notes

- **UIDs are ephemeral**: Always get fresh UIDs from latest snapshot after page changes
- **Async operations**: Always wait for completion signals before proceeding
- **File paths**: Device IDs may vary; use actual IDs from device tree (e.g., PSM2ZAKI)
- **Search results**: Search box requires actual text entry; results appear dynamically
- **Loading states**: Always wait for spinner to disappear before checking results

---

## Navigation Overview

The Player view is the primary interface for browsing and launching audio files (primarily SID files from retro systems).

## Main Layout

### Top Section (Player Info Panel)

- **Album Art**: Large album cover image on the left
- **File Metadata**: Shows artist name and metadata (DeepSID, PAL/NTSC, SID chip type)
- **File Title & Year**: Large text showing current track and release info
- **Links Section**: External links to CSDB Release, CSDB Profile, or other resources
- **Tags Section**: Display relevant tags for the current file
- **Theme Toggle**: Light/dark mode button (top right)

### Player Controls (Bottom Bar)

- **Previous File Button**: Navigate to previous track
- **Play/Pause Button**: Toggle playback (visual indicator shows state)
- **Next File Button**: Jump to next track
- **Playback Time**: Shows current position and total duration (e.g., "00:25 / 04:39")
- **Progress Bar**: Visual representation of playback progress
- **Currently Playing Info**: Small card showing song title and artist
- **Shuffle Toggle Button**: Enable/disable shuffle mode

### File Browser Panel (Right Side)

#### Navigation Toolbar

- **Back Button**: Navigate to previous directory (disabled if no history)
- **Forward Button**: Navigate forward in history (disabled if at end)
- **Up Button**: Go to parent directory
- **Refresh Button**: Reload current directory
- **Toggle History Button**: Show/hide directory history

#### Current Path Display

- Shows breadcrumb path (e.g., "SD Card > music > MUSICIANS > S > Simon_Laszlo")
- Updates dynamically as you navigate

#### Filter Buttons

- **Filter: Allow All Files**: Show all file types
- **Filter: Games Only**: Show only game files
- **Filter: Music Only**: Show only music files
- **Filter: Images Only**: Show only image files

#### Random Launch Button

- **Launch Random File**: Dice icon button
- Randomly selects a file from current directory and launches it
- Great for discovery

#### Search Feature

- **Search Textbox**: Type to filter files in current directory
- **Search Results Section**: Shows filtered results matching your query
- **Clear Search Button**: Resets search filter (appears when searching)

#### File Tree (Left Sidebar)

- **Device PSM2ZAKI**: Primary device with collapsible toggle
  - **USB Storage**: Toggle to expand/collapse USB files
  - **SD Storage**: Toggle to expand/collapse SD card files
- Click toggles to expand/collapse storage devices

#### Current Directory Listing

- Lists all files in current directory (below "Current Directory" heading)
- Shows file names and sizes (e.g., "3_Years_2.sid 4.7 KB")
- Click any file to launch it
- Selected file is highlighted with semi-transparent background

## Interaction Patterns

### Launching Files

1. Navigate to desired folder using directory buttons or file list
2. Click on any file in the listing to launch it
3. Player controls will activate and show playback information
4. Or use **Launch Random File** for surprise discovery

### Browsing Navigation

1. Use **Up** to go to parent directory
2. Use **Back/Forward** to navigate history
3. Use **Toggle History** to see navigation breadcrumb

### Searching for Files

1. Type in the **Search** textbox
2. "Search Results" section appears below tree
3. Filtered results show matching files
4. Click **Clear search results** to return to full directory view
5. Search works across current directory only

### Filtering File Types

- Click any of the 4 filter buttons to toggle file type visibility
- Current filter affects what displays in directory listing
- Active filter persists as you navigate

### Playback Control

- **Previous/Next buttons**: Skip between files in current directory
- **Play/Pause**: Toggle playback
- **Shuffle Toggle**: Randomize playback order
- **Progress Bar**: Click to seek within current track

## Tips for Navigation

- **Explore Hierarchically**: Most content is organized by artist/composer → album/collection
- **Use Search**: Fast way to find specific tracks by name
- **Random Button**: Great for discovering unexpected gems
- **Breadcrumb Path**: Shows exactly where you are in the hierarchy
- **File Sizes**: Helps identify track length quickly (relative to KB size)

## UI State Indicators

- **Loading Animation**: Subtle spinner appears in breadcrumb area when loading directory
- **Disabled Buttons**: Buttons appear grayed out when action unavailable (e.g., "Back" at root)
- **Selected File**: Highlighted with semi-transparent background in file list
- **Active Filter**: Filter buttons appear active when applied

---

## Technical Control Mapping (Master Reference)

**Format**: Control_ID | Role | Selector | State_Check | Wait_Signal | Notes

```
MENU_BUTTON | Toggle navigation menu | button "Menu" | N/A | N/A | Opens sidebar with Player, DJ Mixer, Devices, Settings, Theme Tester
PLAYER_NAV | Navigate to player | button "Player" | N/A | heading "Player" appears | Inside menu dropdown
DEVICES_NAV | Navigate to devices | button "Devices" | N/A | heading "Devices" appears | Inside menu dropdown
DJ_MIXER_NAV | Navigate to mixer | button "DJ Mixer" | N/A | heading "DJ Mixer" appears | Inside menu dropdown
THEME_TOGGLE | Toggle dark/light mode | button "Toggle dark/light theme" | N/A | N/A | Top-right corner

PLAY_PREV | Skip to previous track | button "Previous File" | disableable disabled | progressbar value changes | May be disabled (disableable disabled)
PLAY_PLAY_PAUSE | Toggle playback | button "Pause" or "Play" | Button text toggles | progressbar value > 0 | Text changes based on playback state
PLAY_NEXT | Skip to next track | button "Next File" | disableable disabled | progressbar value changes | May be disabled if at end of list
PLAY_SHUFFLE | Enable/disable shuffle | button "Toggle Shuffle Mode" | Visual toggle indicator | N/A | Toggle state tracking needed
PROGRESS_BAR | Show/seek playback | progressbar "" | value="X" valuemax="100" | value increments | Check valuemax and current value; clickable to seek
TIME_DISPLAY | Show current/total time | StaticText matching "MM:SS / MM:SS" | N/A | Time text updates | Example: "00:25 / 04:39"; parse with split(" / ")

NAV_BACK | Navigate to previous directory | button "Back" | disableable disabled | File list updates, breadcrumb changes | disabled when at history start
NAV_FORWARD | Navigate forward in history | button "Forward" | disableable disabled | File list updates, breadcrumb changes | disabled when at history end
NAV_UP | Go to parent directory | button "Up" | disableable disabled | File list updates, breadcrumb changes | disabled at root level
NAV_REFRESH | Reload current directory | button "Refresh" | N/A | Loading indicator disappears | Usually always enabled
NAV_HISTORY | Show/hide navigation history | button "Toggle History" | N/A | N/A | No disabled state

FILTER_ALL | Show all file types | button "Filter: Allow All Files" | Visual active state | File list updates | Click to apply
FILTER_GAMES | Show game files only | button "Filter: Games Only" | Visual active state | File list updates | Click to apply
FILTER_MUSIC | Show music files only | button "Filter: Music Only" | Visual active state | File list updates | Click to apply
FILTER_IMAGES | Show image files only | button "Filter: Images Only" | Visual active state | File list updates | Click to apply

SEARCH_BOX | Input search query | textbox "Search" | value="search_term" | Search results appear | Fill with search term; results appear immediately
SEARCH_CLEAR | Reset search filter | button "Clear search results" | N/A | File list returns to full directory | Appears only when search active
RANDOM_LAUNCH | Launch random track | button "Launch Random File" | N/A | metadata updates, playback starts | Selector: dice/random icon button

TREE_STORAGE | Storage device tree | tree "" orientation="vertical" | N/A | N/A | Contains device and storage toggles
TREE_DEVICE_TOGGLE | Toggle device in tree | button "Toggle Device {DEVICE_ID}" | Treeitem expanded/collapsed | File list updates | Example: "Toggle Device PSM2ZAKI"
TREE_USB_TOGGLE | Toggle USB storage | button "Toggle USB Storage" | Treeitem expanded/collapsed | File list updates | Under device tree item
TREE_SD_TOGGLE | Toggle SD storage | button "Toggle SD Storage" | Treeitem expanded/collapsed | File list updates | Under device tree item

BREADCRUMB_STORAGE | Current storage label | StaticText "SD Card" or "USB Stick" | N/A | N/A | Shows current storage device
BREADCRUMB_PATH | Directory path segments | StaticText with dir names | N/A | N/A | Example path: "SD Card" > "music" > "MUSICIANS" > "S" > "Simon_Laszlo"
BREADCRUMB_LOADING | Loading indicator | StaticText containing "loading" or " L" | N/A | Text disappears | Dynamic spinner; indicates async operation in progress

DIR_HEADING | Current directory label | heading "Current Directory" level="2" | N/A | N/A | Appears before file list
DIR_LISTING | File list item | button "filename.extension size" | N/A | Metadata updates on click | Example: "3_Years_2.sid 4.7 KB"; parse with regex to extract name, ext, size
SEARCH_HEADING | Search results label | heading "Search Results" level="2" | N/A | N/A | Appears when search active

PANEL_ARTIST | Current artist display | StaticText with artist name | N/A | Updates on file launch | Example: "László Simon (Roy)"
PANEL_ALBUM_ART | Album cover image | image "Current image" | N/A | Image source changes | Left panel; updates with track
PANEL_SONG_TITLE | Song title heading | heading "Player" level="2" | N/A | Text updates on launch | Main heading; displays current track
PANEL_METADATA_DEEPSID | DeepSID indicator | StaticText "DeepSID" | N/A | N/A | Database source indicator
PANEL_METADATA_REGION | System region | StaticText "PAL" or "NTSC" | N/A | N/A | System region info
PANEL_METADATA_CHIP | SID chip type | StaticText "8580" | N/A | N/A | SID chip type
PANEL_HVSC_SECTION | HVSC STIL header | heading "HVSC STIL" level="4" | N/A | N/A | Metadata section header
PANEL_HVSC_INFO | HVSC metadata text | StaticText with HVSC content | N/A | N/A | Title, artist, comment info
PANEL_LINKS_SECTION | Links header | heading "LINKS" level="4" | N/A | N/A | External links section header
PANEL_LINK_ITEM | External link | link "text (opens in new window)" | N/A | N/A | Example: "CSDB Release", "CSDB Profile", "CSDb"
PANEL_TAGS_SECTION | Tags header | heading "TAGS" level="4" | N/A | N/A | Tags section header
PANEL_TAG_ITEM | Individual tag | StaticText with tag name | N/A | N/A | Example: "Cover", "HVSC / DeepSID", "$31"

PLAYBACK_INFO_MINI | Now playing card | StaticText "filename" near progress bar | N/A | Updates on new track | Small card showing current song + artist

EXTRACTION_TIME | Parse playback time | Look for: StaticText "MM:SS / MM:SS" | N/A | N/A | Example: "00:25 / 04:39"; split(" / ") to [current, total]
EXTRACTION_FILE | Parse file entry | button with "name.ext size" | N/A | N/A | Example: "3_Years_2.sid 4.7 KB"; regex: (\w+)\.(\w+)\s+([\d.]+)\s+(\w+)
EXTRACTION_PATH | Build breadcrumb path | Collect StaticText between storage and dir | N/A | N/A | Order: left to right shows hierarchy depth

WORKFLOW_PLAY_FILE | Play specific file | 1. Find: button "filename.sid" 2. Click 3. Check: button "Pause" exists 4. Verify: progressbar value > 0 | N/A | progressbar value > 0 | Standard play workflow
WORKFLOW_SEARCH_PLAY | Search and play | 1. Click: textbox "Search" 2. Fill search term 3. Wait: heading "Search Results" 4. Click result 5. Verify playback | N/A | heading "Search Results" visible, then playback starts | Search followed by launch
WORKFLOW_NAVIGATE | Folder navigation | 1. Click: button "Up" 2. Wait: loading ends 3. Verify: breadcrumb updates 4. Check: file list refreshes | N/A | Breadcrumb changes, loading spinner gone | Multi-level directory traversal
WORKFLOW_FILTER | Apply file filter | 1. Click: button "Filter: Music Only" (etc) 2. Wait: directory reloads 3. Verify: file list shows filtered types | N/A | File list updates, loading spinner gone | Filter persistence across navigation
WORKFLOW_RANDOM | Launch random file | 1. Click: button "Launch Random File" 2. Wait: metadata loads 3. Verify: artist/title changed 4. Check: playback started | N/A | Artist/title changes, progressbar value > 0, time updates | Discovery mode
```

**Programmatic Parsing Guide:**

- File entries: Regex `(\w+)\.(\w+)\s+([\d.]+)\s+(\w+)` extracts [filename, extension, size_value, size_unit]
- Playback time: Split StaticText "HH:MM / HH:MM" by " / " to get [current, total]
- Breadcrumb: Collect sequential StaticText between storage label and "Current Directory" heading
- Loading check: Monitor for StaticText containing "loading" or spinner character animation
- Playback state: Play (button "Play" visible) vs Pause (button "Pause" visible)
- Disabled controls: Check for `disableable disabled` attribute
- Active filters: Cross-reference button appearance with file list contents

**Async Wait Signals:**

- Directory load complete: Loading indicator disappears from breadcrumb area
- File metadata ready: Artist/title text appears, HVSC section populated
- Search results ready: `heading "Search Results"` visible
- Playback started: `progressbar ""` has `value > 0`, time text updates
- Navigation complete: Breadcrumb path changed, file list updated

```

```
