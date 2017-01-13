using System.IO;

namespace Utilities
{
	public class Configuration
	{
		private float groundLength = 50.0f;

		public float GroundLength
		{
			get { return groundLength; }
			set { if (value > 0f) { groundLength = value; } }
		}

		public Configuration(string configFile)
		{
			string filePath = FileUtilities.buildConfigPath(configFile);

			if (File.Exists(filePath))
			{
				float fValue;

				string[] configOptions = File.ReadAllLines(filePath);
				for (int i = 0; i < configOptions.Length; ++i)
				{
					string option = configOptions[i];
					if (option.Trim().Length > 0)
					{
						string key = option.Trim();
						int delimIndex = key.IndexOf('=');

						if (delimIndex != -1)
						{
							string value = key.Substring(delimIndex + 1).Trim();
							key = key.Substring(0, delimIndex).Trim().ToLower();
							if (key != null)
							{
								if (key == "groundlength")
								{
									if (float.TryParse(value, out fValue))
									{
										groundLength = fValue;
									}
									else
									{
										Log.w(LogTag.CONFIG, "Invalid ground length " + value);
									}
								}
								// TODO add other options to if-else here
							}
						}
						else
						{
							Log.e(LogTag.CONFIG, "Invalid key-value format: " + key);
						}
					}
				}
			}
			else
			{
				Log.e(LogTag.CONFIG, "Config file \"" + filePath + "\" not found");
			}
		}
	}
}
