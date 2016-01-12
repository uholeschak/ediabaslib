using System;
using System.Collections.Generic;
using System.IO;
namespace MonoDroid.Utils
{
	internal static class MessageUtils
	{
		internal static string MapGeneratedToProjectFile(string filename)
		{
			string result;
			try
			{
				string directory = filename.Substring(0, filename.IndexOf(string.Format("{0}obj{0}", Path.DirectorySeparatorChar)));
				List<string> list = MessageUtils.FindFileInDirectory(directory, Path.GetFileName(filename));
				if (list.Count == 1)
				{
					result = list[0];
				}
				else
				{
					result = Path.GetFileName(filename);
				}
			}
			catch (Exception)
			{
				result = Path.GetFileName(filename);
			}
			return result;
		}
		private static List<string> FindFileInDirectory(string directory, string filename)
		{
			List<string> list = new List<string>();
			string[] directories = Directory.GetDirectories(directory);
			for (int i = 0; i < directories.Length; i++)
			{
				string text = directories[i];
				if (!(Path.GetFileName(text).ToLowerInvariant() == "obj") && !(Path.GetFileName(text).ToLowerInvariant() == "bin"))
				{
					list.AddRange(MessageUtils.FindFileInDirectory(text, filename));
				}
			}
			string[] files = Directory.GetFiles(directory);
			for (int j = 0; j < files.Length; j++)
			{
				string text2 = files[j];
				if (Path.GetFileName(text2).ToLowerInvariant() == filename.ToLowerInvariant())
				{
					list.Add(text2);
				}
			}
			return list;
		}
	}
}
