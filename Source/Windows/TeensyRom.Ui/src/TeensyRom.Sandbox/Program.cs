using System.Management;

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