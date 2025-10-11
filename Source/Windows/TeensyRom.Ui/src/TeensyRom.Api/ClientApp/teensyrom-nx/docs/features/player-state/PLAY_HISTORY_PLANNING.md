# Play History Feature Planning

**Project Overview**: Add comprehensive play history tracking and navigation to the TeensyROM player, enabling users to view their playback timeline and navigate backward/forward through previously played files in shuffle mode.

**Standards Documentation**:

- **Coding Standards**: [CODING_STANDARDS.md](../../CODING_STANDARDS.md)
- **Testing Standards**: [TESTING_STANDARDS.md](../../TESTING_STANDARDS.md)
- **State Standards**: [STATE_STANDARDS.md](../../STATE_STANDARDS.md)
- **Domain Standards**: [DOMAIN_STANDARDS.md](../../DOMAIN_STANDARDS.md)
- **Style Guide**: [STYLE_GUIDE.md](../../STYLE_GUIDE.md)
- **Player Domain Design**: [PLAYER_DOMAIN_DESIGN.md](./PLAYER_DOMAIN_DESIGN.md)

## 🎯 Project Objective

Enable users to track and navigate through their playback history across all player modes (Directory, Shuffle, Search). The history system provides a familiar browser-like navigation experience, particularly valuable in shuffle mode where users can revisit previously played files. The feature includes a dedicated history view component that can be toggled on/off, intelligently switching between directory navigation, search results, and play history based on user context.

**User Value**: Users in shuffle mode can navigate backward through their play history to replay favorite discoveries, while all users benefit from a complete playback timeline showing what they've listened to or viewed. The browser-style forward/backward navigation feels intuitive and familiar.

---

## 📋 Implementation Phases

<details open>
<summary><h3>Phase 1: Core History Tracking Infrastructure</h3></summary>

### Objective

Establish the foundational state management for tracking play history, including state structure, history recording logic, and forward/backward history management with browser-style clearing behavior.

### Key Deliverables

- [ ] Play history state structure added to player store
- [ ] History recording action that captures all file launches
- [ ] Forward history clearing logic when navigating after going backward
- [ ] History state selectors for reading history data
- [ ] Device cleanup removes history when device disconnects

### High-Level Tasks

1. **Define History State Model**: Create the data structure that holds history entries with file information, timestamps, launch modes, and browser-style navigation tracking
2. **Add History to Player State**: Integrate history tracking into the existing device player state structure
3. **Create History Recording Action**: Build action that captures file launches and adds them to history with proper forward history clearing
4. **Build History Selectors**: Create selectors to read history data, current position, and navigation capabilities
5. **Update Device Cleanup**: Ensure history is properly cleared when devices disconnect

### Open Questions for Phase 1

- **History Entry Deduplication**: If the same file is played multiple times, should each play create a new history entry, or should we group consecutive duplicates?
- **Maximum History Size**: What is the optimal maximum number of history entries before we start removing oldest entries?

</details>

---

<details open>
<summary><h3>Phase 2: History Navigation in Shuffle Mode</h3></summary>

### Objective

Implement backward and forward navigation through play history when in shuffle mode, replacing the current "launch another random file" behavior for previous navigation.

### Key Deliverables

- [ ] Navigate backward in history action (shuffle mode only)
- [ ] Navigate forward in history action (shuffle mode only)
- [ ] Integration with existing previous/next navigation controls
- [ ] Proper state updates when navigating through history
- [ ] Error handling for empty history or invalid positions

### High-Level Tasks

1. **Create Navigate Backward Action**: Build action that moves backward through history and launches the previous file in shuffle mode
2. **Create Navigate Forward Action**: Build action that moves forward through history when available in shuffle mode
3. **Update Existing Navigation**: Modify the current navigate previous logic to use history navigation instead of random file launching in shuffle mode
4. **Test History Boundaries**: Ensure proper behavior at the start and end of history
5. **Validate Mode Restrictions**: Confirm history navigation only works in shuffle mode as intended

### Open Questions for Phase 2

- **Wraparound Behavior**: When at the start of history in shuffle mode and user clicks previous, should it wrap to the end, stay at start, or disable the button?

</details>

---

<details open>
<summary><h3>Phase 3: Play History UI & View Switching Integration</h3></summary>

### Objective

Create the visual components that display play history and integrate them into the storage container's view management system, implementing intelligent switching between directory files, search results, and play history based on user context and mode.

### Key Deliverables

- [ ] Play history component displaying chronological file list
- [ ] History toggle button in directory trail
- [ ] History visibility state management in player store
- [ ] Storage container manages three view states (directory, search, history)
- [ ] Automatic history hiding when searching or navigating directories
- [ ] File selection and currently playing indicators
- [ ] Empty state handling when no history exists
- [ ] Smooth view transitions and state persistence

### High-Level Tasks

1. **Build Play History Component**: Create component similar to search results that displays history entries with file details and timestamps
2. **Add Toggle Button**: Create toggle button in directory trail for showing/hiding history view
3. **Manage Visibility State**: Add `historyViewVisible` boolean to player state, track per-device
4. **Update Storage Container Logic**: Modify view switching logic to handle directory files, search results, and play history views
5. **Implement Automatic Hiding**: Hide history when search becomes active or directory navigation occurs
6. **Style History Entries**: Apply consistent styling with highlighting for currently playing file and selection states
7. **Handle Empty History**: Display appropriate empty state message when history is empty
8. **Polish Transitions**: Ensure smooth animations and state transitions between different views

### Open Questions for Phase 3

- **Timestamp Display**: Should history entries display relative times (e.g., "2 minutes ago") or absolute times (e.g., "3:45 PM")?
- **History View Default State**: Should history view be visible by default when entering shuffle mode, or hidden by default requiring toggle?
- **Animation Timing**: What animation duration should we use for view transitions between directory/search/history views?

</details>

---

<details open>
<summary><h2>🏗️ Architecture Overview</h2></summary>

### Key Design Decisions

- **Browser-Style History**: Use familiar forward/backward navigation pattern where going backward then launching a new file clears forward history, matching user expectations from web browsing
- **Shuffle-Only Navigation**: Restrict backward/forward history navigation to shuffle mode only, while still tracking history in all modes for visibility and reference
- **View Priority System**: Establish clear precedence for view visibility (search results > play history > directory files) to avoid conflicting states
- **State Locality**: Store history visibility toggle state per-device in player state to support multi-device scenarios independently

### Integration Points

- **Player Store**: History tracking integrates directly into existing device player state alongside current file and file context
- **Navigation Actions**: History navigation actions coordinate with existing navigate previous/next actions, conditionally replacing shuffle mode behavior
- **Storage Container**: View switching logic extends to manage three views instead of two, with intelligent automatic hiding
- **Directory Trail**: Toggle button adds new user control alongside existing navigation buttons (back, forward, up, refresh)

</details>

---

<details open>
<summary><h2>🧪 Testing Strategy</h2></summary>

### Unit Tests

- [ ] History state structure initialization and cleanup
- [ ] History recording action adds entries correctly
- [ ] Forward history clearing when navigating after going backward
- [ ] Navigate backward/forward actions update state correctly
- [ ] Selectors return correct history data and navigation capabilities
- [ ] Mode restrictions prevent history navigation outside shuffle mode

### Integration Tests

- [ ] Complete history tracking flow across different launch modes
- [ ] History navigation integration with player service file launching
- [ ] View switching logic between directory, search, and history views
- [ ] Toggle button interaction with history visibility state
- [ ] Multi-device independent history tracking
- [ ] Device cleanup properly removes all history state

### E2E Tests

- [ ] User launches files in shuffle mode and navigates backward through history
- [ ] User toggles history view on/off in directory trail
- [ ] Search automatically hides history view and shows search results
- [ ] Directory navigation hides history view and shows directory files
- [ ] Forward history clears when user launches new file after going backward
- [ ] History displays correct files with proper highlighting and selection

</details>

---

<details open>
<summary><h2>✅ Success Criteria</h2></summary>

- [ ] Play history is tracked for all file launches regardless of mode (Directory, Shuffle, Search)
- [ ] Users can navigate backward/forward through history in shuffle mode using previous/next controls
- [ ] Going backward in history then launching a new file clears forward history (browser behavior)
- [ ] Play history component displays chronological list of played files
- [ ] Directory trail toggle button shows/hides play history view
- [ ] Searching automatically hides play history and reveals search results
- [ ] Directory navigation automatically hides play history and reveals directory files
- [ ] View switching is smooth with proper state management across modes
- [ ] Multi-device support maintains independent history per device
- [ ] All unit, integration, and E2E tests pass successfully
- [ ] Feature ready for production deployment

</details>

---

<details open>
<summary><h2>🎭 User Scenarios</h2></summary>

### History Tracking Scenarios

<details open>
<summary><strong>Scenario 1: Track File Launch in Directory Mode</strong></summary>

```gherkin
Given a user is browsing directory files
When the user double-clicks a file to launch it
Then the file is added to play history with a timestamp and Directory launch mode
```
</details>

<details open>
<summary><strong>Scenario 2: Track File Launch in Shuffle Mode</strong></summary>

```gherkin
Given a user is in shuffle mode
When a random file is launched
Then the file is added to play history with a timestamp and Shuffle launch mode
```
</details>

<details open>
<summary><strong>Scenario 3: Track File Launch in Search Mode</strong></summary>

```gherkin
Given a user has search results displayed
When the user double-clicks a search result to launch it
Then the file is added to play history with a timestamp and Search launch mode
```
</details>

---

### History Navigation Scenarios

<details open>
<summary><strong>Scenario 4: Navigate Backward in Shuffle Mode</strong></summary>

```gherkin
Given a user has launched 3 files in shuffle mode
When the user clicks the previous button
Then the previously played file is launched from history
```
</details>

<details open>
<summary><strong>Scenario 5: Navigate Forward After Going Backward</strong></summary>

```gherkin
Given a user has navigated backward in shuffle history
When the user clicks the next button
Then the next file in forward history is launched
```
</details>

<details open>
<summary><strong>Scenario 6: Clear Forward History After New Launch</strong></summary>

```gherkin
Given a user has navigated backward in shuffle history (forward history exists)
When the user launches a new random file
Then the forward history is cleared and the new file becomes the current history point
```
</details>

<details open>
<summary><strong>Scenario 7: Previous Button in Directory Mode (No History)</strong></summary>

```gherkin
Given a user is in directory mode with file context
When the user clicks the previous button
Then the previous file in the directory context is launched (existing behavior, not history navigation)
```
</details>

<details open>
<summary><strong>Scenario 8: No Backward Navigation at Start of History</strong></summary>

```gherkin
Given a user is at the beginning of play history in shuffle mode
When the user clicks the previous button
Then navigation wraps or is disabled (to be determined during implementation)
```
</details>

---

### UI View Switching Scenarios

<details open>
<summary><strong>Scenario 9: Toggle History View On in Shuffle Mode</strong></summary>

```gherkin
Given a user is in shuffle mode viewing directory files
When the user clicks the history toggle button
Then the directory files view is hidden and play history view is shown
```
</details>

<details open>
<summary><strong>Scenario 10: Toggle History View Off</strong></summary>

```gherkin
Given a user is viewing play history in shuffle mode
When the user clicks the history toggle button
Then the play history view is hidden and directory files view is shown
```
</details>

<details open>
<summary><strong>Scenario 11: Search Hides History View</strong></summary>

```gherkin
Given a user has history view visible
When the user performs a search
Then the history view is automatically hidden and search results are shown
```
</details>

<details open>
<summary><strong>Scenario 12: Directory Navigation Hides History View</strong></summary>

```gherkin
Given a user has history view visible
When the user navigates to a different directory
Then the history view is automatically hidden and directory files are shown
```
</details>

<details open>
<summary><strong>Scenario 13: History View Persists Across File Launches in Shuffle</strong></summary>

```gherkin
Given a user has toggled history view on in shuffle mode
When the user launches a new random file
Then the history view remains visible showing the updated history
```
</details>

---

### Multi-Device Scenarios

<details open>
<summary><strong>Scenario 14: Independent History Per Device</strong></summary>

```gherkin
Given two devices are connected (Device A and Device B)
When files are launched on both devices
Then each device maintains separate independent play history
```
</details>

<details open>
<summary><strong>Scenario 15: Device Disconnect Clears History</strong></summary>

```gherkin
Given a device has accumulated play history
When the device disconnects
Then the play history for that device is cleared from state
```
</details>

---

### Edge Cases and Error Handling

<details open>
<summary><strong>Scenario 16: Empty History Display</strong></summary>

```gherkin
Given a user has just connected a device with no play history
When the user toggles history view on
Then an empty state message is displayed indicating no playback history exists
```
</details>

<details open>
<summary><strong>Scenario 17: History After File Launch Error</strong></summary>

```gherkin
Given a user attempts to launch a file that results in an error
When the file fails to launch
Then the failed file is NOT added to play history
```
</details>

<details open>
<summary><strong>Scenario 18: Currently Playing File Highlighted in History</strong></summary>

```gherkin
Given a user is viewing play history
When a file from history is currently playing
Then that file is visually highlighted in the history list
```
</details>

</details>

---

<details open>
<summary><h2>📚 Related Documentation</h2></summary>

- **Player Domain Design**: [`PLAYER_DOMAIN_DESIGN.md`](./PLAYER_DOMAIN_DESIGN.md)
- **Architecture Overview**: [`OVERVIEW_CONTEXT.md`](../../OVERVIEW_CONTEXT.md)
- **Coding Standards**: [`CODING_STANDARDS.md`](../../CODING_STANDARDS.md)
- **Testing Standards**: [`TESTING_STANDARDS.md`](../../TESTING_STANDARDS.md)
- **State Standards**: [`STATE_STANDARDS.md`](../../STATE_STANDARDS.md)

</details>

---

<details open>
<summary><h2>📝 Notes</h2></summary>

### Design Considerations

- **History Size Limits**: Consider implementing a maximum history size (e.g., 100 entries) to prevent unbounded state growth over long sessions
- **History Persistence**: Current design stores history in memory only; future enhancement could persist history to local storage for cross-session continuity
- **Timestamp Display**: History entries include timestamps; consider displaying relative times (e.g., "2 minutes ago") vs absolute times
- **History in Non-Shuffle Modes**: While history is tracked in all modes, navigation only works in shuffle mode; directory and search modes use their existing file context navigation

### Future Enhancement Ideas

- **History Search/Filter**: Allow users to search or filter their play history
- **Favorite/Bookmark from History**: Let users mark history entries as favorites
- **History Statistics**: Show playback statistics (most played, recently played, etc.)
- **Clear History Action**: Provide user control to clear all or partial history
- **History Export**: Export play history for external analysis or backup

### Summary of Open Questions

These questions are distributed across their respective phases above:

**Phase 1:**
- History Entry Deduplication: Should each play create a new history entry, or group consecutive duplicates?
- Maximum History Size: What is the optimal maximum number of history entries?

**Phase 2:**
- Wraparound Behavior: When at the start of history in shuffle mode and user clicks previous, should it wrap to the end, stay at start, or disable the button?

**Phase 3:**
- Timestamp Display: Should history entries display relative times or absolute times?
- History View Default State: Should history view be visible by default when entering shuffle mode, or hidden by default?
- Animation Timing: What animation duration should we use for view transitions?

</details>
