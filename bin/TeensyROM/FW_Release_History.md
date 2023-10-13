
# FW Release Version history:

### 0.5.2 sub-release 10/10/23
* SID Player
  * Determining Vid standard (NTSC/PAL) and mains freq (50/60Hz) on start
  * Changed SID play interrupt from raster to timer based
  * '+' and '-' to change SID speed from main Menu
  * Playback speed set based on SID and Machine type
  * stopped border color tweak for now since IRQ is not raster
  * Banking out BASIC and Kernal during SID play and init
  * check for SID/TR mem conflict (eventually enable TR code relocation)
  * SID file type association/selectability

* Swiftlink updates:
  * ATSEARCH command initial implementation (more to come)
  * Using www.frogfind.com to get search results

* Arhitecture/General:
  * Changed back to 8k cart from 16k for main menu
  * Changed C64 RAM location from $2400 to $6000
  * Loading SID from Flash after main app startup instead of transfer from cart
  * Removed USB Host menu since file x-fer is direct to USB/SD
  * Menu tweak: F7 for help instead of Space
  * Removed IO1 regs rRegStrAddrLo/Hi, just get from stream
  * First Self modifying code (smc) for smcSIDPlay and smcSIDInit

### 0.5.1 sub-release 10/1/23
* MIDI CC messaging now passthrough/absolute instead of relative/calculated
* Enabled USB MIDI Device In messages: 
  * ControlChange, ProgramChange, and PitchChange
* Revamped Windows File Transfer app support (see [WinApp Release History](../WinApp/WinApp_Release_History.md))

## 0.5: built 9/17/23  14:01:48
* Main UI  improvements:
  * Many ROMs added to main menu (with room for more)
  * 1 level sub-dirs from TR Mem menu
  * Cursor based navigation/selection
  * Joystick 2 menu control: Joystick speed in Settings/EEPROM
  * Display parent dir Path at top
  * Display page number and num pages in Menu/directory
  * alphabetize the directory list   
  * Home key for top of current directory
  * Search in dir for first letter a-z
  * Up arrow for up directory
  * Associate special IO HW with Teensy mem menu items: '+' next to type on TR Mem Menu
  * Longer file names and directory sizes
  * Messaging added for load/xfer/parse opperations
  * Settings menu re-do
  * Help Menu added, space to access   Fx options live in Help Menu
    
* New File/type support:
  * .P00: now supported
  * .CRT: added support for the following types: EasyFlash, Magic Desk, Ocean, Dinamic, Zaxxon/Super Zaxxon, Game System 3, SuperGames, FunPlay/PowerPlay
    * Note: max file size ~650MB, no EasyFlash eapi support

* Swiftlink/Ethernet improvements:
  * Modified built-in CCGMS to start with Swiftlink/38.4k by default
  * added "at?" to list all AT commands
  * upper or lower case accepted in ANSCII or Graphics modes
  * correct backspace handling

* MIDI improvements: 
  * disabled some unused usbDevMIDI(in) commands causing probs using Cakewalk with Sta64/Cynthcart

## 0.4: built 7/29/23  22:56:39
* Enabled TeensyROM as a MIDI USB Device. 
  * USB MIDI Host is still present, can use both at the same time.
* Update TeensyROM Firmware from SD card or USB Drive. 

## 0.3: built 7/15/23  18:56:01
   
## 0.2: 3/16/23
   
## 0.1: 2/9/23
   
## Initial commit: 1/11/23