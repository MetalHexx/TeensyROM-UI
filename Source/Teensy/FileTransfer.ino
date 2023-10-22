
#define SendFileToken  0x64AA 
#define AckToken       0x64CC
#define FailToken      0x9B7F

bool GetFileParameters(uint32_t &len, uint32_t &CheckSum, uint32_t &SD_nUSB, char FileNamePath[])
{
    if (!GetUInt(&len, 4)) return false;
    if (!GetUInt(&CheckSum, 2)) return false;
    if (!GetUInt(&SD_nUSB, 1)) return false;

    return true;
}

bool GetPathParameter(char FileNamePath[]) 
{
    uint16_t CharNum = 0;
    while (1)
    {
        if (!SerialAvailabeTimeout()) return false;
        FileNamePath[CharNum] = Serial.read();
        if (FileNamePath[CharNum] == 0) break;
        if (++CharNum == MaxNamePathLength)
        {
            SendU16(FailToken);
            Serial.print("Path too long!\n");
            return false;
        }
    }
    return true;
}

FS* GetStorageDevice(uint32_t SD_nUSB) {
    if (SD_nUSB) {
        if (!SD.begin(BUILTIN_SDCARD)) 
        {
            SendU16(FailToken);
            Serial.printf("Specified storage device not found: %u\n", SD_nUSB);
            return nullptr;
        }
        return &SD;
    }
    return &firstPartition;
}

File GetFileStream(uint32_t SD_nUSB, char FileNamePath[], FS* sourceFS)
{
    if (sourceFS->exists(FileNamePath))
    {
        SendU16(FailToken);
        Serial.printf("File already exists.\n");
        return File();     
    }

    File myFile = sourceFS->open(FileNamePath, FILE_WRITE);
    if (!myFile)
    {
        SendU16(FailToken);
        Serial.printf("Could not open for write: %s:%s\n", (SD_nUSB ? "SD" : "USB"), FileNamePath);
        return File();
    }
    return myFile;
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

    uint32_t len, CheckSum, SD_nUSB;
    char FileNamePath[MaxNamePathLength];

    if (!GetFileParameters(len, CheckSum, SD_nUSB, FileNamePath)) return;

    if (!GetPathParameter(FileNamePath)) return;
    
    FS *sourceFS = GetStorageDevice(SD_nUSB);

    if (!sourceFS) return;

    if (!EnsureDirectory(FileNamePath, *sourceFS))
    {
      SendU16(FailToken);
      Serial.printf("Failed to ensure directory for: %s\n", FileNamePath);
      return;
    }
   
    File myFile = GetFileStream(SD_nUSB, FileNamePath, sourceFS);

    if (!myFile) return;
   
   SendU16(AckToken);
  
   if (!ReceiveFileData(myFile, len, CheckSum)) return;
   
   SendU16(AckToken);
}
