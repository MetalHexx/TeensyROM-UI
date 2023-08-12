// MIT License
// 
// Copyright (c) 2023 Travis Smith
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software 
// and associated documentation files (the "Software"), to deal in the Software without 
// restriction, including without limitation the rights to use, copy, modify, merge, publish, 
// distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom 
// the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or 
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING 
// BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, 
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

//  TeensyROM: A C64 ROM emulator and loader/interface cartidge based on the Teensy 4.1
//  Copyright (c) 2023 Travis Smith <travis@sensoriumembedded.com> 


#include <SD.h>
#include <USBHost_t36.h>
#include <SPI.h>
#include <NativeEthernet.h>
#include <NativeEthernetUdp.h>
#include <EEPROM.h>
#include "TeensyROM.h"
#include "DriveDirLoad.h"
#include "Menu_Regs.h"
#include "MainMenuItems.h"
#include "IOHandlers.h"

uint8_t* RAM_Image = NULL; //For receiving files from USB Drive & SD
volatile uint8_t BtnPressed = false; 
volatile uint8_t EmulateVicCycles = false;
volatile uint8_t Phi2ISRState = P2I_Normal;
uint8_t CurrentIOHandler = IOH_None;

StructMenuItem *DriveDirMenu;
char DriveDirPath[MaxPathLength];

StructMenuItem USBHostMenu = {
   rtNone,  // ItemType;
   IOH_None, // IOHndlrAssoc;  enumIOHandlers
   "<Nothing Sent>",      // Name[];
   NULL,    // *Code_Image;
   0        // Size;
};
uint8_t* HOST_Image = NULL; //For receiving files from USB Host
uint8_t NumUSBHostItems = 1;

extern "C" uint32_t set_arm_clock(uint32_t frequency);
extern float tempmonGetTemp(void);

void setup() 
{
   set_arm_clock(816000000);  //slight overclocking, no cooling required
   
   Serial.begin(115200);
   if (CrashReport) Serial.print(CrashReport);

   for(uint8_t PinNum=0; PinNum<sizeof(OutputPins); PinNum++) pinMode(OutputPins[PinNum], OUTPUT); 
   DataBufDisable; //buffer disabled
   SetDataPortDirOut; //default to output (for C64 Read)
   SetDMADeassert;
   SetIRQDeassert;
   SetNMIDeassert;
   SetLEDOn;
   SetDebugDeassert;
   SetResetAssert; //assert reset until main loop()
  
   for(uint8_t PinNum=0; PinNum<sizeof(InputPins); PinNum++) pinMode(InputPins[PinNum], INPUT); 
   pinMode(Reset_Btn_In_PIN, INPUT_PULLUP);  //also makes it Schmitt triggered (PAD_HYS)
   pinMode(PHI2_PIN, INPUT_PULLUP);   //also makes it Schmitt triggered (PAD_HYS)
   attachInterrupt( digitalPinToInterrupt(Reset_Btn_In_PIN), isrButton, FALLING );
   attachInterrupt( digitalPinToInterrupt(PHI2_PIN), isrPHI2, RISING );
   NVIC_SET_PRIORITY(IRQ_GPIO6789,16); //set HW ints as high priority, otherwise ethernet int timer causes misses
   
   myusbHost.begin(); // Start USBHost_t36, HUB(s) and USB devices.

   uint32_t MagNumRead;
   EEPROM.get(eepAdMagicNum, MagNumRead);
   if (MagNumRead != eepMagicNum) SetEEPDefaults();

   IO1 = (uint8_t*)calloc(IO1_Size, sizeof(uint8_t)); //allocate IO1 space and init to 0
   IO1[rwRegStatus]        = rsReady;
   IO1[rWRegCurrMenuWAIT] = rmtTeensy;
   SetNumItems(sizeof(TeensyROMMenu)/sizeof(TeensyROMMenu[0]));
   IO1[rRegPresence1]     = 0x55;   
   IO1[rRegPresence2]     = 0xAA;   
   for (uint16_t reg=rRegSIDStrStart; reg<rRegSIDStringTerm; reg++) IO1[reg]=' '; 
   IO1[rRegSIDStringTerm] = 0;   
   IO1[rwRegPwrUpDefaults]= EEPROM.read(eepAdPwrUpDefaults);
   IO1[rwRegTimezone]     = EEPROM.read(eepAdTimezone);  
   //IO1[rwRegNextIOHndlr] = EEPROM.read(eepAdNextIOHndlr); //done each entry into menu
   DriveDirMenu = (StructMenuItem*)malloc(MaxMenuItems * sizeof(StructMenuItem)); //takes about 265k of RAM2
   SetUpMainMenuROM();

   for(uint8_t cnt=0; cnt<IOH_Num_Handlers; cnt++) PadSpace(IOHandler[cnt]->Name, IOHNameLength-1); //done so selection shown on c64 overwrites previous

   //bus timing check prep:
   //BigBuf = (uint32_t*)malloc(BigBufSize*sizeof(uint32_t));
   //BigBufCount = 0;
   //Phi2ISRState = P2I_TimingCheck;
   //while (Phi2ISRState!=P2I_Normal);
   //for (uint8_t SampNum = 0; SampNum < BigBufSize; SampNum++) 
   //{
   //   Serial.print(SampNum);
   //   Serial.print(" : ");
   //   Serial.print(BigBuf[SampNum]);
   //   Serial.print(" : ");
   //   Serial.println(CycTonS(BigBuf[SampNum]));
   //}
   //free(BigBuf);
   
   MakeBuildCPUInfoStr();
   Serial.printf("\n%sTeensyROM %s is on-line\n", SerialStringBuf, strVersionNumber);
} 
     
void loop()
{
   if (BtnPressed)
   {
      Serial.print("Button detected\n"); 
      SetLEDOn;
      BtnPressed=false;
      SetUpMainMenuROM(); //back to main menu
      Phi2ISRState=P2I_Normal;
   }
   
   if (doReset)
   {
      SetResetAssert; 
      Serial.println("Resetting C64"); 
      Serial.flush();
      delay(50); 
      while(ReadButton==0); //avoid self reset detection
      doReset=false;
      BtnPressed = false;
      SetResetDeassert;
   }
  
   if (Serial.available()) ServiceSerial();
   myusbHost.Task();
   
   //handler specific polling items:
   if (IOHandler[CurrentIOHandler]->PollingHndlr != NULL) IOHandler[CurrentIOHandler]->PollingHndlr();
}

void SetUpMainMenuROM()
{
   //emulate 16k cart ROM
   SetIRQDeassert;
   SetNMIDeassert;
   SetGameAssert;
   SetExROMAssert;
   LOROM_Image = TeensyROMC64_bin;
   HIROM_Image = TeensyROMC64_bin+0x2000;
   NVIC_ENABLE_IRQ(IRQ_ENET); //make sure ethernet interrupt is back on
   NVIC_ENABLE_IRQ(IRQ_PIT);
   EmulateVicCycles = false;
   IOHandlerInit(IOH_TeensyROM);   
   doReset = true;
}

void PadSpace(char* StrToPad, uint8_t PadToLength)
{
   while(strlen(StrToPad)<PadToLength) strcat(StrToPad, " ");
}

void EEPwriteBuf(uint16_t addr, const uint8_t* buf, uint8_t len)
{
   while (len--) EEPROM.write(addr+len, buf[len]);    
}

void EEPreadBuf(uint16_t addr, uint8_t* buf, uint8_t len)
{
   while (len--) buf[len] = EEPROM.read(addr+len);   
}

void SetEEPDefaults()
{
   Serial.println("Setting EEPROM to defaults");
   EEPROM.put(eepAdMagicNum, (uint32_t)eepMagicNum);
   EEPROM.write(eepAdPwrUpDefaults, rpudMusicMask /* | rpudNetTimeMask */); //default music on, eth time synch off
   EEPROM.write(eepAdTimezone, -7); //default to pacific time
   EEPROM.write(eepAdNextIOHndlr, IOH_None); //default to no Special HW
   SetEthEEPDefaults();
}

void SetNumItems(uint16_t NumItems)
{
   NumItemsFull = NumItems;
   IO1[rRegNumItemsOnPage] = (NumItemsFull > MaxItemsPerPage ? MaxItemsPerPage : NumItemsFull);
   IO1[rwRegPageNumber] = 1;
   IO1[rRegNumPages] = 
      NumItems/MaxItemsPerPage + 
      (NumItems%MaxItemsPerPage!=0 ? 1 : 0) +
      (NumItems==0 ? 1 : 0);
}
