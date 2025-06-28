namespace TeensyRom.Core.Abstractions
{
    public interface ILogStream 
    {
        Task Push(string logMessage, CancellationToken ct);
    }
}
