namespace TeensyRom.Core.Commands
{
    public class TeensyCommandResult
    {
        public bool IsSuccess { get; set; } = true;
        public bool IsBusy { get; set; } = false;
        public string Error = string.Empty;
        public string ResponseMessages = string.Empty;
    }
}
