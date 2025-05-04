using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using RadEndpoints;
using System.Text.RegularExpressions;
using TeensyRom.Core.Common;

namespace TeensyRom.Api.Endpoints.LaunchFile
{
    public class LaunchFileRequest 
    {
        [FromRoute]
        public string Path { get; set; } = string.Empty;
    }

    //public class LaunchFileRequestValidator : AbstractValidator<LaunchFileRequest>
    //{
    //    public LaunchFileRequestValidator()
    //    {
    //        RuleFor(x => x.Path)
    //        .NotEmpty().WithMessage("Path is required.")
    //        .Must(path => path.IsValidUnixFilePath()).WithMessage("Path must be a valid Unix-style file path.");
    //    }
    //}

    public class LaunchFileResponse
    {
        public string Message { get; set; } = "Success!";
    }
}