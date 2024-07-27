# TeensyROM Desktop UI
Command your Commodore 64 / 128 with a modern desktop user experience.  This user interface is designed to compliment the [TeensyROM Hardware Cartridge](https://github.com/SensoriumEmbedded/TeensyROM), *designed by Travis S/Sensorium* to enable lighting fast exploration of Games, Scene Demos, Music and Images in very large file collections.  Coupled with remote launch capability, you will cover a lot of ground very quickly and discover great content faster than ever.

Inspired by streaming music/video media services, the application comes with the continuous playback, randomization and search capabilities you would expect from one. Your TR effectively becomes a retro-modern streaming service of random entertaining retro content that you never knew existed in your collection.  

<table>
  <tr>
    <td align="center">
      <b>Game Launcher</b><br>
      <img src="https://github.com/MetalHexx/TeensyROM-UI/assets/9291740/c5ebfbcd-4282-49b2-b349-3af5b5d503c5" width="100%" alt="Launch Games">      
    </td>
    <td align="center">
      <b>SID Music Player</b><br>
      <img src="https://github.com/MetalHexx/TeensyROM-UI/assets/9291740/d4bfa1f6-9b7d-4d2c-8d2e-02e2b505a548" width="100%" alt="Launch SIDs">
    </td>
    <td align="center">
      <b>Serial Terminal / CLI</b><br>
      <img src="https://github.com/MetalHexx/TeensyROM-UI/assets/9291740/cd59e5a9-dcd4-424e-8032-87bb1fa526d8" width="100%" alt="Debug and Tinker with a Serial CLI">
    </td>
  </tr>
</table>

## Features
- Media Player / File Explorer hybrid interface
- Remote Launch of Games, SIDS, Images, Firmware Updates
- Launch Randomization by File Type
- Continuous playback of SID music and subtunes using HVSC song lengths
- Continuous playback of Images, Games and Demos with Play Timer
- Search files by type with light search engine syntax
- Tag favorite titles so you don't lose track of gems.
- File transfer via Drag and Drop and One-way watch directory synchronization
- .D64 program file extraction on file transfer

## Integrations
_Integrations with various metadata sources enrich your file collection with interesting information and media._

#### HVSC/DeepSID
An integration with [HVSC](https://www.youtube.com/watch?v=lz0CJbkplj0&list=PLmN5cgEuNrpiCj1LfKBDUZS06ZBCjif5b) brings forth STIL info like accurate SID play times, composer name, release info, and even some interesting historical scene information.  This plethora of information becomes quite effective when digging for cover tunes or other very specific tracks.  Special thanks to the open source [DeepSID](https://github.com/Chordian/deepsid) project, composer images also compliment the HVSC SIDs.

#### OneLoad64
The application comes bundled with [OneLoad64](https://www.youtube.com/watch?v=lz0CJbkplj0&list=PLmN5cgEuNrpiCj1LfKBDUZS06ZBCjif5b) game load and play screens to make it easy to preview games to play.  This collection in combination with this application enable the fastest consecutive game load times possible on a commodore machine.

## Search
Search will make finding specific content very easy with filename/path search for all file types.  Searching for SIDS in the HVSC library has some additional benefits through the integration of STIL song info.  The search will use Artist Name, Song Title and Comments to making finding hidden gems much easier.

## Debug and Serial CLI
The application features a terminal to show you all of the activity happening on the TR. A command line interface is available for sending Serial commands directly to the hardware.  This is useful for debugging or tinkering with your TR.

 ## File Transfer
Incorporating file drag and drop functionality, you can move files to your SD/USB storage pretty quickly, reducing the need to physically remove them.  Watch file capability allows you to designate a folder on your desktop computer to trigger automatic file transfer and launch on your commodore.
   
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
  - The application download is also bundled with the latest validated firmware in the /Assets/Firmware folder 
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
- _The application will great you with a Tutorial/Wizard to get you started.  If you choose to skip this, see the steps below._
- Once the app loads, press the `Connect` button.
  - *Note: The C64 will automatically reset, this is by design.*
- Click on the `Compass` icon in the left navigation menu
- Click the `Download` icon in the upper right corner of the screen to index all the files on your TR storage.
  - The dialog that pops up will further explain why this is important.  See: [Help Guides](#help-guides)
- Click the `Dice` icon or one of the file type filter buttons.  This will launch a random file.
- Enjoy the ride! For more details on the various features available and how to get the most out of the UI, check out the [Help Guides](#help-guides).

## Help Guides
Coming soon.
