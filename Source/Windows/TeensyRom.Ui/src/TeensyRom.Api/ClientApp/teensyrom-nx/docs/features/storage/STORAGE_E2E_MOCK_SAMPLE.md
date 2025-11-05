# TeensyROM Storage E2E Mock Sample Data

> Reference documentation for SD card file structure and representative file paths for testing and mock data generation.

## Directory Structure Overview

```
SD Card (PSM2ZAKI)
├── auto-transfer/
├── demos/
├── favorites/
├── firmware/
├── games/
│   ├── Extras/
│   ├── Large/
│   ├── MultiLoad64/
│   └── Very Large/
├── images/
├── music/
│   ├── DEMOS/
│   │   ├── 0-9/
│   │   ├── A-F/
│   │   ├── Commodore/
│   │   ├── G-L/
│   │   ├── M-R/
│   │   ├── S-Z/
│   │   └── UNKNOWN/
│   ├── GAMES/
│   │   ├── 0-9/
│   │   ├── A-F/
│   │   ├── G-L/
│   │   ├── M-R/
│   │   └── S-Z/
│   └── MUSICIANS/
│       ├── 0-9/
│       ├── A/
│       ├── B/
│       ├── ... (C-Z)
│       └── Z/
├── playlists/
├── System Volume Information/
├── cart-tag.txt
└── remote-config.txt
```

## Games Directory Files

**Location**: `/games/*.crt`

Commodore 64 cartridge ROM files (.crt format). Organized alphabetically with 2000+ games.
File sizes typically: 64.2 KB, 80.2 KB, or 88.2 KB

### Sample Files:

```
/games/10th Frame.crt (64.2 KB)
/games/180.crt (80.2 KB)
/games/1942 (Music v1).crt (80.2 KB)
/games/Donkey Kong (Ocean).crt (88.2 KB)
/games/Donkey Kong Junior (Mr. SID - 2014).crt (88.2 KB)
/games/Mario Bros (Ocean) (J1).crt (88.2 KB)
/games/Pac-Man (J1).crt (80.2 KB)
```

### Games Subdirectories:

```
/games/Extras/ - Extra ROM variants
/games/Large/ - Large ROM files
/games/MultiLoad64/ - Multi-load cartridges
/games/Very Large/ - Very large ROM files
```

## Music Directory - Musicians

**Location**: `/music/MUSICIANS/[Letter]/[Artist Name]/*.sid`

SID music files organized by artist name, grouped alphabetically (A-Z, 0-9).
Individual artist folders contain multiple SID compositions.

### Sample Artist/Track Paths:

```
/music/MUSICIANS/L/LukHash/Alpha.sid (6.7 KB)
/music/MUSICIANS/L/LukHash/Another_World.sid (8.3 KB)
/music/MUSICIANS/L/LukHash/Code_Veronica.sid (9.4 KB)
/music/MUSICIANS/L/LukHash/Dreams.sid (22.9 KB)
/music/MUSICIANS/L/LukHash/Falling_Down.sid (8.3 KB)
/music/MUSICIANS/L/LukHash/Hongdae.sid (7.5 KB)
/music/MUSICIANS/L/LukHash/Neon_Thrills.sid (9.8 KB)
/music/MUSICIANS/L/LukHash/Other_Side.sid (7.9 KB)
/music/MUSICIANS/L/LukHash/Perpetual_Motion.sid (8.0 KB)
/music/MUSICIANS/L/LukHash/Proxima.sid (6.9 KB)
```

### Other Artist Examples:

- L_Oreal
- LaBatt_Darren
- Lagace_Ken
- Landwehr_Bob
- Las_Alex
- Laserboy
- Latifah
- Latimer_Joey
- Latvamaeki_Aki
- Laurikka
- Laxity
- Lead
- Lee_Dave
- Lee_Steve
- Lees_Anthony
- Leffty
- Legg_Stephen
- Leitch_Barry
- Leming
- Lester
- Levine_Thomas

## Music Directory - Demos

**Location**: `/music/DEMOS/[Range]/*.sid`

Demo SID music files organized in alphabetical ranges: 0-9, A-F, G-L, M-R, S-Z, Commodore, UNKNOWN

### Sample Demo Tracks (S-Z range):

```
/music/DEMOS/S-Z/S_O_S.sid (15.1 KB)
/music/DEMOS/S-Z/S_S_B_Melee_Trophy.sid (2.7 KB)
/music/DEMOS/S-Z/S_Stands_for_SID.sid (28.2 KB)
/music/DEMOS/S-Z/Saber_Rider_Theme.sid (5.8 KB)
/music/DEMOS/S-Z/Sabrina_Remix.sid (32.9 KB)
/music/DEMOS/S-Z/Sad_Toad.sid (11.7 KB)
/music/DEMOS/S-Z/Saddamski.sid (31.2 KB)
/music/DEMOS/S-Z/Safe_in_Danger.sid (3.5 KB)
/music/DEMOS/S-Z/Sanxion_Music.sid (18.9 KB)
/music/DEMOS/S-Z/Satan_Demo.sid (7.2 KB)
/music/DEMOS/S-Z/Sanforized_5_intro.sid (2.5 KB)
/music/DEMOS/S-Z/Secret_Dreams.sid (3.4 KB)
/music/DEMOS/S-Z/Secret_of_Monkey_Island.sid (7.6 KB)
/music/DEMOS/S-Z/Seizure.sid (3.1 KB)
/music/DEMOS/S-Z/SCI_Music_1.sid (18.9 KB)
/music/DEMOS/S-Z/Smooth_Criminal.sid (47.3 KB)
/music/DEMOS/S-Z/Smooth_Sound_II.sid (11.1 KB)
/music/DEMOS/S-Z/Snowmen.sid (13.5 KB)
/music/DEMOS/S-Z/Space_Harrier_II_Main_Theme.sid (26.5 KB)
/music/DEMOS/S-Z/Space_Hulk_note.sid (2.2 KB)
/music/DEMOS/S-Z/Space_Quest.sid (2.8 KB)
/music/DEMOS/S-Z/Spacewriter_tune_4.sid (3.1 KB)
/music/DEMOS/S-Z/Sparkle_Nights.sid (4.5 KB)
/music/DEMOS/S-Z/Speedy_Gonzales_tune_2.sid (11.1 KB)
/music/DEMOS/S-Z/Spider_Dance.sid (19.5 KB)
/music/DEMOS/S-Z/Stairway_to_Heaven.sid (10.7 KB)
/music/DEMOS/S-Z/Star_Trek_II.sid (14.3 KB)
/music/DEMOS/S-Z/Star_Trek_III-The_Search_for_Spock.sid (6.5 KB)
/music/DEMOS/S-Z/State_of_the_Art.sid (1.8 KB)
/music/DEMOS/S-Z/Studio_64.sid (31.7 KB)
/music/DEMOS/S-Z/Super_Mario_2_End_Theme.sid (1.7 KB)
/music/DEMOS/S-Z/Super_Mario_Bros-Overworld.sid (3.7 KB)
/music/DEMOS/S-Z/Sweden_Demo.sid (11.1 KB)
/music/DEMOS/S-Z/Sweet_Child_o_Mine.sid (2.4 KB)
/music/DEMOS/S-Z/Take_Five.sid (6.4 KB)
/music/DEMOS/S-Z/Take_My_Breath_Away.sid (11.1 KB)
/music/DEMOS/S-Z/Tarzan_Boy.sid (5.9 KB)
/music/DEMOS/S-Z/Techno_Rythm.sid (11.1 KB)
/music/DEMOS/S-Z/Temple.sid (19.5 KB)
/music/DEMOS/S-Z/This_Is_It.sid (6.5 KB)
/music/DEMOS/S-Z/Time_to_Kill_tune_2.sid (12.3 KB)
/music/DEMOS/S-Z/Trancer.sid (44.0 KB)
/music/DEMOS/S-Z/Tribute_to_Chris_Huelsbeck.sid (11.1 KB)
/music/DEMOS/S-Z/Trick_Theme.sid (56.9 KB)
/music/DEMOS/S-Z/Turbo_Killer.sid (6.2 KB)
/music/DEMOS/S-Z/Twist_of_Fate.sid (8.4 KB)
/music/DEMOS/S-Z/Unbeatable.sid (45.6 KB)
/music/DEMOS/S-Z/Voodoopeople.sid (46.9 KB)
/music/DEMOS/S-Z/Walk_This_Way.sid (3.2 KB)
/music/DEMOS/S-Z/We_Wish_You_a_Merry_Xmas.sid (6.9 KB)
/music/DEMOS/S-Z/Welcome.sid (35.4 KB)
/music/DEMOS/S-Z/Wochenendeeee.sid (57.2 KB)
/music/DEMOS/S-Z/Wonderful_Days.sid (3.9 KB)
/music/DEMOS/S-Z/Xmas_Funs.sid (27.7 KB)
/music/DEMOS/S-Z/Yellow_Submarine_BASIC.sid (8.0 KB)
/music/DEMOS/S-Z/You_Can_Win_If_You_Want.sid (1.1 KB)
/music/DEMOS/S-Z/Zaire.sid (15.1 KB)
```

## Images Directory

**Location**: `/images/*.kla`, `/images/*.seq`

Image files in Koala paint format (.kla) and sequencer format (.seq)
Typical file size: 6.4-9.8 KB

### Sample Image Files:

```
/images/6000Addr.koa (9.8 KB)
/images/ChrisCornell1.kla (9.8 KB)
/images/ChrisCornell2_.kla (9.8 KB)
/images/Dio2.kla (9.8 KB)
/images/Dio3.kla (9.8 KB)
/images/Emperor.kla (9.8 KB)
/images/HA_Sugar_Skull.kla (9.8 KB)
/images/LP_ELP_Tarkus.kla (9.8 KB)
/images/LP_Zappa_Sleep_Dirt.kla (9.8 KB)
/images/minecraft.kla (9.8 KB)
/images/Rush-2112-band.kla (9.8 KB)
/images/Rush2112.kla (9.8 KB)
/images/SonicTheHedgehog.kla (9.8 KB)
/images/Soundgarden1.kla (9.8 KB)
/images/Soundgarden2.kla (9.8 KB)
/images/Superunknown.kla (9.8 KB)
/images/sq3.seq (6.4 KB)
```

## Root Level Configuration Files

**Location**: `/*.txt`

Text-based configuration files at the SD card root level.

### Files:

```
/cart-tag.txt (30.0 B) - Cartridge tag configuration
/remote-config.txt (124.0 B) - Remote configuration settings
```

## File Format Reference

### .crt (Commodore 64 Cartridge ROM)

- **Size**: Typically 64.2 KB, 80.2 KB, or 88.2 KB
- **Location**: `/games/` and subdirectories
- **Usage**: Playable game ROM files for C64 emulation
- **Naming**: Alphabetically sorted, may include release year or variant info

### .sid (SID Music File)

- **Size**: 1 KB - 60+ KB (highly variable)
- **Location**: `/music/DEMOS/` and `/music/MUSICIANS/[Letter]/[Artist]/`
- **Usage**: Commodore 64 music/audio files
- **Format**: SoundInitializer music format (SID)

### .kla (Koala Paint Image)

- **Size**: Typically 9.8 KB
- **Location**: `/images/`
- **Usage**: Commodore 64 bitmap image file
- **Format**: Koala paint bitmap format

### .seq (Sequencer File)

- **Size**: Typically 6.4 KB
- **Location**: `/images/`
- **Usage**: Image sequencer or animation file

### .txt (Text Configuration)

- **Size**: Tiny (30-124 bytes)
- **Location**: Root level `/`
- **Usage**: Configuration metadata

## Storage Statistics

- **Total Games**: 2000+ .crt files
- **Game Subdirectories**: 4 (Extras, Large, MultiLoad64, Very Large)
- **Music Artists**: 100s organized A-Z
- **Music Demo Ranges**: 7 ranges (0-9, A-F, G-L, M-R, S-Z, Commodore, UNKNOWN)
- **Images**: 17+ bitmap/image files
- **Device**: PSM2ZAKI with SD Storage

## Use Cases for E2E Testing

### File Browser Navigation

- Test directory traversal with deep hierarchies (5+ levels)
- Test breadcrumb navigation accuracy
- Test "Up" button functionality at multiple levels
- Test listing performance with 2000+ files

### Search and Filter

- Mock search results for common game/artist names
- Test filtering by file extension (.sid, .crt, .kla)
- Test alphabetical sorting in large directories

### Playback

- Mock loading SID files from `/music/` directories
- Mock loading game ROMs from `/games/` subdirectories
- Test file metadata display (size, artist, title)

### Directory Structure Validation

- Verify alphabetic folder organization in MUSICIANS directory
- Validate range-based demo folder structure
- Test SD card root directory listing

## Sample Mock Data Generation

For E2E tests, use the paths listed above to:

1. Create mock directory structures
2. Generate stub files with appropriate sizes
3. Mock API responses returning these file listings
4. Test UI rendering with realistic file counts and hierarchies
