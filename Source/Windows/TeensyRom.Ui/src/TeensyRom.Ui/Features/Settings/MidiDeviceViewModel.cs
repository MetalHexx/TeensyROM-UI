using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using TeensyRom.Core.Entities.Midi;

namespace TeensyRom.Ui.Features.Settings
{
    public class MidiDeviceViewModel : ReactiveObject
    {
        [Reactive] public string Name { get; set; } = string.Empty;
        [Reactive] public string UnboundName { get; set; } = string.Empty;
        [Reactive] public int Id { get; set; }
        [Reactive] public string ManufacturerName { get; set; } = string.Empty;
        [Reactive] public int ProductId { get; set; }
        [Reactive] public int DriverVersion { get; set; }
        [Reactive] public string ProductName { get; set; } = string.Empty;

        public MidiDeviceViewModel(MidiDevice d)
        {
            Name = d.Name;
            UnboundName = d.Name;
            Id = d.Id;
            ManufacturerName = d.ManufacturerName;
            ProductId = d.ProductId;
            DriverVersion = d.DriverVersion;
            ProductName = d.ProductName;
        }

        public MidiDeviceViewModel(MidiDeviceViewModel? d) 
        {
            Name = d?.Name ?? string.Empty;
            UnboundName = d?.UnboundName ?? string.Empty;
            Id = d is null ? 0 : d.Id;
            ManufacturerName = d?.ManufacturerName ?? string.Empty;
            ProductId = d is null ? 0 : d.ProductId;
            DriverVersion = d is null ? 0 : d.DriverVersion;
            ProductName = d?.ProductName ?? string.Empty;
        }

        public MidiDevice ToMidiDevice() 
        {
            return new MidiDevice
            {
                Name = Name,
                Id = Id,
                ManufacturerName = ManufacturerName,
                ProductId = ProductId,
                DriverVersion = DriverVersion,
                ProductName = ProductName
            };
        }

        public override bool Equals(object? obj)
        {
            if (obj is MidiDeviceViewModel other)
            {
                return UnboundName == other.UnboundName;
            }
            return false;
        }

        public override int GetHashCode() 
        {
            return UnboundName.GetHashCode();
        }
    }
}