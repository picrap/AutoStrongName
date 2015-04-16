﻿#region AutoStrongName
// AutoStrongName
// An automatic tool to presign unsigned dependencies
// https://github.com/picrap/AutoStrongName
#endregion

namespace AutoStrongName
{
    using CommandLine;

    public class Parameters
    {
        public const string ProjectOption = "project";

        [Option('p', ProjectOption, Required = true, HelpText = "Project path.")]
        public string ProjectPath { get; set; }
    }
}
