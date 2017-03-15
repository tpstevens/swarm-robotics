using UnityEngine;
using UnityEngine.SceneManagement;

using System;

using CommSystem;

namespace Utilities
{
    public class ApplicationManager
    {
        public static readonly int LAYER_ROBOTS = 8;
        public static readonly int LAYER_MESSAGES = 9;
        public static readonly int LAYER_RESOURCES = 10;

        private static bool handledQuitEvent = false;
        private static float[] timeScales = { 0.25f, 0.5f, 1, 2, 4, 8 };
        private static int currentTimeScale = 2;

        public static void decreaseSimulationSpeed()
        {
            int lastTimeScale = currentTimeScale;
            currentTimeScale = Math.Max(currentTimeScale - 1, 0);

            if (currentTimeScale != lastTimeScale)
            {
                if (Time.timeScale != 0)
                    Time.timeScale = timeScales[currentTimeScale];

                Log.w(LogTag.MAIN, "Set timescale to " + timeScales[currentTimeScale]);
            }
        }

        public static void increaseSimulationSpeed()
        {
            int lastTimeScale = currentTimeScale;
            currentTimeScale = Math.Min(currentTimeScale + 1, timeScales.Length - 1);

            if (currentTimeScale != lastTimeScale)
            {
                if (Time.timeScale != 0)
                    Time.timeScale = timeScales[currentTimeScale];

                Log.w(LogTag.MAIN, "Set timescale to " + timeScales[currentTimeScale]);
            }
        }

        /// <summary>
        /// Write the current log to file, clear the log, and reload the current scene.
        /// </summary>
        public static void reloadScene()
        {
            Log.w(LogTag.MAIN, "Writing log to file and reloading scene");

            Time.timeScale = 0.0f; // disable updates until reload has completed

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

        public static void pause()
        {
            if (Time.timeScale != 0.0f)
                Time.timeScale = 0.0f;
        }

        public static void togglePause()
        {
            if (Time.timeScale == 0.0f)
                Time.timeScale = timeScales[currentTimeScale];
            else
                Time.timeScale = 0.0f;
        }

        public static void unpause()
        {
            if (Time.timeScale == 0.0f)
                Time.timeScale = timeScales[currentTimeScale];
        }
    }
}
