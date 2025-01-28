# TeensyROM Desktop UI
Command your Commodore 64 / 128 with a modern desktop user experience.  This user interface is designed to compliment the [TeensyROM Hardware Cartridge](https://github.com/SensoriumEmbedded/TeensyROM), *designed by Travis S/Sensorium* to enable lighting fast exploration of Games, Scene Demos, Music and Images in very large file collections.  Coupled with remote launch capability, you will cover a lot of ground very quickly and discover great content faster than ever.

Inspired by streaming music/video media services, the application comes with the continuous playback, randomization and search capabilities you would expect from one. Your TR effectively becomes a retro-modern streaming service of random entertaining retro content that you never knew existed in your collection.  

## Demo / Tutorial Video
A demonstration of the UI features.  Watch this before you run the app. Click image or [here](https://www.youtube.com/watch?v=xUcfgGMYOpM) to watch demo!  App can be downloaded [here](https://github.com/MetalHexx/TeensyROM-UI/releases).
<div align="center">
  <a href="https://www.youtube.com/watch?v=xUcfgGMYOpM">
    <img src="https://github.com/user-attachments/assets/75af344d-ee5f-42b2-b258-88af4a669743" alt="Watch the video" width="75%">
  </a>
</div>
<br>

## Screenshots
<table>
  <tr>
    <td style="
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
An integration with [HVSC](https://hvsc.c64.org/) brings forth STIL info like accurate SID play times, composer name, release info, and even some interesting historical scene information.  This plethora of information becomes quite effective when digging for cover tunes or other very specific tracks.  Special thanks to the open source [DeepSID](https://github.com/Chordian/deepsid) project, composer images also compliment the HVSC SIDs.

#### OneLoad64
The application comes bundled with [OneLoad64](https://www.youtube.com/watch?v=lz0CJbkplj0&list=PLmN5cgEuNrpiCj1LfKBDUZS06ZBCjif5b) game load and play screens to make it easy to preview games to play.  This collection in combination with this application enable the fastest consecutive game load times possible on a commodore machine.  Make sure to keep 

## Search
Search will make finding specific content very easy with filename/path search for all file types.  Searching for SIDS in the HVSC library has some additional benefits through the integration of STIL song info.  The search will use Artist Name, Song Title and Comments to making finding hidden gems much easier.  Type keywords to search.  `+` in front a keyword that is required in the search results.  Double quotes around to search for a match on a phrase.  You can also combine the two.  Example: `Iron Maiden +"Aces High"`

## File Transfer
Incorporating file drag and drop functionality, you can move files to your SD/USB storage pretty quickly, reducing the need to physically remove them.  Watch file capability allows you to designate a folder on your desktop computer to trigger automatic file transfer and launch on your commodore.

## Realtime Code Testing / SID Composition
Try using the watch directory by configuring the UI to the location of build / music artifacts and automatically launch them on your Commodore for a true continuous testing environment.
Click image or [here](https://youtu.be/Lm5OzDft1AQ?si=08C4-TIu80syaVTP) to watch demo!
<div align="center">
  <a href="https://youtu.be/Lm5OzDft1AQ?si=08C4-TIu80syaVTP">
    <img src="https://github.com/user-attachments/assets/20bdc9d3-f916-4a77-abc2-5c27f38bf7ad" alt="Watch the video" width="75%">
  </a>  
</div>
<br>

## Debug and Serial CLI
The application features a terminal to show you all of the activity happening on the TR. A command line interface is available for sending Serial commands directly to the hardware.  This is useful for debugging or tinkering with your TR.
   
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
*See the [Help Guides](#Tips and ) for in-depth feature guidance.*
- Firstly, ensure your have your TeensyROM SD/USB storage filled with some games and SIDs.
  - HVSC and OneLoad64 are highly recommended to maximize the UI experience.
- Connect the computer to the micro-USB port on the TeensyROM cartridge.
- Execute TeensyRom.Ui.exe
- You will get a Windows Defender / SmartScreen warning.
- Click `More Info` --> `Run anyway`
  - *This is due to the application not being signed with a certificate authority yet.*
- _The application will great you with a Tutorial/Wizard to get you started.  If you choose to skip this, see the steps below._
- Once the app loads, it should automatically detect and connect to the correct TeensyROM Cartridge port.  
- Click on the `Compass` icon in the left navigation menu
- Click the `Download` icon in the upper right corner of the screen to index all the files on your TR storage.
  - The dialog that pops up will further explain why this is important.  See: [Tips and Tricks](#tips-and-tricks)
- Click the `Dice` icon or one of the file type filter buttons.  This will launch a random file.
- Enjoy the ride! Check out [Tips and Tricks](#tips-and-tricks) for additional info.

## Tips and Tricks
- One day we'll have better docs. Maybe never, we'll see how well the UI explains itself. ;)
- For now, the [demo](https://www.youtube.com/watch?v=xUcfgGMYOpM) goes over the feature set pretty well.
- Be sure to check out the `Settings` view for some options to better suit your needs.
- Hover over any UI control (buttons, etc) to get some help text.
- Don't skip indexing to get max usage of the features.
- If you don't do a full indexing, search and randomization will only work on the directories you've visited.
- If you're storing a VERY large number of files like HVSC, I recommend doing it the old way with USB/SD. It's simply faster.
- If you change the contents of the SD or USB storage outside of the UI, be sure to re-index.  Otherwise they'll appear to be missing.
- If you feel like being adventurous,
  - Settings that haven't been exposed in the UI are located : `\Assets\System\Config\Settings.json`
  - If you mess something up, no big deal, just delete your file and it'll restore to factory defaults.
  - You can find the index files here `\Assets\System\Cache`
  - Back up your configuration files before changing them. 
