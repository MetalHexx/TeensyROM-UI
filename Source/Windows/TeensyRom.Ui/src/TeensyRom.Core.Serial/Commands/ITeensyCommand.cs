using MediatR;
using TeensyRom.Core.Serial.State;

namespace TeensyRom.Core.Serial.Commands
{
    public interface ITeensyCommand<T> : IRequest<T> 
    {
        string? DeviceId { get; set; }
        ISerialStateContext Serial { get; set; }
    }
}