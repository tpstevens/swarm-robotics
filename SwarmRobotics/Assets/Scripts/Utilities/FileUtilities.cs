using UnityEngine;

using System;
using System.IO;

namespace Utilities
{
	public class FileUtilities
	{
		private static readonly string runtimePath = "Runtime/";

		public static string buildArgsPath(string filename)
		{
			string path = (!Application.isEditor) ? "" : runtimePath;
			ensureFolderExists(path);
			return path + filename;
		}

		public static string buildConfigPath(string filename)
		{
			string path = ((!Application.isEditor) ? "" : runtimePath) + "config/";
			ensureFolderExists(path);
			return path + filename;
		}

		public static string buildLogPath(string filename)
		{
			string path = ((!Application.isEditor) ? "" : runtimePath) + "logs/";
			ensureFolderExists(path);
			return path + filename;
		}

		public static void ensureFolderExists(string folderPath)
		{
			if (!folderPath.EndsWith("/"))
			{
				int slashIndex = folderPath.LastIndexOf('/');
				folderPath = ((slashIndex == -1) ? "" : folderPath.Substring(0, slashIndex + 1));
			}

			if (folderPath != "" && !Directory.Exists(folderPath))
			{
				try
				{
					Directory.CreateDirectory(folderPath);
					Log.d(LogTag.FILEUTILITIES, "Successfully created directory \"" + folderPath + "\"");
				}
				catch (Exception ex)
				{
					Log.e(LogTag.FILEUTILITIES, "Failed to create directory \"" + folderPath + "\": " + ex.Message);
				}
			}
		}
	}
}
