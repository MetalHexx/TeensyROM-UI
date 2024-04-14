# TeensyROM Desktop UI
Command your Commodore 64 / 128 with a modern desktop user experience.  This UI frontend integrates with the [TeensyROM Hardware Cartridge](https://github.com/SensoriumEmbedded/TeensyROM), *designed by Travis S/Sensorium*, to provide robust and performant file transfer, exploration and remote launch capability to your 8-bit machine.  Metadata enhancements also bring a rich user experience to the exploration process.  

## Features
- Media Player / File Explorer hybrid interface
- Remote Launch of Games, SIDS, Images, Firmware Updates
- Launch Randomization by File Type
- Continuous playback of SID music using HVSC song lengths
- Continuous playback of Images, Games and Demos with Play Timer
- Search files by type
- Tag favorite titles
- SID metadata integration with [HVSC](https://www.youtube.com/watch?v=lz0CJbkplj0&list=PLmN5cgEuNrpiCj1LfKBDUZS06ZBCjif5b) and [DeepSID](https://github.com/Chordian/deepsid)
- Game image preview integration with [OneLoad64](https://www.youtube.com/watch?v=lz0CJbkplj0&list=PLmN5cgEuNrpiCj1LfKBDUZS06ZBCjif5b)
- File transfer via Drag and Drop and One-way Watch Directory syncronization
- .D64 program file extraction on file transfer

## Requirements
- A Commodore 64/128 personal computer
- A windows 10/11 laptop, tablet or desktop computer
- [TeensyROM Hardware Cartridge](https://github.com/SensoriumEmbedded/TeensyROM)
- Micro-USB Cable
- NET 8 Runtime
- Some Game, Demo and/or SID files

## Installation
- Install the latest TeensyROM Firmware.  0.5.12 or later is required.
  - Latest firmware (.hex file) found [here](https://github.com/SensoriumEmbedded/TeensyROM/blob/main/bin/TeensyROM/FW_Release_History.md)
- Download the latest [desktop UI application release](https://github.com/MetalHexx/TeensyROM-UI/releases)
- It comes bundled with NET 8 runtime.
  - If run into issues, try the installing the runtime found [here](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- Unzip the release into a directory of your choice
- Make a USB connection between the computer and the micro-SD 
 
## Quick Start
*This quick start guide will get you up and running quickly with little explaintion.  See the [Help Guides](#help-guides) for in-depth feature guidance.*
- Firstly, ensure your have your TeensyROM SD/USB storage filled with some games and SIDs.
  - HVSC and OneLoad64 are highly recommended to maximize the UI experience.
- Execute TeensyRom.Ui.exe
- You will get a Windows Defender / SmartScreen warning.
- Click `More Info` --> `Run anyway`
  - *This is due to the application not being signed with a certificate authority yet.*  
- Once the app loads, press the `Connect` button.
  - *Note: The C64 will automatically reset, this is by design.*
- Click on the `Compass` icon in the left navigation menu
- Click the `Download` icon in the upper right corner of the screen to index all the files on your TR storage.
  - The dialog that pops up will further explain why this is important.  See: [Help Guides](#help-guides)
- Click the `Dice` icon or one of the file type filter buttons.  This will launch a random file.
- Enjoy the ride! For more details on the various features available and how to get the most out of the UI, check out the [Help Guides](#help-guides).

## Help Guides
Coming soon.

## Who should use this and why?
#### Modern Desktop Warriors
If you multitask a lot and don't always have a lot of time to sink into a full C64 play session, this is for you.  Turn on the C64, load the UI, click random, and you get instant gratification from a quick play session.

#### Chiptune / Scene Demo Lovers 
This is the retro jukebox you've been looking for.  The continuous playing capability turns your 8-bit machine into a streaming service so can enjoy tunes hands free while you do other things.  Try adding scene demos and even games into the music rotation -- retro MTV? lol

#### Purists 
Even if your prefer to interact directly with the commodore, I think you'll find a lot of great utility here.  If you tag favorites, they're copied to the /Favorites folder on the TR.  Try the SID music player while you're busy doing other things.  The file transfer and serial debugging tools are handy too.

#### Realtime Programming Development Environment
The automatic detection and synchronization of files has potential for realtime development or music making applications.  Send your feature suggestions for anything I can do to help here.

#### Kids / Social Activities
Kids have short attention spans.  The randomization features are invaluable here and will keep them engaged for much longer.  Try the play timer to automatically jump to new games.  

#### Conference / Meetup Demonstration Booths
The selectable play timer provides a great way to automatically launch titles for hands free demonstration at a conference or meetup.

## The Project Story
I am a late bloomer commodore 64 nerd.  I found a C64 at an auction and thought, hey, maybe I can find a way to control one of these with MIDI to make some music.  I started researching ways to get some games on this thing.  

I experimented with KungFu Flash which was really cool.  I pondered an Ultimate II+.  The problem was, none of them had midi and the Kerberos is impossible to find.  But then -- I found TeensyROM and it had MIDI support too.  Perfect!

The one thing that all these amazing carts have in common is that they're kind of slow at navigating over thousands of files.  It's a bit overwhelming. Trying to find that one game you played before but having trouble? I've been there. :) This project primary aims to solve that problem.

I contacted the creator, Mr Travis S, and started brainstorming.  A lot of really great ideas were been shared and in no time, the UI was born.  Enjoy!

## Screenshots
#### Launch Games
![image](https://github.com/MetalHexx/TeensyROM-UI/assets/9291740/c5ebfbcd-4282-49b2-b349-3af5b5d503c5)

#### Launch SIDs
![image](https://github.com/MetalHexx/TeensyROM-UI/assets/9291740/d4bfa1f6-9b7d-4d2c-8d2e-02e2b505a548)

#### Debug and Tinker
![image](https://github.com/MetalHexx/TeensyROM-UI/assets/9291740/cd59e5a9-dcd4-424e-8032-87bb1fa526d8)

