Ok, let's design a music player store that will track the player state. We're going to focus on what we need to create, not how!!! So ignore the code for now and just focus on the User perspective, not the code for now!!

General:

- There will be an dependent player state per TR device (we're being a combination media player / DJ system).
- All commands listed below will come with an associated backend api call.
- Generally, files can be launched explictely by selecting them from a directory. But most of the logic here is about player controls that will have unique behaviors.

The player is going to work across a variety of file types:

- Games
- Music
- Images

For all types:

- You can play the file.
- Next and Previous will generally launch the next or previous file, with some nuances per type.
- Double clicking a file in the file listing will automatically play the file
- So there will be a stateful concept of "Current" file being played that we'll need to track
- There will a Next and Previous button to play the next file or previous file. (See: Launch Modes to determine the file played next or previous)

Filter Mode:

- We're going to have a way to filter files for the purpose of selecting the next song to play.
- Filter Types: Games, Music, Images, or All (any type)
- We'll have a control per mode that can be set by the user.

Launch Mode State:

- These apply to all file types.
- Directory Mode
  - Plays files sequentially within the directory of the currently playing file.
- Shuffle Mode (also known as random) will play a random file.
  - This will have unique control to enable it.
  - Shuffle mode has a scope that can be set:
    - TR Device Global Mode (selects a file across all storage devices, USB and SD)
    - Storage Device Global Mode (selects a file across a single storage device -- same device for the currently playing file)
    - Directory Pinning
      - Directories can be pinned to set a more specific scope.
    - Directory Shallow Mode - Random file within the current pinned directory
    - Directory Deep Mode - Random file within the current pinned directory and any sub-directory below it.
  - Play History
    - When in shuffle mode, we'll keep a play history
    - So hitting previous will go to the last song we played.
    - Like in a browser, if we go backward in history and then launch something new, forward history gets cleared.
  - If a file in a directory is launched directly, we'll automatically switch out of Shuffle Mode into directory mode.

Games:

- Can be only stopped.
- Games usually do not have a timer like songs. BUT, we'll have a special "timer" mode that can be enabled for games. (See: Play Timer)

Songs:

- Can be Paused or resumed.
- Pausing or resuming will trigger toggle command to the backend.
- We'll have a progress bar that will track the current position the song is playing.
- We'll need a timer to track this.
- The timer is based on song metadata that tells us the song duration.
- So we'll set time timer to the duration of the song.
- When the time ends, we'll take an action to launch another song (See: Play Modes)
- When the next song is launched, the timer will be reset to 0 and the duration set to the new songs duration.
- Unique Previous Button Behavior:
  - If the song timer is less than 5s, the previous song is launched.
  - If over 5s, the song is restarted.
- Unique Launch Mode: Loop
  - The song does not need to be re-launched with a backend call. The backend handles that automatically.
  - Repeat will simply restart the timer and continue playing the current song.

Custom Play Timer:

- Although Songs have an associated duration, games and images do not.
- So we have a special mode that enables a play timer with a progress bar.
- Play timer can be toggled at any time with a unique control.
- When play timer is toggled, progress bar appears and vice versa (unless it's a song)
- Play timer will only tick if we're playing a file. So a stopped or paused state will pause the timer.
- When we play a game or image with timer mode is enabled, when the timer ends, we'll go to the next file (next file type is determined by the filter).
- We'll have an option to set the duration of the play timer. (5s, 10s, 15s, 30s, 1m, 3m, 5m, 10m, 15m, 30m, 1hr)
- Timer Mode: We'll have an additional option to override the play time of a song instead of using the songs normal duration.
- If we're playing songs,
  - Play timer does not need to be enabled.
  - Unless the play timer is enabled with the Song override option, songs will always play sequentially (or repeat) based on their duration

Special Song Behaviors:

- All behaviors will require a backend call to engage.
- Speed is defined as the delta of the original song speed (0%) + delta. e.g., +-10% being a delta of speed increase/decrease.
- Base Speed: is defined as the currently explicitly speed changed by the "Set Speed" functions.
- Other functions besides "Set Speed" are considered momentary speed changes.
- So we're going to basically want to track the base speed and altered speed independently.
- Altered speed will always be a function calculated from the currently user-defined base speed.

- Set Song Speed:

  - We will have the ability to speed up or slow down a song using a set speed control.
  - Changing the set speed control will set the user-defined "base speed"
  - The song timer speed will also increase or decrease at the same rate.
  - We'll have an option to change the song speed in increments of a tenth of a percent.
  - We'll have 2 speed curves: Linear and Logarithmic.
  - We'll need to keep track of the set speed for various operations (Seek, Nudge, +-50% speed jump. See docs below)

- Set Song Speed Fine:

  - Like Set Song Speed, but the speed control is a delta of one hundredth of a percent.

- Home Speed:
  - Resets the song base speed delta to 0% (original speed)

-Song Seek:

- We will have the ability seek to a point in a song using a typical media player scrub control.
- This is not like a normal seek/scrub feature. We're dealing with SID files here.
- SID files cannot be randomly seeked to a specific point in time.
- For seeking forward, we need to fast forward at tremendous speed to the point of time we want to play at.
- For seeking backward, we need to restart the song and fast forward to the desired point in time.
- The fast forward speed will have 2 modes: Accurate and Insane.
- Accurate Speed: +1000%
- Insane Speed: +10,000%
  - insane generally falls out of sync with the timer and not useful for use cases requiring accuracy in seek / timer sync.
  - use cases are limited, but it's fun and can create unique dj "effect" opportunities.
- When seeking, the timer speed will be adjusted according to the seek mode.
- When we arrive to the time we're seeking to, we'll set the speed back to the previously user set speed.
- Hitting play, pause or any speed altering behaviors will cancel the seeking and revert the currently base speed + relevant altered speed for given behavior.

- Song Nudge:

  - The purpose of this function is for "beat matching" 2 playing songs to get them in sync.
    - Remember: Changing the set speed control will set the user-defined "base speed"
  - We will have the ability to "nudge" or adjust the speed temporarily +-5%.
  - We'll have one control to nudge +5% and a separate control to nudge -5%.
  - When the nudge is released, we'll go back to the base speed.
  - Special conditions:
    - Nudge is always a function of the base speed.
    - If the song base speed is set to +10% by the user
      - Engaging nudge will change the temp speed to +-15%
      - Disengage goes back to 10% (base speed).
    - If the user changes the base speed while nudge is engaged
      - The user's change will adjust the base speed.
      - The nudge will always calculate based on the current base speed.
      - When the nudge releases, it should revert to the current base speed.

- Song Speed Jump

  - Similar to nudge, except we'll have a +-50% option
  - It's a different use case to play the song at half time or double time and usually intended for interesting "effect" in a mix set.
  - Jump speed DOES NOT alter the base speed. It acts just like nudge, setting a calculated "Altered Speed"
  - If Song Jump Speed is engaged while nudge is engaged, it is cumulative for a total of +-55%
  - If either is mutually disengaged, you'll jump to the expect +-5% or +-50% accordingly.

- Song Hold

  - This is a function that effectively pauses the song, but with a unique use case.
  - This is intended as a live DJ effect or as utility to "cue" a song for "release" when mixing 2 songs.
    - Like a DJ holding a record and release it to time the syncing of the songs.
  - Uses the same mechanism as pausing a song.
  - But the control is different as it'll engage on mouse down and release on mouse up.
  - Normal pause is more like a switch where a mouse-up and mouse down need to happen to engage.

- Fast Forward

  - This is a special 4-step behavior for fast(50%), faster(100%), even faster(200%), fastest(1000%)
  - After you toggle through the 4-steps, you're returned to the base speed.
  - Like nudge and jump speed,
    - calculated as of function dynamically alterable base speed

- Restart Song

  - This is similar in mechanics to a "Previous" button click, except there is no 5-second rule (the song always restarts).
  - This will have a different control from the Previous button.
  - It'll trigger on mouse down -- Unlike "Previous" which triggers on mouse up.

- Definition: SID Voices

  - A SID song has 3 voice or tracks.
  - These voices can be enabled or disabled.
  - Voices are defined as Voice 1, Voice 2, Voice 3
  - Voices can be changed one at a time, All at once, or any combination in between.

- Toggle Voice

  - Toggles one or more voices.
  - Each voice has a unique control.
  - Mouse up and mouse down combination will enable or disable a voice.

- Kill/Activate Voice
  - Similar to Toggle voice -- 3 distinct controls.
  - By default, it's a "kill switch".
  - Mouse down engages and Mouse Up disengages.
  - If a voice is already disabled by "Toggle Voice",
    - the state is respected and "Kill voice" becomes "activate voice" and vice versa.
    - This is for utility -- and also simplifies the state tracking logic.
