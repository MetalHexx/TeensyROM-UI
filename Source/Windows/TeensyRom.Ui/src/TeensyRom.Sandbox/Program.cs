using NAudio;
using NAudio.Midi;
using System.Diagnostics;
using System.Management;
using System.Reflection.Metadata.Ecma335;

GetUsbDevices();
ReceiveMidiInputLoop();

static void ReceiveMidiInputLoop()
{
    Console.WriteLine("Starting MIDI Listening...");

    for (int i = 0; i < MidiIn.NumberOfDevices; i++)
    {
        try
        {
            var midiIn = TryGetMidiIn(i);

            if (midiIn is null) continue;

            string deviceName = "Unknown Device";
            try { deviceName = MidiIn.DeviceInfo(i).ProductName; } catch { }

            midiIn.MessageReceived += (sender, e) =>
            {
                Console.WriteLine($"{e.MidiEvent}");
            };
            midiIn.Start();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error receiving MIDI from device index {i}: {ex.Message}");
        }
    }

    while (true)
    {
        System.Threading.Thread.Sleep(1000);
    }
}

static MidiIn? TryGetMidiIn(int deviceId)
{
    try
    {
        return new MidiIn(deviceId);
    }
    catch (MmException) { }

    return null;
}

static void GetUsbDevices()
{
    //Gets all the serial devices in the system and prints their info
    var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_SerialPort");
    foreach (ManagementObject port in searcher.Get())
    {
        Console.WriteLine("---- Serial Port Device ----");
        foreach (PropertyData property in port.Properties)
        {
            Console.WriteLine("{0}: {1}", property.Name, property.Value);
        }
        Console.WriteLine("-----------------------------\n");
    }
}