using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Core.Midi;
using FluentAssertions;
using NAudio.Midi;
using System.Diagnostics;

namespace TeensyRom.Tests.Unit
{
    public class MidiServiceTests
    {
        //[Fact]
        //public void GetMidiDevices_ShouldReturnListOfMidiDevices()
        //{
        //    // Arrange
            
        //    var midiService = new MidiService();
        //    // Act
        //    var devices = midiService.GetMidiDevices();
        //    // Assert
        //    devices.Should().NotBeNull();

        //}

        //[Fact]
        //public void SendRandomMidiNotes_ShouldSendNotesWithoutException()
        //{
        //    var midiService = new MidiService();
        //    midiService.SendRandomMidiNotesToAllDevices();
        //    Assert.True(true);
        //}

        //[Fact]
        //public void SendRandomMidiNotes_ToSingleDevice_ShouldSendNotesWithoutException()
        //{
        //    var midiService = new MidiService();
        //    midiService.SendRandomMidiNotes(3, 250, 100);
        //    Assert.True(true);
        //}

        //[Fact]
        //public void ReceiveMidiInputLoop_Should_NotThrowException()
        //{
        //    for (int i = 0; i < MidiIn.NumberOfDevices; i++)
        //    {
        //        try
        //        {
        //            var midiIn = new MidiIn(i);
        //            string deviceName = "Unknown Device";
        //            try { deviceName = MidiIn.DeviceInfo(i).ProductName; } catch { }

        //            midiIn.MessageReceived += (sender, e) =>
        //            {
        //                Debug.WriteLine($"[MIDI IN {deviceName}] {e.MidiEvent}");
        //            };
        //            midiIn.Start();
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine($"Error receiving MIDI from device index {i}: {ex.Message}");
        //        }
        //    }

        //    while (true)
        //    {
        //        System.Threading.Thread.Sleep(1000);
        //    }
        //    Assert.True(true);
        //}
    }
}
