using UnityEngine.SceneManagement;

using System;

namespace Utilities
{
    public class ApplicationManager
    {
        private static bool handledQuitEvent = false;

        /// <summary>
        /// Write the current log to file, clear the log, and reload the current scene.
        /// </summary>
        public static void reloadScene()
        {
            Log.w(LogTag.MAIN, "Writing log to file and reloading scene");
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_hh-mm-ss");
            Log.writeToFile("log_" + timestamp + ".txt");
            Log.clear();

            Comm.clear();

            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        /// <summary>
        /// Write the current log to file and quit the application.
        /// </summary>
        public static void quit()
        {
            // to prevent MonoBehaviour.OnApplicationQuit() from writing duplicate logs
            if (!handledQuitEvent)
            {
                handledQuitEvent = true;
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
}
