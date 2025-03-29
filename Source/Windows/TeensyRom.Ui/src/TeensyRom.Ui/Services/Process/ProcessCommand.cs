namespace TeensyRom.Ui.Services.Process
{
    public class ProcessCommand<T>
    {
        public ProcessCommandType MessageType { get; set; }
        public T Value { get; set; } = default!;
    }
}