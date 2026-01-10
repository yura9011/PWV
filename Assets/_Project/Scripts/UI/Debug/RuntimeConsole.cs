using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text;

namespace EtherDomes.UI.Debug
{
    public class RuntimeConsole : MonoBehaviour
    {
        [SerializeField] private Text _consoleText;
        [SerializeField] private int _maxLines = 8;
        
        private Queue<string> _logQueue = new Queue<string>();

        private void OnEnable()
        {
            Application.logMessageReceived += HandleLog;
        }

        private void OnDisable()
        {
            Application.logMessageReceived -= HandleLog;
        }

        private void HandleLog(string logString, string stackTrace, LogType type)
        {
            string color = "white";
            if (type == LogType.Error || type == LogType.Exception) color = "red";
            else if (type == LogType.Warning) color = "yellow";

            string formattedLog = $"<color={color}>{logString}</color>";
            
            _logQueue.Enqueue(formattedLog);
            if (_logQueue.Count > _maxLines)
            {
                _logQueue.Dequeue();
            }

            UpdateUI();
        }

        private void UpdateUI()
        {
            if (_consoleText == null) return;
            
            StringBuilder sb = new StringBuilder();
            foreach (var log in _logQueue)
            {
                sb.AppendLine(log);
            }
            _consoleText.text = sb.ToString();
        }
    }
}
