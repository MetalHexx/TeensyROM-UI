
#define AckToken                0x64CC
#define FailToken               0x9B7F

bool GetPathParameter(char FileNamePath[]) 
{
    uint16_t CharNum = 0;
    char currentChar;

    while (CharNum < MaxNamePathLength)
    {
        if (!SerialAvailabeTimeout())
        {
            Serial.print("Timed out getting path param!\n");
            return false;
        }
        currentChar = Serial.read();

        if (currentChar == 0) break;

        FileNamePath[CharNum] = currentChar;

        CharNum++;
    }
    FileNamePath[CharNum] = 0;

    if (CharNum == MaxNamePathLength)
    {
        Serial.print("Path too long!\n");
        return false;
    }
    return true;
}

FS* GetStorageDevice(uint32_t storageType)
{
    if (!storageType) return &firstPartition;

    if (!SD.begin(BUILTIN_SDCARD)) 
    {            
        Serial.printf("Specified storage device was not found: %u\n", storageType);
        return nullptr;
    }
    return &SD;
}

bool GetFileStream(uint32_t SD_nUSB, char FileNamePath[], FS* sourceFS, File& file)
{
    if (sourceFS->exists(FileNamePath))
    {
        SendU16(FailToken);
        Serial.printf("File already exists.\n");
        return false;
    }
    file = sourceFS->open(FileNamePath, FILE_WRITE);

    if (!file)
    {
        SendU16(FailToken);
        Serial.printf("Could not open for write: %s:%s\n", (SD_nUSB ? "SD" : "USB"), FileNamePath);
        return false;
    }
    return true;
}

bool EnsureDirectory(const char* path, FS& fs)
{
    char directoryPath[256];
    const char* lastSlash = strrchr(path, '/');
    if (lastSlash != nullptr)
    {
        size_t directoryLength = lastSlash - path;
        strncpy(directoryPath, path, directoryLength);
        directoryPath[directoryLength] = '\0';
    }
    else
    {
        strcpy(directoryPath, path);
    }

    if (lastSlash == path || lastSlash == nullptr || directoryPath[0] == '\0')
    {
        return true;
    }

    if (fs.exists(directoryPath))
    {
        return true;
    }

    char subPath[256];
    size_t subPathLength = lastSlash - path;
    strncpy(subPath, directoryPath, subPathLength);
    subPath[subPathLength] = '\0';

    if (!EnsureDirectory(subPath, fs))
    {
        return false;
    }

    return fs.mkdir(directoryPath);
}

bool ReceiveFileData(File& myFile, uint32_t len, uint32_t& CheckSum)
{
    uint32_t bytenum = 0;
    uint8_t ByteIn;

    while (bytenum < len)
    {
        if (!SerialAvailabeTimeout())
        {
            SendU16(FailToken);
            Serial.printf("Rec %lu of %lu bytes\n", bytenum, len);
            myFile.close();
            return false;
        }
        myFile.write(ByteIn = Serial.read());
        CheckSum -= ByteIn;
        bytenum++;
    }

    myFile.close();

    CheckSum &= 0xffff;
    if (CheckSum != 0)
    {
        SendU16(FailToken);
        Serial.printf("CS Failed! RCS:%lu\n", CheckSum);
        return false;
    }

    return true;
}


// Command: 
// Post File to target directory and storage type on TeensyROM.
// Automatically creates target directory if missing.
//
// Workflow:
// Receive <-- Post File Token 0x64BB 
// Send --> AckToken 0x64CC
// Receive <-- Length(4), Checksum(2), SD_nUSB(1) Destination Path(MaxNameLength, null terminator)
// Send --> 0x64CC on Pass,  0x9b7f on Fail 
// Receive <-- File(length)
// Send --> AckToken 0x64CC on Pass,  0x9b7f on Fail
//
// Notes: Once Post File Token Received, responses are 2 bytes in length
void PostFileCommand()
{  
    SendU16(AckToken);

    uint32_t fileLength, checksum, storageType;
    char FileNamePath[MaxNamePathLength];

    if (!GetUInt(&fileLength, 4))
    {
        SendU16(FailToken);
        Serial.println("Error receiving file length value!");
        return;
    }

    if (!GetUInt(&checksum, 2))
    {
        SendU16(FailToken);
        Serial.println("Error receiving checksum value!");
        return;
    }

    if (!GetUInt(&storageType, 1))
    {
        SendU16(FailToken);
        Serial.println("Error receiving storage type value!");
        return;
    }

    if (!GetPathParameter(FileNamePath)) 
    {
        SendU16(FailToken);
        return;
    }
    
    FS* sourceFS = GetStorageDevice(storageType);

    if (!sourceFS)
    {        
        SendU16(FailToken);
        Serial.println("Unable to get storage device!");
        return;
    }

    if (!EnsureDirectory(FileNamePath, *sourceFS))
    {
      SendU16(FailToken);
      Serial.printf("Failed to ensure directory for: %s\n", FileNamePath);
      return;
    }
   
    File fileStream;

    if (!GetFileStream(storageType, FileNamePath, sourceFS, fileStream)) return;
   
   SendU16(AckToken);
  
   if (!ReceiveFileData(fileStream, fileLength, checksum)) return;
   
   SendU16(AckToken);
}


bool SendPagedDirectoryContents(FS& fileStream, const char* directoryPath, int skip, int take)
{
    File directory = fileStream.open(directoryPath);
    if (!directory)
    {
        SendU16(FailToken);
        Serial.printf("Failed to open directory: %s\n", directoryPath);
        return false;
    }

    String dirString = "";
    String fileString = "";

    File directoryItem = directory.openNextFile();

    int currentCount = 0;
    int addedCount = 0;

    while (directoryItem && addedCount < take)
    {
        if (!directoryItem)
        {
            Serial.println("No items in the directory.");
            return false;
        }

        if (currentCount >= skip)
        {
            if (directoryItem.isDirectory())
            {
                dirString += "{\"Name\":\"" + String(directoryItem.name()) + "\",\"Path\":\"" + String(directoryPath) + String(directoryItem.name()) + "\"},";
            }
            else
            {
                fileString += "{\"Name\":\"" + String(directoryItem.name()) + "\",\"Size\":" + String(directoryItem.size()) + ",\"Path\":\"" + String(directoryPath) + String(directoryItem.name()) + "\"},";
            }
            addedCount++;
        }

        currentCount++;
        directoryItem.close();
        directoryItem = directory.openNextFile();
    }
    directoryItem.close();

    if (dirString.endsWith(",")) {
        dirString.remove(dirString.length() - 1);
    }
    if (fileString.endsWith(",")) {
        fileString.remove(fileString.length() - 1);
    }

    String jsonData = "{\"Path\":\"" + String(directoryPath) + "\",\"Directories\":[" + dirString + "],\"Files\":[" + fileString + "]}";

    Serial.print(jsonData);

    return true;
}


// Command: 
// List Directory Contents on TeensyROM given a take and skip value
// to faciliate batch processing.
//
// Workflow:
// Receive <-- List Directory Token 0x64DD 
// Send --> AckToken 0x64CC
// Receive <-- SD_nUSB(1), Destination Path(MaxNameLength, null terminator), sake(1), skip(1)
// Send --> StartDirectoryListToken 0x5A5A or FailToken 0x9b7f
// Send --> Write content as json
// Send --> EndDirectoryListToken 0xA5A5,  0x9b7f on Fail
void ListDirectoryCommand()
{
    const uint16_t StartDirectoryListToken = 0x5A5A;
    const uint16_t EndDirectoryListToken = 0xA5A5;

    SendU16(AckToken);

    uint32_t storageType, skip, take;
    char path[MaxNamePathLength];

    if (!GetUInt(&storageType, 1))
    {
        SendU16(FailToken);
        Serial.println("Error receiving storage type value!");
        return;
    }
    if (!GetUInt(&skip, 1))
    {
        SendU16(FailToken);
        Serial.println("Error receiving skip value!");
        return;
    }
    if (!GetUInt(&take, 1))
    {
        SendU16(FailToken);
        Serial.println("Error receiving take value!");
        return;
    }
    if (!GetPathParameter(path)) 
    {
        SendU16(FailToken);
        Serial.println("Error receiving path value!");
        return;
    }

    FS* sourceFS = GetStorageDevice(storageType);

    if (!sourceFS)
    {        
        SendU16(FailToken);
        Serial.println("Unable to get storage device!");
        return;
    }

    SendU16(StartDirectoryListToken);  

    if (!SendPagedDirectoryContents(*sourceFS, path, skip, take)) return;

    SendU16(EndDirectoryListToken);
}
