using Spectre.Console;
using System.Reactive.Linq;
using System.Text;
using TeensyRom.Cli.Helpers;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Common;

namespace TeensyRom.Cli.Services
{
    internal interface ICliLoggingService : ILoggingService
    {
        bool Enabled { get; set; }
    }
    internal class CliLoggingService : LoggingService, ICliLoggingService
    {
        public bool Enabled { get; set; } = true;
        public override void Log(string message, string hExColor, bool newLine = true)
        {
            var sb = new StringBuilder();

            var log = message
                .SplitAtCarriageReturn()
                .Select(line => WithColor(line, hExColor))
                .Aggregate(sb, (acc, line) => acc.AppendWithLimit(line))
                .ToString()
                .DropLastNewLine();

            _logs.OnNext(log);

            if (Enabled)
            {
                var carriageReturn = newLine ? "\r\n\r\n" : "\r\n";
                AnsiConsole.Markup($"{log}{carriageReturn}");
            }
            base.Log(message, hExColor);
        }

        private string WithColor(string message, string hexColor)
        {
            message = message.EscapeBrackets();
            message = $"[{hexColor}]{message}[/]\r\n";
            return message;
        }
    }
}
