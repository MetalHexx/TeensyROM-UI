

//these functions triggered from ISR and use current menu selection information while c64 code waits

void HandleExecution()
{
   //DisablePhi2ISR = true; //may want to do this through here
   //interpret/load rtDir, rtCrt, and rtPrg from SD card or USB Drive
   if (CurrentMenu == rmtSD || CurrentMenu == rmtUSBDrive) LoadDirPrgCrt(CurrentMenu == rmtSD);
   
   //has to be distilled down to one of these by this point, only ones supported so far.
   //Execute ROM or prep PRG tranfer
   switch(MenuSource[RegSelect].ItemType)
   {
      case rt16k:
         SetGameAssert;
         SetExROMAssert;
         LOROM_Image = MenuSource[RegSelect].Code_Image;
         HIROM_Image = MenuSource[RegSelect].Code_Image+0x2000;
         doReset=true;
         break;
      case rt8kHi:
         SetGameAssert;
         SetExROMDeassert;
         LOROM_Image = NULL;
         HIROM_Image = MenuSource[RegSelect].Code_Image;
         doReset=true;
         break;
      case rt8kLo:
         SetGameDeassert;
         SetExROMAssert;
         LOROM_Image = MenuSource[RegSelect].Code_Image;
         HIROM_Image = NULL;
         doReset=true;
         break;
      case rtPrg:
         //set up for transfer
         StreamStartAddr = (MenuSource[RegSelect].Code_Image[1]<<8) + MenuSource[RegSelect].Code_Image[0];
         StreamOffsetAddr = 2; //set to start of data
         break;
   }
}

void MenuChange()
{
   switch(CurrentMenu)
   {
      case rmtTeensy:
         MenuSource = ROMMenu; 
         NumMenuItems = sizeof(ROMMenu)/sizeof(USBHostMenu);
         break;
      case rmtSD:
         stpcpy(SDPath, "/");
         LoadSDDirectory();
         MenuSource = SDMenu; 
         NumMenuItems = NumSDItems;
         break;
      case rmtUSBDrive:
         stpcpy(USBDrivePath, "/");
         LoadUSBDriveDirectory();
         MenuSource = USBDriveMenu; 
         NumMenuItems = NumUSBDriveItems;
         break;
      case rmtUSBHost:
         MenuSource = &USBHostMenu; 
         NumMenuItems = NumUSBHostItems;
         break;
   }
}


void LoadDirPrgCrt(bool SD_nUSBDrive)
{
   char * myPath;
   if (SD_nUSBDrive) myPath = SDPath;
   else myPath = USBDrivePath;
   
   switch(MenuSource[RegSelect].ItemType)
   {
      case rtDir: //load the new directory from SD to SDMenu/NumSDItems
         if(strcmp(MenuSource[RegSelect].Name, UpDirString)==0)
         {  //up dir
            char * LastSlash = strrchr(myPath, '/'); //find last slash
            if (LastSlash != NULL) LastSlash[0] = 0;  //terminate it there 
         }
         else strcat(myPath, MenuSource[RegSelect].Name); //append dir name
         if (SD_nUSBDrive)
         {
            LoadSDDirectory(); 
            NumMenuItems = NumSDItems;
         }
         else 
         {
            LoadUSBDriveDirectory(); 
            NumMenuItems = NumUSBDriveItems;
         }
         return;
         break;
      case rtPrg: //Load the prg file to RAM
         if(!LoadFile(SD_nUSBDrive)) MenuSource[RegSelect].ItemType=rtUnk; //mark unknown if error
         break;
      case rtCrt: //load the Crt in to RAM and parse it for emulation
         if(!LoadFile(SD_nUSBDrive)) MenuSource[RegSelect].ItemType=rtUnk; //mark unknown if error
         else ParseCRTFile(&MenuSource[RegSelect]); //will convert type, if checks ok
         break;
   } 
   
}

bool LoadFile(bool SD_nUSBDrive) 
{
   char *myPath;
   if (SD_nUSBDrive) myPath= SDPath;
   else myPath = USBDrivePath;
   
   char FullFilePath[300];

   if (strlen(myPath) == 1 && myPath[0] == '/') sprintf(FullFilePath, "%s%s", myPath, MenuSource[RegSelect].Name);  // at root
   else sprintf(FullFilePath, "%s/%s", myPath, MenuSource[RegSelect].Name);
      
   Serial.printf("Openning: %s\n", FullFilePath);
   
   File myFile;
   if (SD_nUSBDrive) myFile= SD.open(FullFilePath, FILE_READ);
   else myFile= firstPartition.open(FullFilePath, FILE_READ);
   
   if (!myFile) return false;

   uint16_t count=0;
   while (myFile.available()) RAM_Image[count++]=myFile.read();
   
   MenuSource[RegSelect].Code_Image = RAM_Image;
   MenuSource[RegSelect].Size = count;
   myFile.close();
   return true;
}


// ***********  These can be called anytime, but will likely disrupt emulation isr if not called from above

void LoadUSBDriveDirectory()
{  
   File USBDrivedir = firstPartition.open(USBDrivePath);
   LoadDirectory(&NumUSBDriveItems, USBDrivedir, USBDrivePath, USBDriveMenu);
   Serial.printf("%d files from USBDrive%s\nLast: %s\n", NumUSBDriveItems, USBDrivePath, USBDriveMenu[NumUSBDriveItems-1].Name); 
}

void LoadSDDirectory() 
{
   File SDdir = SD.open(SDPath);
   LoadDirectory(&NumSDItems, SDdir, SDPath, SDMenu);
   Serial.printf("%d files from SDCard%s\nLast: %s\n", NumSDItems, SDPath, SDMenu[NumSDItems-1].Name); 
}

void LoadDirectory(uint8_t *NumItems, File dir, char *DrvPath, StructMenuItem* DrvMenuItem) 
{
   *NumItems = 0;
   if(!dir) return;   
   
   if (!(strlen(DrvPath) == 1 && DrvPath[0] == '/'))
   {  //not at root, add up dir option
      *NumItems = 1;
      strcpy(DrvMenuItem[0].Name, UpDirString);
      DrvMenuItem[0].ItemType = rtDir;
   }
   
   do
   {
      File entry = dir.openNextFile();
      if (! entry) return;
      if (entry.isDirectory())
      {
         DrvMenuItem[*NumItems].Name[0] = '/';
         memcpy(DrvMenuItem[*NumItems].Name+1, entry.name(), MaxItemNameLength-1);
      }
      else memcpy(DrvMenuItem[*NumItems].Name, entry.name(), MaxItemNameLength);
      
      DrvMenuItem[*NumItems].Name[MaxItemNameLength-1]=0; //terminate in case too long. 
      
      if (entry.isDirectory()) DrvMenuItem[*NumItems].ItemType = rtDir;
      else 
      {
         char* Extension = (DrvMenuItem[*NumItems].Name + strlen(DrvMenuItem[*NumItems].Name) - 4);
         for(uint8_t cnt=1; cnt<=3; cnt++) if(Extension[cnt]>='A' && Extension[cnt]<='Z') Extension[cnt]+=32;
         
         if (strcmp(Extension, ".prg")==0) DrvMenuItem[*NumItems].ItemType = rtPrg;
         else if (strcmp(Extension, ".crt")==0) DrvMenuItem[*NumItems].ItemType = rtCrt;
         else DrvMenuItem[*NumItems].ItemType = rtUnk;
      }
      
      //Serial.printf("%d- %s\n", *NumItems, DrvMenuItem[*NumItems].Name); 
      entry.close();
   } while((*NumItems)++ < MaxMenuItems);
   
   Serial.print("Too many files!");
}

void ParseCRTFile(StructMenuItem* MyMenuItem)   
{
   //https://vice-emu.sourceforge.io/vice_17.html#SEC369
   //http://ist.uwaterloo.ca/~schepers/formats/CRT.TXT
   
   if (memcmp(RAM_Image, "C64 CARTRIDGE", 13)!=0)
   {
      Serial.println("\"C64 CARTRIDGE\" not found");
      return;
   }
   
   uint32_t HeaderLen = toU32(RAM_Image+0x10);
   Serial.printf("Header len: %lu\n", HeaderLen);
   if (HeaderLen < 0x40) HeaderLen = 0x40;
   
   Serial.printf("HW Type: %d\n", toU16(RAM_Image+0x16));
   if (toU16(RAM_Image+0x16) !=0)
   {
      Serial.println("Only \"Normal\" carts *currently* supported");
      return;
   }
   
   uint8_t EXROM = RAM_Image[0x18];
   uint8_t GAME = RAM_Image[0x19];
   Serial.printf("EXROM: %d\n", EXROM);
   Serial.printf(" GAME: %d\n", GAME);
   
   Serial.printf("Name: %s\n", (RAM_Image+0x20));
   
   uint8_t *ChipImage = RAM_Image+HeaderLen;
   //On to CHIP packet(s)...
   if (memcmp(ChipImage, "CHIP", 4)!=0)
   {
      Serial.println("\"CHIP\" not found");
      return;
   }
  
   Serial.printf("Packet len: $%08x\n",  toU32(ChipImage+0x04));
   Serial.printf("Chip Type: %d\n",      toU16(ChipImage+0x08));
   Serial.printf(" Bank Num: %d\n",      toU16(ChipImage+0x0A));
   Serial.printf("Load Addr: $%04x\n",   toU16(ChipImage+0x0C));
   Serial.printf(" ROM Size: $%04x\n",   toU16(ChipImage+0x0E));
   
   //We have a good CRT image, new defaults:
   MyMenuItem->ItemType = rtUnk;
   NumUSBHostItems = 1;
   
   MyMenuItem->Code_Image=RAM_Image+HeaderLen+0x10;
   
   if(EXROM==0 && GAME==1 && toU16(ChipImage+0x0C) == 0x8000 && toU16(ChipImage+0x0E) == 0x2000)
   {
      MyMenuItem->ItemType = rt8kLo;
      Serial.println("\n 8kLo config");
      return;
   }      

   if(EXROM==1 && GAME==0 && toU16(ChipImage+0x0C) == 0xe000 && toU16(ChipImage+0x0E) == 0x2000)
   {
      MyMenuItem->ItemType = rt8kHi;
      Serial.println("\n 8kHi config");
      return;
   }      

   if(EXROM==0 && GAME==0 && toU16(ChipImage+0x0C) == 0x8000 && toU16(ChipImage+0x0E) == 0x4000)
   {
      MyMenuItem->ItemType = rt16k;
      Serial.println("\n 16k config");
      return;
   }      
   
   Serial.println("\nHW config unknown!");
}
