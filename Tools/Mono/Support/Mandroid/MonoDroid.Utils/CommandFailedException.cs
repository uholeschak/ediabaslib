using System;
using System.Text.RegularExpressions;
namespace MonoDroid.Utils
{
	internal class CommandFailedException : InvalidOperationException
	{
		public string FileName
		{
			get;
			private set;
		}
		public string Arguments
		{
			get;
			private set;
		}
		public string ErrorLog
		{
			get;
			private set;
		}
		public int ExitCode
		{
			get;
			private set;
		}
		public new string Message
		{
			get;
			private set;
		}
		public string VSFormattedErrorLog
		{
			get
			{
				return this.FormatForVS(this.ErrorLog);
			}
		}
		public CommandFailedException()
		{
		}
		public CommandFailedException(string message) : base(message)
		{
		}
		public CommandFailedException(string filename, string arguments, string errorLog, int exitCode)
		{
			this.FileName = filename;
			this.Arguments = arguments;
			this.ErrorLog = errorLog;
			this.ExitCode = exitCode;
			this.Message = string.Concat(new string[]
			{
				"Command failed. Command: ",
				this.FileName,
				" ",
				this.Arguments,
				"\n\t",
				(!string.IsNullOrEmpty(this.ErrorLog)) ? this.ErrorLog : "<none>\n"
			});
		}
		private string FormatForVS(string text)
		{
			Regex regex = new Regex("(?<FileName>.+):(?<LineNumber>\\d+): error: Error: (?<Error>.+)");
			string result;
			if (!regex.IsMatch(text))
			{
				result = text;
			}
			else
			{
				Match match = regex.Match(text);
				string value = match.Groups["FileName"].Value;
				string value2 = match.Groups["LineNumber"].Value;
				string value3 = match.Groups["Error"].Value;
				int num;
				if (!int.TryParse(value2, out num))
				{
					result = text;
				}
				else
				{
					num++;
					result = string.Format("{0}({1}): error 1: {2}", MessageUtils.MapGeneratedToProjectFile(value), num, value3);
				}
			}
			return result;
		}
	}
}
