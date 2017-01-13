using UnityEngine.SceneManagement;

using System;

namespace Utilities
{
	public class ApplicationManager
	{
		public static void reloadScene()
		{
			Log.w(LogTag.MAIN, "Writing log to file and reloading scene");
			string timestamp = DateTime.Now.ToString("yyyy-MM-dd_hh-mm-ss");
			Log.writeToFile("log_" + timestamp + ".txt");
			Log.clear();
			SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		}

		public static void quit()
		{
			string timestamp = DateTime.Now.ToString("yyyy-MM-dd_hh-mm-ss");
			Log.writeToFile("log_" + timestamp + ".txt");

#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#else
			UnityEngine.Application.Quit();
#endif
		}
	}
}
