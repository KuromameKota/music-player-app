using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Debugs
{
    public class DebugLogScrollView : MonoBehaviour
    {
        [SerializeField]
        private Text debugText = null;

        private void Awake()
        {
            Application.logMessageReceived += OnLogMessage;
        }

        private void OnDestroy()
        {
            Application.logMessageReceived -= OnLogMessage;
        }

        private void OnLogMessage(string logText, string stackTrace, LogType type)
        {
            if (string.IsNullOrEmpty(logText))
            {
                return;
            }

            if (!string.IsNullOrEmpty(stackTrace))
            {
                switch (type)
                {
                    case LogType.Error:
                    case LogType.Assert:
                    case LogType.Exception:
                        logText += System.Environment.NewLine + stackTrace;
                        break;
                    default:
                        break;
                }
            }

            switch (type)
            {
                case LogType.Error:
                case LogType.Assert:
                case LogType.Exception:
                    logText = string.Format("<color=red>{0}</color>", logText);
                    break;
                case LogType.Warning:
                    logText = string.Format("<color=yellow>{0}</color>", logText);
                    break;
                default:
                    break;
            }

            debugText.text += logText + System.Environment.NewLine;

        }
    }
}