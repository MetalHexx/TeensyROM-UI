# DeepSID Database Structure

## Overview
This document describes the structure of the DeepSID database, which contains comprehensive information about the High Voltage SID Collection (HVSC) - the largest collection of Commodore 64 music files.

**Source**: DeepSID Database Export (HVSC v80, CGSC v146)
**Format**: Normalized JSON with relational data
**Total Records**: 273,523 rows across 17 tables
**File**: `deepsid-database.json` (54MB)

## Database Tables

### Core SID Files

#### `hvsc_files` (36,301 rows)
The main table containing all SID music files from the HVSC collection.

**Key Fields**:
- `id` - Primary key (unique file ID)
- `fullname` - Full path to the SID file in HVSC
- `name` - Song title
- `author` - Composer/author name (may include handle)
- `copyright` - Copyright year and group
- `player` - Music player/editor used (e.g., "GoatTracker_V2.x")
- `lengths` - Duration of subtunes (e.g., "1:17" or "3:27 1:00 0:42")
- `type` - SID file type ("PSID" or "RSID")
- `version` - SID file format version
- `sidmodel` - Target SID chip ("MOS6581" or "MOS8580")
- `clockspeed` - Clock speed ("PAL 50Hz", "NTSC 60Hz", etc.)
- `subtunes` - Number of subtunes in the file
- `hash` - MD5 hash of the file
- `stil` - STIL (SID Tune Information List) entry with song info
- `csdbid` - C64 Scene Database ID (for linking to CSDB)
- `csdbtype` - Type in CSDB ("sid", "release", etc.)
- `gb64` - Gamebase64 data (JSON string)

**Relationships**:
- Links to `composers` via author name (fuzzy match)
- Links to `hvsc_folders` via path parsing
- Links to `hvsc_lengths` via file path for extended length data
- Links to `youtube` via `csdbid`
- Links to `tags_lookup` via file ID

#### `hvsc_folders` (3,352 rows)
Directory structure of the HVSC collection.

**Key Fields**:
- `id` - Primary key
- `fullname` - Full folder path
- `numfiles` - Number of files in folder
- `csdbid` - CSDB ID if folder represents a group/release

**Relationships**:
- Parent-child relationships via path hierarchy
- Links to `hvsc_files` via path matching

#### `hvsc_lengths` (83,708 rows)
Extended duration information for SID files with multiple subtunes.

**Key Fields**:
- `id` - Primary key
- `path` - Path to SID file
- `lengths` - Comma-separated list of subtune durations
- `times` - Duration in seconds

**Relationships**:
- Links to `hvsc_files` via `path` field

### Composers & Groups

#### `composers` (1,372 rows)
Information about SID music composers and musicians.

**Key Fields**:
- `id` - Primary key
- `fullname` - HVSC path to composer's folder
- `name` - Full real name
- `shortname` - Common name variant
- `handles` - Scene handles/aliases
- `shorthandle` - Primary handle
- `focus` - Type: "PRO" (professional), "SCENER", "BOTH", "UNKNOWN"
- `active` - Year of activity
- `born` - Birth date
- `died` - Death date (if applicable)
- `country` - Country/location history
- `employment` - Career history
- `affiliation` - Groups/companies
- `csdbid` - CSDB scener ID
- `imagesource` - Source of profile image

**Relationships**:
- Links to `hvsc_files` via name matching in author field
- Links to `composers_links` for external profiles
- Links to `groups` via affiliation

#### `composers_links` (2,505 rows)
External links for composers (websites, social media, etc.).

**Key Fields**:
- `id` - Primary key
- `composerid` - Foreign key to `composers.id`
- `linktype` - Type of link ("WEBSITE", "FACEBOOK", "TWITTER", etc.)
- `url` - Link URL

**Relationships**:
- Foreign key: `composerid` → `composers.id`

#### `groups` (233 rows)
Scene groups and companies.

**Key Fields**:
- `id` - Primary key
- `name` - Group name
- `logo` - Logo filename
- `logodark` - Dark theme logo
- `csdbid` - CSDB group ID

**Relationships**:
- Links to `composers` via affiliation
- Links to `hvsc_files` via copyright field

### Tagging & Metadata

#### `tags_info` (743 rows)
Tag definitions for categorizing SID files.

**Key Fields**:
- `id` - Primary key
- `tag` - Tag name
- `category` - Tag category
- `parent` - Parent tag ID (for hierarchical tags)

**Relationships**:
- Self-referential via `parent` field
- Links to `tags_lookup` via tag ID

#### `tags_lookup` (131,475 rows)
Many-to-many relationship between files and tags.

**Key Fields**:
- `id` - Primary key
- `fileid` - Foreign key to `hvsc_files.id`
- `tagid` - Foreign key to `tags_info.id`

**Relationships**:
- Foreign key: `fileid` → `hvsc_files.id`
- Foreign key: `tagid` → `tags_info.id`

### Community & Uploads

#### `uploads` (2,905 rows)
User-uploaded SID files not in HVSC.

**Key Fields**:
- `id` - Primary key
- `fullname` - File path
- `name` - Song title
- `author` - Composer
- `copyright` - Copyright info
- (Similar structure to `hvsc_files`)

**Relationships**:
- Similar relationships to `hvsc_files`
- Represents user contributions

#### `users` (1 row)
User information (minimal in this export).

**Key Fields**:
- `id` - Primary key
- `username` - User name
- `email` - Email address

**Relationships**:
- Links to `uploads` and `ratings` via user ID

#### `ratings` (5 rows)
User ratings for SID files.

**Key Fields**:
- `id` - Primary key
- `fileid` - Foreign key to file ID
- `userid` - Foreign key to `users.id`
- `rating` - Rating value

**Relationships**:
- Foreign key: `fileid` → `hvsc_files.id` or `uploads.id`
- Foreign key: `userid` → `users.id`

### Competitions

#### `competitions` (482 rows)
SID music competitions and compos.

**Key Fields**:
- `id` - Primary key
- `name` - Competition name
- `year` - Year held
- `party` - Party/event name
- `csdbid` - CSDB event ID

**Relationships**:
- Links to `competitions_cache` via competition ID

#### `competitions_cache` (5,642 rows)
Competition entries and results.

**Key Fields**:
- `id` - Primary key
- `competitionid` - Foreign key to `competitions.id`
- `fileid` - Foreign key to file ID
- `rank` - Competition ranking

**Relationships**:
- Foreign key: `competitionid` → `competitions.id`
- Foreign key: `fileid` → `hvsc_files.id`

### Music Players

#### `players_info` (1 row)
Information about SID music players/editors.

**Key Fields**:
- `id` - Primary key
- `player` - Player name
- `description` - Player description

**Relationships**:
- Links to `players_lookup` via player name

#### `players_lookup` (154 rows)
Mapping of player names to files.

**Key Fields**:
- `id` - Primary key
- `player` - Player name
- `variant` - Player variant

**Relationships**:
- Links to `hvsc_files` via `player` field

### External Media

#### `youtube` (4,644 rows)
YouTube videos related to SID files.

**Key Fields**:
- `id` - Primary key
- `file_id` - Foreign key to `hvsc_files.id`
- `subtune` - Subtune number (0 for all subtunes)
- `channel` - YouTube channel name
- `video_id` - YouTube video ID (11-character string)
- `tab_order` - Display order in tabs
- `tab_default` - Whether this is the default video (1 = yes, 0 = no)

**Relationships**:
- Foreign key: `file_id` → `hvsc_files.id`

### Other

#### `symlists` (0 rows)
Symbolic links list (empty in this export).

## Common Relationships & Queries

### Finding Files by Composer
```javascript
// Get all files by Rob Hubbard
const robHubbard = database.composers.find(c => c.shortname === 'Rob Hubbard');
const robFiles = database.hvsc_files.filter(f =>
  f.author.includes('Hubbard') ||
  f.fullname.includes(robHubbard.fullname)
);
```

### Getting Tags for a File
```javascript
// Get tags for a specific file
const fileId = 1;
const tagLookups = database.tags_lookup.filter(tl => tl.fileid === fileId);
const tags = tagLookups.map(tl =>
  database.tags_info.find(ti => ti.id === tl.tagid)
);
```

### Finding Competition Winners
```javascript
// Get 1st place entries for a competition
const compId = 1;
const winners = database.competitions_cache
  .filter(cc => cc.competitionid === compId && cc.rank === 1)
  .map(cc => database.hvsc_files.find(f => f.id === cc.fileid));
```

### Getting YouTube Videos for a File
```javascript
// Get videos for a file
const file = database.hvsc_files[0];
const videos = database.youtube.filter(yt => yt.file_id === file.id);

// Build YouTube URLs
videos.forEach(v => {
  console.log(`${v.channel}: https://youtube.com/watch?v=${v.video_id}`);
});
```

## Data Quality Notes

- **Null Values**: Empty/missing data is represented as `null`, `""`, or `0` depending on field type
- **Dates**: Stored as strings in "YYYY-MM-DD" format, "0000-00-00" indicates unknown
- **IDs**: All ID fields are integers, maintain referential integrity
- **Paths**: File paths use forward slashes (`/`), start with collection name
- **Durations**: Format is "M:SS" or "H:MM:SS", multiple subtunes separated by spaces
- **CSDB Links**: `csdbid` fields link to Commodore 64 Scene Database (csdb.dk)
- **HTML Content**: Some fields (like `stil`, `description`) contain HTML markup

## File Size & Performance

- **Total Size**: 54MB uncompressed JSON
- **Records**: 273,523 total rows
- **Largest Table**: `tags_lookup` (131,475 rows)
- **Recommended**: Use streaming or chunked parsing for large-scale processing
- **Indexing**: Consider creating indices on commonly queried fields (`csdbid`, `author`, etc.)

## Version Information

- **HVSC Version**: 80 (High Voltage SID Collection)
- **CGSC Version**: 146 (Compute's Gazette SID Collection)
- **Export Date**: May 18, 2024
- **Database**: chordian_net_deepsid (MariaDB)

## Usage in TeensyROM

This database can be used for:
- SID file metadata display
- Composer information lookup
- Playlist generation by tags/genres
- Competition history browsing
- YouTube video integration
- Search and filtering functionality
- Related files/composers discovery

## Additional Resources

- **HVSC Homepage**: https://www.hvsc.c64.org/
- **DeepSID Player**: https://deepsid.chordian.net/
- **CSDB**: https://csdb.dk/
- **STIL Documentation**: https://www.hvsc.c64.org/download/C64Music/DOCUMENTS/STIL.txt
