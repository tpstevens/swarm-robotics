using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using Utilities;

namespace UserInterface
{
    [System.Serializable]
    public class ConsoleLine
    {
        public InputField input;

        private bool active;
        private bool initialized;
        private MainInterface mainScript;

        /// <summary>
        /// Initialize the console's internals.
        /// </summary>
        /// <returns>Whether initialization was successful.</returns>
        public bool initialize()
        {
            initialized = true;

            GameObject mainObject = GameObject.Find("Scripts");
            if (mainObject != null)
            {
                mainScript = mainObject.GetComponent<MainInterface>();
            }

            if (input == null)
            {
                Log.e(LogTag.UI, "ConsoleLine missing InputField!");
                initialized = false;
            }
            
            if (mainScript == null)
            {
                Log.e(LogTag.UI, "ConsoleLine missing MainScript!");
                initialized = false;
            }

            if (initialized)
            {
                input.onEndEdit.AddListener(delegate(string txt) {
                    if (Input.GetKeyDown(KeyCode.Return))
                    {
                        queueConsoleCommand(txt);

                        input.text = "";
                        input.ActivateInputField();
                        input.Select();
                    }
                });

                active = true;
                toggle();
            }

            return active;
        }

        /// <summary>
        /// Returns whether the console is active and has user focus.
        /// </summary>
        /// <returns>Whether the console is active and has user focus.</returns>
        public bool isSelected()
        {
            return initialized && input.isFocused;
        }

        /// <summary>
        /// Toggles the console on or off.
        /// </summary>
        /// <returns>Whether the console is currently active.</returns>
        public bool toggle()
        {
            if (initialized)
            {
                active = !active;
                input.gameObject.SetActive(active);

                if (active)
                {
                    input.text = "";
                    input.ActivateInputField();
                    input.Select();
                }
            }

            return active;
        }

        /// <summary>
        /// Send a (trimmed) command to the main script.
        /// </summary>
        /// <param name="cmd">The command, which will be trimmed.</param>
        private void queueConsoleCommand(string cmd)
        {
            cmd = cmd.Trim();

            if (initialized)
            {
                Log.w(LogTag.UI, "Queueing console command: " + cmd);
                mainScript.queueConsoleCommand(cmd);
            }
            else
            {
                Log.e(LogTag.UI, "Failed to queue console command because console not initialized");
            }
        }
    }
}
