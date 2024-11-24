using Spectre.Console;
using TeensyRom.Cli.Helpers;
using static System.Runtime.InteropServices.JavaScript.JSType;
using TeensyRom.Core.Logging;

namespace TeensyRom.Cli.Services
{
    internal class CliAlertService : IAlertService
    {
        public IObservable<string> CommandErrors => throw new NotImplementedException();

        public void Publish(string error)
        {
            if(!string.IsNullOrWhiteSpace(error))
            {
                RadHelper.WriteLine(error.EscapeBrackets());                
                
            }
            AnsiConsole.WriteLine(RadHelper.ClearHack);
        }

        public void PublishError(string message)
        {
            RadHelper.WriteError(message.EscapeBrackets());
            AnsiConsole.WriteLine(RadHelper.ClearHack);
        }
    }
}
