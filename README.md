# TeensyROM Desktop UI
Command your Commodore 64 / 128 with a modern desktop user experience.  This UI frontend integrates with the [TeensyROM Hardware Cartridge](https://github.com/SensoriumEmbedded/TeensyROM), *designed by Travis S/Sensorium*, to provide robust and performant file transfer, exploration and remote launch capability to your 8-bit machine.  Metadata enhancements also bring a rich user experience to the exploration process.  

## Features
- Media Player / File Explorer hybrid interface
- Instant remote launch of your games and SID music on a C64/128
- Random play / shuffle mode capabilities for easy discovery of new content
- Search capabilities to find instantly locate specific files in massive collections
- Tag your favorite titles so you don't lose track of them 
- Continuous playback of SID music creates a hands free streaming service-like experience
- Enhanced with SID composer, song lengths and scene information with metadata integration from [HVSC](https://www.youtube.com/watch?v=lz0CJbkplj0&list=PLmN5cgEuNrpiCj1LfKBDUZS06ZBCjif5b) and [DeepSID](https://github.com/Chordian/deepsid)
- Enhanced with game preview screenshots with metadata integration from [OneLoad64](https://www.youtube.com/watch?v=lz0CJbkplj0&list=PLmN5cgEuNrpiCj1LfKBDUZS06ZBCjif5b)
- Transfer files to your TeensyROM SD/USB storage with drag and drop capabilities
- Set a watch folder on your desktop to automatically sync files to your TeensyROM SD/USB storage.

## Requirements
- A Commodore 64/128 personal computer
- A windows 10/11 laptop, tablet or desktop computer
- [TeensyROM Hardware Cartridge](https://github.com/SensoriumEmbedded/TeensyROM)
- Micro-USB Cable
- NET 8 Runtime
- Games and SIDs!

## Installation
- Install the latest TeensyROM Firmware.  0.5.12 or later is required.
  - Latest firmware (.hex file) found [here](https://github.com/SensoriumEmbedded/TeensyROM/blob/main/bin/TeensyROM/FW_Release_History.md)
- Download the latest [release](https://github.com/MetalHexx/TeensyROM-UI/releases)
- It comes bundled with a locally bundled NET 8 runtime.
  - If run into issues, try the installing the runtime found [here](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- Unzip the release into a directory of your choice
- Make a USB connection between the computer and the micro-SD 
 
## Quick Start
- Firstly, ensure your have your TeensyROM SD/USB storage filled with some games and sids.
  - See the help guides below for how to install HVSC and OneLoad64.
  - They're not required but highly recommended.  You will get a lot of cool information about SID music, composers and game previews
- Execute TeensyRom.Ui.exe
- You will get a Windows Defender / SmartScreen warning.  This is due to the application not being signed with a certificate authority yet.
  - Click "More Info" --> "Run anyway"
- You'll be greeted with a connection screen.  Press the "Connect" button.
  - *Note -- when "Auto-detect" is selected in the drop down, the application will attempt to the correct COM port.*
- Go to "Settings" by clicking the "Gear" icon in the left navigation menu
- Configure the library paths to point to your game and music folders on your chose storage device
  - This step will determine where your favorites and auto-transfer files are placed.
- Click the save button
- Click on the "Compass" icon in the left navigation menu
- Click the "Download" icon in the upper right corner of the screen to cache all your file locations.
  - The dialog that pops up will further explain why this is important.
- Click the "Dice" icon in the upper right corner of the screen (next to the "Download" button)
- Enjoy the ride! For more details on the various features available, see the "Help Guides" below.

## Help Guides
Coming soon.

## Who is this for?
#### Modern Desktop Warriors
If you multitask a lot and don't always have a lot of time to sink into a full C64 play session, this is for you.  Turn on the C64, load the UI, click random, and you get instant gratification from a quick play session.

#### Kids
Kids have short attention spans.  The randomization features are invaluable here and will keep them engaged for much longer.

#### Chiptune Lovers 
This is the retro jukebox you've been looking for.  The continuous playing capability turns your 8-bit machine into a streaming service so can enjoy tunes hands free while you do other things.  Nothing beats a true hardware SID experience. :)

#### Purists 
Sure, this lacks the oldschool nostalgia.  But I challenge you to give it a shot.  Try the SID music player while you're busy doing other things.  It also has plenty of file transfer utility to reduce the SD card / USB juggling operation.  

## The Project Story
I am a late bloomer commodore 64 nerd.  I found a C64 at an auction and thought, hey, maybe I can find a way to control one of these with MIDI to make some music.  I started researching ways to get some games on this thing.  

I experimented with KungFu Flash which was really cool.  I pondered an Ultimate II+.  The problem was, none of them had midi and the Kerberos is impossible to find.  But then -- I found TeensyROM and it had MIDI capability too.  Perfect!

The one thing that all these amazing carts have in common is that they're kind of slow at navigating over thousands of files.  It's a bit overwhelming. Trying to find that one game you played before but having trouble? I've been there. :) This project aims to solve that problem.

I contacted the creator, Mr Travis S, and started brainstorming.  After a lot of hard work, late nights and bugging Travis with crazy ideas, I now offer this UI and hope it finds a place in your retro workflow. Enjoy!
